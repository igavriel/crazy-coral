using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using DG;
using DG.Tweening;
using System.Transactions;
using UnityEngine.Rendering.Universal;

public class BubbleMovement : MonoBehaviour
{
 [Tooltip("Vertical speed at which the bubble floats upward.")]
    public float speed = 1.5f;

    [Tooltip("Amplitude of the bubble's horizontal floating motion.")]
    public float floatAmplitude = 0.5f;

    [Tooltip("Frequency of the bubble's horizontal floating motion.")]
    public float floatFrequency = 1.5f;

    [Tooltip("Lifetime of the bubble in seconds before it is destroyed.")]
    public float lifeTime = 10f;

    [Tooltip("Speed at which the bubble moves when dragged.")]
    public float dragSpeed = 8f;

    [Tooltip("Maximum level the bubble can reach by merging.")]
    public int maxLevel = 3;

    [Tooltip("Factor by which the bubble's scale increases when merged.")]
    public float increaseScaleFactor = 1.3f; // Factor by which the bubble's scale increases

    private Vector3 startPos;
    private float timer;
    private bool isDragging = false;
    private int bubbleLevel = 1;
    public bool hasTrash = false;
    [SerializeField] SpriteRenderer secondSprite;

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
            return;
            
            //Vector3 mousePos = Input.mousePosition;
            //mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            //Vector3 targetWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            //transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, dragSpeed * Time.deltaTime);
        }
        else
        {
            float horizontalOffset = Mathf.Sin(timer * floatFrequency) * floatAmplitude;
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(horizontalOffset, speed);
            //transform.position = new Vector3(startPos.x + horizontalOffset, transform.position.y + speed * Time.deltaTime, 0);
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        StartCoroutine(ReleaseBubble());
    }

    void OnMouseExit()
    {
        //StartCoroutine(ReleaseBubble());
    }

    private IEnumerator ReleaseBubble()
    {
        yield return new WaitForSeconds(0.5f); // Small delay before allowing movement again
        isDragging = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTrash)
            return;

        BubbleMovement otherBubble = other.GetComponent<BubbleMovement>();
        if (otherBubble != null && otherBubble.bubbleLevel == bubbleLevel && bubbleLevel < maxLevel)
        {
            
            bubbleLevel++;
            Vector3 direction = other.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            GetComponent<Animator>().SetTrigger("Merge");

            Destroy(otherBubble.gameObject);
            GetComponent<AudioSource>().Play();            
        }

        if (other.TryGetComponent<TrashBubbleInteraction>(out TrashBubbleInteraction Interaction))
        {
            if (bubbleLevel < Interaction.trashLevel)
                return;

            other.transform.SetParent(transform, true);
            Destroy(other.GetComponent<Collider2D>());
            if (other.GetComponent<Rigidbody2D>())
                other.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            if (other.GetComponent<FishSwim>())
                other.GetComponent<FishSwim>().enabled = false;

            other.transform.DOLocalMove(Vector3.zero, 0.5f);
            hasTrash = true;
            Destroy(GetComponent<Collider2D>());
        }
    }

    public void increaseSize()
    {
        // Increase scale x and y twice
        Vector3 newScale = transform.localScale;
        newScale.x *= increaseScaleFactor;
        newScale.y *= increaseScaleFactor;
        transform.localScale = newScale;
    }

    public void enableSecondSprite()
    {
        secondSprite.enabled = true;
    }

    public void disableSecondSprite()
    {
        secondSprite.enabled = false;
    }
}
