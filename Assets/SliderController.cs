using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    Slider slider;
    public int maxUnits;
    int currUnits = 0;

    private void Start()
    {
        slider = GetComponent<Slider>();
        slider.maxValue = maxUnits;
        slider.value = currUnits;
    }

    public void increaseUnits()
    {
        currUnits++;
        slider.DOValue(currUnits, 1f);

        if (currUnits == maxUnits)
        {
            QuestManager.instance.nextStep();
        }
    }
}
