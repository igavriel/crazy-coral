using UnityEngine;
using DG;
using DG.Tweening;

public class UIUtil : MonoBehaviour
{
    [SerializeField] Vector2 startPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = gameObject.GetComponent<RectTransform>().anchoredPosition;
    }

    public void returnPosition()
    {
        gameObject.GetComponent<RectTransform>().DOAnchorPos(startPos, 0.5f);
    }
}
