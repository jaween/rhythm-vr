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

    private Quaternion previousRotation;
    private float lastNodTime;
    private bool updated;
    private BaseChoreographer.PlayerAction debugStoredAction = 
        BaseChoreographer.PlayerAction.NONE;

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
        previousRotation = Cardboard.SDK.HeadPose.Orientation;
        lastNodTime = Time.time;
    }

    private void Update()
    {
        updated = false;  // OK to recompute head pose.
        if (updateEarly)
        {
            UpdateHead();
        }

        DebugInput();
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

        previousRotation = transform.rotation;
    }

    private void DetectAndDispatchHeadMotionInput(Vector3 acceleration)
    {
        float deltaTime = Time.time - lastNodTime;
        const float nodTimeDelay = 0.4f;

        float magnitude = acceleration.magnitude;
        const float nodThreshold = 1.1f;

        // Nod gesture
        if (deltaTime > nodTimeDelay && magnitude > nodThreshold)
        {
            lastNodTime = Time.time;
            choreographer.InputAction(BaseChoreographer.PlayerAction.MOTION_NOD);
        }
        // TODO(jaween): Detect deep nod gestures
        // TODO(jaween): Detect head tilt gesture

        // Debug input
        choreographer.InputAction(debugStoredAction);
        debugStoredAction = BaseChoreographer.PlayerAction.NONE;
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
}
