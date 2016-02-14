using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireworksController : MonoBehaviour
{
    public GameObject fireworksSprite;
    public GameObject rollFireworksSprite;
    public float radius;
    
    private void Start()
    {
        SetFireworksAlpha(fireworksSprite, 0.0f);
        SetFireworksAlpha(rollFireworksSprite, 0.0f);
    }

    public void StartFireworks(bool roll)
    {
        StartCoroutine(UpdateFireworksCoroutine(roll));
    }

    private void SetFireworksAlpha(GameObject gameObject, float alpha)
    {
        SpriteRenderer renderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        Color color = renderer.material.color;
        color.a = alpha;
        renderer.material.color = color;
    }

    private IEnumerator UpdateFireworksCoroutine(bool roll)
    {
        float startTime = Time.time;
        float scaleInterpolant = 0.0f;
        float alphaInterpolant = 0.0f;
        float duration = roll ? 2.0f : 1.0f;
        float ellapsedRatio = 0.0f;
        GameObject fireworks = roll ? rollFireworksSprite : fireworksSprite;
        float offsetAmount = roll ? -0.05f : 0.0f;
        while (ellapsedRatio <= 1.0f)
        {
            // Scale upwards
            Vector3 localScale = fireworks.transform.localScale;
            localScale.x = scaleInterpolant;
            localScale.y = scaleInterpolant;
            fireworks.transform.localScale = localScale;

            // Move downwards
            Vector3 position = fireworks.transform.position;
            position.y += Mathf.Lerp(0.0f, offsetAmount, scaleInterpolant);
            fireworks.transform.position = position;

            // Fade out
            float alpha = Mathf.Lerp(1.0f, 0.0f, alphaInterpolant);
            SetFireworksAlpha(fireworks, alpha);

            float ellapsedTime = (Time.time - startTime);
            ellapsedRatio = ellapsedTime / duration;
            scaleInterpolant = Mathf.Sin(ellapsedRatio * Mathf.PI / 2);
            alphaInterpolant = ellapsedRatio;
            yield return new WaitForEndOfFrame();
        }
        DestroyFireworks();
    }
    
    public void DestroyFireworks()
    {
        Destroy(fireworksSprite);
        Destroy(rollFireworksSprite);
    }
}
