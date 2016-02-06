using UnityEngine;
using System.Collections;

public class NightWalkCharacterController : MonoBehaviour
{
    public float jumpHeight = 1;

    private Animator animator;
    private bool isJumping = false;
    private bool isRolling = false;
    private float jumpStartTime;
    private float angle = 90 * Mathf.Deg2Rad;
    private float rollAngle = 0;
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
        const float degreesPerSecond = 25f;
        Vector3 newPosition = transform.position;
        angle -= degreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime;
        newPosition.x = Mathf.Cos(angle) * radius;
        newPosition.z = Mathf.Sin(angle) * radius;

        if (isJumping)
        {
            float duration = Time.time - jumpStartTime;
            float timeInAirMultiplier = 327;
            float up = jumpHeight *
                Mathf.Sin(duration * timeInAirMultiplier * Mathf.Deg2Rad);
            newPosition.y = groundY + up;

            if (up < 0)
            {
                newPosition.y = groundY;
                isJumping = false;
                animator.SetBool("IsJumping", isJumping);
            }
        }

        transform.position = newPosition;
        transform.rotation = Quaternion.LookRotation(newPosition, Vector3.up);
        transform.rotation *= Quaternion.Euler(0.0f, 0.0f, -rollAngle);
    }

    public void Jump()
    {
        if (!isJumping)
        {
            jumpStartTime = Time.time;
            isJumping = true;
            animator.SetBool("IsJumping", isJumping);
        }
    }

    public void Roll()
    {
        rollAngle += 360 * Time.fixedDeltaTime * 2;
        isRolling = true;
        animator.SetBool("IsRolling", isRolling);
    }

    public void EndRoll()
    {
        rollAngle = 0;
        isRolling = false;
        Jump();
    }
}
