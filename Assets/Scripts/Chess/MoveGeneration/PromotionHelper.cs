public static class PromotionHelper
{
    public static bool IsPromotionMove(Move move)
    {
        if (!Piece.IsType(move.MovingPiece, Piece.Pawn)) return false;

        int targetRank = move.TargetSquare / 8;
        return targetRank == 7 || targetRank == 0;
    }
}
