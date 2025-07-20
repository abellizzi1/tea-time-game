using UnityEngine;
using UnityEngine.SceneManagement;

public class DisableListenerOnStart : MonoBehaviour
{
    void Awake()         // or Start()
    {
        AudioListener listener = GetComponent<AudioListener>();
        if (SceneManager.GetActiveScene().name == "ShopScene")
        {
            if (listener != null)
            {
                listener.enabled = false;                                                     
            }
        }
        else
        {
            if (listener != null)
            {
                listener.enabled = true;
            }
        }
    }
}