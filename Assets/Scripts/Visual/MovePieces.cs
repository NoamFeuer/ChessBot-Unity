using UnityEngine;
using System.Collections.Generic;

public class MovePieces : MonoBehaviour
{
    public static List<Move> legalMoves;
    public AudioSource moveSound;

    Camera cam;
    GameObject draggedPiece;
    int fromIndex;

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

        int index = Board.PositionToIndex(hit.collider.gameObject.transform.position);
        if (!Piece.IsColor(Board.Squares[index], Board.colorToMove)) return;

        draggedPiece = hit.collider.gameObject;
        fromIndex = index;

        legalMoves = MoveGeneration.GenerateLegalMovesForPiece(fromIndex);
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
        int toIndex = Board.PositionToIndex(mousePos);
        Move move = new Move(fromIndex, toIndex);

        if (toIndex >= 0 && toIndex < 64 && toIndex != fromIndex && IsLegalMove(move))
        {
            moveSound.Play();

            if (pieceObjects.ContainsKey(toIndex))
            {
                Destroy(pieceObjects[toIndex]);
                pieceObjects.Remove(toIndex);
            }

            pieceObjects.Remove(fromIndex);
            pieceObjects[toIndex] = draggedPiece;

            Board.MakeMove(move, true);

            draggedPiece.transform.position = (Vector3)Board.IndexToPosition(toIndex);

            CheckGameState();
        }
        else
            draggedPiece.transform.position = (Vector3)Board.IndexToPosition(fromIndex);

        draggedPiece = null;
        legalMoves = null;
    }

    void CheckGameState()
    {
        bool inCheck      = MoveGeneration.IsKingInCheck(Board.colorToMove);
        bool hasLegalMoves = MoveGeneration.GenerateLegalMoves().Count > 0;

        if (!hasLegalMoves)
        {
            if (inCheck)
                Debug.Log("Checkmate! " + (Board.colorToMove == Piece.White ? "Black" : "White") + " wins!");
            else
                Debug.Log("Stalemate!");
        }
        else if (inCheck)
            Debug.Log((Board.colorToMove == Piece.White ? "White" : "Black") + " is in check!");
    }

    public static bool IsLegalMove(Move move)
    {
        return legalMoves != null && legalMoves.Contains(move);
    }
}