using UnityEngine;
using System.Collections.Generic;

public class MovePieces : MonoBehaviour
{
    Camera cam;
    GameObject draggedPiece;
    int fromIndex;
    List<Move> legalMoves;

    public static Dictionary<int, GameObject> pieceObjects = new Dictionary<int, GameObject>();

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) StartDrag();
        if (Input.GetMouseButton(0))     DragPiece();
        if (Input.GetMouseButtonUp(0))   DropPiece();
    }

    void StartDrag()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;
        if (hit.collider.gameObject.name != "Piece") return;

        draggedPiece = hit.collider.gameObject;
        fromIndex = PositionToIndex(draggedPiece.transform.position);

        // Generate all legal moves and filter to only this piece's moves
        legalMoves = MoveGeneration.GenerateMoves()
            .FindAll(m => m.StartSquare == fromIndex);
    }

    void DragPiece()
    {
        if (draggedPiece == null) return;

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        draggedPiece.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
    }

    void DropPiece()
    {
        if (draggedPiece == null) return;

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        int toIndex = PositionToIndex(mousePos);

        if (toIndex >= 0 && toIndex < 64 && toIndex != fromIndex && IsLegalMove(toIndex))
        {
            if (pieceObjects.ContainsKey(toIndex))
            {
                Destroy(pieceObjects[toIndex]);
                pieceObjects.Remove(toIndex);
            }

            pieceObjects.Remove(fromIndex);
            pieceObjects[toIndex] = draggedPiece;

            Board.Squares[toIndex] = Board.Squares[fromIndex];
            Board.Squares[fromIndex] = Piece.None;

            Board.colorToMove = (Board.colorToMove == Piece.White) ? Piece.Black : Piece.White;

            draggedPiece.transform.position = (Vector3)Board.IndexToPosition(toIndex);
        }
        else
            draggedPiece.transform.position = (Vector3)Board.IndexToPosition(fromIndex);

        draggedPiece = null;
        legalMoves = null;
    }

    bool IsLegalMove(int toIndex)
    {
        return legalMoves != null && legalMoves.Exists(m => m.TargetSquare == toIndex);
    }

    int PositionToIndex(Vector2 worldPos)
    {
        int file = Mathf.RoundToInt(worldPos.x / BoardDrawer.squareSize + 3.5f);
        int rank = Mathf.RoundToInt(worldPos.y / BoardDrawer.squareSize + 3.5f);

        if (file < 0 || file > 7 || rank < 0 || rank > 7) return -1;

        return rank * 8 + file;
    }
}