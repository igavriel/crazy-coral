using UnityEngine;
using DG;
using Unity.VisualScripting;
using DG.Tweening;
using System.Collections.Generic;

public class MainPageUIController : MonoBehaviour
{
    [SerializeField] RectTransform shop, corals;
    Stack<RectTransform> rectTransforms = new Stack<RectTransform>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            rectTransforms.Pop().GetComponent<UIUtil>().returnPosition();
        }
    }

    public void openShop()
    {
        shop.DOAnchorPos(Vector2.zero, 0.5f);
        rectTransforms.Push(shop);
    }

    public void openCorals()
    {
        corals.DOAnchorPos(Vector2.zero, 0.5f);
        rectTransforms.Push(corals);
    }
}
