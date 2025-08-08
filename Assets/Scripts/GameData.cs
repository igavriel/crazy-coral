using System;
using System.Text;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int TotalTokens { get; private set; } = 0;
    public int LastGameTokens { get; private set; } = 0;
    public DateTime LastGameDate { get; private set; } = DateTime.Now;

    public void Reset()
    {
        LastGameTokens = 0;
        TotalTokens = 0;
        LastGameDate = DateTime.Now;
    }

    public void StartGame()
    {
        LastGameDate = DateTime.Now; // Update last game time to current time
        LastGameTokens = 0;
    }

    public void EndGame()
    {
        // Calculate total tokens based on accumulated sleep time
        TotalTokens += LastGameTokens;

        StringBuilder debugMessage = new StringBuilder();
        debugMessage.Append($"Session from {LastGameDate.ToString("o")} ended: ");
        debugMessage.Append($"Tokens earned this session: {LastGameTokens} ");
        debugMessage.Append($"Total tokens: {TotalTokens} ");
        Debug.Log(debugMessage.ToString());
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("totalTokens", TotalTokens);
        PlayerPrefs.SetInt("lastGameTokens", LastGameTokens);
        PlayerPrefs.SetString("lastGameDate", LastGameDate.ToString("o")); // ISO 8601 format
    }

    public void LoadProgress()
    {
        TotalTokens = PlayerPrefs.GetInt("totalTokens", 0);
        string lastGameDateString = PlayerPrefs.GetString(
            "lastGameDate",
            DateTime.Now.ToString("o")
        );
        if (DateTime.TryParse(lastGameDateString, out DateTime parsedDate))
        {
            LastGameDate = parsedDate;
        }
        else
        {
            LastGameDate = DateTime.Now; // Fallback to current time if parsing fails
        }
    }
}
