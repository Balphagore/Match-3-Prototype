using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class NodePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int value;
    public Point index;
    public Vector2 position;
    public RectTransform rectTransform;
    public Image image;
    private bool isUpdating;
    public void Initialize(int value, Point point, Sprite sprite)
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        this.value = value;
        SetIndex(point);
        image.sprite = sprite;
    }
    public void SetIndex(Point point)
    {
        index = point;
        ResetPosition();
        UpdateName();
    }
    public void ResetPosition()
    {
        position = new Vector2(32 + (64 * index.x), -32 - (64 * index.y));
    }
    public void MovePosition(Vector2 move)
    {
        rectTransform.anchoredPosition += move * Time.deltaTime * 16f;
    }
    public void MovePositionTo(Vector2 move)
    {
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, move, Time.deltaTime * 16f);
    }
    public bool UpdatePiece()
    {
        if (Vector3.Distance(rectTransform.anchoredPosition, position) > 1)
        {
            MovePositionTo(position);
            isUpdating = true;
            return true;
        }
        else
        {
            rectTransform.anchoredPosition = position;
            isUpdating = false;
            return false;
        }
    }
    private void UpdateName()
    {
        transform.name = "Node [" + index.x + ", " + index.y + "]";
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isUpdating)
        {
            return;
        }
        MovePieces.instance.MovePiece(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MovePieces.instance.DropPiece();
    }
}