using UnityEngine;
using UnityEngine.SceneManagement;

public class Router : MonoBehaviour
{
    public static readonly string SCENE_MAIN = "MainScene";
    public static readonly string SCENE_GAME = "GameScene";
    public static readonly string SCENE_MAP = "MapScene";

    private void LoadSceneByName(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        // If running in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a standalone build, quit the application
        Application.Quit();
#endif
    }

    public void OnMainScene() => LoadSceneByName(SCENE_MAIN);

    public void OnStartGame() => LoadSceneByName(SCENE_GAME);

    public void OnMapScene() => LoadSceneByName(SCENE_MAP);
}
