using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class ConfigureMainCameraForShop : MonoBehaviour
{
    public GameObject shopCameraPrefab;
    private string lastScene = "";

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "ShopScene")
        {
            Debug.Log("ShopScene loaded. Replacing camera...");

            // Destroy the existing MainCamera if any
            Camera oldCam = Camera.main;
            if (oldCam != null)
            {
                Destroy(oldCam.gameObject);
            }

            // Instantiate new shop camera
            GameObject newCam = Instantiate(shopCameraPrefab);
            newCam.tag = "MainCamera";
        }
    }
}
