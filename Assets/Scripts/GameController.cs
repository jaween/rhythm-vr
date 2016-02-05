using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

    public CardboardHeadController cardboardHeadController;

	void Awake () 
    {
        // TODO(jaween): Load the timings from a file
        //var times = new List<float>() { 1f, 3f, 5f, 7f, 9f, 10f, 11f };
        //Timings timings = new Timings(times);
        //Choreographer choreographer = new Choreographer(timings);
        //choreographer.musicAudioSource = cardboardHeadController;
        //cardboardHeadController.choreographer = choreographer;
	}
}
