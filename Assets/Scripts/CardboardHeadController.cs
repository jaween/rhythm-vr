using UnityEngine;
using UnityEngine.UI;

public class CardboardHeadController : MonoBehaviour
{
    public delegate void HeadUpdatedDelegate(GameObject head);
    public event HeadUpdatedDelegate OnHeadUpdated;
    public BaseChoreographer choreographer;
    public bool updateEarly = false;
    public bool trackRotation = true;
    public bool trackPosition = true;

    private Quaternion startRotation;
    private float lastNodTime;
    private bool isDeepNodding = false;
    private bool updated;
    private BaseChoreographer.PlayerAction debugStoredAction =
        BaseChoreographer.PlayerAction.NONE;
    private int frameNumber = 0;
    private float startTime;
    private bool debugMode = false;

    public Ray Gaze
    {
        get
        {
            UpdateHead();
            return new Ray(transform.position, transform.forward);
        }
    }

    private void Awake()
    {
        Cardboard.Create();
    }

    private void Start()
    {
        //startRotation = Quaternion.LookRotation(Gaze.direction);
        startRotation = Quaternion.LookRotation(transform.forward);

        lastNodTime = Time.time;
        startTime = Time.time;

    }

    private void Update()
    {
        updated = false;  // OK to recompute head pose.
        if (updateEarly)
        {
            UpdateHead();
        }

        if (debugMode)
        {
            DebugInput();
        }

        choreographer.CardboardTriggered = Cardboard.SDK.Triggered;
    }

    // Normally, update head pose now.
    private void LateUpdate()
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

    private void FixedUpdate()
    {
        DetectAndDispatchHeadMotionInput(Input.acceleration);
    }

    private void DetectAndDispatchHeadMotionInput(Vector3 acceleration)
    {
        float magnitude = acceleration.magnitude;
        const float nodTimeDelay = 0.2f;
        const float nodThreshold = 1.05f;
        const float deepNodUpAngle = 5;
        const float deepNodDownAngle = 20;
        float deltaTime = Time.time - lastNodTime;

        // Angle between quaternions 
        Quaternion gazeRotation = Quaternion.LookRotation(Gaze.direction);
        float yawDegrees = Mathf.Atan2(Gaze.direction.x, Gaze.direction.z) * Mathf.Rad2Deg;
        Quaternion rotateAroundAxis = Quaternion.AngleAxis(yawDegrees, Vector3.up);
        Quaternion alignedStartRotation = rotateAroundAxis * startRotation;
        float angle = Quaternion.Angle(gazeRotation, alignedStartRotation);

        //float signedAngle = SignedAngle(gazeRotation, startRotation);
        
        if (isDeepNodding && angle <= deepNodUpAngle)
        {
            // Deep nod up
            isDeepNodding = false;
            choreographer.InputAction(BaseChoreographer.PlayerAction.MOTION_DEEP_NOD_UP);
        }
        else if (angle >= deepNodDownAngle && !isDeepNodding)
        {
            // Deep nod down
            isDeepNodding = true;
            choreographer.InputAction(BaseChoreographer.PlayerAction.MOTION_DEEP_NOD_DOWN);
        }
        else if (deltaTime > nodTimeDelay && magnitude > nodThreshold && !isDeepNodding)
        {
            // Nod gesture
            lastNodTime = Time.time;
            choreographer.InputAction(BaseChoreographer.PlayerAction.MOTION_NOD);
        }

        // TODO(jaween): Detect head tilt gesture

        // Debug input
        if (debugStoredAction != BaseChoreographer.PlayerAction.NONE)
        {
            choreographer.InputAction(debugStoredAction);
            debugStoredAction = BaseChoreographer.PlayerAction.NONE;
        }
    }

    private void DebugInput()
    {
        // Debug input
        if (Input.GetMouseButtonDown(0))
        {
            debugStoredAction = BaseChoreographer.PlayerAction.MOTION_NOD;
        }
        if (Input.GetMouseButtonDown(1))
        {
            debugStoredAction = BaseChoreographer.PlayerAction.MOTION_DEEP_NOD_DOWN;
        }
        if (Input.GetMouseButtonUp(1))
        {
            debugStoredAction = BaseChoreographer.PlayerAction.MOTION_DEEP_NOD_UP;
        }
    }

    private float SignedAngle(Quaternion a, Quaternion b)
    {
        var forwardA = a * Vector3.forward;
        var forwardB = b * Vector3.forward;

        var angleA = Mathf.Atan2(forwardA.y, forwardA.z) * Mathf.Rad2Deg;
        var angleB = Mathf.Atan2(forwardB.y, forwardB.z) * Mathf.Rad2Deg;

        return Mathf.DeltaAngle(angleA, angleB);
    }
}
