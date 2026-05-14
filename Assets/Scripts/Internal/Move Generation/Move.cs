public struct Move
{
    public readonly int StartSquare;
    public readonly int TargetSquare;
    public readonly bool CastelingMove;
    public readonly int MovingPiece;

    public Move(int startSquare, int targetSquare, bool castelingMove = false)
    {
        StartSquare = startSquare;
        TargetSquare = targetSquare;
        CastelingMove = castelingMove;

        MovingPiece = Board.Squares[startSquare];
    }
}