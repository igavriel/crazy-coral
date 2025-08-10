using TMPro;
using UnityEngine;

public class UI_MAnager : MonoBehaviour
{
    public TMP_Text scoreCoinsText;
    [SerializeField] intSO coins;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Util.AssertObject(scoreCoinsText, "Score Coins Text is not assigned in the inspector.");
        scoreCoinsText.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        scoreCoinsText.text = coins.Value.ToString();
    }
}
