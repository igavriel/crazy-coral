using DG.Tweening;
using UnityEngine;

public class CoralController : MonoBehaviour
{
    [SerializeField] float FadeTime = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Renderer>().material.DOFloat(1, "_Fade", FadeTime).SetEase(Ease.InOutSine)
            .OnComplete(() => QuestManager.instance.nextStep());
    }
}
