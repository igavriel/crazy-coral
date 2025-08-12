using UnityEngine;

public class LinkController : MonoBehaviour
{
    [SerializeField] GameObject hook;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider == null)
                return;
            if (hit.collider.TryGetComponent<LinkController>(out LinkController link))
            {
                RemoveRope();
            }
        }
    }
    private void RemoveRope()
    {
        if (hook.transform.parent == transform.parent)
        {
            hook.GetComponent<BoxCollider2D>().isTrigger = false;
            hook.GetComponent<FishCatcher>().enabled = false;
            hook.GetComponent<TrashController>().enabled = true;
            hook.transform.parent = null;
        }
        

        Destroy(transform.parent.gameObject);
    }
}
