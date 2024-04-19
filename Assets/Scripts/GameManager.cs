using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private AnimationCurve scoreMultiplier;
    [SerializeField] private CinemachineImpulseSource impulse;
    [SerializeField] private Image fadeImage;
    [SerializeField] private CanvasGroup pauseScreen;
    private float score;
    public int Score => (int)(score * 100);
    public float ScoreMultiplier => scoreMultiplier.Evaluate(timeElapsed / 60f);
    private float timeElapsed;

    private bool paused;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Time.timeScale = 0f;
        pauseScreen.alpha = 0f;
        pauseScreen.blocksRaycasts = false;
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        UnFade();
    }

    private async void UnFade()
    {
        await DOTween.To(() => fadeImage.color.a, x => fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, x), 0f, 0.5f).SetUpdate(true).AsyncWaitForCompletion();
        //await fadeImage.DOColor(new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f), 0.5f). SetUpdate(true).AsyncWaitForCompletion();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        if (Time.timeScale == 0f) return;
        timeElapsed += Time.deltaTime;
        score += timeElapsed / 126000f * ScoreMultiplier;
        
    }

    public void IncreaseScore(float amount)
    {
        score += amount;
    }

    public async void EndGame(bool saveScore = true)
    {
        if(Score > PlayerPrefs.GetInt("HighScore") && saveScore)
        {
            PlayerPrefs.SetInt("HighScore", Score);
        }
        await fadeImage.DOColor(new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1), 0.5f).SetUpdate(true).AsyncWaitForCompletion();
        Time.timeScale = 1f;
        SceneController.instance.LoadScene("MainMenu");

    }

    public async void TogglePause()
    {
        Click();
        paused = !paused;
        pauseScreen.blocksRaycasts = paused;
        if (paused)
        {
            Time.timeScale = 0f;
            await DOTween.To(() => pauseScreen.alpha, x => pauseScreen.alpha = x, 1f, 0.35f).SetUpdate(true).AsyncWaitForCompletion();
        }
        else
        {
            await DOTween.To(() => pauseScreen.alpha, x => pauseScreen.alpha = x, 0f, 0.35f).SetUpdate(true).AsyncWaitForCompletion();
            Time.timeScale = 1f;
        }
    }

    public void Click()
    {
        AudioManager.instance.Play("Click");
    }

    public void CameraShake(float strength, float time = 0.2f, CinemachineImpulseDefinition.ImpulseShapes shape = CinemachineImpulseDefinition.ImpulseShapes.Recoil)
    {
        impulse.m_ImpulseDefinition.m_ImpulseShape = shape;
        impulse.m_ImpulseDefinition.m_ImpulseDuration = time;
        impulse.m_DefaultVelocity = Vector3.down;
        impulse.GenerateImpulse(strength);
    }
    public void CameraShake(float strength, Vector3 direction, float time = 0.2f, CinemachineImpulseDefinition.ImpulseShapes shape = CinemachineImpulseDefinition.ImpulseShapes.Recoil)
    {
        impulse.m_ImpulseDefinition.m_ImpulseShape = shape;
        impulse.m_ImpulseDefinition.m_ImpulseDuration = time;
        impulse.m_DefaultVelocity = direction;
        impulse.GenerateImpulse(strength);
    }
}
