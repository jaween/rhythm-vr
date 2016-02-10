using UnityEngine;
using System.Collections;

public class PoleBoxController : MonoBehaviour {

    public GameObject poleBox;
    public GameObject innerBox;
    public GameObject positivePopup;
    public GameObject negativePopup;
    public ParticleSystem fireworks;
    public GameObject longPlatform;
    public GameObject mediumPlatform;
    public GameObject shortPlatform;
    public GameObject spinA;
    public GameObject spinB;
    public FireworksController fireworksController;
    
    private GameObject popup;
    private GameObject platform = null;
    private bool popped = false;
    private float popTime;

    public enum PlatformType
    {
        PLATFORM_NONE,
        PLATFORM_LONG,
        PLATFORM_MEDIUM,
        PLATFORM_SHORT
    }

    public enum PopType
    {
        POP_POSITIVE,
        POP_NEGATIVE,
        POP_EMPTY
    }

    public void Pop(PopType popType)
    {
        if (popType == PopType.POP_POSITIVE)
        {
            popup = positivePopup;
            bool rollFireworks = false;
            fireworksController.CreateFireworks(rollFireworks);
        }
        else if (popType == PopType.POP_NEGATIVE)
        {
            popup = negativePopup;
        }

        // Animations
        popped = true;
        popTime = Time.time;
        if (popType != PopType.POP_EMPTY)
        {
            popup.SetActive(true);
        }
        Animator animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsOpen", popped);

        innerBox.SetActive(false);
    }

    public void FixedUpdate()
    {
        if (popped)
        {
            // Popup movement
            float popupMultiplier = 13.0f;
            float popupInterpolant = Time.fixedDeltaTime * popupMultiplier;
            Vector3 fromPosition = popup.transform.position;
            Vector3 toPosition = fireworksController.transform.position;
            popup.transform.position = Vector3.Lerp(
                fromPosition, toPosition, popupInterpolant);

            // Popup fading
            float fadingMultiplier = 3.0f;
            float timeDelta = Time.time - popTime;
            float fadingInterpolant = Mathf.Sin(timeDelta * fadingMultiplier);
            if (fadingInterpolant < 0)
            {
                enabled = false;
            }

            Renderer renderer = popup.GetComponentInChildren<SpriteRenderer>();
            Color color = renderer.material.color;
            color = new Color(color.r, color.g, color.b, fadingInterpolant);
            renderer.material.color = color;
        }
    }

    public void SetPlatform(PlatformType type)
    {
        switch (type)
        {
            case PlatformType.PLATFORM_NONE:
                platform = null;
                break;
            case PlatformType.PLATFORM_LONG:
                platform = longPlatform;
                break;
            case PlatformType.PLATFORM_MEDIUM:
                platform = mediumPlatform;
                break;
            case PlatformType.PLATFORM_SHORT:
                platform = shortPlatform;
                break;
        }

        if (platform != null)
        {
            platform.SetActive(true);
        }
    }

    public void Lower(float amount)
    {
        StartCoroutine(LowerCoroutine(amount));
    }

    public void RollSpinEffects(bool first)
    {
        StartCoroutine(SpinCoroutine(first));
    }

    public void RollFireworks()
    {
        bool rollFireworks = true;
        fireworksController.CreateFireworks(rollFireworks);
    }

    public void DestroyPoleBox()
    {
        Destroy(gameObject);
    }

    private IEnumerator LowerCoroutine(float amount)
    {
        Vector3 fromPosition = transform.position;
        Vector3 toPosition = transform.position;
        toPosition.y -= amount;
        transform.position = toPosition;
        float startTime = Time.time;
        while (true)
        {
            float interpolant = (Time.time - startTime) * 5;
            transform.position = Vector3.Lerp(fromPosition, toPosition, interpolant);
            if (interpolant >= 1.0f)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator SpinCoroutine(bool first)
    {
        GameObject gameObject = spinA;
        if (!first)
        {
            gameObject = spinB;
            yield return new WaitForSeconds(0.4f);
        }
        gameObject.SetActive(true);

        float startTime = Time.time;
        float interpolant = 0;
        Quaternion fromRotation = gameObject.transform.rotation;
        while (true)
        {
            float angle = first ? 360 : -360;
            Quaternion spin = Quaternion.AngleAxis(
                angle * interpolant, Vector3.forward);
            gameObject.transform.rotation = fromRotation * spin;

            Vector3 scale = gameObject.transform.localScale;
            scale = Vector3.one * (first ? (1 - interpolant) : interpolant);
            gameObject.transform.localScale = scale;

            // Fade
            SpriteRenderer renderer = gameObject.GetComponentInChildren<SpriteRenderer>();
            Color color = renderer.material.color;
            float fromAlpha = first ? 0.3f : 1.0f;
            float toAlpha = first ? 0.9f : 0.0f;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, interpolant);
            renderer.material.color = color;

            if (interpolant >= 1.0f)
            {
                break;
            }
            interpolant = (Time.time - startTime) * 3.0f;
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }
}
