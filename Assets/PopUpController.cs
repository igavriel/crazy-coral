using UnityEngine;
using TMPro;
using DG;
using DG.Tweening;

public class PopUpController : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    RectTransform rect;
    CanvasGroup group;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();   
    }

    public void close ()
    {
        rect.DOScale(0.1f, 0.5f);
        rect.DORotate(new Vector3(0, 0, -720), 0.5f, RotateMode.FastBeyond360).OnComplete(() => rect.rotation = Quaternion.Euler(Vector3.zero));
        group.DOFade(0, 0.5f);
    }

    public void open (string newText)
    {
        text.text = newText;
        rect.DOScale(0.75f, 0.5f);
        rect.DORotate(new Vector3(0, 0, 720), 0.5f, RotateMode.FastBeyond360).OnComplete(() => rect.rotation = Quaternion.Euler(Vector3.zero));
        group.DOFade(1, 0.5f);
    }
}
