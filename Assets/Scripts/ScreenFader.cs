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

    public void Fade(bool fadeFromBlack, float duration)
    {
        StartCoroutine(FadeCoroutine(fadeFromBlack, duration));
    }

    private IEnumerator FadeCoroutine(bool fadeFromBlack, float duration)
    {
        
        if (fadeFromBlack)
        {
            SetRawImageAlpha(fadingImage, 1.0f);
        }

        // TODO(jaween): Why doesn't the fading adhere to the duration value? 
        done = false;
        float startTime = Time.time;
        float interpolant = 0;
        while (interpolant <= 1.0f)
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
