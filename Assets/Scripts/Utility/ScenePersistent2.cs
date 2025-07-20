using UnityEngine;

public class ScenePersistent2 : MonoBehaviour
{
    public static ScenePersistent2 instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }
}
