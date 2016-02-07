using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NightWalkChoreograhper : BaseChoreographer
{
    public GameObject poleBox;
    public NightWalkCharacterController characterController;
    public Slider slider;
    public float jumpHeight = 1;

    private Queue<GameObject> poleBoxes = new Queue<GameObject>();
    private int instantiatedIndex = -1;
    private float characterRadius;

    protected override void Initialise()
    {
        // TODO(jaween): Load these from a file
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

        // Temp offset due to using the wrong file to do timings
        for (var i = 0; i < times.Count; i++)
        {
            times[i] -= 83.55f;
        }
        TimingsManager tempTimings = new TimingsManager(times);
        this.timings = tempTimings;

        musicAudioSource.time = timings.Timings[timings.CurrentTimingIndex] - 3.0f;

        characterRadius = characterController.transform.position.z;

        // Debug UI
        slider.maxValue = musicAudioSource.clip.length;
        slider.minValue = 0;
    }

    protected override void GameUpdate()
    { 
        CreatePoles();
        HandleInput();

        // Update debug UI
        slider.value = musicAudioSource.time;
    }

    private void CreatePoles()
    {
        if (instantiatedIndex < timings.CurrentTimingIndex + 5 && 
            timings.CurrentTimingIndex + 5 < timings.Timings.Count)
        {
            instantiatedIndex++;

            float startAngle = 90f;
            float angleGap = 14f;
            float value = startAngle - instantiatedIndex * angleGap * Mathf.Deg2Rad;
            float x = Mathf.Cos(value) * characterRadius;
            float z = Mathf.Sin(value) * characterRadius;
            
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

    private void HandleInput()
    {
        switch (playerAction)
        {
            case PlayerAction.MOTION_NOD:
                characterController.Jump();
                break;
            case PlayerAction.MOTION_DEEP_NOD_DOWN:
                characterController.Roll();
                break;
            case PlayerAction.MOTION_DEEP_NOD_UP:
                characterController.EndRoll();
                break;
            case PlayerAction.MOTION_HEAD_TILT:
            case PlayerAction.NONE:
                break;
            default:
                break;
        }
    }
}