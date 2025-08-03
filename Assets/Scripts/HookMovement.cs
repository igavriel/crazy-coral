using UnityEngine;

public class HookMovement : MonoBehaviour
{
    [Header("Hook Movement Settings")]
    [Tooltip("Controls the vertical movement speed of the hook.")]
    public float verticalSpeed = 1.5f;

    [Tooltip("Maximum vertical range the hook can move from its starting position.")]
    public float verticalRange = 5f;

    [Tooltip("Amplitude of the hook's side-to-side sway.")]
    public float swayAmplitude = 0.3f;

    [Tooltip("Frequency of the hook's side-to-side sway.")]
    public float swayFrequency = 1.5f;

    private Vector3 startPosition;
    private float direction = -1f; // Start moving down
    private float timer;

    void Start()
    {
        // Start at the upper side of the screen
        float upperY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, Mathf.Abs(Camera.main.transform.position.z))).y;
        startPosition = new Vector3(transform.position.x, upperY, transform.position.z);
        transform.position = startPosition;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Sway movement (side-to-side)
        float swayOffset = Mathf.Sin(timer * swayFrequency) * swayAmplitude;

        // Vertical movement (down then up)
        float newY = transform.position.y + verticalSpeed * direction * Time.deltaTime;

        // Check limits (upper = startPosition.y, lower = startPosition.y - verticalRange)
        if (newY < startPosition.y - verticalRange)
        {
            direction = 1f; // go up
            newY = startPosition.y - verticalRange;
        }
        else if (newY > startPosition.y)
        {
            direction = -1f; // go down
            newY = startPosition.y;
        }

        // Apply position
        transform.position = new Vector3(startPosition.x + swayOffset, newY, startPosition.z);
    }
}
