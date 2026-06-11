using System.Collections;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    [Header("References")]
    public GameHandler gameHandler;

    private bool isBotThinking = false;

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

        Move best = NegaMax.GetBestMove(gameHandler.botDepth);

        if (best.StartSquare != best.TargetSquare)
            gameHandler.TryApplyMove(best);

        isBotThinking = false;
    }
}