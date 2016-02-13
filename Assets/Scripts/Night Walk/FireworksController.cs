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
        Vector3 fromPosition = fireworks.transform.transform.position;
        Vector3 toPosition = roll ? fromPosition - 2*Vector3.up : fromPosition;
        while (ellapsedRatio <= 1.0f)
        {
            // Scale upwards
            Vector3 localScale = fireworks.transform.localScale;
            localScale.x = scaleInterpolant;
            localScale.y = scaleInterpolant;
            fireworks.transform.localScale = localScale;

            // Move downwards
            fireworks.transform.position = Vector3.Lerp(
                fromPosition, toPosition, scaleInterpolant);

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

    // TODO(jaween): Clean up and merge duplicate code with the PoleBoxController
    /*private IEnumerator UpdateFireworksCoroutine(bool roll) 
    {
        float startTime = Time.time;
        float duration = roll ? 4.0f : 1.0f;
        Vector3 offset = Vector3.zero;
        float positionInterpolant = 0.0f;
        float alphaInterpolant = 0.0f;
        const float angleDelta = 360.0f / fireworksPerLayer * Mathf.Deg2Rad;

        // Gets the relevent fireworks to animate
        List<GameObject> fireworks = new List<GameObject>();
        Vector3 startingPosition;
        Transform node;
        if (roll)
        {
            rollFireworksNode.SetActive(true);
            node = rollFireworksNode.transform;
            startingPosition = rollFireworksNode.transform.position;
        } else
        {
            fireworksNode.SetActive(true);
            node = fireworksNode.transform;
            startingPosition = fireworksNode.transform.position;
        }

        for (var i = 0; i < node.childCount; i++)
        {
            Transform fireworkTransform = node.GetChild(i);
            GameObject firework = fireworkTransform.gameObject;
            float angleDeltaDegrees = 360 / fireworksPerLayer;
            firework.transform.rotation *= 
                Quaternion.AngleAxis(90 + angleDeltaDegrees * i, 
                Vector3.forward);
            fireworks.Add(firework.gameObject);
        }

        float ellapsedRatio = 0;
        while (ellapsedRatio <= 1)
        {
            if (roll) 
            {
                startingPosition = rollFireworksNode.transform.position +
                    Vector3.up * -5 * alphaInterpolant;
            }

            
            for (var i = 0; i < fireworks.Count; i++)
            {
                GameObject firework = fireworks[i];

                // Determines the starting position of the roll fireworks
                float divisor = (int)(i / fireworksPerLayer);
                float ratio = divisor != 0 ? fireworks.Count / divisor : 0;
                float newRadius = radius + radius * 0.05f * ratio;

                // Fly out
                float angle = angleDelta * i;
                Vector3 direction = transform.right * Mathf.Cos(angle) +
                    transform.up * Mathf.Sin(angle);
                Vector3 fromPosition = startingPosition;
                Vector3 toPosition = startingPosition + direction * newRadius;
                firework.transform.position = Vector3.Lerp(
                    fromPosition, toPosition, positionInterpolant);

                // Fade
                SpriteRenderer renderer = firework.GetComponent<SpriteRenderer>();
                Color color = renderer.material.color;
                color.a = Mathf.Lerp(color.a, 0.0f, alphaInterpolant);
                renderer.material.color = color;
            }

            float ellapsedTime = Time.time - startTime;
            ellapsedRatio = ellapsedTime/duration;
            positionInterpolant = Mathf.Sin(ellapsedRatio * Mathf.PI / 2);
            alphaInterpolant = ellapsedTime * ellapsedTime / duration;
            yield return new WaitForEndOfFrame();
        }
        DestroyFireworks();
    }*/

    public void DestroyFireworks()
    {
        Destroy(fireworksSprite);
        Destroy(rollFireworksSprite);
    }
}
