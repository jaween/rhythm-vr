using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour {

    public RawImage fadingImage;

    private bool done = false;

    private void Awake()
    {
        SetRawImageAlpha(fadingImage, 0.0f);
    }

    public void Fade(bool fadeToBlack, float duration)
    {
        StartCoroutine(FadeCoroutine(fadeToBlack, duration));
    }

    private IEnumerator FadeCoroutine(bool fadeFromBlack, float duration)
    {
        done = false;
        float startTime = Time.time;
        float interpolant = 0;
        if (fadeFromBlack)
        {
            SetRawImageAlpha(fadingImage, 1.0f);
        }
        while (interpolant <= 1.0)
        {
            float fromAlpha = fadingImage.color.a;
            float toAlpha = fadeFromBlack ? 0.0f : 1.0f;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, interpolant);
            SetRawImageAlpha(fadingImage, alpha);

            interpolant = (Time.time - startTime) / duration;
            yield return new WaitForEndOfFrame();
        }
        done = true;
    }

    public bool Done
    {
        get { return done; }
    }

    private void SetRawImageAlpha(RawImage image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
