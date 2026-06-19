using System.Collections;
using System.IO;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    [Header("References")]
    public GameHandler gameHandler;

    private bool isBotThinking = false;

    void Start()
    {
        string modelPath = Path.Combine(Application.streamingAssetsPath, "chess_model.onnx");
        ChessEvaluator.Initialize(modelPath);

        string bookPath = Path.Combine(Application.streamingAssetsPath, "opening_book.json");
        OpeningBook.Initialize(bookPath);
    }

    void Update()
    {
        if (isBotThinking) return;
        if (!IsCurrentPlayerBot()) return;
        if (GameState.CheckGameState() != null) return;
        if (gameHandler.promotionHandler.IsAwaitingPromotion) return;

        StartCoroutine(MakeBotMove());
    }

    bool IsCurrentPlayerBot()
    {
        if (Position.colorToMove == Piece.White)
            return gameHandler.white == GameHandler.PlayerType.Bot;
        else
            return gameHandler.black == GameHandler.PlayerType.Bot;
    }

    IEnumerator MakeBotMove()
    {
        isBotThinking = true;
        yield return null;

        Move best = default;

        string fen = Position.GetFen();
        Debug.Log($"Looking up FEN: {fen}");

        if (OpeningBook.TryGetMove(fen, out string bookMove))
        {
            Debug.Log($"Book move found: {bookMove}");
            best = ParseUciMove(bookMove);
        }
        else
        {
            Debug.Log("No book move found, using engine");
            best = MiniMax.GetBestMove(gameHandler.botDepth);
        }

        if (best.StartSquare != best.TargetSquare)
            gameHandler.TryApplyMove(best);

        isBotThinking = false;
    }

    Move ParseUciMove(string uci)
    {
        int from = SquareIndex(uci.Substring(0, 2));
        int to   = SquareIndex(uci.Substring(2, 2));

        int promotionType = Piece.None;
        if (uci.Length == 5)
        {
            promotionType = uci[4] switch
            {
                'q' => Piece.Queen,
                'r' => Piece.Rook,
                'b' => Piece.Bishop,
                'n' => Piece.Knight,
                _   => Piece.None
            };
        }

        return new Move(from, to, promotionType: promotionType);
    }

    int SquareIndex(string algebraic)
    {
        int file = algebraic[0] - 'a';
        int rank = algebraic[1] - '1';
        return rank * 8 + file;
    }
}