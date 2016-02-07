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
        if (playerAction != PlayerAction.NONE)
        {
            TimingsManager.TimingResult result = timings.checkAttempt(musicAudioSource.time);
            Debug.Log("BaseChoreographer " + Time.time + ": " + result.ToString());
            if (result == TimingsManager.TimingResult.GOOD)
            {
                TempPlaySound(positiveSoundEffect);
            }
            else if (result == TimingsManager.TimingResult.BAD)
            {
                TempPlaySound(negativeSoundEffect);
            }
        }
        else
        {
            // User didn't make an attempt and had missed the timing
            if (timings.checkForMiss(musicAudioSource.time) == TimingsManager.TimingResult.MISS)
            {
                if (showDebugTimingResults)
                {
                    Debug.Log("BaseChoreograhper " + Time.time + ": " + TimingsManager.TimingResult.MISS.ToString());
                }
                TempPlaySound(negativeSoundEffect);
            }
        }
    }

    private void TempPlaySound(AudioClip clip)
    {
        soundEffectsAudioSource.clip = clip;
        soundEffectsAudioSource.Play();
    }

    protected abstract void Initialise();

    protected abstract void GameUpdate();

    public void InputAction(PlayerAction playerAction)
    {
        this.playerAction = playerAction;
    }
}
