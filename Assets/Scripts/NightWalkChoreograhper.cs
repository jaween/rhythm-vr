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
    public float jumpHeight = 1;
    public float groundY;

    private Dictionary<float, PoleBoxController> poleBoxes =
        new Dictionary<float, PoleBoxController>();
    private int nextIndexToInstantiate = 0;
    private int nextIndexToDestroy = 0;
    private float characterRadius;
    private float debugMusicStartOffset;
    private PoleBoxController previousPoleBox = null;

    protected override void Initialise()
    {
        // TODO(jaween): Load these from a file
        var times = new List<float>() {
            90.426431f,
            90.975231f,
            91.526727f,
            92.062818f,
            92.600449f,
            93.147323f,
            93.679178f,
            94.198709f,
            94.775622f,
            95.316335f,
            95.835480f,
            96.381584f,
            96.913053f,
            97.462239f,
            98.016815f, // a
            98.252510f, // b
            99.076673f,
            99.635101f
        };

        // Temp offset due to using the wrong file to do timings
        for (var i = 0; i < times.Count; i++)
        {
            times[i] -= 83.55f;
        }
        TimingsManager tempTimings = new TimingsManager(times);
        this.timings = tempTimings;

        // Saves time to skip the intro music when debugging
        debugMusicStartOffset = timings.Timings[timings.CurrentTimingIndex] - 3.0f;
        musicAudioSource.time = debugMusicStartOffset;

        groundY = characterController.transform.position.y - 0.5f;
        characterRadius = characterController.transform.position.z;

        // Debug UI
        slider.maxValue = musicAudioSource.clip.length;
        slider.minValue = 0;
    }

    protected override void GameUpdate()
    {
        CreateAndDestroyPoles();
        HandleInput();

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
        float timing = timings.Timings[timings.CurrentTimingIndex - 1];
        PoleBoxController poleBox = poleBoxes[timing];

        if (result == TimingsManager.TimingResult.GOOD)
        {
            TempPlaySound(positiveSoundEffect);
            poleBox.Pop(true);
        }
        else if (result == TimingsManager.TimingResult.BAD ||
                 result == TimingsManager.TimingResult.MISS)
        {
            TempPlaySound(negativeSoundEffect);
            poleBox.Pop(false);
        }
    }

    private void CreateAndDestroyPoles()
    {
        const float secondsInAdvance = 6;
        const float secondsBehind = 4;
        float leadingTime = musicAudioSource.time + secondsInAdvance;
        float laggingTime = musicAudioSource.time - secondsBehind;

        if (nextIndexToInstantiate < timings.Timings.Count)
        {
            float previousTiming = timings.Timings[nextIndexToInstantiate];
            const float longThreshold = 0.6f;
            const float shortThreshold = 0.4f;

            if (nextIndexToInstantiate > 0)
            {
                previousTiming = timings.Timings[nextIndexToInstantiate - 1];
            }

            float timing = timings.Timings[nextIndexToInstantiate];
            if (leadingTime >= timing)
            {
                const float degreesPerSecond = 18f;
                const float startAngleDegrees = 90f;

                float angleOffsetDegrees = 
                    (timing - debugMusicStartOffset) * degreesPerSecond;
                float angle = (startAngleDegrees - angleOffsetDegrees) * 
                    Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * characterRadius;
                float y = groundY;
                float z = Mathf.Sin(angle) * characterRadius;

                Vector3 position = new Vector3(x, y, z);
                Quaternion rotation = Quaternion.LookRotation(position, Vector3.up);
                PoleBoxController poleBox = (PoleBoxController) Instantiate(
                    poleBoxPrefab, position, rotation);
                float timeDelta = timing - previousTiming;
                if (timeDelta < shortThreshold && previousPoleBox != null)
                {
                    previousPoleBox.ShowPlatform(PoleBoxController.PlatformType.PLATFORM_SHORT);
                }
                else if (timeDelta > longThreshold && previousPoleBox != null)
                {
                    previousPoleBox.ShowPlatform(PoleBoxController.PlatformType.PLATFORM_LONG);
                }
                else if (previousPoleBox != null)
                {
                    previousPoleBox.ShowPlatform(PoleBoxController.PlatformType.PLATFORM_MEDIUM);
                }
                poleBoxes.Add(timing, poleBox);
                previousPoleBox = poleBox;

                nextIndexToInstantiate++;
            }
        }

        // Destroys pole boxes
        if (nextIndexToDestroy < timings.Timings.Count)
        {
            float nextToDestroyTiming = timings.Timings[nextIndexToDestroy];
            if (nextToDestroyTiming < laggingTime)
            {
                PoleBoxController oldBox = poleBoxes[nextToDestroyTiming];
                poleBoxes.Remove(nextToDestroyTiming);
                oldBox.DestroyPoleBox();
                nextIndexToDestroy++;
            }
        }
    }

    private void HandleInput()
    {
        switch (playerAction)
        {
            case PlayerAction.MOTION_NOD:
                characterController.Jump();
                break;
            case PlayerAction.MOTION_DEEP_NOD_DOWN:
                characterController.Roll();
                break;
            case PlayerAction.MOTION_DEEP_NOD_UP:
                characterController.EndRoll();
                break;
            case PlayerAction.MOTION_HEAD_TILT:
            case PlayerAction.NONE:
                break;
            default:
                break;
        }
    }

    private void TempPlaySound(AudioClip clip)
    {
        soundEffectsAudioSource.clip = clip;
        soundEffectsAudioSource.Play();
    }
}