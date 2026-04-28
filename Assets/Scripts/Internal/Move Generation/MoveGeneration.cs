using System.Collections.Generic;

public static class MoveGeneration
{
    static List<Move> moves;

    public static List<Move> GenerateMoves()
    {
        moves = new List<Move>();

        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            int piece = Board.Squares[startSquare];
            if (Piece.IsColor(piece, Board.colorToMove))
            {
                if (Piece.IsSlidingPiece(piece))
                    GenerateSlidingMoves(startSquare, piece);
            }
        }

        return moves;
    }

    static void GenerateSlidingMoves(int startSquare, int piece)
    {
        int friendlyColor = Board.colorToMove;
        int oppositeColor = (friendlyColor == Piece.White) ? Piece.Black : Piece.White;

        int startDirIndex = Piece.IsType(piece, Piece.Bishop) ? 4 : 0;
        int endDirIndex   = Piece.IsType(piece, Piece.Rook)   ? 4 : 8;

        for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
        {
            for (int n = 0; n < PrecomputedMoveData.NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + PrecomputedMoveData.DirectionOffsets[directionIndex] * (n + 1);
                int pieceOnTargetSquare = Board.Squares[targetSquare];

                if (Piece.IsColor(pieceOnTargetSquare, friendlyColor))
                    break;

                moves.Add(new Move(startSquare, targetSquare));

                if (Piece.IsColor(pieceOnTargetSquare, oppositeColor))
                    break;
            }
        }
    }
}