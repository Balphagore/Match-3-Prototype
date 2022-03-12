using UnityEngine;
public class MovePieces : MonoBehaviour
{
    public static MovePieces instance;
    private Match3 game;
    private NodePiece moving;
    private Point newIndex;
    private Vector2 mouseStart;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        game = GetComponent<Match3>();
    }
    private void Update()
    {
        if (moving != null)
        {
            Vector2 direction = ((Vector2)Input.mousePosition - mouseStart);
            Vector2 normalizedDirection = direction.normalized;
            Vector2 absoluteDirection = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
            newIndex = Point.Clone(moving.index);
            Point add = Point.Zero;
            if (direction.magnitude > 32)
            {
                if (absoluteDirection.x > absoluteDirection.y)
                {
                    add = (new Point((normalizedDirection.x > 0) ? 1 : -1, 0));
                }
                else
                {
                    add = (new Point(0, (normalizedDirection.y > 0) ? -1 : 1));
                }
            }
            newIndex.Add(add);
            Vector2 position = game.GetPositionFromPoint(moving.index);
            if (!newIndex.Equals(moving.index))
            {
                position += Point.Multiple(new Point(add.x,-add.y), 16).ToVector();
            }
            moving.MovePositionTo(position);
        }
    }
    public void MovePiece(NodePiece piece)
    {
        if (moving != null)
        {
            return;
        }
        moving = piece;
        mouseStart = Input.mousePosition;
    }
    public void DropPiece()
    {
        if (moving == null)
        {
            return;
        }
        Debug.Log("Dropped");
        if (!newIndex.Equals(moving.index))
        {
            game.FlipPieces(moving.index, newIndex,true);
        }
        else
        {
            game.ResetPiece(moving);
        }
        moving = null;
    }
}