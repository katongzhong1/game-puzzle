using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneInitializer : MonoBehaviour
{
    [Header("Game Settings")]
    public Sprite puzzleSprite;
    public int gridSize = 5;
    public float puzzleSize = 400f;
    public float pieceSpacing = 2f;

    private TMP_FontAsset chineseFont;

    private void Start()
    {
        FindChineseFont();
        
        if (puzzleSprite == null)
        {
            Debug.LogError("请在Inspector中设置拼图图片！");
            return;
        }

        CreateGameScene();
    }

    private void FindChineseFont()
    {
        if (chineseFont != null) return;

        TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        
        foreach (var font in allFonts)
        {
            if (font.name.Contains("Chinese") || font.name.Contains("中文") || font.name.Contains("SimSun") || font.name.Contains("Microsoft"))
            {
                chineseFont = font;
                Debug.Log("找到中文字体: " + font.name);
                break;
            }
        }
    }

    private void CreateGameScene()
    {
        Canvas canvas = CreateCanvas();
        Transform canvasTransform = canvas.transform;

        GameObject gameManager = CreateGameManager();
        GameObject puzzleGameObj = CreatePuzzleGame(canvasTransform);
        GameObject puzzleUIObj = CreatePuzzleUI(canvasTransform, gameManager.GetComponent<PuzzleGameManager>());

        SetupReferences(gameManager, puzzleGameObj, puzzleUIObj);

        Debug.Log("游戏场景创建完成！");
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        return canvas;
    }

    private GameObject CreateGameManager()
    {
        GameObject gameManagerObj = new GameObject("PuzzleGameManager");
        PuzzleGameManager gameManager = gameManagerObj.AddComponent<PuzzleGameManager>();
        return gameManagerObj;
    }

    private GameObject CreatePuzzleGame(Transform parent)
    {
        GameObject puzzleGameObj = new GameObject("PuzzleGame");
        puzzleGameObj.transform.SetParent(parent, false);

        PuzzleGame puzzleGame = puzzleGameObj.AddComponent<PuzzleGame>();
        puzzleGame.gridSize = gridSize;
        puzzleGame.pieceSpacing = pieceSpacing;

        RectTransform puzzleContainer = CreatePuzzleContainer(parent, puzzleSize);
        puzzleGame.puzzleContainer = puzzleContainer;

        CreateReferenceImage(parent, puzzleContainer, puzzleSprite);

        GameObject puzzleImageObj = new GameObject("PuzzleImage");
        puzzleImageObj.transform.SetParent(puzzleGameObj.transform, false);
        Image puzzleImage = puzzleImageObj.AddComponent<Image>();
        puzzleImage.sprite = puzzleSprite;
        puzzleGame.puzzleImage = puzzleImage;

        puzzleGameObj.SetActive(false);

        return puzzleGameObj;
    }

    private RectTransform CreatePuzzleContainer(Transform parent, float size)
    {
        GameObject containerObj = new GameObject("PuzzleContainer");
        containerObj.transform.SetParent(parent, false);

        RectTransform rectTransform = containerObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = containerObj.AddComponent<RectTransform>();
        }

        float aspectRatio = (float)puzzleSprite.rect.width / puzzleSprite.rect.height;

        float containerWidth, containerHeight;

        if (aspectRatio >= 1f)
        {
            containerWidth = size;
            containerHeight = size / aspectRatio;
        }
        else
        {
            containerWidth = size * aspectRatio;
            containerHeight = size;
        }

        rectTransform.sizeDelta = new Vector2(containerWidth, containerHeight);
        rectTransform.anchoredPosition = new Vector2(0, 150);

        Image containerImage = containerObj.AddComponent<Image>();
        containerImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        return rectTransform;
    }

    private void CreateReferenceImage(Transform parent, RectTransform puzzleContainer, Sprite sprite)
    {
        GameObject refImageObj = new GameObject("ReferenceImage");
        refImageObj.transform.SetParent(parent, false);

        RectTransform rectTransform = refImageObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = refImageObj.AddComponent<RectTransform>();
        }

        float refSize = 200f;
        float aspectRatio = (float)sprite.rect.width / sprite.rect.height;

        float refWidth, refHeight;

        if (aspectRatio >= 1f)
        {
            refWidth = refSize;
            refHeight = refSize / aspectRatio;
        }
        else
        {
            refWidth = refSize * aspectRatio;
            refHeight = refSize;
        }

        rectTransform.sizeDelta = new Vector2(refWidth, refHeight);
        rectTransform.anchoredPosition = new Vector2(0, -700);

        Image refImage = refImageObj.AddComponent<Image>();
        refImage.sprite = sprite;
        refImage.preserveAspect = false;
    }

    private GameObject CreatePuzzleUI(Transform parent, PuzzleGameManager gameManager)
    {
        GameObject puzzleUIObj = new GameObject("PuzzleUI");
        puzzleUIObj.transform.SetParent(parent, false);

        PuzzleUI puzzleUI = puzzleUIObj.AddComponent<PuzzleUI>();

        GameObject infoPanel =CreateInfoPanel(parent);
        GameObject winScreen = CreateWinScreen(parent);
        GameObject pauseScreen = CreatePauseScreen(parent);
        GameObject pauseButton = CreatePauseButton(parent);
        GameObject restartButton = CreateRestartButton(parent);
        GameObject difficultyButton = CreateDifficultyButton(parent);
        GameObject resumeButton = CreateResumeButton(pauseScreen.transform);
        GameObject quitButton = CreateQuitButton(parent);

        puzzleUI.winScreen = winScreen;
        puzzleUI.pauseScreen = pauseScreen;
        puzzleUI.pauseButton = pauseButton.GetComponent<Button>();
        puzzleUI.restartButton = restartButton.GetComponent<Button>();
        puzzleUI.resumeButton = resumeButton.GetComponent<Button>();
        puzzleUI.quitButton = quitButton.GetComponent<Button>();

        Button diffButton = difficultyButton.GetComponent<Button>();
        if (diffButton != null)
        {
            diffButton.onClick.AddListener(() => gameManager.ChangeDifficulty(gameManager.currentDifficulty % 3 + 1));
        }

        Transform moveCountTransform = infoPanel.transform.Find("MoveCountText");
        if (moveCountTransform != null)
        {
            puzzleUI.moveCountText = moveCountTransform.GetComponent<TextMeshProUGUI>();
        }

        Transform timeTransform = infoPanel.transform.Find("TimeText");
        if (timeTransform != null)
        {
            puzzleUI.timeText = timeTransform.GetComponent<TextMeshProUGUI>();
        }

        return puzzleUIObj;
    }

    private GameObject CreateWinScreen(Transform parent)
    {
        GameObject winScreen = new GameObject("WinScreen");
        winScreen.transform.SetParent(parent, false);

        RectTransform rectTransform = winScreen.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = winScreen.AddComponent<RectTransform>();
        }

        rectTransform.sizeDelta = new Vector2(1080, 1920);
        rectTransform.anchoredPosition = Vector2.zero;

        Image backgroundImage = winScreen.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.85f);

        GameObject titleObj = CreateTextElement(winScreen.transform, "TitleText", "恭喜完成！", 80, new Vector2(0, 300), Color.yellow);
        GameObject subtitleObj = CreateTextElement(winScreen.transform, "SubtitleText", "你成功完成了拼图！", 50, new Vector2(0, 150), Color.white);
        GameObject restartBtn = CreateButtonElement(winScreen.transform, "RestartButton", "重新开始", new Vector2(0, -100), 60);

        return winScreen;
    }

    private GameObject CreatePauseScreen(Transform parent)
    {
        GameObject pauseScreen = new GameObject("PauseScreen");
        pauseScreen.transform.SetParent(parent, false);

        RectTransform rectTransform = pauseScreen.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = pauseScreen.AddComponent<RectTransform>();
        }

        rectTransform.sizeDelta = new Vector2(1080, 1920);
        rectTransform.anchoredPosition = Vector2.zero;

        Image backgroundImage = pauseScreen.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.85f);

        GameObject titleObj = CreateTextElement(pauseScreen.transform, "PauseTitleText", "游戏暂停", 80, new Vector2(0, 400), Color.yellow);
        GameObject resumeBtn = CreateButtonElement(pauseScreen.transform, "ResumeButton", "继续游戏", new Vector2(0, 100), 60);
        GameObject quitBtn = CreateButtonElement(pauseScreen.transform, "QuitButton", "退出游戏", new Vector2(0, -100), 60);

        return pauseScreen;
    }

    private GameObject CreatePauseButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("PauseButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = buttonObj.AddComponent<RectTransform>();
        }

        rectTransform.sizeDelta = new Vector2(200, 80);
        rectTransform.anchoredPosition = new Vector2(400, 850);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null)
        {
            textRect = textObj.AddComponent<RectTransform>();
        }

        textRect.sizeDelta = new Vector2(180, 60);
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = "暂停";
        textComponent.fontSize = 36;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        SetChineseFont(textComponent);

        return buttonObj;
    }

    private GameObject CreateRestartButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("RestartButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = buttonObj.AddComponent<RectTransform>();
        }

        rectTransform.sizeDelta = new Vector2(200, 80);
        rectTransform.anchoredPosition = new Vector2(-400, 850);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(1f, 0.4f, 0.4f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null)
        {
            textRect = textObj.AddComponent<RectTransform>();
        }

        textRect.sizeDelta = new Vector2(180, 60);
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = "重置";
        textComponent.fontSize = 36;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        SetChineseFont(textComponent);

        return buttonObj;
    }

    private GameObject CreateResumeButton(Transform parent)
    {
        GameObject buttonObj = CreateButtonElement(parent, "ResumeButton", "继续", new Vector2(0, 100), 60);
        return buttonObj;
    }

    private GameObject CreateQuitButton(Transform parent)
    {
        GameObject buttonObj = CreateButtonElement(parent, "QuitButton", "退出", new Vector2(0, -100), 60);
        return buttonObj;
    }

    private GameObject CreateButtonElement(Transform parent, string name, string text, Vector2 position, int fontSize)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = buttonObj.AddComponent<RectTransform>();
        }

        rectTransform.sizeDelta = new Vector2(400, 120);
        rectTransform.anchoredPosition = position;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null)
        {
            textRect = textObj.AddComponent<RectTransform>();
        }

        textRect.sizeDelta = new Vector2(380, 100);
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        SetChineseFont(textComponent);

        return buttonObj;
    }

    private GameObject CreateTextElement(Transform parent, string name, string text, int fontSize, Vector2 position, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = textObj.AddComponent<RectTransform>();
        }

        rectTransform.sizeDelta = new Vector2(1000, 200);
        rectTransform.anchoredPosition = position;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = color;
        SetChineseFont(textComponent);

        return textObj;
    }

    private GameObject CreateInfoPanel(Transform parent)
    {
        GameObject infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(parent, false);

        RectTransform rectTransform = infoPanel.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = infoPanel.AddComponent<RectTransform>();
        }

        rectTransform.sizeDelta = new Vector2(1000, 120);
        rectTransform.anchoredPosition = new Vector2(0, 880);

        Image panelImage = infoPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        GameObject moveCountText = CreateTextElement(infoPanel.transform, "MoveCountText", "移动: 0", 40, new Vector2(-300, 0), Color.white);
        GameObject timeText = CreateTextElement(infoPanel.transform, "TimeText", "时间: 0s", 40, new Vector2(300, 0), Color.white);

        return infoPanel;
    }

    private GameObject CreateDifficultyButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("DifficultyButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = buttonObj.AddComponent<RectTransform>();
        }

        rectTransform.sizeDelta = new Vector2(200, 80);
        rectTransform.anchoredPosition = new Vector2(0, 850);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.6f, 0.2f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect == null)
        {
            textRect = textObj.AddComponent<RectTransform>();
        }

        textRect.sizeDelta = new Vector2(180, 60);
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = "难度";
        textComponent.fontSize = 36;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        SetChineseFont(textComponent);

        return buttonObj;
    }

    private void SetChineseFont(TextMeshProUGUI textComponent)
    {
        if (chineseFont != null)
        {
            textComponent.font = chineseFont;
        }
    }

    private void SetupReferences(GameObject gameManagerObj, GameObject puzzleGameObj, GameObject puzzleUIObj)
    {
        PuzzleGameManager gameManager = gameManagerObj.GetComponent<PuzzleGameManager>();
        PuzzleGame puzzleGame = puzzleGameObj.GetComponent<PuzzleGame>();
        PuzzleUI puzzleUI = puzzleUIObj.GetComponent<PuzzleUI>();

        gameManager.puzzleGame = puzzleGame;
        gameManager.puzzleUI = puzzleUI;

        puzzleUI.InitializeUI();

        puzzleGameObj.SetActive(true);
    }
}
