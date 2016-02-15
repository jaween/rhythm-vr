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
    public GameObject groundNode;
    public ScreenFader fader;
    public AudioSource attemptAudioSource;
    public AudioSource rollAAudioSource;
    public AudioSource rollBAudioSource;
    public AudioSource popAudioSource;
    public AudioClip attemptGoodA;
    public AudioClip attemptGoodB;
    public AudioClip attemptBad;
    public AudioClip attemptMiss;
    public AudioClip attemptNoBeat;
    public AudioClip rollA;
    public AudioClip rollB;
    public AudioClip superb;
    public AudioClip japaneseMusic;
    public AudioClip englishMusic;
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
    private bool controllableState = false;
    private bool gameOver = false;
    private bool gameComplete = false;
    private bool englishAudio = true;
    private int goodTimings = 0;
    private int badTimings = 0;
    private int missTimings = 0;
    private float lastTriggerTime = 0.0f;
    
    protected override void Initialise() 
    {
        // Loads the timings
        timingsManager = new TimingsManager(timingsTextAsset, new NightWalkTriggers());

        // Saves time to skip the intro music when debugging
        debugMusicStartOffset = 0;
        musicAudioSource.time = debugMusicStartOffset;
        musicAudioSource.pitch = Time.timeScale;

        groundY = groundNode.transform.position.y;
        characterRadius = characterController.transform.position.z;

        // Screen fade in
        bool fadeFromBlack = true;
        float duration = 2.0f;
        fader.Fade(fadeFromBlack, duration);
    }

    private void Update()
    {
        // TODO(jaween): Should we try to resync the audio at intervals?
        /*if ((Time.time - musicStartTime) % 5.0f == 0.0f)
        {
            musicAudioSource.time = Time.time - musicStartTime;
        }*/
        SwitchAudioOnTriggerHold();
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
                if (characterController.IsReadyForNextAction())
                {
                    playerMadeAnAttempt = true;
                }
                break;
            case PlayerAction.MOTION_DEEP_NOD_DOWN:
                if (characterController.IsReadyForNextAction())
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
        // TODO(jaween): Replace this with something good and sane
        if (characterController.StartedRunning && !gameOver)
        {
            controllableState = true;
        }

        CreateAndDestroyPoles();
    }

    protected override void PlayerTimingResult(
        TimingsManager.TimingResult result, List<int> triggers, PlayerAction playerAction)
    {
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
                    characterController.Pop();
                    popAudioSource.Play();
                    break;
                case NightWalkTriggers.ROLL_AUDIO:
                    rollAAudioSource.Play();
                    PoleBoxController poleBox;
                    if (GetNextPoleBox(0, true, out poleBox))
                    {
                        poleBox.RollSpinEffects(true);
                    }
                    if (GetNextPoleBox(1, true, out poleBox))
                    {
                        poleBox.RollSpinEffects(false);
                    }
                    break;
                case NightWalkTriggers.COMPLETE:
                    bool fadeFromBlack = false;
                    float duration = 3.0f;
                    fader.Fade(fadeFromBlack, duration);
                    controllableState = false;
                    gameComplete = true;
                    StartCoroutine(GameCompleteCoroutine());
                    break;
            }
        }
    }
    
    private void PopPoleBoxAndPlayAudio(PoleBoxController poleBox, 
        PoleBoxController.PopType popupType)
    {
        switch (popupType)
        {
            case PoleBoxController.PopType.POP_GOOD:
            case PoleBoxController.PopType.POP_FIREWORKS:
                PlayAttemptSound(TimingsManager.TimingResult.GOOD);
                break;
            case PoleBoxController.PopType.POP_BAD:
                PlayAttemptSound(TimingsManager.TimingResult.BAD);
                break;
            case PoleBoxController.PopType.POP_MISS:
                PlayAttemptSound(TimingsManager.TimingResult.MISS);
                break;
        }

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
                characterController.Jump();
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_GOOD);
            }
            else
            {
                // Rolls can be initiated any time
                if (playerAction == PlayerAction.MOTION_DEEP_NOD_DOWN)
                {
                    characterController.Roll();
                }
                else
                { 
                    characterController.StuntedJump();
                }
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_MISS);
            }
        }
        else if (triggers.Contains(NightWalkTriggers.START_ROLL))
        {
            if (playerAction == PlayerAction.MOTION_DEEP_NOD_DOWN)
            {
                actionWasSuccess = true;
                characterController.Roll();
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_EMPTY);
            }
            else
            {
                if (playerAction == PlayerAction.MOTION_NOD)
                {
                    characterController.StuntedJump();
                }
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_MISS);
            }
        } 
        else if (triggers.Contains(NightWalkTriggers.END_ROLL))
        {
            if (playerAction == PlayerAction.MOTION_DEEP_NOD_UP)
            {
                actionWasSuccess = true;
                characterController.HighJump();
                if (poleBox != null)
                {
                    poleBox.Pop(PoleBoxController.PopType.POP_FIREWORKS);
                }
                rollBAudioSource.Play();
            }
            else
            {
                // Rolls can be initiated any time
                if (playerAction == PlayerAction.MOTION_DEEP_NOD_DOWN)
                {
                    characterController.Roll();
                }
                else
                {
                    characterController.StuntedJump();
                }
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_MISS);
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
        goodTimings++;
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
                characterController.Jump();
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_BAD);
            }
            else
            {
                // Rolls can be initiated any time
                if (playerAction == PlayerAction.MOTION_DEEP_NOD_DOWN)
                {
                    characterController.Roll();
                }
                else
                {
                    characterController.StuntedJump();
                }
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_MISS);
            }
        }
        else if (triggers.Contains(NightWalkTriggers.START_ROLL))
        {
            if (playerAction == PlayerAction.MOTION_DEEP_NOD_DOWN)
            {
                actionWasSuccess = true;
                characterController.Roll();
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_EMPTY);
            }
            else
            {
                if (playerAction == PlayerAction.MOTION_NOD)
                {
                    characterController.StuntedJump();
                }
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_MISS);
            }
        }
        else if (triggers.Contains(NightWalkTriggers.END_ROLL))
        {
            if (playerAction == PlayerAction.MOTION_DEEP_NOD_UP)
            {
                // Showing the positive effects here just for feedback to the
                // user that their gesture was successful
                actionWasSuccess = true;
                characterController.HighJump();
                if (poleBox != null)
                {
                    poleBox.Pop(PoleBoxController.PopType.POP_FIREWORKS);
                }
                rollBAudioSource.Play();
            }
            else
            {
                // TODO(jaween): End roll negatively
                // Rolls can be initiated any time
                if (playerAction == PlayerAction.MOTION_DEEP_NOD_DOWN)
                {
                    characterController.Roll();
                }
                else
                {
                    characterController.StuntedJump();
                }
                PopPoleBoxAndPlayAudio(poleBox, 
                    PoleBoxController.PopType.POP_MISS);
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
        badTimings++;
    }

    private void TimingResultMiss(List<int> triggers)
    {
        PoleBoxController poleBox;
        if (GetNextPoleBox(-1, false, out poleBox))
        {
            if (triggers.Contains(NightWalkTriggers.START_ROLL))
            {
                PopPoleBoxAndPlayAudio(poleBox,
                    PoleBoxController.PopType.POP_EMPTY);
                missTimings++;
            }
            else if (triggers.Contains(NightWalkTriggers.END_ROLL))
            {
                PopPoleBoxAndPlayAudio(poleBox,
                    PoleBoxController.PopType.POP_MISS);
                missTimings++;
            }
            else
            {
                PopPoleBoxAndPlayAudio(poleBox,
                    PoleBoxController.PopType.POP_MISS);
                missTimings++;
            }
        }

        // Game over player one
        if (triggers.Contains(NightWalkTriggers.RAISES))
        {
            GameOver();
        }
    }

    private void TimingResultNoBeat(PlayerAction playerAction)
    {
        if (playerAction != PlayerAction.NONE)
        {
            // Rolls can be initiated any time
            if (playerAction == PlayerAction.MOTION_DEEP_NOD_DOWN)
            {
                characterController.Roll();
            }
            else
            {
                characterController.StuntedJump();
                PlayAttemptSound(TimingsManager.TimingResult.NO_BEAT);
            }
        }
    }

    private void GameOver()
    {
        gameOver = true;
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
        const float secondsInAdvance = 5;
        const float secondsBehind = 4;
        float leadingTime = Time.time - musicStartTime + secondsInAdvance;
        float laggingTime = Time.time - musicStartTime - secondsBehind;

        if (nextIndexToInstantiate < timingsManager.Timings.Count)
        {
            TimingsManager.Timing timing = timingsManager.Timings[nextIndexToInstantiate];
            if (!timing.triggers.Contains(NightWalkTriggers.BEAT) &&
                !timing.triggers.Contains(NightWalkTriggers.START_ROLL) &&
                !timing.triggers.Contains(NightWalkTriggers.END_ROLL))
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
                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * characterRadius,
                    groundY + newPoleHeight,
                    Mathf.Sin(angle) * characterRadius);
                Vector3 lookForward = position;
                lookForward.y = 0;
                Quaternion rotation = Quaternion.LookRotation(lookForward, Vector3.up);
                PoleBoxController poleBox = (PoleBoxController) Instantiate(
                    poleBoxPrefab, position, rotation);

                SetPlatformType(poleBox, timing.triggers);
                poleBoxes.Add(timing.time, poleBox);

                if (timing.triggers.Contains(NightWalkTriggers.RAISES))
                {
                    newPoleHeight += raiseHeight;
                }

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
                    StartCoroutine(RemovePoleBoxWhenDestroyedCoroutine(oldBox,
                        nextToDestroyTiming));
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

    private void SwitchAudioOnTriggerHold()
    {
        float deltaTime = 1.0f;

        if (isCardboardTriggered)
        {
            deltaTime = Time.time - lastTriggerTime;
            lastTriggerTime = Time.time;    
        }

        if (deltaTime < 0.5f)
        {
            englishAudio = !englishAudio;
            musicAudioSource.Stop();
            musicAudioSource.clip = englishAudio ? englishMusic : japaneseMusic;
            musicAudioSource.time = Time.time - musicStartTime;
            musicAudioSource.Play();
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
    
    /** Keeps the PoleBox around in the Dictionary until it has finished its
        fade out animation (as it can still be lowered while fading out) **/
    private IEnumerator RemovePoleBoxWhenDestroyedCoroutine
        (PoleBoxController poleBox, float timing)
    {
        while (poleBox != null)
        {
            yield return new WaitForEndOfFrame();
        }
        poleBoxes.Remove(timing);
    }
    
    private void PlayAttemptSound(TimingsManager.TimingResult timingResult)
    {
        AudioClip clip = attemptNoBeat;
        switch (timingResult)
        {
            case TimingsManager.TimingResult.GOOD:
                clip = audioIsOnUpBeat == true ? attemptGoodA : attemptGoodB;
                audioIsOnUpBeat = !audioIsOnUpBeat;
                break;
            case TimingsManager.TimingResult.BAD:
                clip = attemptBad;
                break;
            case TimingsManager.TimingResult.MISS:
                clip = attemptMiss;
                break;
            case TimingsManager.TimingResult.NO_BEAT:
                clip = attemptNoBeat;
                break;
        }
        attemptAudioSource.clip = clip;
        attemptAudioSource.Play();
    }

    private IEnumerator Restart()
    {
        while (musicAudioSource.volume > 0.0f)
        {
            musicAudioSource.volume -= 0.02f;
            yield return new WaitForEndOfFrame();
        }
        musicAudioSource.Stop();

        bool fadeFromBlack = false;
        float duration = 10.0f;
        fader.Fade(fadeFromBlack, duration);
        yield return new WaitForSeconds(2.0f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator GameCompleteCoroutine()
    {
        PlayerPrefs.SetInt("good", goodTimings);
        PlayerPrefs.SetInt("bad", badTimings);
        PlayerPrefs.SetInt("miss", missTimings);

        yield return new WaitForSeconds(2.0f);

        musicAudioSource.Stop();
        SceneManager.LoadScene(2);
    }
}