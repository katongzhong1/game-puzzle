using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public int correctRow;
    public int correctCol;
    public int currentRow;
    public int currentCol;
    public PuzzleGame puzzleGame;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private bool isDragging;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetCorrectPosition(int row, int col)
    {
        correctRow = row;
        correctCol = col;
    }

    public void SetCurrentPosition(int row, int col)
    {
        currentRow = row;
        currentCol = col;
    }

    public bool IsInCorrectPosition()
    {
        return currentRow == correctRow && currentCol == correctCol;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (puzzleGame != null && !puzzleGame.canMovePieces)
            return;
        
        isDragging = true;
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
        
        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
        
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
        
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        
        if (puzzleGame != null)
        {
            puzzleGame.OnPieceDropped(this);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (puzzleGame != null)
        {
            puzzleGame.OnPieceClicked(this);
        }
    }

    public void ReturnToOriginalPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
        transform.SetParent(originalParent, true);
    }

    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }
}
