using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireworksController : MonoBehaviour {

    public GameObject fireworkPrefab;
    public GameObject[] rollFireworkPrefabs;
    public GameObject rollFireworksNode;
    public float radius;

    private bool createdFireworks = false;
    private List<GameObject> fireworks = new List<GameObject>();
    private const int fireworksPerLayer = 16;
    private bool rollFireworks = false;

    public void CreateFireworks(bool roll)
    {
        createdFireworks = true;

        int layerCount = 1;
        if (roll)
        {
            layerCount = rollFireworkPrefabs.Length;
        }

        for (var layer = 0; layer < layerCount; layer++)
        {
            Vector3 position = transform.position;
            GameObject prefab = fireworkPrefab;
            if (roll)
            {
                position = rollFireworksNode.transform.position;
                prefab = rollFireworkPrefabs[layer];
            }
            for (var i = 0; i < fireworksPerLayer; i++)
            {
                const float angleDeltaDegrees = 360.0f / fireworksPerLayer;
                Quaternion relativeRotation = Quaternion.AngleAxis(90 + angleDeltaDegrees * i, Vector3.forward);
                GameObject firework = (GameObject)Instantiate(
                    prefab, position, transform.rotation * relativeRotation);
                firework.transform.parent = transform;
                fireworks.Add(firework);
            }
        }
        rollFireworks = roll;
    }

	void FixedUpdate() 
    {
        if (createdFireworks)
        {
            float multiplier = 9.0f;
            const float angleDelta = 360.0f / fireworksPerLayer * Mathf.Deg2Rad;
            float newRadius = radius;

            int layerCount = 1;
            Vector3 nodePosition = transform.position;
            if (rollFireworks)
            {
                nodePosition = rollFireworksNode.transform.position;
                Vector3 newNodePosition = nodePosition;
                newNodePosition.y = Mathf.Lerp(nodePosition.y, nodePosition.y - 0.5f, Time.deltaTime * 5);
                rollFireworksNode.transform.position = newNodePosition;
                layerCount = rollFireworkPrefabs.Length;
                multiplier = 3.0f;
            }

            for (int layer = 0; layer < layerCount; layer++)
            {
                if (rollFireworks)
                {
                    newRadius = (layer + 1) * radius / 2.0f;
                }
                for (var i = 0; i < fireworksPerLayer; i++)
                {
                    GameObject firework = fireworks[fireworksPerLayer * layer + i];

                    // Fly out
                    Vector3 direction = transform.right * Mathf.Cos(angleDelta * i) +
                        transform.up * Mathf.Sin(angleDelta * i);
                    direction = Vector3.Normalize(direction);
                    Vector3 fromPosition = firework.transform.position;
                    Vector3 toPosition = nodePosition + direction * newRadius;
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
}
