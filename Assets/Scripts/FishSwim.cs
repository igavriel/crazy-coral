using UnityEngine;

public class FishSwim : MonoBehaviour
{
    [Tooltip("Speed at which the fish swims.")]
    public float swimSpeed = 1f;

    [Tooltip("Time interval (in seconds) before the fish randomly changes direction.")]
    public float directionChangeInterval = 2f;

    [Tooltip("Horizontal and vertical bounds for fish movement (centered at origin).")]
    public Vector2 swimBounds = new Vector2(2.5f, 5f); // Horizontal & vertical limits (based on camera)

    private Vector2 swimDirection;
    private float timer;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Look for the first child with a SpriteRenderer and a sprite
        foreach (Transform child in transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                spriteRenderer = sr;
                break;
            }
        }
        Util.AssertObject(spriteRenderer, "FishSwim: No SpriteRenderer with a sprite found in children.");

        ChooseNewDirection();
    }

    void Update()
    {
        // Move the fish
        transform.Translate(swimDirection * swimSpeed * Time.deltaTime);

        // Flip sprite depending on direction
        if (spriteRenderer != null)
            spriteRenderer.flipX = swimDirection.x > 0;

        // Keep within bounds and change direction if touching bound
        Vector3 pos = transform.position;
        bool touchedBound = false;

        if (pos.x <= -swimBounds.x || pos.x >= swimBounds.x)
        {
            touchedBound = true;
            pos.x = Mathf.Clamp(pos.x, -swimBounds.x, swimBounds.x);
        }
        if (pos.y <= -swimBounds.y || pos.y >= swimBounds.y)
        {
            touchedBound = true;
            pos.y = Mathf.Clamp(pos.y, -swimBounds.y, swimBounds.y);
        }
        transform.position = pos;

        if (touchedBound)
        {
            ChooseNewDirection();
            timer = 0f;
        }
        else
        {
            // Change direction occasionally
            timer += Time.deltaTime;
            if (timer >= directionChangeInterval)
            {
                ChooseNewDirection();
                timer = 0f;
            }
        }
    }

    void ChooseNewDirection()
    {
        swimDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.3f, 0.3f)).normalized;
    }
}
