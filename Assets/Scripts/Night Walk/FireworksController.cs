using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireworksController : MonoBehaviour {

    public GameObject fireworkPrefab;
    public float radius;

    private bool createFireworks = false;
    private float startTime;
    private List<GameObject> fireworks = new List<GameObject>();
    private const int fireworkCount = 16;

    public void CreateFireworks()
    {
        createFireworks = true;
        startTime = Time.time;

        for (var i = 0; i < fireworkCount; i++) 
        {
            GameObject firework = (GameObject) Instantiate(
                fireworkPrefab, transform.position, transform.rotation);
            firework.transform.parent = transform;
            fireworks.Add(firework);
        }
    }

	void FixedUpdate() 
    {
        if (createFireworks)
        {
            const float multiplier = 9.0f;
            const float angleDelta = 360.0f / fireworkCount * Mathf.Deg2Rad;
            for (var i = 0; i < fireworkCount; i++)
            {
                GameObject firework = fireworks[i];

                // Fly out
                Vector3 direction = transform.right * Mathf.Cos(angleDelta * i) +
                    transform.up * Mathf.Sin(angleDelta * i);
                direction = Vector3.Normalize(direction);
                Vector3 fromPosition = firework.transform.position;
                Vector3 toPosition = transform.position + direction * radius;
                firework.transform.position = Vector3.Lerp(
                    fromPosition, toPosition, Time.fixedDeltaTime * multiplier);

                // Fade
                SpriteRenderer renderer = firework.GetComponent<SpriteRenderer>();
                Color color = renderer.material.color;
                color.a = Mathf.Lerp(color.a, 0.0f, Time.fixedDeltaTime * multiplier);
                renderer.material.color = color;
            }
        }
	}
}
