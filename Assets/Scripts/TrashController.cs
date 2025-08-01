using Unity.VisualScripting;
using UnityEngine;

public class TrashController : MonoBehaviour
{

    Rigidbody2D rb2d;
    [SerializeField] Vector2 recycleDest;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        limitGravityVelocity();

        if (Vector2.Distance(transform.position, recycleDest) < 0.5f)
            destroyTrash();
    }

    private void limitGravityVelocity()
    {
        Vector2 currentVelocity = rb2d.linearVelocity;
        currentVelocity.y = Mathf.Max(currentVelocity.y, -1.5f);
        rb2d.linearVelocity = currentVelocity;
    }

    private void destroyTrash()
    {
        //add score
        Destroy(gameObject);
    }

    private void OnMouseDown()
    {
        rb2d.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnMouseDrag()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = position;
    }

    private void OnMouseUp()
    {
        rb2d.bodyType = RigidbodyType2D.Dynamic;
    }
}
