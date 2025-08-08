using System.Collections.Generic;
using UnityEngine;

public class TrashSpawn : MonoBehaviour
{
    [Header("Trash Prefabs")]
    public List<GameObject> trashTypePrefabs;

    public float spawnInterval = 1f;

    private float timer = 0f;
    private BoxCollider2D spawnArea;

    void Start()
    {
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
        int trashTypeIndex = UnityEngine.Random.Range(0, trashTypePrefabs.Count);

        // Get bounds of the spawn area
        Bounds bounds = spawnArea.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float y = bounds.center.y;

        Vector3 spawnPos = new Vector3(randomX, y, 0);
        Instantiate(
            trashTypePrefabs[trashTypeIndex],
            spawnPos,
            Quaternion.identity,
            spawnArea.transform
        );
    }
}
