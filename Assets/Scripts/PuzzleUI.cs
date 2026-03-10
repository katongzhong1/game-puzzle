using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject winScreen;
    public GameObject pauseScreen;
    public Button pauseButton;
    public Button restartButton;
    public Button resumeButton;
    public Button quitButton;
    
    [Header("Info Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI pauseTitleText;
    public TextMeshProUGUI moveCountText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI timeText;
    
    [Header("Settings")]
    public string winTitle = "恭喜完成！";
    public string winSubtitle = "你成功完成了拼图！";
    public string pauseTitle = "游戏暂停";
    
    private float startTime;
    
    private void Awake()
    {
        SetupButtonListeners();
        InitializeScreens();
    }
    
    private void Update()
    {
        if (!PuzzleGameManager.Instance.isPaused && !PuzzleGameManager.Instance.isGameOver)
        {
            UpdateGameTime();
        }
    }
    
    private void SetupButtonListeners()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClick);
        }
        
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OnPauseClick);
        }
        
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeClick);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClick);
        }
    }
    
    private void InitializeScreens()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
        
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
    }
    
    public void InitializeUI()
    {
        InitializeScreens();
        
        if (titleText != null)
        {
            titleText.text = winTitle;
        }
        
        if (subtitleText != null)
        {
            subtitleText.text = winSubtitle;
        }
        
        if (pauseTitleText != null)
        {
            pauseTitleText.text = pauseTitle;
        }
        
        if (difficultyText != null)
        {
            difficultyText.text = "难度: " + PuzzleGameManager.Instance.GetDifficultyName();
        }
        
        startTime = Time.time;
    }
    
    public void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
    }
    
    public void HideWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
    }
    
    public void ShowPauseScreen()
    {
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(true);
        }
    }
    
    public void HidePauseScreen()
    {
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
    }
    
    public void UpdateMoveCount(int count)
    {
        if (moveCountText != null)
        {
            moveCountText.text = "移动: " + count;
        }
    }
    
    public void UpdateWinInfo(float playTime, int moves)
    {
        if (subtitleText != null)
        {
            subtitleText.text = $"完成用时: {playTime:F1}秒\n移动次数: {moves}";
        }
    }
    
    private void UpdateGameTime()
    {
        if (timeText != null)
        {
            float currentTime = Time.time - startTime;
            timeText.text = "时间: " + currentTime.ToString("F0") + "s";
        }
    }
    
    private void OnRestartClick()
    {
        if (PuzzleGameManager.Instance != null)
        {
            PuzzleGameManager.Instance.RestartGame();
        }
    }
    
    private void OnPauseClick()
    {
        if (PuzzleGameManager.Instance != null)
        {
            PuzzleGameManager.Instance.PauseGame();
        }
    }
    
    private void OnResumeClick()
    {
        if (PuzzleGameManager.Instance != null)
        {
            PuzzleGameManager.Instance.ResumeGame();
        }
    }
    
    private void OnQuitClick()
    {
        if (PuzzleGameManager.Instance != null)
        {
            PuzzleGameManager.Instance.QuitGame();
        }
    }
}
