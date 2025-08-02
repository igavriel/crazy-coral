using UnityEngine;

public class BigTrashController : MonoBehaviour
{
    [SerializeField] GameObject TrashBubble;
    Rigidbody2D rb2d;
    [SerializeField] bool isInBubble = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<BubbleMovement>(out BubbleMovement bubble))
        {
            Destroy(collision);
            TrashBubble.SetActive(true);
            rb2d.gravityScale = -0.2f;
            isInBubble = true;
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("clicked on " + gameObject.name);
        if (isInBubble && transform.position.y > 4)
        {
            Destroy(gameObject);
        }
    }
}
