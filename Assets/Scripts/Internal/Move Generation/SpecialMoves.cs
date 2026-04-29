using System.Collections.Generic;

public static class SpecialMoves
{
    static bool whiteCastleKingside  = true;
    static bool whiteCastleQueenside = true;
    static bool blackCastleKingside  = true;
    static bool blackCastleQueenside = true;

    public static List<Move> GetCastlingMoves()
    {
        List<Move> castlingMoves = new List<Move>();

        int kingIndex = Board.colorToMove == Piece.White ? 4 : 60;
        int kingPiece = Piece.King | Board.colorToMove;

        if (Board.Squares[kingIndex] != kingPiece)
            return castlingMoves;

        int rookPiece = Piece.Rook | Board.colorToMove;

        bool castleKingside  = Board.colorToMove == Piece.White ? whiteCastleKingside  : blackCastleKingside;
        bool castleQueenside = Board.colorToMove == Piece.White ? whiteCastleQueenside : blackCastleQueenside;

        // Kingside
        if (castleKingside)
        {
            int rookIndex = kingIndex + 3;
            if (Board.Squares[rookIndex] == rookPiece &&
                Board.Squares[kingIndex + 1] == Piece.None &&
                Board.Squares[kingIndex + 2] == Piece.None &&
                !Board.IsSquareAttacked(kingIndex) &&
                !Board.IsSquareAttacked(kingIndex + 1) &&
                !Board.IsSquareAttacked(kingIndex + 2))
            {
                castlingMoves.Add(new Move(kingIndex, kingIndex + 2));
            }
        }

        // Queenside
        if (castleQueenside)
        {
            int rookIndex = kingIndex - 4;
            if (Board.Squares[rookIndex] == rookPiece &&
                Board.Squares[kingIndex - 1] == Piece.None &&
                Board.Squares[kingIndex - 2] == Piece.None &&
                Board.Squares[kingIndex - 3] == Piece.None &&
                !Board.IsSquareAttacked(kingIndex) &&
                !Board.IsSquareAttacked(kingIndex - 1) &&
                !Board.IsSquareAttacked(kingIndex - 2))
            {
                castlingMoves.Add(new Move(kingIndex, kingIndex - 2));
            }
        }

        return castlingMoves;
    }

    public static void UpdateCastlingRights(Move move)
    {
        // King moved
        if (move.StartSquare == 4)  { whiteCastleKingside = false; whiteCastleQueenside = false; }
        if (move.StartSquare == 60) { blackCastleKingside = false; blackCastleQueenside = false; }

        // White rooks moved or captured
        if (move.StartSquare == 0  || move.TargetSquare == 0)  whiteCastleQueenside = false;
        if (move.StartSquare == 7  || move.TargetSquare == 7)  whiteCastleKingside  = false;

        // Black rooks moved or captured
        if (move.StartSquare == 56 || move.TargetSquare == 56) blackCastleQueenside = false;
        if (move.StartSquare == 63 || move.TargetSquare == 63) blackCastleKingside  = false;
    }
}
