using UnityEngine;

public class mapCameraController : MonoBehaviour
{
    public float panSpeed = 0.5f;
    private Vector3 dragOrigin;

    void Update()
    {
        // Check for left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        // Check for left mouse button held down
        if (!Input.GetMouseButton(0)) return;

        // Calculate the mouse movement delta
        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);

        // Invert the movement and apply to camera position
        Vector3 move = new Vector3(-pos.x * panSpeed, -pos.y * panSpeed, 0);
        transform.Translate(move, Space.Self);

        // Update drag origin for continuous panning
        dragOrigin = Input.mousePosition;
    }
}
