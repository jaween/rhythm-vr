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
        var times = new List<float>() { 0.536961f,
1.088435f,
1.712472f,
2.321995f,
2.917007f,
3.541043f,
4.150567f,
4.774603f,
5.442177f,
6.051701f,
6.675737f,
7.343311f,
7.981859f,
8.591383f,
9.186395f,
9.520181f,
9.839456f,
10.507029f,
11.160091f,
11.813152f,
12.480726f,
13.061224f,
13.670748f,
14.294785f,
14.962358f,
15.629932f,
16.239456f,
16.848980f,
17.458503f,
18.111565f,
18.721088f,
19.330612f,
19.664399f,
19.998186f };
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
