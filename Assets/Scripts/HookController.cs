using UnityEngine;

public class HookController : MonoBehaviour
{

    [SerializeField] Vector2 startingForce;
    [SerializeField] GameObject Hook;
    [SerializeField] float downDistance;
    [SerializeField] float speed;
    public bool fishCatched = false;
    bool isGoingDown = true;
    Vector2 startPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        Hook.GetComponent<Rigidbody2D>().AddForce(Vector2.right * Random.Range(startingForce.x, startingForce.y));    
    }

    // Update is called once per frame
    void Update()
    {
        if (fishCatched && startPosition.y < transform.position.y)
            Destroy(gameObject);
        if (Vector2.Distance(startPosition, transform.position) > downDistance || fishCatched)
            isGoingDown = false;
        else if (startPosition.y < transform.position.y)
            isGoingDown = true;
 
        if (isGoingDown)
            gameObject.GetComponent<Rigidbody2D>().linearVelocityY = -speed * Time.deltaTime;
        else
            gameObject.GetComponent<Rigidbody2D>().linearVelocityY = speed * Time.deltaTime;
    }
}
