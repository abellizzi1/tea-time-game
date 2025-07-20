using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject MenuScreen;
    [SerializeField] private GameObject CreditsScreen;
    [SerializeField] private GameObject TeaBag;
    [SerializeField] private GameObject player;

    public void Start()
    {
        TeaBag.SetActive(true);
        MenuScreen.SetActive(true);
        CreditsScreen.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Room1Scene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void RollCreditsScreen()
    {
        MenuScreen.SetActive(false);
        CreditsScreen.SetActive(true);
    }

    public void BackToMain()
    {
        MenuScreen.SetActive(true);
        CreditsScreen.SetActive(false);
    }
}
