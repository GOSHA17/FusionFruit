using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    public const int WIDTH = 5, HEIGHT = 8;
    public const float MULTIPLY_WIDTH = 0.037f, MULTIPLY_HEIGHT = 0.02f;

    [SerializeField] private ArrayLayout _boardLayout;
    [SerializeField] private PointsSystem _pointsSystem;
    [SerializeField] private Game _game;

    [Header("UI Elements")]
    [SerializeField] private Sprite[] _pieces;
    [SerializeField] private RectTransform _gameBoard;
    [SerializeField] private RectTransform _killedBoard;

    [Header("Prefabs")]
    [SerializeField] private GameObject _nodePiece;
    [SerializeField] private GameObject _killedPiece;

    private int[] _fills;
    private Node[,] _board;

    private List<NodePiece> _update;
    private List<FlippedPieces> _flipped;
    private List<NodePiece> _dead;
    private List<KilledPiece> _killed;
    private AudioSource _audioSource;

    private System.Random _random;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        List<NodePiece> finishedUpdating = new List<NodePiece>();

        for (int i = 0; i < _update.Count; i++)
        {
            NodePiece piece = _update[i];
            if (!piece.UpdatePiece()) finishedUpdating.Add(piece);
        }

        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            NodePiece piece = finishedUpdating[i];
            FlippedPieces flip = GetFlipped(piece);
            NodePiece flippedPiece = null;

            int x = (int)piece.index.x;
            _fills[x] = Mathf.Clamp(_fills[x] - 1, 0, WIDTH);

            List<Point> connected = isConnected(piece.index, true);
            bool wasFlipped = (flip != null);

            if (wasFlipped) 
            {
                flippedPiece = flip.GetOtherPiece(piece);
                AddPoints(ref connected, isConnected(flippedPiece.index, true));
            }

            if (connected.Count == 0) 
            {
                if (wasFlipped) 
                    FlipPieces(piece.index, flippedPiece.index, false);
            }
            else 
            {
                _audioSource.Play();
                foreach (Point pnt in connected) 
                {
                    KillPiece(pnt);
                    if (_pointsSystem != null) _pointsSystem.AddPoints(1);
                    Node node = GetNodeAtPoint(pnt);
                    if (_game != null) _game.AddPieceToCount(node.value);
                    NodePiece nodePiece = node.GetPiece();
                    if (nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false);
                        _dead.Add(nodePiece);
                    }
                    node.SetPiece(null);
                }

                ApplyGravityToBoard();
            }

            _flipped.Remove(flip); 
            _update.Remove(piece);
        }
    }

    public Sprite[] GetPieces()
    {
        return _pieces;
    }

    public void ApplyGravityToBoard()
    {
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = (HEIGHT - 1); y >= 0; y--) 
            {
                Point p = new Point(x, y);
                Node node = GetNodeAtPoint(p);
                int val = GetValueAtPoint(p);
                if (val != 0) continue; 
                for (int ny = (y - 1); ny >= -1; ny--)
                {
                    Point next = new Point(x, ny);
                    int nextVal = GetValueAtPoint(next);

                    if (nextVal == 0)
                        continue;

                    if (nextVal != -1)
                    {
                        Node gotten = GetNodeAtPoint(next);
                        NodePiece piece = gotten.GetPiece();

                        node.SetPiece(piece);
                        _update.Add(piece);

                        gotten.SetPiece(null);
                    }
                    else
                    {
                        int newVal = FillPiece();
                        NodePiece piece;
                        Point fallPnt = new Point(x, (-1 - _fills[x]));

                        if (_dead.Count > 0)
                        {
                            NodePiece revived = _dead[0];
                            revived.gameObject.SetActive(true);
                            piece = revived;

                            _dead.RemoveAt(0);
                        }
                        else
                        {
                            GameObject obj = Instantiate(_nodePiece, _gameBoard);
                            NodePiece n = obj.GetComponent<NodePiece>();
                            piece = n;
                        }

                        piece.Initialize(newVal, p, _pieces[newVal - 1]);
                        piece.rect.anchoredPosition = GetPositionFromPoint(fallPnt);

                        Node hole = GetNodeAtPoint(p);
                        hole.SetPiece(piece);
                        ResetPiece(piece);
                        _fills[x]++;
                    }
                    break;
                }
            }
        }
    }

    private FlippedPieces GetFlipped(NodePiece p)
    {
        FlippedPieces flip = null;
        for (int i = 0; i < _flipped.Count; i++)
        {
            if (_flipped[i].GetOtherPiece(p) != null)
            {
                flip = _flipped[i];
                break;
            }
        }
        return flip;
    }

    private void StartGame()
    {
        //WIDTH = Screen.width / 64;
        //HEIGHT = Screen.height / 64;
        _fills = new int[WIDTH];
        string seed = GetRandomSeed();
        //_random = new System.Random(seed.GetHashCode());
        _random = new System.Random();
        _update = new List<NodePiece>();
        _flipped = new List<FlippedPieces>();
        _dead = new List<NodePiece>();
        _killed = new List<KilledPiece>();

        InitializeBoard();
        VerifyBoard();
        InstantiateBoard();
    }

    private void InitializeBoard()
    {
        _board = new Node[WIDTH, HEIGHT];
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                _board[x, y] = new Node((_boardLayout.rows[y].row[x]) ? -1 : FillPiece(), new Point(x, y));
            }
        }
    }

    private void VerifyBoard()
    {
        List<int> remove;
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                Point p = new Point(x, y);
                int val = GetValueAtPoint(p);
                if (val <= 0) continue;

                remove = new List<int>();
                while (isConnected(p, true).Count > 0)
                {
                    val = GetValueAtPoint(p);
                    if (!remove.Contains(val))
                        remove.Add(val);
                    SetValueAtPoint(p, NewValue(ref remove));
                }
            }
        }
    }

    private void InstantiateBoard()
    {
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                Node node = GetNodeAtPoint(new Point(x, y));

                int val = node.value;
                if (val <= 0) continue;

                GameObject p = Instantiate(_nodePiece, _gameBoard);
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(Screen.width * MULTIPLY_WIDTH + (72 * x), -MULTIPLY_HEIGHT * Screen.height - (72 * y));
                piece.Initialize(val, new Point(x, y), _pieces[val - 1]);
                node.SetPiece(piece);
            }
        }
    }

    public void ResetPiece(NodePiece piece)
    {
        piece.ResetPosition();
        _update.Add(piece);
    }

    public void FlipPieces(Point one, Point two, bool main)
    {
        if (GetValueAtPoint(one) < 0) return;

        Node nodeOne = GetNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.GetPiece();

        if (GetValueAtPoint(two) > 0)
        {
            Node nodeTwo = GetNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.GetPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            if (main)
                _flipped.Add(new FlippedPieces(pieceOne, pieceTwo));

            _update.Add(pieceOne);
            _update.Add(pieceTwo);
        }
        else
        {
            ResetPiece(pieceOne);
        }
    }

    private void KillPiece(Point p)
    {
        List<KilledPiece> available = new List<KilledPiece>();

        for (int i = 0; i < _killed.Count; i++)
            if (!_killed[i].falling) 
                available.Add(_killed[i]);

        KilledPiece set = null;

        if (available.Count > 0)
        {
            set = available[0];
        }
        else
        {
            GameObject kill = Instantiate(_killedPiece, _killedBoard);
            KilledPiece kPiece = kill.GetComponent<KilledPiece>();
            set = kPiece;
            _killed.Add(kPiece);
        }

        int val = GetValueAtPoint(p) - 1;

        if (set != null && val >= 0 && val < _pieces.Length)
            set.Initialize(_pieces[val], GetPositionFromPoint(p));
    }

    private List<Point> isConnected(Point p, bool main)
    {
        List<Point> connected = new List<Point>();
        int val = GetValueAtPoint(p);
        Point[] directions =
        {
            Point.Up,
            Point.Right,
            Point.Down,
            Point.Left
        };

        foreach (Point dir in directions) 
        {
            List<Point> line = new List<Point>();

            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.Add(p, Point.Multiply(dir, i));
                if (GetValueAtPoint(check) == val)
                {
                    line.Add(check);
                    same++;
                }
            }

            if (same > 1) 
                AddPoints(ref connected, line); 
        }

        for (int i = 0; i < 2; i++) 
        {
            List<Point> line = new List<Point>();

            int same = 0;
            Point[] check = { Point.Add(p, directions[i]), Point.Add(p, directions[i + 2]) };

            foreach (Point next in check) 
            {
                if (GetValueAtPoint(next) == val)
                {
                    line.Add(next);
                    same++;
                }
            }

            if (same > 1)
                AddPoints(ref connected, line);
        }

        for (int i = 0; i < 4; i++)
        {
            List<Point> square = new List<Point>();

            int same = 0;
            int next = i + 1;
            if (next >= 4)
                next -= 4;

            Point[] check = { Point.Add(p, directions[i]), Point.Add(p, directions[next]), Point.Add(p, Point.Add(directions[i], directions[next])) };

            foreach (Point pnt in check)
            {
                if (GetValueAtPoint(pnt) == val)
                {
                    square.Add(pnt);
                    same++;
                }
            }

            if (same > 2)
                AddPoints(ref connected, square);
        }

        if (main)
        {
            for (int i = 0; i < connected.Count; i++)
                AddPoints(ref connected, isConnected(connected[i], false));
        }

        return connected;
    }

    private void AddPoints(ref List<Point> points, List<Point> add)
    {
        foreach (Point p in add)
        {
            bool doAdd = true;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Equals(p))
                {
                    doAdd = false;
                    break;
                }
            }

            if (doAdd) 
                points.Add(p);
        }
    }

    private int FillPiece()
    {
        //return (_random.Next(0, 100) / (100 / _pieces.Length)) + 1;
        return _random.Next(1, _pieces.Length);
    }

    private int GetValueAtPoint(Point p)
    {
        if (p.x < 0 || p.x >= WIDTH || p.y < 0 || p.y >= HEIGHT) return -1;
        return _board[p.x, p.y].value;
    }

    private void SetValueAtPoint(Point p, int v)
    {
        _board[p.x, p.y].value = v;
    }

    private Node GetNodeAtPoint(Point p)
    {
        return _board[p.x, p.y];
    }

    private int NewValue(ref List<int> remove)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < _pieces.Length; i++)
            available.Add(i + 1);

        foreach (int i in remove)
            available.Remove(i);

        if (available.Count <= 0) return 0;

        return available[_random.Next(0, available.Count)];
    }

    private string GetRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdeghijklmnopqrstuvwxyz1234567890!@#$%^&*()";

        for (int i = 0; i < 20; i++)
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];

        return seed;
    }

    public Vector2 GetPositionFromPoint(Point p)
    {
        return new Vector2(Screen.width * MULTIPLY_WIDTH + (72 * p.x), -MULTIPLY_HEIGHT * Screen.height - (72 * p.y));
    }
}

[System.Serializable]
public class Node
{
    public int value;
    public Point index;
    private NodePiece piece;

    public Node(int v, Point i)
    {
        value = v;
        index = i;
    }

    public void SetPiece(NodePiece p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null) return;
        piece.SetIndex(index);
    }

    public NodePiece GetPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class FlippedPieces
{
    public NodePiece one;
    public NodePiece two;

    public FlippedPieces(NodePiece o, NodePiece t)
    {
        one = o; two = t;
    }

    public NodePiece GetOtherPiece(NodePiece p)
    {
        if (p == one)
            return two;
        else if (p == two)
            return one;
        else
            return null;
    }
}