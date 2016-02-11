using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class NightWalkChoreograhper : BaseChoreographer
{
    public PoleBoxController poleBoxPrefab;
    public NightWalkCharacterController characterController;
    public Slider slider;
    public AudioClip attemptGoodA;
    public AudioClip attemptGoodB;
    public AudioClip attemptBad;
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
    private readonly string[] events = new string[] { "pop", "roll_audio", "start_roll", "end_roll", "gap", "raises", "raised", "result" };
    private float newPoleHeight = 0;
    private float raiseHeight = 0.7f;
    private bool controllableState = true;

    protected override void Initialise()
    {
        // Loads the timings
        timingsManager = new TimingsManager(timingsTextAsset, new List<string>(events));

        // Saves time to skip the intro music when debugging
        debugMusicStartOffset = 0;// timingsManager.Timings[timingsManager.NextPlayerTimingIndex].time - 3f;
        musicAudioSource.time = debugMusicStartOffset;
        musicAudioSource.pitch = Time.timeScale;

        groundY = characterController.transform.position.y - 0.5f;
        characterRadius = characterController.transform.position.z;

        // Debug UI
        slider.maxValue = musicAudioSource.clip.length;
        slider.minValue = 0;
    }

    protected override void HandleInput(PlayerAction playerAction)
    {
        if (!controllableState)
        {
            return;
        }

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
        TimingsManager.TimingResult result, List<string> triggers)
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
        
        // TODO(jaween): Clean up this repeated code
        switch (result) {
            case TimingsManager.TimingResult.GOOD:
                if (triggers.Contains(events[5]))
                {
                    LowerAllPoles();
                }
                if (triggers.Contains(events[3]))
                {
                    // End of roll
                    poleBox.RollFireworks();
                }
                else if (!triggers.Contains(events[2]))
                {
                    // No effects on a successful start roll
                    TempPlayPositiveSound();
                    poleBox.Pop(PoleBoxController.PopType.POP_GOOD);
                }
                break;
            case TimingsManager.TimingResult.BAD:
                if (triggers.Contains(events[5]))
                {
                    LowerAllPoles();
                }
                if (triggers.Contains(events[3]))
                {
                    // End of roll
                    poleBox.RollFireworks();
                }
                else if (!triggers.Contains(events[2]))
                {
                    // No effects on a successful start roll
                    TempPlayBadSound();
                    poleBox.Pop(PoleBoxController.PopType.POP_BAD);
                }
                break;
            case TimingsManager.TimingResult.MISS:
                poleBox.Pop(PoleBoxController.PopType.POP_MISS);
                if (triggers.Contains(events[5]))
                {
                    musicAudioSource.Stop();
                    characterController.Fall();
                    controllableState = false;
                    StartCoroutine(Restart());
                }
                break;
            default:
                Debug.Log("Unknown TimingResult");
                break;
        }
    }

    protected override void HandleTriggers(List<string> triggers)
    {
        float nextTriggerTiming = 
            timingsManager.Timings[timingsManager.NextTriggerTimingIndex].time;
        float twoNextTriggerTiming = 
            timingsManager.Timings[timingsManager.NextTriggerTimingIndex + 1].time;
        PoleBoxController poleBox;
        foreach (var trigger in triggers)
        {
            // TODO(jaween): Replace with enums or class of const ints
            switch (Array.IndexOf(events, trigger)) {
                case 0:
                    // TODO(jaween): Implement
                    break;
                case 1:
                    tempAudioSourceB.Play();
                    poleBox = poleBoxes[nextTriggerTiming];
                    poleBox.RollSpinEffects(true);
                    poleBox = poleBoxes[twoNextTriggerTiming];
                    poleBox.RollSpinEffects(false);
                    break;
                case 2:
                    // No implementation
                    break;
                case 3:
                    // No implementation
                    break;
                case 4:
                    // No implmentation
                    break;
                case 5:
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
            if (timing.triggers.Contains(events[0]))
            {
                nextIndexToInstantiate++;
                return;
            }

            if (leadingTime >= timing.time)
            {
                const float degreesPerSecond = 11.5f;
                const float startAngleDegrees = 90f;

                if (timing.triggers.Contains(events[6]))
                {
                    newPoleHeight += raiseHeight;
                }

                float angleOffsetDegrees = 
                    (timing.time - debugMusicStartOffset) * degreesPerSecond;
                float angle = (startAngleDegrees - angleOffsetDegrees) * 
                    Mathf.Deg2Rad;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * characterRadius,
                    groundY + newPoleHeight,
                    Mathf.Sin(angle) * characterRadius);
                Quaternion rotation = Quaternion.LookRotation(position, Vector3.up);
                PoleBoxController poleBox = (PoleBoxController) Instantiate(
                    poleBoxPrefab, position, rotation);
                SetPlatformType(poleBox, timing.triggers);
                
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

    private void SetPlatformType(PoleBoxController poleBox, List<string> triggers)
    {
        // Platform
        if (triggers.Contains(events[4]))
        {
            poleBox.SetPlatform(PoleBoxController.PlatformType.PLATFORM_NONE);
        }
        else if (triggers.Contains(events[2]))
        {
            poleBox.SetPlatform(PoleBoxController.PlatformType.PLATFORM_SHORT);
        }
        else if (triggers.Contains(events[3]))
        {
            poleBox.SetPlatform(PoleBoxController.PlatformType.PLATFORM_LONG);
        }
        else
        {
            poleBox.SetPlatform(PoleBoxController.PlatformType.PLATFORM_MEDIUM);
        }
    }

    private void LowerAllPoles()
    {
        foreach (var pole in poleBoxes.Values)
        {
            pole.Lower(raiseHeight);
        }
        newPoleHeight -= raiseHeight;
    }
    
    private void TempPlayPositiveSound()
    {
        AudioClip clip;
        clip = audioIsOnUpBeat == true ? attemptGoodA : attemptGoodB;
        audioIsOnUpBeat = !audioIsOnUpBeat;
        soundEffectsAudioSource.clip = clip;
        soundEffectsAudioSource.Play();
    }

    private void TempPlayBadSound()
    {
        soundEffectsAudioSource.clip = attemptBad;
        soundEffectsAudioSource.Play();
    }

    private IEnumerator Restart()
    {
        yield return new WaitForSeconds(4.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}