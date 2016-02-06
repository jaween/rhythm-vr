using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NightWalkChoreograhper : Choreographer
{
    public GameObject poleBox;
    public NightWalkCharacterController characterController;
    public Slider slider;
    public float jumpHeight = 1;

    private Queue<GameObject> poleBoxes = new Queue<GameObject>();
    private int instantiatedIndex = -1;

    private new void Awake()
    {
        base.Awake();

        // TODO(jaween): Load these from a file
        //this.timings = timings;
        var times = new List<float>() {
            90.426431f,
            90.975231f,
            91.526727f,
            92.062818f,
            92.600449f,
            93.147323f,
            93.679178f,
            94.198709f,
            94.775622f,
            95.316335f,
            95.835480f,
            96.381584f,
            96.913053f,
            97.462239f,
            98.016815f, // a
            98.252510f, // b
            99.076673f,
            99.635101f
        };
        for (var i = 0; i < times.Count; i++)
        {
            times[i] -= 83.55f;
        }
        TimingsManager tempTimings = new TimingsManager(times);
        this.timings = tempTimings;
    }

    private new void Start()
    {
        musicAudioSource.time = timings.Timings[timings.CurrentTimingIndex] - 3.0f;
        base.Start();

        // Debug UI
        slider.maxValue = musicAudioSource.clip.length;
        slider.minValue = 0;
    }

    private new void FixedUpdate()
    {
        base.FixedUpdate();

        float radius = 3;

        CreatePoles(radius);

        // Debug input
        if (Input.GetButton("Fire1"))
        {
            characterController.Jump();
        }
        if (Input.GetButton("Fire2"))
        {
            characterController.Roll();
        }
        if (Input.GetButtonUp("Fire2"))
        {
            characterController.EndRoll();
        }

        // Update debug UI
        slider.value = musicAudioSource.time;
    }

    private void CreatePoles(float radius)
    {
        if (instantiatedIndex < timings.CurrentTimingIndex + 5 && 
            timings.CurrentTimingIndex + 5 < timings.Timings.Count)
        {
            instantiatedIndex++;

            float startAngle = 90f;
            float angleGap = 14f;
            float value = startAngle - instantiatedIndex * angleGap * Mathf.Deg2Rad;
            float x = Mathf.Cos(value) * radius;
            float z = Mathf.Sin(value) * radius;
            
            Vector3 position = new Vector3(x, -1.0f, z);
            Quaternion rotation = Quaternion.LookRotation(position, Vector3.up);
            GameObject box = (GameObject)GameObject.Instantiate(poleBox, position, rotation);
            poleBoxes.Enqueue(box);
            if (poleBoxes.Count > 10)
            {
                GameObject oldBox = poleBoxes.Dequeue();
                Destroy(oldBox);
            }
        }
    }
}