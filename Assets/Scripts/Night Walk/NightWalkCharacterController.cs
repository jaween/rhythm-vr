using UnityEngine;
using System.Collections;

public class NightWalkCharacterController : MonoBehaviour
{
    public GameObject jumpMask;
    public GameObject[] balloons;
    public GameObject[] pops;
    public GameObject groundNode;

    private Animator animator;
    private bool isRunning = true;
    private bool isJumping = false;
    private bool isHighJumping = false;
    private bool isStuntedJumping = false;
    private bool isRolling = false;
    private bool isGrabbing = false;
    private bool isFallingFromBalloons = false;
    private bool jumpWithLeftArm = false;
    private float jumpStartTime;
    private float angle = 90 * Mathf.Deg2Rad;
    private float rollAngle = 0;
    private float ratioOfJump = 0;
    private float groundY;
    private float radius;
    private int popIndex = 0;
    private float startY;
    private bool startedRunning = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        radius = transform.position.z;
        startY = transform.position.y;
    }

    private void Update()
    {
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        const float degreesPerSecond = 11.5f;
        Vector3 newPosition = transform.position;
        if (isRunning)
        {
            angle -= degreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime;
            newPosition.x = Mathf.Cos(angle) * radius;
            newPosition.z = Mathf.Sin(angle) * radius;
        }

        if (isJumping || isFallingFromBalloons)
        {
            float airTime = Time.time - jumpStartTime;
            const float regularJumpAirTime = 0.542f;
            const float highJumpAirTime = 0.800f;
            const float stuntedJumpAirTime = 0.3f;
            const float fallingFromBalloonsAirTime = 0.6f;
            const float degreesPerJump = 180.0f;

            float jumpMultiplier = 1;
            if (isHighJumping)
            {
                ratioOfJump = airTime / highJumpAirTime;
                jumpMultiplier = 2.4f;
            }
            else if (isStuntedJumping)
            {
                ratioOfJump = airTime / stuntedJumpAirTime;
                jumpMultiplier = 0.3f;
            }
            else if (isFallingFromBalloons)
            {
                ratioOfJump = airTime / fallingFromBalloonsAirTime;
            }
            else
            {
                ratioOfJump = airTime / regularJumpAirTime;
            }

            float jumpCurve;
            float heightAboveGround;
            if (!isFallingFromBalloons)
            {
                jumpCurve = Mathf.Sin(ratioOfJump * degreesPerJump * Mathf.Deg2Rad);
                heightAboveGround = jumpMultiplier * jumpCurve;
                newPosition.y = groundY + heightAboveGround;
            } 
            else
            {
                jumpCurve = -Mathf.Pow(2.5f  * ratioOfJump - 0.5f, 2) + 0.25f;
                heightAboveGround = transform.position.y - groundY;
                newPosition.y = startY + jumpCurve;
            }

            if (heightAboveGround < 0)
            {
                newPosition.y = groundY;
                ratioOfJump = 0;
                isJumping = false;
                isHighJumping = false;
                isStuntedJumping = false;
                if (isFallingFromBalloons)
                {
                    isFallingFromBalloons = false;
                    startedRunning = true;
                }
            }
        }
        else if (isRolling)
        {
            // TODO(jaween): Fix the cause so this isn't needed
            const float rollingHeightAboveGround = 0.3f;
            newPosition.y = groundY - rollingHeightAboveGround;
        }

        transform.position = newPosition;
        transform.rotation = Quaternion.LookRotation(newPosition, Vector3.up);

        if (isRolling)
        {
            rollAngle += 360 * Time.fixedDeltaTime * 2;
            transform.rotation *= Quaternion.Euler(0.0f, 0.0f, -rollAngle);
        }
    }

    public bool IsReadyForNextAction()
    {
        bool ready = false;
        if (!isJumping || ratioOfJump >= nextJumpThreshold)
        {
            ready = true;
        }
        return ready;
    }

    public void Pop()
    {
        StartCoroutine(PopCoroutine(popIndex));
        popIndex++;

        if (popIndex == balloons.Length)
        {
            isFallingFromBalloons = true;
            jumpStartTime = Time.time;
        }
    }

    public bool IsRolling()
    {
        return isRolling;
    }

    // While the jump ratio is greater than these thresholds, the character can
    // jump again or roll, this makes the controls feel nicer
    // TODO(jaween): Implement this properly not in this temp way
    private const float nextJumpThreshold = 0.8f;
    private const float nextRollThreshold = 0.95f;

    private void JumpInternal()
    {
        jumpWithLeftArm = !jumpWithLeftArm;
        jumpStartTime = Time.time;
        isJumping = true;
        isRolling = false;
        animator.SetTrigger("StartJumpTrigger");
    }

    public void Jump()
    {
        if (IsReadyForNextAction())
        {
            JumpInternal();
            isHighJumping = false;
            isFallingFromBalloons = false;
        }
    }

    public void HighJump()
    {
        if (IsReadyForNextAction())
        {
            JumpInternal();
            isHighJumping = true;
            jumpMask.SetActive(true);
            rollAngle = 0;
            StartCoroutine(HideJumpMask());
        }
    }

    public void StuntedJump()
    {
        if (IsReadyForNextAction())
        {
            JumpInternal();
            isStuntedJumping = true;
        }
    }

    public void Roll()
    {
        if (IsReadyForNextAction())
        {
            isJumping = false;
            isRolling = true;
            rollAngle = 0;
            animator.SetTrigger("StartRollTrigger");
        }
    }

    public void Fall()
    {
        isRolling = false;
        isRunning = false;

        StartCoroutine(FallCoroutine());
    }

    private IEnumerator FallCoroutine()
    {
        Vector3 newPosition = transform.position;
        newPosition.y = groundY - 0.6f;
        transform.position = newPosition;
        isRunning = false;
        isGrabbing = true;
        yield return new WaitForSeconds(1.0f);

        while (true)
        {
            newPosition.y -= Time.fixedDeltaTime * 5;
            transform.position = newPosition;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator HideJumpMask()
    {
        yield return new WaitForSeconds(Time.deltaTime);
        jumpMask.SetActive(false);
    }

    private IEnumerator PopCoroutine(int index)
    {
        balloons[index].SetActive(false);
        pops[index].SetActive(true);
        yield return new WaitForEndOfFrame();

        pops[index].SetActive(false);
    }

    public bool StartedRunning
    {
        get { return startedRunning; }
    }

    private void UpdateAnimations()
    {
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsRolling", isRolling);
        animator.SetBool("LeftArm", jumpWithLeftArm);
        animator.SetBool("IsGrabbing", isGrabbing);
        animator.SetBool("IsStuntedJumping", isStuntedJumping);
        animator.SetBool("IsFallingFromBalloons", isFallingFromBalloons);
    }
}
