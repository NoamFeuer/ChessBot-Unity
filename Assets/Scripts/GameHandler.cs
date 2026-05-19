using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public enum PlayerType { Human, Bot }

    public string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public PieceDrawer pieceDrawer;
    public PawnPromotion promotionHandler;
    public PlayerType white;
    public PlayerType black;

    void Start()
    {
        Board.LoadPositionFromFen(fen);
        pieceDrawer.DrawPieces();
    }

    public void TryApplyMove(Move move)
    {
        if (PromotionHelper.IsPromotionMove(move))
        {
            promotionHandler.Open(move, m =>
            {
                Board.MakeMove(m);
                AfterMove();
            });
            return;
        }

        Board.MakeMove(move);
        AfterMove();
    }

    void AfterMove()
    {
        pieceDrawer.DrawPieces();

        int? gameState = GameState.CheckGameState();

        if (gameState == -1)
            Debug.Log("Black wins by checkmate!");
        else if (gameState == 1)
            Debug.Log("White wins by checkmate!");
        else if (gameState == 0)
            Debug.Log("Game drawn!");
    }
}