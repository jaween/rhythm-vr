using UnityEngine;
using UnityEngine.UI;

public class CardboardHeadController : MonoBehaviour
{    
    public bool trackRotation = true;
    public bool trackPosition = true;

    public bool updateEarly = false;
    public Text debugText;
    public AudioSource tempAudio;

    private Quaternion previousRotation;
    private float lastNodTime;


    public Ray Gaze
    {
        get
        {
            UpdateHead();
            return new Ray(transform.position, transform.forward);
        }
    }

    public delegate void HeadUpdatedDelegate(GameObject head);
    public event HeadUpdatedDelegate OnHeadUpdated;

    void Awake()
    {
        Cardboard.Create();
    }

    void Start()
    {
        previousRotation = Cardboard.SDK.HeadPose.Orientation;
        lastNodTime = Time.time;
    }

    private bool updated;

    void Update()
    {
        debugText.text = "AccMag is " + Input.acceleration.magnitude;

        updated = false;  // OK to recompute head pose.
        if (updateEarly)
        {
            UpdateHead();
        }
    }

    // Normally, update head pose now.
    void LateUpdate()
    {
        UpdateHead();
    }

    // Compute new head pose.
    private void UpdateHead()
    {
        if (updated)
        {  // Only one update per frame, please.
            return;
        }
        updated = true;
        Cardboard.SDK.UpdateState();

        var rot = Cardboard.SDK.HeadPose.Orientation;
        transform.localRotation = rot;

        Vector3 pos = Cardboard.SDK.HeadPose.Position;
        transform.localPosition = pos;

        if (OnHeadUpdated != null)
        {
            OnHeadUpdated(gameObject);
        }
    }

    void FixedUpdate()
    {
        CheckForNod(Input.acceleration);
        previousRotation = transform.rotation;
    }

    void CheckForNod(Vector3 acceleration)
    {
        float deltaTime = Time.time - lastNodTime;
        const float nodTimeDelay = 0.4f;

        float magnitude = acceleration.magnitude;
        const float nodThreshold = 1.1f;

        if (deltaTime > nodTimeDelay && magnitude > nodThreshold)
        {
            lastNodTime = Time.time;
            tempAudio.Play();
        }
    }
}
