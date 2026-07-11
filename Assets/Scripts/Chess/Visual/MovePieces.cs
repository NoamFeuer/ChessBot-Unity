using UnityEngine;
using System.Collections.Generic;

public class MovePieces : MonoBehaviour
{
    public static List<Move> legalMoves;
    public AudioSource moveSound;
    public GameHandler gameHandler;

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

        int index = Position.PositionToIndex(hit.collider.gameObject.transform.position);
        if (!Piece.IsColor(Position.Squares[index], Position.colorToMove)) return;

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
        int toIndex = Position.PositionToIndex(mousePos);
        Move inputMove = new Move(fromIndex, toIndex);

        Move? matchedMove = FindLegalMove(inputMove);

        if (toIndex >= 0 && toIndex < 64 && toIndex != fromIndex && matchedMove != null)
        {
            moveSound.Play();

            draggedPiece.transform.position = (Vector3)Position.IndexToPosition(toIndex);

            // Route through GameHandler instead of MoveMaker.MakeMove directly
            gameHandler.TryApplyMove(matchedMove.Value);
        }
        else
            draggedPiece.transform.position = (Vector3)Position.IndexToPosition(fromIndex);

        draggedPiece = null;
        legalMoves = null;
    }

   static Move? FindLegalMove(Move input)
{
    if (legalMoves == null) return null;

    foreach (Move legal in legalMoves)
    {
        UnityEngine.Debug.Log($"Legal: {legal.StartSquare}->{legal.TargetSquare} castling:{legal.CastlingMove}");
        if (legal.StartSquare == input.StartSquare &&
            legal.TargetSquare == input.TargetSquare)
            return legal;
    }

    UnityEngine.Debug.Log($"No match for {input.StartSquare}->{input.TargetSquare}");
    return null;
}
}
