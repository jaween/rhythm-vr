using UnityEngine;
using System.Collections;

public class NightWalkCharacterController : MonoBehaviour
{
    public float jumpHeight = 1;

    private Animator animator;
    private bool isJumping = false;
    private bool isSuperJumping = false;
    private bool isRolling = false;
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
            newPosition.y = groundY;
        }

        transform.position = newPosition;
        transform.rotation = Quaternion.LookRotation(newPosition, Vector3.up);

        if (isRolling)
        {
            rollAngle += 360 * Time.fixedDeltaTime * 2;
            transform.rotation *= Quaternion.Euler(0.0f, 0.0f, -rollAngle);
        }
    }

    public void Jump()
    {
        // While the jump ratio is below this threshold the player can make
        // the character jump again, this makes the controls feel nicer
        const float jumpWhileJumpingThreshold = 0.4f;

        if (!isJumping || ratioOfJump < jumpWhileJumpingThreshold)
        {
            jumpStartTime = Time.time;
            isJumping = true;
            isRolling = false;
            animator.SetBool("IsJumping", isJumping);
        }
    }

    public void Roll()
    {
        isJumping = false;
        isRolling = true;
        animator.SetBool("IsRolling", isRolling);
    }

    public void EndRoll()
    {
        rollAngle = 0;
        isRolling = false;
        isSuperJumping = true;
        Jump();
    }
}
