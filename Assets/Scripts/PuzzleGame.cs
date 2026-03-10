using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class PuzzleGame : MonoBehaviour
{
    [Header("Game Settings")]
    public int gridSize = 5;
    public float pieceSpacing = 2f;
    
    [Header("References")]
    public RectTransform puzzleContainer;
    public Image puzzleImage;
    public GameObject piecePrefab;
    public RectTransform referenceImage;
    
    [Header("Game State")]
    public bool canMovePieces = true;
    public bool isGameComplete = false;
    
    private List<PuzzlePiece> pieces = new List<PuzzlePiece>();
    private Vector2 pieceSize;
    private float totalSpacing;
    private int totalPieces;

    private void Start()
    {
        if (puzzleImage == null)
        {
            Debug.LogError("Please assign a puzzle image in the inspector!");
            return;
        }

        if (puzzleImage.sprite == null)
        {
            Debug.LogError("Puzzle image sprite is null!");
            return;
        }

        InitializePuzzle();
    }

    private void InitializePuzzle()
    {
        if (puzzleImage.sprite != null)
        {
            Debug.Log($"拼图图片信息: 宽度={puzzleImage.sprite.rect.width}, 高度={puzzleImage.sprite.rect.height}, 宽高比={puzzleImage.sprite.rect.width / puzzleImage.sprite.rect.height:F2}");
            Debug.Log($"拼图容器信息: 宽度={puzzleContainer.rect.width}, 高度={puzzleContainer.rect.height}");
        }

        totalPieces = gridSize * gridSize;
        totalSpacing = pieceSpacing * (gridSize - 1);
        
        CalculatePieceSize();
        CreatePuzzlePieces();
        ShufflePieces();
        SetupReferenceImage();
    }

    private void CalculatePieceSize()
    {
        Sprite sprite = puzzleImage.sprite;
        if (sprite == null) return;

        float imageWidth = sprite.rect.width;
        float imageHeight = sprite.rect.height;
        float aspectRatio = imageWidth / imageHeight;

        float containerWidth = puzzleContainer.rect.width;
        float containerHeight = puzzleContainer.rect.height;

        float availableWidth = containerWidth - totalSpacing;
        float availableHeight = containerHeight - totalSpacing;

        float containerAspectRatio = containerWidth / containerHeight;

        if (containerAspectRatio > aspectRatio)
        {
            float adjustedWidth = availableHeight * aspectRatio;
            pieceSize = new Vector2(adjustedWidth / gridSize, availableHeight / gridSize);
        }
        else
        {
            float adjustedHeight = availableWidth / aspectRatio;
            pieceSize = new Vector2(availableWidth / gridSize, adjustedHeight / gridSize);
        }
    }

    private void CreatePuzzlePieces()
    {
        if (piecePrefab == null)
        {
            CreatePiecePrefab();
        }

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                CreatePiece(row, col);
            }
        }
    }

    private void CreatePiecePrefab()
    {
        piecePrefab = new GameObject("PuzzlePiecePrefab");
        piecePrefab.AddComponent<RectTransform>();
        piecePrefab.AddComponent<Image>();
        piecePrefab.AddComponent<PuzzlePiece>();
        piecePrefab.AddComponent<CanvasGroup>();
    }

    private void CreatePiece(int row, int col)
    {
        GameObject pieceObj = Instantiate(piecePrefab, puzzleContainer);
        PuzzlePiece piece = pieceObj.GetComponent<PuzzlePiece>();
        
        RectTransform rectTransform = pieceObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = pieceSize;
        
        Image pieceImage = pieceObj.GetComponent<Image>();
        
        Sprite originalSprite = puzzleImage.sprite;
        Texture2D texture = originalSprite.texture;
        
        float originalRectX = originalSprite.rect.x;
        float originalRectY = originalSprite.rect.y;
        float originalRectWidth = originalSprite.rect.width;
        float originalRectHeight = originalSprite.rect.height;
        
        float pieceWidth = originalRectWidth / gridSize;
        float pieceHeight = originalRectHeight / gridSize;
        
        float rectX = originalRectX + col * pieceWidth;
        float rectY = originalRectY + (gridSize - 1 - row) * pieceHeight;
        
        Rect spriteRect = new Rect(rectX, rectY, pieceWidth, pieceHeight);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        float pixelsPerUnit = 100f;
        
        Sprite pieceSprite = Sprite.Create(texture, spriteRect, pivot, pixelsPerUnit);
        pieceImage.sprite = pieceSprite;
        pieceImage.preserveAspect = false;
        
        piece.SetCorrectPosition(row, col);
        piece.SetCurrentPosition(row, col);
        piece.puzzleGame = this;
        
        SetPiecePosition(rectTransform, row, col);
        
        pieces.Add(piece);
    }

    private void SetPiecePosition(RectTransform rectTransform, int row, int col)
    {
        float totalGridWidth = gridSize * pieceSize.x + (gridSize - 1) * pieceSpacing;
        float totalGridHeight = gridSize * pieceSize.y + (gridSize - 1) * pieceSpacing;

        float xOffset = (puzzleContainer.rect.width - totalGridWidth) / 2f;
        float yOffset = (puzzleContainer.rect.height - totalGridHeight) / 2f;

        float xPos = xOffset + col * (pieceSize.x + pieceSpacing) + pieceSize.x / 2 - puzzleContainer.rect.width / 2;
        float yPos = -yOffset - row * (pieceSize.y + pieceSpacing) - pieceSize.y / 2 + puzzleContainer.rect.height / 2;

        rectTransform.anchoredPosition = new Vector2(xPos, yPos);
    }

    private void ShufflePieces()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                positions.Add(new Vector2Int(row, col));
            }
        }

        positions = positions.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < pieces.Count; i++)
        {
            PuzzlePiece piece = pieces[i];
            piece.SetCurrentPosition(positions[i].x, positions[i].y);
            
            RectTransform rectTransform = piece.GetComponent<RectTransform>();
            SetPiecePosition(rectTransform, positions[i].x, positions[i].y);
        }
    }

    private void SetupReferenceImage()
    {
        if (referenceImage != null)
        {
            Image refImage = referenceImage.GetComponent<Image>();
            if (refImage != null)
            {
                refImage.sprite = puzzleImage.sprite;
                refImage.preserveAspect = true;
            }
        }
    }

    public void OnPieceDropped(PuzzlePiece piece)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            puzzleContainer,
            Input.mousePosition,
            null,
            out localPoint
        );

        int targetRow = GetRowFromPosition(localPoint.y);
        int targetCol = GetColFromPosition(localPoint.x);

        if (IsValidPosition(targetRow, targetCol))
        {
            PuzzlePiece targetPiece = GetPieceAtPosition(targetRow, targetCol);
            
            if (targetPiece != null && targetPiece != piece)
            {
                SwapPieces(piece, targetPiece);
            }
            else
            {
                MovePieceToPosition(piece, targetRow, targetCol);
            }
        }
        else
        {
            piece.ReturnToOriginalPosition();
        }

        CheckWinCondition();
    }

    public void OnPieceClicked(PuzzlePiece piece)
    {
    }

    private int GetRowFromPosition(float yPos)
    {
        float normalizedY = (yPos + puzzleContainer.rect.height / 2) / puzzleContainer.rect.height;
        int row = Mathf.RoundToInt((1 - normalizedY) * gridSize);
        return Mathf.Clamp(row, 0, gridSize - 1);
    }

    private int GetColFromPosition(float xPos)
    {
        float normalizedX = (xPos + puzzleContainer.rect.width / 2) / puzzleContainer.rect.width;
        int col = Mathf.RoundToInt(normalizedX * gridSize);
        return Mathf.Clamp(col, 0, gridSize - 1);
    }

    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < gridSize && col >= 0 && col < gridSize;
    }

    private PuzzlePiece GetPieceAtPosition(int row, int col)
    {
        return pieces.Find(p => p.currentRow == row && p.currentCol == col);
    }

    private void SwapPieces(PuzzlePiece piece1, PuzzlePiece piece2)
    {
        int tempRow = piece1.currentRow;
        int tempCol = piece1.currentCol;
        
        piece1.SetCurrentPosition(piece2.currentRow, piece2.currentCol);
        piece2.SetCurrentPosition(tempRow, tempCol);
        
        RectTransform rect1 = piece1.GetComponent<RectTransform>();
        RectTransform rect2 = piece2.GetComponent<RectTransform>();
        
        SetPiecePosition(rect1, piece1.currentRow, piece1.currentCol);
        SetPiecePosition(rect2, piece2.currentRow, piece2.currentCol);
        
        if (PuzzleGameManager.Instance != null)
        {
            PuzzleGameManager.Instance.OnPieceMoved();
        }
    }

    private void MovePieceToPosition(PuzzlePiece piece, int row, int col)
    {
        piece.SetCurrentPosition(row, col);
        RectTransform rectTransform = piece.GetComponent<RectTransform>();
        SetPiecePosition(rectTransform, row, col);
    }

    private void CheckWinCondition()
    {
        bool allCorrect = pieces.All(p => p.IsInCorrectPosition());
        
        if (allCorrect && !isGameComplete)
        {
            isGameComplete = true;
            canMovePieces = false;
            OnGameComplete();
        }
    }

    private void OnGameComplete()
    {
        Debug.Log("恭喜！拼图完成！");
        
        if (PuzzleGameManager.Instance != null)
        {
            PuzzleGameManager.Instance.OnPuzzleComplete();
        }
    }

    public void ResetGame()
    {
        isGameComplete = false;
        canMovePieces = true;
        ShufflePieces();
    }
}
