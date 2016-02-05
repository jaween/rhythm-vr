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

    private Timings timings;
    private bool userMadeAnAttempt = false;
    public Text debugText;

    private void Awake()
    {
        // TODO(jaween): Load these from a file
        //this.timings = timings;
        var times = new List<float>() { 1f, 4f, 7f, 10f, 13f, 16f, 19f };
        Timings tempTimings = new Timings(times);
        this.timings = tempTimings;
    }

	// Use this for initialization
	private void Start () {
        musicAudioSource.clip = music;
        musicAudioSource.Play();
	}
	
    private void FixedUpdate()
    {
        if (userMadeAnAttempt)
        {
            Timings.TimingResult result = timings.checkAttempt(musicAudioSource.time);
            Debug.Log(Time.time + ": " + result.ToString());
            if (result == Timings.TimingResult.GOOD)
            {
                Play(positiveSoundEffect);
            }
            else if (result == Timings.TimingResult.BAD)
            {
                Play(negativeSoundEffect);
            }
            userMadeAnAttempt = false;
        }
        else
        {
            // User didn't make an attempt an had missed the timing
            if (timings.checkForMiss(musicAudioSource.time) == Timings.TimingResult.MISS)
            {
                Debug.Log(Time.time + ": " + Timings.TimingResult.MISS.ToString());
                Play(negativeSoundEffect);
            }
        }
        debugText.text = "Time: " + musicAudioSource.time;
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
