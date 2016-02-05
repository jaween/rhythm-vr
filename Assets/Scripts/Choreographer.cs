using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Choreographer : MonoBehaviour {

    private Timings timings;
    private AudioSource music;
    private AudioSource positiveSoundEffect;
    private AudioSource negativeSoundEffect;

    private bool userMadeAnAttempt = false;

    Choreographer(Timings timings) {
        this.timings = timings;
    }

	// Use this for initialization
	private void Start () {
        Debug.Log("Started even when set to private");
	}
	
	// Update is called once per frame
	private void Update () {
	       
	}

    private void FixedUpdate()
    {
        if (userMadeAnAttempt)
        {
            Timings.TimingResult result = timings.checkAttempt(music.time);
            Debug.Log("Result was " + result.ToString());
            if (result == Timings.TimingResult.GOOD || 
                result == Timings.TimingResult.BAD)
            {
                positiveSoundEffect.Play();
            }
            userMadeAnAttempt = false;
        }
        else
        {
            if (timings.checkForMiss(music.time) == Timings.TimingResult.MISS)
            {
                negativeSoundEffect.Play();
            }
        }
    }

    public void UserBeat()
    {
        userMadeAnAttempt = true;
    }
}
