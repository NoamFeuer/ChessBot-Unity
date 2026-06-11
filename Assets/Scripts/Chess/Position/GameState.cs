using UnityEditor;

public static class GameState
{
    public static int? CheckGameState()
    {
        bool inCheck       = Position.IsKingInCheck(Position.colorToMove);
        bool hasLegalMoves = MoveGeneration.GenerateLegalMoves().Count > 0;

        if (!hasLegalMoves)
        {
            if (inCheck)
                return (Position.colorToMove == Piece.White) ? -1 : 1;
            else
                return 0;
        }

        return null;
    }
}
