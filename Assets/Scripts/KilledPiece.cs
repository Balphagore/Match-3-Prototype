using UnityEngine;
using UnityEngine.UI;
public class KilledPiece : MonoBehaviour
{
    public bool isFalling;
    private float speed = 16f;
    private float gravity = 32f;
    private Vector2 moveDirection;
    private RectTransform rectTransform;
    private Image image;
    public void Initialize(Sprite piece, Vector2 start)
    {
        isFalling = true;
        moveDirection = Vector2.up;
        moveDirection.x = Random.Range(-1.0f, 1.0f);
        moveDirection *= speed / 2;
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        image.sprite = piece;
        rectTransform.anchoredPosition = start;
    }
    private void Update()
    {
        if (!isFalling)
        {
            return;
        }
        moveDirection.y -= Time.deltaTime * gravity;
        moveDirection.x = Mathf.Lerp(moveDirection.x, 0, Time.deltaTime);
        rectTransform.anchoredPosition += moveDirection * Time.deltaTime * speed;
        if (rectTransform.position.x < -64f || rectTransform.position.x > Screen.width+64f|| rectTransform.position.y < -64f || rectTransform.position.y > Screen.height+64f)
        {
            isFalling = false;
        }
    }
}