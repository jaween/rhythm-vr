using UnityEngine;
using System.Collections;

public class NightWalkCharacterController : MonoBehaviour
{
    public float jumpHeight;
    public GameObject jumpMask;

    private Animator animator;
    private bool isRunning = true;
    private bool isJumping = false;
    private bool isSuperJumping = false;
    private bool isRolling = false;
    private bool isGrabbing = false;
    private bool jumpWithLeftArm = false;
    private float jumpStartTime;
    private float angle = 90 * Mathf.Deg2Rad;
    private float rollAngle = 0;
    private float ratioOfJump = 0;
    private float groundY;
    private float radius;
    
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        groundY = transform.position.y;
        radius = transform.position.z;
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

        if (isJumping)
        {
            float airTime = Time.time - jumpStartTime;
            const float regularJumpAirTime = 0.5f;
            const float superJumpAirTime = 0.7f;
            const float degreesPerJump = 180.0f;

            float superJumpMultiplier = 1;
            if (isSuperJumping)
            {
                ratioOfJump = airTime / superJumpAirTime;
                superJumpMultiplier = 2.4f;
            }
            else
            {
                ratioOfJump = airTime / regularJumpAirTime;
            }
            
            float jumpCurve = Mathf.Sin(ratioOfJump * degreesPerJump * Mathf.Deg2Rad);
            float heightAboveGround = jumpHeight * superJumpMultiplier * jumpCurve;
            newPosition.y = groundY + heightAboveGround;

            if (heightAboveGround < 0)
            {
                newPosition.y = groundY;
                ratioOfJump = 0;
                isJumping = false;
                isSuperJumping = false;
                animator.SetBool("IsJumping", isJumping);
            }
        }
        else if (isRolling)
        {
            const float rollingHeightAboveGround = 0.2f;
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

    // While the jump ratio is greater than these thresholds, the character can
    // jump again or roll, this makes the controls feel nicer
    // TODO(jaween): Implement this properly not in this temp way
    private const float nextJumpThreshold = 0.8f;
    private const float nextRollThreshold = 0.95f;

    public bool Jump(bool superJump)
    {
        bool jumped = false;
        if (!isRolling && !isJumping || ratioOfJump >= nextJumpThreshold)
        {
            jumpWithLeftArm = !jumpWithLeftArm;
            jumpStartTime = Time.time;
            isJumping = true;
            isRolling = false;
            isSuperJumping = superJump;
            jumpMask.SetActive(superJump);
            animator.SetTrigger("StartJumpTrigger");
            if (superJump)
            {
                StartCoroutine(HideJumpMask());
            }
            jumped = true;
        }

        return jumped;
    }

    public bool Roll()
    {
        bool rolled = false;
        if (!isRolling && !isJumping || ratioOfJump >= nextRollThreshold)
        {
            isJumping = false;
            isRolling = true;
            animator.SetTrigger("StartRollTrigger");
            rolled = true;
        }

        return rolled;
    }

    public bool EndRoll()
    {
        bool endRolled = false;
        if (isRolling)
        {
            rollAngle = 0;
            isRolling = false;
            bool superJump = true;
            Jump(superJump);
            endRolled = true;
        }

        return endRolled;
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

    private void UpdateAnimations()
    {
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsRolling", isRolling);
        animator.SetBool("LeftArm", jumpWithLeftArm);
        animator.SetBool("IsGrabbing", isGrabbing);
    }
}
