using UnityEngine;

public class FishCatcher : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<FishSwim>(out FishSwim fish))
        {
            fish.enabled = false;
            fish.transform.parent = transform;
            transform.parent.GetComponent<HookController>().fishCatched = true;
        }
    }
}
