using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MenuUtils : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenu;

    public void ResumeGame()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void BackToMainMenu()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
