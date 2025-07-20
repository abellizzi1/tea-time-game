using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Using tutorial from: https://www.youtube.com/watch?v=WiUUW9RSa5Y&ab_channel=CoffeeCupDev

public class FadeAwayText : MonoBehaviour
{
    private float fadeTime;
    private TextMeshProUGUI fadeAwayText;
    private float alphaValue;
    private float fadeAwayPerSecond;

    void Start()
    {
        fadeAwayText = GetComponent<TextMeshProUGUI>();
        alphaValue = fadeAwayText.color.a;
    }

    void Update()
    {
        if (fadeTime > 0)
        {
            fadeTime -= Time.deltaTime;
            alphaValue -= fadeAwayPerSecond * Time.deltaTime;
            fadeAwayText.color = new Color(fadeAwayText.color.r, fadeAwayText.color.g, fadeAwayText.color.b, alphaValue);
        }
        else
        {
            SetText("", 0.0f);
        }
    }

    public void SetText(string message, float fadeLength, int fontSize=35)
    {
        fadeAwayText.fontSize = fontSize;
        fadeAwayText.text = message;
        alphaValue = 1f;
        fadeTime = fadeLength;
        fadeAwayPerSecond = 1 / fadeTime;

        fadeAwayText.color = new Color(
            255,
            255,
            255,
            alphaValue);

        fadeAwayText.gameObject.SetActive(true);

    }

    public void DisableText()
    {
        fadeAwayText.gameObject.SetActive(false);
    }
}