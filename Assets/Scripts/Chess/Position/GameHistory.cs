public struct GameHistory
{
    public int     movingPiece;
    public int     capturedPiece;
    public int     capturedPawnSquare;
    public int     enPassantSquare;
    public bool[][] castlingRights;
    public int     halfMoveClock;
    public int     fullMoveNumber;

    public GameHistory(int movingPiece, int capturedPiece, int capturedPawnSquare,
        int enPassantSquare, bool[][] castlingRights, int halfMoveClock, int fullMoveNumber)
    {
        this.movingPiece        = movingPiece;
        this.capturedPiece      = capturedPiece;
        this.capturedPawnSquare = capturedPawnSquare;
        this.enPassantSquare    = enPassantSquare;
        this.halfMoveClock      = halfMoveClock;
        this.fullMoveNumber     = fullMoveNumber;

        this.castlingRights = new bool[2][]
        {
            new bool[] { castlingRights[0][0], castlingRights[0][1] },
            new bool[] { castlingRights[1][0], castlingRights[1][1] }
        };
    }
}