public struct GameHistory
{
    public int movingPiece;
    public int capturedPiece;
    public int capturedPawnSquare;
    public int enPassantSquare;
    public bool[][] castlingRights;

    public GameHistory(int moving, int captured, int capturedPawnSq, int epSquare, bool[][] rights)
    {
        movingPiece        = moving;
        capturedPiece      = captured;
        capturedPawnSquare = capturedPawnSq;
        enPassantSquare    = epSquare;

        castlingRights = new bool[2][]
        {
            new bool[] { rights[0][0], rights[0][1] },
            new bool[] { rights[1][0], rights[1][1] }
        };
    }
}
