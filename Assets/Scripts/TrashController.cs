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
        coins.Value++;
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bottom"))
        {
            GetComponent<AudioSource>().Play();
        }
    }

    IEnumerator swing()
    {
        rb2d.angularVelocity = 30;
        yield return new WaitForSeconds(1.5f);
        rb2d.angularVelocity = -15;
        yield return new WaitForSeconds(1.5f);

        StartCoroutine(swing());
    }
}
