using System.Threading;
using UnityEngine;

public class BigTrashController : MonoBehaviour
{
    [SerializeField] GameObject TrashBubble;
    Rigidbody2D rb2d;
    [SerializeField] bool isInBubble = false;

    [SerializeField] float floatFrequency, floatAmplitude, speed;
    float timer;
    Vector2 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        //    if (hit.collider != null)
        //    {
        //        if (hit.collider.TryGetComponent<BigTrashController>(out BigTrashController bubble))
        //        {
        //            Debug.Log("clicked on " + gameObject.name);
        //            if (bubble.isInBubble && bubble.transform.position.y > 4)
        //            {
        //                Destroy(bubble.gameObject);
        //            }
        //        }
        //    }
        //}
        //if (isInBubble)
        //{
        //    timer += Time.deltaTime;
        //    float horizontalOffset = Mathf.Sin(timer * floatFrequency) * floatAmplitude;
        //    transform.position = new Vector2(startPos.x + horizontalOffset, Mathf.Clamp(transform.position.y + speed * Time.deltaTime, -10, 5));
        //}
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.TryGetComponent<BubbleMovement>(out BubbleMovement bubble) && !isInBubble)
    //    {
    //        Destroy(collision.gameObject);
    //        TrashBubble.SetActive(true);
    //        rb2d.bodyType = RigidbodyType2D.Kinematic;
    //        isInBubble = true;
    //        startPos = transform.position;
    //    }
    //}
}
