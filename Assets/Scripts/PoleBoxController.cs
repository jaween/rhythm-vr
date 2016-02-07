using UnityEngine;
using System.Collections;

public class PoleBoxController : MonoBehaviour {

    public GameObject positivePopup;
    public GameObject negativePopup;
    public ParticleSystem fireworks;
    public GameObject longPlatform;
    public GameObject mediumPlatform;
    public GameObject shortPlatform;

    private GameObject popup;
    private bool popped = false;
    private float popTime;

    public enum PlatformType
    {
        PLATFORM_LONG,
        PLATFORM_MEDIUM,
        PLATFORM_SHORT
    }

    public void Pop(bool positive)
    {
        if (positive)
        {
            popup = positivePopup;
        }
        else
        {
            popup = negativePopup;
        }

        popTime = Time.time;
        popup.SetActive(true);
        fireworks.Play();
        popped = true;
    }

    public void FixedUpdate()
    {
        if (popped)
        {

            // Popup movement
            float popupMultiplier = 10.0f;
            float popupInterpolant = Time.fixedDeltaTime * popupMultiplier;
            Vector3 fromPosition = popup.transform.position;
            Vector3 toPosition = transform.position + Vector3.up * 2.5f;
            popup.transform.position = Vector3.Lerp(
                fromPosition, toPosition, popupInterpolant);

            // Popup fading
            float fadingMultiplier = 3.0f;
            float timeDelta = Time.time - popTime;
            float fadingInterpolant = Mathf.Sin(timeDelta * fadingMultiplier);
            if (fadingInterpolant < 0)
            {
                enabled = false;
                fireworks.Stop();
            }
            Renderer renderer = popup.GetComponentInChildren<SpriteRenderer>();
            Color color = renderer.material.color;
            float alpha = Mathf.Lerp(color.a, 1.0f, popupInterpolant);
            color = new Color(color.r, color.g, color.b, fadingInterpolant);
            renderer.material.color = color;

        }
    }

    public void DestroyPoleBox()
    {
        Destroy(gameObject);
    }

    public void ShowPlatform(PlatformType type)
    {
        switch (type)
        {
            case PlatformType.PLATFORM_LONG:
                longPlatform.SetActive(true);
                break;
            case PlatformType.PLATFORM_MEDIUM:
                mediumPlatform.SetActive(true);
                break;
            case PlatformType.PLATFORM_SHORT:
                shortPlatform.SetActive(true);
                break;
        }
    }
}
