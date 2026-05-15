public struct Move
{
    public readonly int StartSquare;
    public readonly int TargetSquare;
    public readonly int MovingPiece;

    public readonly bool CastelingMove;
    public readonly bool EnPassantMove;
    public readonly bool CaptureMove;
    public readonly bool IsPromotion;
    public readonly int PromotionType;

    public Move(int startSquare, int targetSquare, bool castelingMove = false, bool enPassantMove = false, int promotionType = Piece.None)
    {
        StartSquare = startSquare;
        TargetSquare = targetSquare;
        CastelingMove = castelingMove;
        EnPassantMove = enPassantMove;
        PromotionType = promotionType;

        IsPromotion = promotionType != Piece.None;
        CaptureMove = Board.Squares[targetSquare] != Piece.None;
        MovingPiece = Board.Squares[startSquare];
    }
}