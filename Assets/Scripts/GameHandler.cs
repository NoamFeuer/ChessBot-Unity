using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public enum PlayerType { Human, Bot }

    public string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public PieceDrawer pieceDrawer;
    public PawnPromotion promotionHandler;
    public PlayerType white;
    public PlayerType black;

    private PlayerType CurrentPlayer =>
        Board.colorToMove == Piece.White ? white : black;

    void Start()
    {
        Board.LoadPositionFromFen(fen);
        pieceDrawer.DrawPieces();
    }

    public void TryApplyMove(Move move)
    {
        if (PromotionHelper.IsPromotionMove(move))
        {
            promotionHandler.Open(move, ApplyMove);
            return;
        }

        ApplyMove(move);
    }

    private void ApplyMove(Move move)
    {
        int movingPiece = Board.Squares[move.StartSquare];

        if (move.IsPromotion)
            Board.Squares[move.TargetSquare] = Board.colorToMove | move.PromotionType;
        else
            Board.Squares[move.TargetSquare] = movingPiece;

        Board.Squares[move.StartSquare] = Piece.None;

        if (move.EnPassantMove)
        {
            int direction = (Board.colorToMove == Piece.White) ? -1 : 1;
            Board.Squares[move.TargetSquare + direction * 8] = Piece.None;
        }

        if (move.CastelingMove)
        {
            bool kingside  = move.TargetSquare > move.StartSquare;
            int rookStart  = kingside ? move.StartSquare + 3 : move.StartSquare - 4;
            int rookTarget = kingside ? move.StartSquare + 1 : move.StartSquare - 1;
            Board.Squares[rookTarget] = Board.Squares[rookStart];
            Board.Squares[rookStart]  = Piece.None;
        }

        SpecialMoves.UpdateEnPassant(move);

        Board.colorToMove = (Board.colorToMove == Piece.White) ? Piece.Black : Piece.White;

        pieceDrawer.DrawPieces();
        CheckGameState();
    }

    void CheckGameState()
    {
        bool inCheck       = MoveGeneration.IsKingInCheck(Board.colorToMove);
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
}