using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class NightWalkChoreograhper : BaseChoreographer
{
    public PoleBoxController poleBoxPrefab;
    public NightWalkCharacterController characterController;
    public Slider slider;
    public AudioClip regularBeatA;
    public AudioClip regularBeatB;
    public AudioClip doubleBeat;
    public AudioSource tempAudioSourceB;
    public float jumpHeight = 1;
    public float groundY;

    private Dictionary<float, PoleBoxController> poleBoxes =
        new Dictionary<float, PoleBoxController>();
    private int nextIndexToInstantiate = 0;
    private int nextIndexToDestroy = 0;
    private float characterRadius;
    private bool audioIsOnUpBeat = false;
    private float debugMusicStartOffset;
    private const float longBeatThreshold = 0.65f;
    private const float shortBeatThreshold = 0.3f;
    private readonly string[] events = new string[] { "pop", "roll_audio", "start_roll", "end_roll", "result" };

    protected override void Initialise()
    {
        // Loads the timings
        timingsManager = new TimingsManager(timingsTextAsset, new List<string>(events));

        // Saves time to skip the intro music when debugging
        debugMusicStartOffset = 4;// timingsManager.Timings[timingsManager.NextPlayerTimingIndex].time - 3f;
        musicAudioSource.time = debugMusicStartOffset;

        groundY = characterController.transform.position.y - 0.5f;
        characterRadius = characterController.transform.position.z;

        // Debug UI
        slider.maxValue = musicAudioSource.clip.length;
        slider.minValue = 0;
    }

    protected override void HandleInput(PlayerAction playerAction)
    {
        bool playerMadeAnAttempt = false;
        switch (playerAction)
        {
            case PlayerAction.MOTION_NOD:
                bool superJump = false;
                if (characterController.Jump(superJump))
                {
                    playerMadeAnAttempt = true;
                }
                break;
            case PlayerAction.MOTION_DEEP_NOD_DOWN:
                if (characterController.Roll())
                {
                    playerMadeAnAttempt = true;
                }
                break;
            case PlayerAction.MOTION_DEEP_NOD_UP:
                if (characterController.EndRoll())
                {
                    playerMadeAnAttempt = true;
                }
                break;
            case PlayerAction.MOTION_HEAD_TILT:
            case PlayerAction.NONE:
                break;
            default:
                break;
        }

        if (playerMadeAnAttempt)
        {
            HandlePlayerTimings(playerAction);
        }
    }

    protected override void GameUpdate()
    {
        CreateAndDestroyPoles();

        // Update debug UI
        slider.value = musicAudioSource.time;
    }

    protected override void PlayerTimingResult(
        TimingsManager.TimingResult result)
    {
        if (result == TimingsManager.TimingResult.IGNORE_ATTEMPT)
        {
            return;
        }

        // TODO(jaween): Fix ArgumentOutOfRangeException after final timing
        float timing = timingsManager.Timings[timingsManager.NextPlayerTimingIndex - 1].time;

        if (!poleBoxes.ContainsKey(timing))
        {
            return;
        }

        PoleBoxController poleBox = poleBoxes[timing];

        if (result == TimingsManager.TimingResult.GOOD)
        {
            bool positive = true;
            TempPlaySound(positive);
            poleBox.Pop(positive);
        }
        else if (result == TimingsManager.TimingResult.BAD ||
                 result == TimingsManager.TimingResult.MISS)
        {
            bool positive = false;
            TempPlaySound(positive);
            poleBox.Pop(positive);
        }
    }

    protected override void HandleTriggers(List<string> triggers)
    {
        if (triggers == null)
        {
            return;
        }

        foreach (var trigger in triggers)
        {
            // TODO(jaween): Replace with enums or class of const ints
            switch (Array.IndexOf(events, trigger)) {
                case 0:
                    Debug.Log("Pop!");
                    break;
                case 1:
                    tempAudioSourceB.Play();
                    break;
                case 2:
                    // No implementation
                    break;
                case 3:
                    // No implementation
                    break;
                default:
                    Debug.Log("Unknown trigger " + trigger);
                    break;
            }
        }
    }

    private void CreateAndDestroyPoles()
    {
        const float secondsInAdvance = 6;
        const float secondsBehind = 4;
        float leadingTime = musicAudioSource.time + secondsInAdvance;
        float laggingTime = musicAudioSource.time - secondsBehind;

        if (nextIndexToInstantiate < timingsManager.Timings.Count)
        {
            TimingsManager.Timing timing = timingsManager.Timings[nextIndexToInstantiate];
            if (timing.triggers != null && timing.triggers.Contains(events[0]))
            {
                nextIndexToInstantiate++;
                return;
            }

            if (leadingTime >= timing.time)
            {
                const float degreesPerSecond = 11.5f;
                const float startAngleDegrees = 90f;

                float angleOffsetDegrees = 
                    (timing.time - debugMusicStartOffset) * degreesPerSecond;
                float angle = (startAngleDegrees - angleOffsetDegrees) * 
                    Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * characterRadius;
                float y = groundY;
                float z = Mathf.Sin(angle) * characterRadius;

                Vector3 position = new Vector3(x, y, z);
                Quaternion rotation = Quaternion.LookRotation(position, Vector3.up);
                PoleBoxController poleBox = (PoleBoxController) Instantiate(
                    poleBoxPrefab, position, rotation);
                if (timing.triggers != null && timing.triggers.Contains(events[2]))
                {
                    poleBox.SetPlatform(PoleBoxController.PlatformType.PLATFORM_SHORT);
                }
                else if (timing.triggers != null && timing.triggers.Contains(events[3]))
                {
                    poleBox.SetPlatform(PoleBoxController.PlatformType.PLATFORM_LONG);
                }
                else
                {
                    poleBox.SetPlatform(PoleBoxController.PlatformType.PLATFORM_MEDIUM);
                }
                poleBoxes.Add(timing.time, poleBox);

                nextIndexToInstantiate++;
            }
        }

        // Destroys pole boxes
        if (nextIndexToDestroy < timingsManager.Timings.Count)
        {
            float nextToDestroyTiming = timingsManager.Timings[nextIndexToDestroy].time;
            if (nextToDestroyTiming < laggingTime)
            {
                if (poleBoxes.ContainsKey(nextToDestroyTiming))
                {
                    PoleBoxController oldBox = poleBoxes[nextToDestroyTiming];
                    poleBoxes.Remove(nextToDestroyTiming);
                    oldBox.DestroyPoleBox();
                }
                nextIndexToDestroy++;
            }
        }
    }
    
    private void TempPlaySound(bool positive)
    {
        AudioClip clip;
        if (positive)
        {
            clip = audioIsOnUpBeat == true ? regularBeatA: regularBeatB;
            audioIsOnUpBeat = !audioIsOnUpBeat;
            soundEffectsAudioSource.clip = clip;
        } else {
            clip = null;
            audioIsOnUpBeat = false;
        }
        soundEffectsAudioSource.clip = clip;
        soundEffectsAudioSource.Play();

    }
}