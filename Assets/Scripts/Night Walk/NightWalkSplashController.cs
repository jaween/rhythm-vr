using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NightWalkSplashController : MonoBehaviour {

    public Text infoText;
    public RawImage fadingImage;
    public ScreenFader fader;

    private bool clicked = false;
    private bool faded = false;

	private void Start () {
        SetTextAlpha(infoText, 0.0f);
        StartCoroutine(SplashCoroutine());
	}

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clicked = true;
        }
    }
	
	private IEnumerator SplashCoroutine()
    {
        yield return new WaitForSeconds(0.8f);

        StartCoroutine(TextFadeCoroutine());

        while (!clicked)
        {
            yield return new WaitForEndOfFrame();
        }

        Fade();
        while (!fader.Done)
        {
            yield return new WaitForEndOfFrame();
        }
        SceneManager.LoadScene(1);
    }

    private IEnumerator TextFadeCoroutine()
    {
        const float duration = 0.8f;
        float startTime = Time.time;
        float interpolant = 0;
        while (interpolant <= 1.0)
        {
            float alpha = Mathf.Lerp(0.0f, 1.0f, interpolant);
            SetTextAlpha(infoText, alpha);

            interpolant = (Time.time - startTime) / duration;
            yield return new WaitForEndOfFrame();
        }
    }

    private void SetTextAlpha(Text text, float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    private void Fade()
    {
        bool fadeFromBlack = false;
        float duration = 1.0f;
        fader.Fade(fadeFromBlack, duration); 
    }
}
