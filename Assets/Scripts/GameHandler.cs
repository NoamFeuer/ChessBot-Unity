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

        Position.LoadPositionFromFen(fen);

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
            bool isBot = (Position.colorToMove == Piece.White && white == PlayerType.Bot) ||
                        (Position.colorToMove == Piece.Black && black == PlayerType.Bot);

            if (isBot)
            {
                Move queenPromotion = new Move(
                    move.StartSquare,
                    move.TargetSquare,
                    move.CastlingMove,
                    move.EnPassantMove,
                    promotionType: Piece.Queen
                );
                MoveMaker.MakeMove(queenPromotion);
                AfterMove();
            }
            else
            {
                promotionHandler.Open(move, m =>
                {
                    MoveMaker.MakeMove(m);
                    AfterMove();
                });
            }
            return;
        }

        MoveMaker.MakeMove(move);
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