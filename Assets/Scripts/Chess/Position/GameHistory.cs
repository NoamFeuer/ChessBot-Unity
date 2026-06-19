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
        this.castlingRights     = castlingRights;
        this.halfMoveClock      = halfMoveClock;
        this.fullMoveNumber     = fullMoveNumber;
    }
}