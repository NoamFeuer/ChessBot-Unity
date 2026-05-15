using UnityEngine;
using UnityEngine.UI;
using System;

public class PawnPromotion : MonoBehaviour
{
    public GameObject promotionPanel;
    public Button queenButton;
    public Button rookButton;
    public Button bishopButton;
    public Button knightButton;

    private Move pendingMove;
    private bool awaitingPromotion = false;
    private Action<Move> onPromotionChosen;

    public bool IsAwaitingPromotion => awaitingPromotion;

    void Awake()
    {
        queenButton .onClick.AddListener(() => Confirm(Piece.Queen));
        rookButton  .onClick.AddListener(() => Confirm(Piece.Rook));
        bishopButton.onClick.AddListener(() => Confirm(Piece.Bishop));
        knightButton.onClick.AddListener(() => Confirm(Piece.Knight));

        promotionPanel.SetActive(false);
    }

    public void Open(Move move, Action<Move> callback)
    {
        pendingMove       = move;
        onPromotionChosen = callback;
        awaitingPromotion = true;
        promotionPanel.SetActive(true);
    }

    private void Confirm(int pieceType)
    {
        awaitingPromotion = false;
        promotionPanel.SetActive(false);

        // Rebuild the move with the chosen promotion type
        Move finalMove = new Move(
            pendingMove.StartSquare,
            pendingMove.TargetSquare,
            pendingMove.CastelingMove,
            pendingMove.EnPassantMove,
            promotionType: pieceType
        );

        onPromotionChosen?.Invoke(finalMove);
    }
}