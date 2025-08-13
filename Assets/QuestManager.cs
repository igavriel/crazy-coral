using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] string quest;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("startQuest", 1f);   
    }

    void startQuest()
    {
        FindFirstObjectByType<PopUpController>().open(quest);
    }
}
