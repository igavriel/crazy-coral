using TMPro;
using UnityEngine;

public class UI_MAnager : MonoBehaviour
{
    public TMP_Text scoreCoinsText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Util.AssertObject(scoreCoinsText, "Score Coins Text is not assigned in the inspector.");
        scoreCoinsText.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        GameManager.Instance.buildScoreText(scoreCoinsText);
    }
}
