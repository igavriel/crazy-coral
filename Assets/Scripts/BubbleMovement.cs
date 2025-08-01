using System;
using UnityEngine;

public class BubbleMovement : MonoBehaviour
{
    public float speed = 1.5f;
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 1.5f;
    public float lifeTime = 10f;
    public float dragSpeed = 8f;
    public int maxLevel = 3;

    public float increaseScaleFactor = 1.3f; // Factor by which the bubble's scale increases

    private Vector3 startPos;
    private float timer;
    private bool isDragging = false;
    private int bubbleLevel = 1;

    void Start()
    {
        startPos = transform.position;
        timer = 0f;
        Destroy(gameObject, lifeTime); // Destroy after a few seconds
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 targetWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, dragSpeed * Time.deltaTime);
        }
        else
        {
            float horizontalOffset = Mathf.Sin(timer * floatFrequency) * floatAmplitude;
            transform.position = new Vector3(startPos.x + horizontalOffset, transform.position.y + speed * Time.deltaTime, 0);
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void OnMouseExit()
    {
        isDragging = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        BubbleMovement otherBubble = other.GetComponent<BubbleMovement>();
        if (otherBubble != null && otherBubble.bubbleLevel == bubbleLevel && bubbleLevel < maxLevel)
        {
            bubbleLevel++;
            Destroy(otherBubble.gameObject);

            // Increase scale x and y twice
            Vector3 newScale = transform.localScale;
            newScale.x *= increaseScaleFactor;
            newScale.y *= increaseScaleFactor;
            transform.localScale = newScale;
        }
    }
}
