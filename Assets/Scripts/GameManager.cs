using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameData GameData { get; private set; } = new();

    public bool IsDebugMode { get; private set; } = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        GameData.StartGame();
    }

    public void EndGame()
    {
        GameData.EndGame();
        SaveProgress();
    }

    public void ToggleDebugMode()
    {
        IsDebugMode = !IsDebugMode;
        Debug.Log($"Debug mode is now {(IsDebugMode ? "enabled" : "disabled")}");
    }

    public void SaveProgress()
    {
        // Save game data to persistent storage (e.g., PlayerPrefs, file, etc.)
        // This is a placeholder for actual saving logic
        Debug.Log("Saving game progress...");
        GameData.SaveProgress();
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        // Load game data from persistent storage (e.g., PlayerPrefs, file, etc.)
        // This is a placeholder for actual loading logic
        Debug.Log("Loading game progress...");
        GameData.LoadProgress();
    }

    public void ResetGame()
    {
        Debug.Log("Resetting game progress");

        PlayerPrefs.DeleteAll();
        GameData.Reset();
    }

    public string buildSummaryText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Summary:");
        sb.AppendLine($"Collected tokens: {GameData.LastGameTokens}");
        sb.AppendLine($"Total Tokens: {GameData.TotalTokens}");
        return sb.ToString();
    }

    public void buildScoreText(TMP_Text tokens)
    {
        tokens.text = $"{GameData.TotalTokens} ({GameData.LastGameTokens})";
    }
}
