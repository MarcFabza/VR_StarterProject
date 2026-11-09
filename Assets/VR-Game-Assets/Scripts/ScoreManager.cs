using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Canvas (se les asigna la cámara VR al empezar)")]
    public Canvas[] canvases;

    [Header("Partida")]
    public float gameDuration = 60f;
    public CubeSpawner cubeSpawner;

    [Header("UI_Timer")]
    public TMP_Text timeText;
    public Image timeBar;

    [Header("UI_ScoreCurrent")]
    public TMP_Text currentScoreText;

    [Header("UI_ScoreFinal")]
    public GameObject finalPanel;
    public TMP_Text finalScoreText;
    public TMP_Text highScoreText;
    public string lobbySceneName = "LobbyScene";

    private const string HighScoreKey = "HighScore";

    private int score = 0;
    private float timeLeft;
    private bool gameOver = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupCanvasCamera();

        timeLeft = gameDuration;
        score = 0;
        gameOver = false;

        if (finalPanel != null)
            finalPanel.SetActive(false);

        UpdateScoreText();
        UpdateTimer();
    }

    void Update()
    {
        if (gameOver)
            return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateTimer();
            EndGame();
            return;
        }

        UpdateTimer();
    }

    void SetupCanvasCamera()
    {
        Camera vrCamera = Camera.main;
        if (vrCamera == null)
            return;

        foreach (Canvas canvas in canvases)
        {
            if (canvas != null)
                canvas.worldCamera = vrCamera;
        }
    }

    public static void AddScore(int points)
    {
        if (Instance == null || Instance.gameOver)
            return;

        Instance.score = Mathf.Max(0, Instance.score + points);
        Instance.UpdateScoreText();
        Instance.SaveHighScoreIfBeaten();
    }

    int SaveHighScoreIfBeaten()
    {
        int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }
        return highScore;
    }

    void UpdateScoreText()
    {
        if (currentScoreText != null)
            currentScoreText.text = score.ToString();
    }

    void UpdateTimer()
    {
        if (timeText != null)
        {
            int seconds = Mathf.CeilToInt(timeLeft);
            timeText.text = (seconds / 60) + ":" + (seconds % 60).ToString("00");
        }

        if (timeBar != null)
            timeBar.fillAmount = gameDuration > 0f ? timeLeft / gameDuration : 0f;
    }

    void EndGame()
    {
        gameOver = true;

        if (cubeSpawner != null)
            cubeSpawner.StopSpawning();
        CubeSpawner.DestroyAllCubes();

        int highScore = SaveHighScoreIfBeaten();

        if (finalScoreText != null)
            finalScoreText.text = score.ToString();
        if (highScoreText != null)
            highScoreText.text = highScore.ToString();

        if (finalPanel != null)
            finalPanel.SetActive(true);
    }

    public void BackToLobby()
    {
        SceneManager.LoadScene(lobbySceneName);
    }
}
