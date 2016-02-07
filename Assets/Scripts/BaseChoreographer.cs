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
    public AudioClip positiveSoundEffect;
    public AudioClip negativeSoundEffect;
    public bool showDebugTimingResults = true;

    protected TimingsManager timings;
    protected PlayerAction playerAction = PlayerAction.NONE;
    
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
        HandleTimings();
        GameUpdate();
        playerAction = PlayerAction.NONE;
    }

    private void HandleTimings()
    {
        TimingsManager.TimingResult result;
        bool debugWorthShowing = true;
        if (playerAction != PlayerAction.NONE)
        {
            result = timings.checkAttempt(musicAudioSource.time);
        }
        else
        {
            // User didn't make an attempt and had missed the timing window
            result = timings.checkForMiss(musicAudioSource.time);

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

    protected abstract void GameUpdate();

    protected abstract void PlayerTimingResult(
        TimingsManager.TimingResult result);

    public void InputAction(PlayerAction playerAction)
    {
        this.playerAction = playerAction;
    }
}
