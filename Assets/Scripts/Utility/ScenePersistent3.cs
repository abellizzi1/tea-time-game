using UnityEngine;

public class ScenePersistent3 : MonoBehaviour
{
    public static ScenePersistent3 instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }
}
