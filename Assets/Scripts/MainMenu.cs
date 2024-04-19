using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highscore;
    [SerializeField] private Image fadeImage;
    private void Awake()
    {
        if (!PlayerPrefs.HasKey("HighScore"))
        {
            PlayerPrefs.SetInt("High Score", 0);
        }

        highscore.text = $"High Score: {PlayerPrefs.GetInt("HighScore")}";
        FadeIn();
    }

    private async void FadeIn()
    {
        await fadeImage.DOColor(new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f), 0.5f).AsyncWaitForCompletion();
    }

    public async void LoadGame()
    {
        AudioManager.instance.Play("Click");
        await fadeImage.DOColor(new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1f), 0.5f).AsyncWaitForCompletion();
        SceneController.instance.LoadScene("MainScene");
    }

    public void QuitGame()
    {
        AudioManager.instance.Play("Click");
#if !UNITY_WEBGL
        Application.Quit();
#endif
    }
}
