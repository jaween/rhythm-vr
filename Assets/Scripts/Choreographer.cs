using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Choreographer : MonoBehaviour 
{
    public AudioSource musicAudioSource;
    public AudioSource soundEffectsAudioSource;
    public AudioClip music;
    public AudioClip positiveSoundEffect;
    public AudioClip negativeSoundEffect;

    protected TimingsManager timings;
    private bool userMadeAnAttempt = false;

	protected void Awake() {
        musicAudioSource.clip = music;
	}
	
    protected void Start()
    {
        musicAudioSource.Play();
    }

    protected void FixedUpdate()
    {
        if (userMadeAnAttempt)
        {
            TimingsManager.TimingResult result = timings.checkAttempt(musicAudioSource.time);
            Debug.Log("BaseChoreographer " + Time.time + ": " + result.ToString());
            if (result == TimingsManager.TimingResult.GOOD)
            {
                Play(positiveSoundEffect);
            }
            else if (result == TimingsManager.TimingResult.BAD)
            {
                Play(negativeSoundEffect);
            }
            userMadeAnAttempt = false;
        }
        else
        {
            // User didn't make an attempt an had missed the timing
            if (timings.checkForMiss(musicAudioSource.time) == TimingsManager.TimingResult.MISS)
            {
                Debug.Log("BaseChoreograhper " + Time.time + ": " + TimingsManager.TimingResult.MISS.ToString());
                Play(negativeSoundEffect);
            }
        }
    }

    public void UserBeat()
    {
        userMadeAnAttempt = true;
    }

    private void Play(AudioClip clip)
    {
        soundEffectsAudioSource.clip = clip;
        soundEffectsAudioSource.Play();
    }
}
