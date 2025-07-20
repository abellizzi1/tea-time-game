using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TeaFill : MonoBehaviour
{

    [SerializeField] GameObject teabag;
    [SerializeField] GameObject IntroTeafill;
    [SerializeField] GameObject MenuTeafill;
    [SerializeField] GameObject MainMenuScreen;
    [SerializeField] GameObject MainMenuCamera;

    void Start()
    {
        MainMenuCamera.SetActive(false);
    }

    public void RemoveTeaBag()
    {
        teabag.SetActive(false);
        MainMenuScreen.SetActive(true);
        MainMenuCamera.SetActive(true);
    }

    void Update()
    {
        if (GlobalMenuState.introCinematicPlayed)
        {
            PostCinematic();
        }
    }

    public void PostCinematic()
    {
        Debug.Log("Setting Post-Cinematic menu rules");
        teabag.SetActive(false);
        MainMenuScreen.SetActive(true);
        MainMenuCamera.SetActive(true);
        IntroTeafill.SetActive(false);
        MenuTeafill.SetActive(false);
        GlobalMenuState.introCinematicPlayed = false;
    }
}
