using UnityEngine;
using System;
using System.Collections.Generic;
public class Match3 : MonoBehaviour
{
    [SerializeField]
    private int columns;
    [SerializeField]
    private int rows;
    [Header("UI elements")]
    [SerializeField]
    private RectTransform gameBoard;
    [SerializeField]
    private RectTransform killedBoard;
    [Header("Prefabs")]
    [SerializeField]
    private GameObject nodePiecePrefab;
    [SerializeField]
    private GameObject killedPiecePrefab;
    [SerializeField]
    public Sprite[] pieces;
    [SerializeField]
    private ArrayLayout arrayLayout;
    private int[] fills;
    private Node[,] board;
    private System.Random random;
    private List<NodePiece> update;
    private List<FlippedPieces> flipped;
    private List<NodePiece> deadPieces;
    private List<KilledPiece> killedPieces;
    private void Start()
    {
        StartGame();
    }
    private void Update()
    {
        List<NodePiece> finishedUpdating = new List<NodePiece>();
        for (int i = 0; i < update.Count; i++)
        {
            NodePiece piece = update[i];
            if (!piece.UpdatePiece())
            {
                finishedUpdating.Add(piece);
            }
        }
        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            NodePiece piece = finishedUpdating[i];
            FlippedPieces flip = GetFlipped(piece);
            NodePiece flippedPiece = null;
            int x = (int)piece.index.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, columns);
            List<Point> connected = IsConnected(piece.index, true);
            bool wasFlipped = (flip != null);
            if (wasFlipped)
            {
                flippedPiece = flip.GetOtherPiece(piece);
                AddPoints(ref connected, IsConnected(flippedPiece.index, true));
            }
            if (connected.Count == 0)
            {
                if (wasFlipped)
                {
                    FlipPieces(piece.index, flippedPiece.index, false);
                }
            }
            else
            {
                foreach (Point point in connected)
                {
                    KillPiece(point);
                    Node node = GetNodeAtPoint(point);
                    NodePiece nodePiece = node.GetPiece();
                    if (nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false);
                        deadPieces.Add(nodePiece);
                    }
                    node.SetPiece(null);
                }
                ApplyGravityToBoard();
            }
            flipped.Remove(flip);
            update.Remove(piece);
        }
    }
    private void ApplyGravityToBoard()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = (rows - 1); y >= 0; y--)
            {
                Point point = new Point(x, y);
                Node node = GetNodeAtPoint(point);
                int value = GetValueAtPoint(point);
                if (value != 0)
                {
                    continue; //if it is not a hole, do nothing
                }
                for (int ny = (y - 1); ny >= -1; ny--)
                {
                    Point next = new Point(x, ny);
                    int nextValue = GetValueAtPoint(next);
                    if (nextValue == 0)
                    {
                        continue;
                    }
                    if (nextValue != -1) //if we did not hit an end, but its not 0 then use this to fill the current hole
                    {
                        Node got = GetNodeAtPoint(next);
                        NodePiece piece = got.GetPiece();
                        //set the hole
                        node.SetPiece(piece);
                        update.Add(piece);
                        //make a new the hole
                        got.SetPiece(null);
                    }
                    else
                    {
                        //fill in the hole
                        int newValue = FillPiece();
                        NodePiece piece;
                        Point fallPoint = new Point(x, (-1 - fills[x]));
                        if (deadPieces.Count > 0)
                        {
                            NodePiece revived = deadPieces[0];
                            revived.gameObject.SetActive(true);
                            piece = revived;
                            deadPieces.RemoveAt(0);
                        }
                        else
                        {
                            GameObject instance = Instantiate(nodePiecePrefab, gameBoard);
                            NodePiece nodePiece = instance.GetComponent<NodePiece>();
                            piece = nodePiece;
                        }
                        piece.Initialize(newValue, point, pieces[newValue - 1]);
                        piece.rectTransform.anchoredPosition = GetPositionFromPoint(fallPoint);
                        Node hole = GetNodeAtPoint(point);
                        hole.SetPiece(piece);
                        ResetPiece(piece);
                        fills[x]++;
                    }
                    break;
                }
            }
        }
    }
    private FlippedPieces GetFlipped(NodePiece nodePiece)
    {
        FlippedPieces flip = null;
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].GetOtherPiece(nodePiece) != null)
            {
                flip = flipped[i];
                break;
            }
        }
        return flip;
    }
    private void StartGame()
    {
        fills = new int[columns];
        string seed = GetRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<NodePiece>();
        flipped = new List<FlippedPieces>();
        deadPieces = new List<NodePiece>();
        killedPieces = new List<KilledPiece>();
        InitializeGameBoard();
        VerifyBoard();
        InstantiateBoard();
    }
    private void InitializeGameBoard()
    {
        board = new Node[columns, rows];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                board[x, y] = new Node(arrayLayout.rows[y].row[x] ? -1 : FillPiece(), new Point(x, y));
            }
        }
    }
    private void VerifyBoard()
    {
        List<int> remove;
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Point point = new Point(x, y);
                int value = GetValueAtPoint(point);
                if (value <= 0)
                {
                    continue;
                }
                remove = new List<int>();
                while (IsConnected(point, true).Count > 0)
                {
                    value = GetValueAtPoint(point);
                    if (!remove.Contains(value))
                    {
                        remove.Add(value);
                    }
                    SetValueAtPoint(point, NewValue(ref remove));
                }
            }
        }
    }
    private void InstantiateBoard()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Node node = GetNodeAtPoint(new Point(x, y));
                int value = node.value;
                if (value <= 0)
                {
                    continue;
                }
                GameObject point = Instantiate(nodePiecePrefab, gameBoard);
                NodePiece nodePiece = point.GetComponent<NodePiece>();
                RectTransform rectTransform = point.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
                nodePiece.Initialize(value, new Point(x, y), pieces[value - 1]);
                node.SetPiece(nodePiece);
            }
        }
    }
    public void ResetPiece(NodePiece piece)
    {
        piece.ResetPosition();
        update.Add(piece);
    }
    public void FlipPieces(Point one, Point two, bool isMain)
    {
        if (GetValueAtPoint(one) < 0)
        {
            return;
        }
        Node nodeOne = GetNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.GetPiece();
        if (GetValueAtPoint(two) > 0)
        {
            Node nodeTwo = GetNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.GetPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);
            if (isMain)
            {
                flipped.Add(new FlippedPieces(pieceOne, pieceTwo));
            }
            update.Add(pieceOne);
            update.Add(pieceTwo);
        }
        else
        {
            ResetPiece(pieceOne);
        }
    }
    private void KillPiece(Point point)
    {
        List<KilledPiece> available = new List<KilledPiece>();
        for (int i = 0; i < killedPieces.Count; i++)
        {
            if (!killedPieces[i].isFalling)
            {
                available.Add(killedPieces[i]);
            }
        }
        KilledPiece set = null;
        if (available.Count > 0)
        {
            set = available[0];
        }
        else
        {
            GameObject kill = Instantiate(killedPiecePrefab, killedBoard);
            KilledPiece killedPiece = kill.GetComponent<KilledPiece>();
            set = killedPiece;
            killedPieces.Add(killedPiece);
        }
        int value = GetValueAtPoint(point) - 1;
        if (set != null && value >= 0 && value < pieces.Length)
        {
            set.Initialize(pieces[value], GetPositionFromPoint(point));
        }
    }
    private List<Point> IsConnected(Point point, bool isMain)
    {
        List<Point> connectedPoints = new List<Point>();
        int value = GetValueAtPoint(point);
        Point[] directions =
        {
            Point.Up,
            Point.Right,
            Point.Down,
            Point.Left
        };
        foreach (Point direction in directions)
        {
            List<Point> line = new List<Point>();
            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.Add(point, Point.Multiple(direction, i));
                if (GetValueAtPoint(check) == value)
                {
                    line.Add(check);
                    same++;
                }
            }
            if (same > 1)
            {
                AddPoints(ref connectedPoints, line);
            }
        }
        for (int i = 0; i < 2; i++)
        {
            List<Point> line = new List<Point>();
            int same = 0;
            Point[] check = { Point.Add(point, directions[i]), Point.Add(point, directions[i + 2]) };
            foreach (Point next in check)
            {
                if (GetValueAtPoint(next) == value)
                {
                    line.Add(next);
                    same++;
                }
            }
            if (same > 1)
            {
                AddPoints(ref connectedPoints, line);
            }
        }
        for (int i = 0; i < 4; i++)
        {
            List<Point> square = new List<Point>();
            int same = 0;
            int next = i + 1;
            if (next >= 4)
            {
                next -= 4;
            }
            Point[] check = { Point.Add(point, directions[i]), Point.Add(point, directions[next]), Point.Add(point, Point.Add(directions[i], directions[next])) };
            foreach (Point nextPoint in check)
            {
                if (GetValueAtPoint(nextPoint) == value)
                {
                    square.Add(nextPoint);
                    same++;
                }
            }
            if (same > 2)
            {
                AddPoints(ref connectedPoints, square);
            }
        }
        if (isMain)
        {
            for (int i = 0; i < connectedPoints.Count; i++)
            {
                AddPoints(ref connectedPoints, IsConnected(connectedPoints[i], false));
            }
        }
        /*if (connectedPoints.Count > 0)
        {
            connectedPoints.Add(point);
        }*/
        return connectedPoints;
    }
    private void AddPoints(ref List<Point> points, List<Point> addedPoints)
    {
        foreach (Point point in addedPoints)
        {
            bool isAdding = true;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Equals(point))
                {
                    isAdding = false;
                    break;
                }
            }
            if (isAdding)
            {
                points.Add(point);
            }
        }
    }
    private int FillPiece()
    {
        int value = 1;
        value = (random.Next(0, 100) / (100 / pieces.Length)) + 1;
        return value;
    }
    private int GetValueAtPoint(Point point)
    {
        if (point.x < 0 || point.x >= columns || point.y < 0 || point.y >= rows)
        {
            return -1;
        }
        return board[point.x, point.y].value;
    }
    private void SetValueAtPoint(Point point, int value)
    {
        board[point.x, point.y].value = value;
    }
    private Node GetNodeAtPoint(Point point)
    {
        return board[point.x, point.y];
    }
    private int NewValue(ref List<int> remove)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < pieces.Length; i++)
        {
            available.Add(i + 1);
        }
        foreach (int i in remove)
        {
            available.Remove(i);
        }
        if (available.Count <= 0)
        {
            return 0;
        }
        return available[random.Next(0, available.Count)];
    }
    private string GetRandomSeed()
    {
        string seed = "";
        string acceptableCaracters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
        for (int i = 0; i < 20; i++)
        {
            seed += acceptableCaracters[UnityEngine.Random.Range(0, acceptableCaracters.Length)];
        }
        return seed;
    }
    public Vector2 GetPositionFromPoint(Point point)
    {
        return new Vector2(32 + (64 * point.x), -32 - (64 * point.y));
    }
}
[Serializable]
public class Node
{
    public int value;
    public Point index;
    private NodePiece piece;
    public Node(int value, Point index)
    {
        this.value = value;
        this.index = index;
    }
    public void SetPiece(NodePiece nodePiece)
    {
        piece = nodePiece;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null)
        {
            return;
        }
        piece.SetIndex(index);
    }
    public NodePiece GetPiece()
    {
        return piece;
    }
}
[Serializable]
public class FlippedPieces
{
    public NodePiece one;
    public NodePiece two;
    public FlippedPieces(NodePiece one, NodePiece two)
    {
        this.one = one;
        this.two = two;
    }
    public NodePiece GetOtherPiece(NodePiece nodePiece)
    {
        if (nodePiece == one)
        {
            return two;
        }
        else
        {
            if (nodePiece == two)
            {
                return one;
            }
            else
            {
                return null;
            }
        }
    }
}