using UnityEngine;

public class MovePieces : MonoBehaviour
{
    public static MovePieces instance;

    private Match3 _game;
    private NodePiece _moving;
    private Point _newIndex;
    private Vector2 _mouseStart;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _game = GetComponent<Match3>();
    }

    private void Update()
    {
        if (_moving != null)
        {
            Vector2 dir = ((Vector2)Input.mousePosition - _mouseStart);
            Vector2 nDir = dir.normalized;
            Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

            _newIndex = Point.Clone(_moving.index);
            Point add = Point.Zero;

            if (dir.magnitude > 32)
            {
                if (aDir.x > aDir.y)
                    add = (new Point((nDir.x > 0) ? 1 : -1, 0));
                else if (aDir.y > aDir.x)
                    add = (new Point(0, (nDir.y > 0) ? -1 : 1));
            }

            _newIndex.Add(add);

            Vector2 pos = _game.GetPositionFromPoint(_moving.index);
            if (!_newIndex.Equals(_moving.index))
                pos += Point.Multiply(new Point(add.x, -add.y), 16).ToVector();
            _moving.MovePositionTo(pos);
        }
    }

    public void MovePiece(NodePiece piece)
    {
        if (_moving != null) return;
        _moving = piece;
        _mouseStart = Input.mousePosition;
    }

    public void DropPiece()
    {
        if (_moving == null) return;
        
        if (!_newIndex.Equals(_moving.index))
            _game.FlipPieces(_moving.index, _newIndex, true);
        else
            _game.ResetPiece(_moving);

        _moving = null;
    }
}