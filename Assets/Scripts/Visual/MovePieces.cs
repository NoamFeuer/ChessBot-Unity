using UnityEngine;
using System.Collections.Generic;

public class MovePieces : MonoBehaviour
{
    public static List<Move> legalMoves;
    public AudioSource moveSound;
    public GameHandler gameHandler; // <-- assign in inspector

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
        if (gameHandler.promotionHandler.IsAwaitingPromotion) return; // block input during promotion menu

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
        Move inputMove = new Move(fromIndex, toIndex);

        Move? matchedMove = FindLegalMove(inputMove);

        if (toIndex >= 0 && toIndex < 64 && toIndex != fromIndex && matchedMove != null)
        {
            moveSound.Play();

            draggedPiece.transform.position = (Vector3)Board.IndexToPosition(toIndex);

            // Route through GameHandler instead of Board.MakeMove directly
            gameHandler.TryApplyMove(matchedMove.Value);

            // CheckGameState is now called from GameHandler after move is applied
        }
        else
            draggedPiece.transform.position = (Vector3)Board.IndexToPosition(fromIndex);

        draggedPiece = null;
        legalMoves = null;
    }

    static Move? FindLegalMove(Move input)
    {
        if (legalMoves == null) return null;

        foreach (Move legal in legalMoves)
        {
            if (legal.StartSquare == input.StartSquare &&
                legal.TargetSquare == input.TargetSquare)
                return legal;
        }

        return null;
    }
}