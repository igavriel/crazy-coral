using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TrashController : MonoBehaviour
{
    Rigidbody2D rb2d;
    [SerializeField] Vector2 recycleDest;
    [SerializeField] intSO coins;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //StartCoroutine(swing());
        limitGravityVelocity();
    }

    private void limitGravityVelocity()
    {
        Vector2 currentVelocity = rb2d.linearVelocity;
        currentVelocity.y = Mathf.Max(currentVelocity.y, -1.5f);
        rb2d.linearVelocity = currentVelocity;
    }

    private void OnDestroy()
    {
        FindFirstObjectByType<SliderController>().increaseUnits();
        coins.Value++;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bottom"))
        {
            GetComponent<AudioSource>().Play();
        }
    }
}
