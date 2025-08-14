using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] string[] quest;
    [SerializeField] GameObject arrow;
    private int questIndex;
    private PopUpController popUp;

    public static QuestManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        popUp = FindFirstObjectByType<PopUpController>();
        Invoke("startQuest", 1f);   
    }

    void startQuest()
    {
        popUp.open(quest[questIndex]);
    }

    public void nextStep()
    {
        questIndex++;
        startQuest();
        if (questIndex == 2)
        {
            ArrowAlgea();
        }
    }

    public void removeArrow()
    {
        arrow.SetActive(false);
    }

    public void ArrowAlgea()
    {
        arrow.SetActive(true);
    }
}
