using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public abstract class BaseChoreographer : MonoBehaviour 
{
    public AudioSource musicAudioSource;
    public AudioSource soundEffectsAudioSource;
    public AudioClip music;
    public TextAsset timingsTextAsset;
    public bool showDebugTimingResults = true;

    protected TimingsManager timingsManager;
    private PlayerAction storedPlayerAction = PlayerAction.NONE;
    private bool timingsHandled = false;
    
    public enum PlayerAction
    {
        MOTION_NOD,
        MOTION_DEEP_NOD_DOWN,
        MOTION_DEEP_NOD_UP,
        MOTION_HEAD_TILT,
        NONE
    }

	private void Awake() {
        musicAudioSource.clip = music;
	}
	
    private void Start()
    {
        Initialise();
        musicAudioSource.Play();
    }

    private void FixedUpdate()
    {
        timingsHandled = false;
        HandleInput(storedPlayerAction);
        storedPlayerAction = PlayerAction.NONE;

        GameUpdate();
        HandlePlayerTimings(PlayerAction.NONE);
        BaseHandleTriggers();
    }
    
    private void BaseHandleTriggers()
    {
        List<string> triggers = timingsManager.checkForTrigger(musicAudioSource.time);
        HandleTriggers(triggers);
    }

    /** Checks whether the player was on beat or not **/
    protected void HandlePlayerTimings(PlayerAction playerAction)
    {
        // Only handles timings once per FixedUpdate
        if (timingsHandled)
        {
            return;
        }
        timingsHandled = true;

        TimingsManager.TimingResult result;
        bool debugWorthShowing = true;
        if (playerAction != PlayerAction.NONE)
        {
            result = timingsManager.checkAttempt(musicAudioSource.time);
        }
        else
        {
            // User didn't make an attempt and had missed the timing window
            result = timingsManager.checkForMiss(musicAudioSource.time);

            if (result == TimingsManager.TimingResult.IGNORE_ATTEMPT)
            {
                debugWorthShowing = false;
            }
        }

        if (showDebugTimingResults && debugWorthShowing)
        {
            Debug.Log("BaseChoreographer " + Time.time + ": " + result.ToString());
        }

        PlayerTimingResult(result);
    }

    protected abstract void Initialise();

    protected abstract void HandleInput(PlayerAction playerAction);

    protected abstract void GameUpdate();

    protected abstract void HandleTriggers(List<string> triggers);

    protected abstract void PlayerTimingResult(
        TimingsManager.TimingResult result);

    public void InputAction(PlayerAction playerAction)
    {
        storedPlayerAction = playerAction;
    }
}
