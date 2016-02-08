using UnityEngine;
using System.Collections;

public class NightWalkCharacterController : MonoBehaviour
{
    public float jumpHeight = 1;

    private Animator animator;
    private bool isJumping = false;
    private bool isSuperJumping = false;
    private bool isRolling = false;
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

    private void FixedUpdate()
    {
        const float degreesPerSecond = 18f;
        Vector3 newPosition = transform.position;
        angle -= degreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime;
        newPosition.x = Mathf.Cos(angle) * radius;
        newPosition.z = Mathf.Sin(angle) * radius;

        if (isJumping)
        {
            float duration = Time.time - jumpStartTime;
            const float timeInAirMultiplier = 327;
            ratioOfJump = Mathf.Sin(duration * timeInAirMultiplier * Mathf.Deg2Rad);
            float heightAboveGround = jumpHeight * ratioOfJump;
            if (isSuperJumping)
            {
                heightAboveGround *= 2;
            }
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

    // While the jump ratio is below this threshold the player can make
    // the character jump again, this makes the controls feel nicer
    // TODO(jaween): Implement this properly not in this temp way
    private const float jumpWhileJumpingThreshold = 0.3f;

    public bool Jump()
    {
        bool jumped = false;
        if (!isJumping || ratioOfJump < jumpWhileJumpingThreshold)
        {
            jumpWithLeftArm = !jumpWithLeftArm;
            jumpStartTime = Time.time;
            isJumping = true;
            isRolling = false;
            animator.SetTrigger("StartJumpTrigger");
            jumped = true;
        }

        UpdateAnimations();
        return jumped;
    }

    public bool Roll()
    {
        bool rolled = false;
        if (!isJumping || ratioOfJump < jumpWhileJumpingThreshold)
        {
            isJumping = false;
            isRolling = true;
            rolled = true;
        }
        UpdateAnimations();

        return rolled;
    }

    public bool EndRoll()
    {
        bool endRolled = false;
        if (isRolling)
        {
            rollAngle = 0;
            isRolling = false;
            isSuperJumping = true;
            Jump();
            endRolled = true;
        }
        UpdateAnimations();

        return endRolled;
    }

    private void UpdateAnimations()
    {
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsRolling", isRolling);
        animator.SetBool("LeftArm", jumpWithLeftArm);
    }
}
