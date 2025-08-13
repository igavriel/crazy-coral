using UnityEngine;
using UnityEngine.SceneManagement;
using DG;
using DG.Tweening;
public class SceneLoader : MonoBehaviour
{
    [SerializeField] CanvasGroup blackScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blackScreen.DOFade(0, 0.5f);
    }

    public void loadNextScene()
    {
        blackScreen.DOFade(1, 0.5f).OnComplete( () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void loadSceneByName(string name)
    {
        blackScreen.DOFade(1, 0.5f).OnComplete(() => SceneManager.LoadScene(name));
    }

    public void loadSceneByNum(int index)
    {
        blackScreen.DOFade(1, 0.5f).OnComplete(() => SceneManager.LoadScene(index));
    }
}
