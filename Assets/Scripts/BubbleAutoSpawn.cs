using UnityEngine;
using UnityEngine.Diagnostics;
using DG;

public class BubbleAutoSpawn : MonoBehaviour
{
    public GameObject bubblePrefab;
    public float spawnInterval = 1f;

    private float timer = 0f;
    private BoxCollider2D spawnArea;

    void Start()
    {
        Util.AssertObject(bubblePrefab, "Bubble prefab is not assigned in BubbleAutoSpawn.");

        spawnArea = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnBubble();
            timer = 0f;
        }
    }

    void SpawnBubble()
    {
        if (bubblePrefab == null || spawnArea == null)
            return;

        // Get bounds of the spawn area
        Bounds bounds = spawnArea.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float y = bounds.center.y;

        Vector3 spawnPos = new Vector3(randomX, y, 0);
        Instantiate(bubblePrefab, spawnPos, Quaternion.identity, spawnArea.transform);
    }
}
