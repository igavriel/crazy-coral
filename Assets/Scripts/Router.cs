using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Router : MonoBehaviour
{
    public static readonly string SCENE_MAIN = "MainScene";
    public static readonly string SCENE_HOME = "HomeScene";
    public static readonly string SCENE_GAME = "GameScene";
    public static readonly string SCENE_MAP = "MapScene";
    public static readonly string SCENE_Fish = "FishScene";

    [SerializeField] CanvasGroup fade;

    public static Router instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        fade.DOFade(0, 0.5f);
    }

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

    public void OnMainScene() => fade.DOFade(1, 0.5f).OnComplete( () => LoadSceneByName(SCENE_MAIN));

    public void OnHomeScene() => fade.DOFade(1, 0.5f).OnComplete(() => LoadSceneByName(SCENE_HOME));

    public void OnStartGame() => fade.DOFade(1, 0.5f).OnComplete(() => LoadSceneByName(SCENE_GAME));

    public void OnMapScene()  => fade.DOFade(1, 0.5f).OnComplete(() => LoadSceneByName(SCENE_MAP));

    public void OnFishScene() => fade.DOFade(1, 0.5f).OnComplete(() => LoadSceneByName(SCENE_Fish));
}
