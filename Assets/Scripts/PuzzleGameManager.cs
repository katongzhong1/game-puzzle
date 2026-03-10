using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PuzzleGameManager : MonoBehaviour
{
    public static PuzzleGameManager Instance { get; private set; }
    
    [Header("Game References")]
    public PuzzleGame puzzleGame;
    public PuzzleUI puzzleUI;
    
    [Header("Game Settings")]
    public bool isPaused = false;
    public bool isGameOver = false;
    public float gameStartTime = 0f;
    public float gameEndTime = 0f;
    public int moveCount = 0;
    
    [Header("Difficulty Settings")]
    public int currentDifficulty = 1;
    public int maxDifficulty = 3;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        isPaused = false;
        isGameOver = false;
        gameStartTime = Time.time;
        moveCount = 0;
        
        if (puzzleUI != null)
        {
            puzzleUI.InitializeUI();
        }
        
        Debug.Log("游戏开始！难度: " + currentDifficulty);
    }
    
    public void OnPuzzleComplete()
    {
        isGameOver = true;
        isPaused = true;
        gameEndTime = Time.time;
        
        float playTime = gameEndTime - gameStartTime;
        Debug.Log($"拼图完成！用时: {playTime:F1}秒, 移动次数: {moveCount}");
        
        if (puzzleUI != null)
        {
            puzzleUI.ShowWinScreen();
            puzzleUI.UpdateWinInfo(playTime, moveCount);
        }
    }
    
    public void OnPieceMoved()
    {
        moveCount++;
        if (puzzleUI != null)
        {
            puzzleUI.UpdateMoveCount(moveCount);
        }
    }
    
    public void RestartGame()
    {
        isGameOver = false;
        isPaused = false;
        gameStartTime = Time.time;
        moveCount = 0;
        
        if (puzzleGame != null)
        {
            puzzleGame.ResetGame();
        }
        
        if (puzzleUI != null)
        {
            puzzleUI.HideWinScreen();
            puzzleUI.InitializeUI();
            puzzleUI.UpdateMoveCount(0);
        }
        
        Debug.Log("游戏重新开始");
    }
    
    public void ChangeDifficulty(int newDifficulty)
    {
        if (newDifficulty < 1) newDifficulty = 1;
        if (newDifficulty > maxDifficulty) newDifficulty = maxDifficulty;
        
        currentDifficulty = newDifficulty;
        
        if (puzzleGame != null)
        {
            puzzleGame.gridSize = 3 + currentDifficulty;
            puzzleGame.ResetGame();
        }
        
        RestartGame();
        
        Debug.Log("难度已切换到: " + currentDifficulty);
    }
    
    public void PauseGame()
    {
        if (!isGameOver)
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            if (puzzleUI != null)
            {
                puzzleUI.ShowPauseScreen();
            }
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (puzzleUI != null)
        {
            puzzleUI.HidePauseScreen();
        }
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public int GetGridSize()
    {
        return 3 + currentDifficulty;
    }
    
    public string GetDifficultyName()
    {
        switch(currentDifficulty)
        {
            case 1: return "简单 (3x3)";
            case 2: return "中等 (4x4)";
            case 3: return "困难 (5x5)";
            default: return "简单 (3x3)";
        }
    }
}
