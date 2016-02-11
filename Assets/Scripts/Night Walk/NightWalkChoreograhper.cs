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
    private float newPoleHeight = 0;
    private float raiseHeight = 0.7f;
    private bool controllableState = true;

    protected override void Initialise() 
    {
        // Loads the timings
        timingsManager = new TimingsManager(timingsTextAsset, new NightWalkTriggers());

        // Saves time to skip the intro music when debugging
        debugMusicStartOffset = 6;// timingsManager.Timings[timingsManager.NextPlayerTimingIndex].time - 3f;
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
                if (characterController.IsReadyForAction())
                {
                    playerMadeAnAttempt = true;
                }
                break;
            case PlayerAction.MOTION_DEEP_NOD_DOWN:
                if (characterController.IsReadyForAction())
                {
                    playerMadeAnAttempt = true;
                }
                break;
            case PlayerAction.MOTION_DEEP_NOD_UP:
                if (characterController.IsRolling())
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
        TimingsManager.TimingResult result, List<int> triggers, PlayerAction playerAction)
    {       
        // TODO(jaween): Clean up this repeated code
        switch (result) {
            case TimingsManager.TimingResult.GOOD:
                TimingResultGood(triggers, playerAction);
                break;
            case TimingsManager.TimingResult.BAD:
                TimingResultBad(triggers, playerAction);
                break;
            case TimingsManager.TimingResult.MISS:
                TimingResultMiss(triggers);
                break;
            case TimingsManager.TimingResult.NO_BEAT:
                TimingResultNoBeat(playerAction);
                break;
            default:
                Debug.Log("Unknown TimingResult");
                break;
        }
    }

    protected override void HandleTriggers(List<int> triggers)
    {        
        foreach (var trigger in triggers)
        {
            // TODO(jaween): Replace with enums or class of const ints
            switch (trigger) {
                case NightWalkTriggers.POP:
                    // TODO(jaween): Implement
                    break;
                case NightWalkTriggers.ROLL_AUDIO:
                    tempAudioSourceB.Play();
                    PoleBoxController poleBox;
                    if (GetNextPoleBox(0, true, out poleBox))
                    {
                        poleBox.RollSpinEffects(true);
                    }
                    if (GetNextPoleBox(1, true, out poleBox))
                    {
                        poleBox.RollSpinEffects(true);
                    }
                    poleBox.RollSpinEffects(false);
                    break;
                case 3:
                    // No implementation
                    break;
                case 4:
                    // No implementation
                    break;
                case 5:
                    // No implmentation
                    break;
                case 6:
                    // No implementation
                    break;
                default:
                    Debug.Log("Unknown trigger " + trigger);
                    break;
            }
        }
    }

    private void PerformGoodAttemptMedia(PoleBoxController poleBox, PoleBoxController.PopType popupType)
    {
        TempPlayGoodAttemptSound();
        if (poleBox != null)
        {
            poleBox.Pop(popupType);
        }
    }

    private void PerformBadAttemptMedia(PoleBoxController poleBox, PoleBoxController.PopType popupType)
    {
        TempPlayBadAttemptSound();
        if (poleBox != null)
        {
            poleBox.Pop(popupType);
        }
    }

    private void TimingResultGood(List<int> triggers, PlayerAction playerAction)
    {
        PoleBoxController poleBox = null;
        GetNextPoleBox(-1, false, out poleBox);

        // TODO(jaween): Fix ArgumentOutOfRangeException after final timing
        bool actionWasSuccess = false;
        if (triggers.Contains(NightWalkTriggers.BEAT))
        {
            if (playerAction == PlayerAction.MOTION_NOD)
            {
                actionWasSuccess = true;
                characterController.Jump(false, false);
                PerformGoodAttemptMedia(poleBox, PoleBoxController.PopType.POP_GOOD);
            }
            else
            {
                characterController.Jump(false, true);
                PerformBadAttemptMedia(poleBox, PoleBoxController.PopType.POP_MISS);
            }
        }
        else if (triggers.Contains(NightWalkTriggers.START_ROLL))
        {
            if (playerAction == PlayerAction.MOTION_DEEP_NOD_DOWN)
            {
                actionWasSuccess = true;
                characterController.Roll();
                PerformGoodAttemptMedia(poleBox, PoleBoxController.PopType.POP_EMPTY);
            }
            else
            {
                if (playerAction == PlayerAction.MOTION_NOD)
                {
                    characterController.Jump(false, true);
                }
                PerformBadAttemptMedia(poleBox, PoleBoxController.PopType.POP_MISS);
            }
        } 
        else if (triggers.Contains(NightWalkTriggers.END_ROLL))
        {
            if (playerAction == PlayerAction.MOTION_DEEP_NOD_UP)
            {
                actionWasSuccess = true;
                characterController.EndRoll();
                PerformGoodAttemptMedia(poleBox, PoleBoxController.PopType.POP_EMPTY);
            }
            else
            {
                // TODO(jaween): End roll negatively
                characterController.EndRoll();
                PerformBadAttemptMedia(poleBox, PoleBoxController.PopType.POP_MISS);
            }
        }

        if (triggers.Contains(NightWalkTriggers.RAISES))
        {
            if (playerAction == PlayerAction.MOTION_NOD)
            {
                actionWasSuccess = true;
            }
            else
            {
                GameOver();
            }

            LowerAllPoles();
        }

        // AddToScore(actionWasSucess);
    }

    private void TimingResultBad(List<int> triggers, PlayerAction playerAction)
    {
        PoleBoxController poleBox = null;
        GetNextPoleBox(-1, false, out poleBox);

        // TODO(jaween): Fix ArgumentOutOfRangeException after final timing
        bool actionWasSuccess = false;
        if (triggers.Contains(NightWalkTriggers.BEAT))
        {
            if (playerAction == PlayerAction.MOTION_NOD)
            {
                actionWasSuccess = true;
                characterController.Jump(false, false);
                PerformBadAttemptMedia(poleBox, PoleBoxController.PopType.POP_BAD);
            }
            else
            {
                characterController.Jump(false, true);
                PerformBadAttemptMedia(poleBox, PoleBoxController.PopType.POP_MISS);
            }
        }
        else if (triggers.Contains(NightWalkTriggers.START_ROLL))
        {
            if (playerAction == PlayerAction.MOTION_DEEP_NOD_DOWN)
            {
                actionWasSuccess = true;
                characterController.Roll();
                PerformBadAttemptMedia(poleBox, PoleBoxController.PopType.POP_EMPTY);
            }
            else
            {
                if (playerAction == PlayerAction.MOTION_NOD)
                {
                    characterController.Jump(false, true);
                }
                PerformBadAttemptMedia(poleBox, PoleBoxController.PopType.POP_MISS);
            }
        }
        else if (triggers.Contains(NightWalkTriggers.END_ROLL))
        {
            if (playerAction == PlayerAction.MOTION_DEEP_NOD_UP)
            {
                // TODO(jaween): No fireworks?
                actionWasSuccess = true;
                characterController.EndRoll();
                PerformBadAttemptMedia(poleBox, PoleBoxController.PopType.POP_EMPTY);
            }
            else
            {
                // TODO(jaween): End roll negatively
                characterController.EndRoll();
                PerformBadAttemptMedia(poleBox, PoleBoxController.PopType.POP_MISS);
            }
        }

        if (triggers.Contains(NightWalkTriggers.RAISES))
        {
            if (playerAction == PlayerAction.MOTION_NOD)
            {
                actionWasSuccess = true;
            }
            else
            {
                GameOver();
            }

            LowerAllPoles();
        }

        // AddToScore(actionWasSucess);
    }

    private void TimingResultMiss(List<int> triggers)
    {
        PoleBoxController poleBox;
        if (GetNextPoleBox(-1, false, out poleBox))
        {
            poleBox.Pop(PoleBoxController.PopType.POP_MISS);
        }

        // TODO(jaween): End roll unsucessfully
        // Just trying to avoid being stuck rolling
        characterController.EndRoll();

        // Game over player one
        if (triggers.Contains(NightWalkTriggers.RAISES))
        {
            GameOver();
        }
    }

    private void TimingResultNoBeat(PlayerAction playerAction)
    {
        if (playerAction == PlayerAction.MOTION_DEEP_NOD_UP ||
            playerAction == PlayerAction.MOTION_NOD)
        {
            characterController.Jump(false, true);
            TempPlayBadAttemptSound();
        }
    }

    private void GameOver()
    {
        musicAudioSource.Stop();
        characterController.Fall();
        controllableState = false;
        StartCoroutine(Restart());
    }

    private bool GetNextPoleBox(int offset, bool trigger, out PoleBoxController poleBox)
    {
        poleBox = null;
        int index;
        if (trigger)
        {
            index = timingsManager.NextTriggerTimingIndex + offset;
        }
        else
        {
            index = timingsManager.NextPlayerTimingIndex + offset;
        }

        float timing = 0;
        if (index >= 0 && index < timingsManager.Timings.Count)
        {
            timing = timingsManager.Timings[index].time;
        }
        else
        {
            return false;
        }
        
        if (poleBoxes.ContainsKey(timing))
        {
            poleBox = poleBoxes[timing];
            return true;
        }
        return false;
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
            if (timing.triggers.Contains(NightWalkTriggers.POP))
            {
                nextIndexToInstantiate++;
                return;
            }

            if (leadingTime >= timing.time)
            {
                const float degreesPerSecond = 11.5f;
                const float startAngleDegrees = 90f;

                if (timing.triggers.Contains(NightWalkTriggers.RAISES))
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

    private void SetPlatformType(PoleBoxController poleBox, List<int> triggers)
    {
        // Platform
        if (triggers.Contains(NightWalkTriggers.GAP))
        {
            poleBox.SetPlatform(PoleBoxController.PlatformType.PLATFORM_NONE);
        }
        else if (triggers.Contains(NightWalkTriggers.START_ROLL))
        {
            poleBox.SetPlatform(PoleBoxController.PlatformType.PLATFORM_SHORT);
        }
        else if (triggers.Contains(NightWalkTriggers.END_ROLL))
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
    
    private void TempPlayGoodAttemptSound()
    {
        AudioClip clip;
        clip = audioIsOnUpBeat == true ? attemptGoodA : attemptGoodB;
        audioIsOnUpBeat = !audioIsOnUpBeat;
        soundEffectsAudioSource.clip = clip;
        soundEffectsAudioSource.Play();
    }

    private void TempPlayBadAttemptSound()
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