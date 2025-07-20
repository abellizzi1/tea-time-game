using UnityEngine;

public class ScenePersistent1 : MonoBehaviour
{
    public static ScenePersistent1 instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }
}
