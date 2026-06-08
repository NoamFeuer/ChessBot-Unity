using System.IO;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public enum PlayerType { Human, Bot }
    public enum Mode { Normal, PerftTesting, PerftTestingInfo }

    public string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public PieceDrawer pieceDrawer;
    public PawnPromotion promotionHandler;
    public Mode mode;
    public int depth = 2;
    public int botDepth = 3;
    public PlayerType white;
    public PlayerType black;

    void Start()
    {
        string modelPath = Path.Combine(Application.streamingAssetsPath, "chess_model.onnx");
        ChessEvaluator.Initialize(modelPath);

        Board.LoadPositionFromFen(fen);

        if (mode == Mode.PerftTestingInfo)
            Perft.PerftDivide(depth);
        else if (mode == Mode.PerftTesting)
            Debug.Log(Perft.PerftCheck(depth));

        pieceDrawer.DrawPieces();
    }

    public void TryApplyMove(Move move)
    {
        if (PromotionHelper.IsPromotionMove(move))
        {
            bool isBot = (Board.colorToMove == Piece.White && white == PlayerType.Bot) ||
                        (Board.colorToMove == Piece.Black && black == PlayerType.Bot);

            if (isBot)
            {
                // Bot always promotes to queen
                Move queenPromotion = new Move(
                    move.StartSquare,
                    move.TargetSquare,
                    move.CastlingMove,
                    move.EnPassantMove,
                    promotionType: Piece.Queen
                );
                Board.MakeMove(queenPromotion);
                AfterMove();
            }
            else
            {
                promotionHandler.Open(move, m =>
                {
                    Board.MakeMove(m);
                    AfterMove();
                });
            }
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

    void OnApplicationQuit()
    {
        ChessEvaluator.Shutdown();
    }
}