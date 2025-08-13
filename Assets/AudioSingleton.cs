using UnityEngine;

public class AudioSingleton : MonoBehaviour
{
    public static AudioSingleton instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}
