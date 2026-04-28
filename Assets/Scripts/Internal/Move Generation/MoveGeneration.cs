using System.Collections.Generic;

public static class MoveGeneration
{
    static List<Move> moves;
    static int friendlyColor;
    static int oppositeColor;

    public static List<Move> GenerateMoves()
    {
        moves = new List<Move>();
        friendlyColor = Board.colorToMove;
        oppositeColor = (friendlyColor == Piece.White) ? Piece.Black : Piece.White;

        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            int piece = Board.Squares[startSquare];
            if (!Piece.IsColor(piece, friendlyColor)) continue;

            if (Piece.IsSlidingPiece(piece))
                GenerateSlidingMoves(startSquare, piece);
            else if (Piece.IsType(piece, Piece.Knight))
                GenerateKnightMoves(startSquare);
            else if (Piece.IsType(piece, Piece.King))
                GenerateKingMoves(startSquare);
            else if (Piece.IsType(piece, Piece.Pawn))
                GeneratePawnMoves(startSquare);
        }

        return moves;
    }

    static void GenerateSlidingMoves(int startSquare, int piece)
    {
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

    static void GenerateKnightMoves(int startSquare)
    {
        int file = startSquare % 8;
        int rank = startSquare / 8;

        foreach (int jump in PrecomputedMoveData.knightJumps)
        {
            int targetSquare = startSquare + jump;

            if (targetSquare < 0 || targetSquare >= 64) continue;

            // Make sure it didn't wrap around the board
            int targetFile = targetSquare % 8;
            int targetRank = targetSquare / 8;
            int fileDiff = System.Math.Abs(targetFile - file);
            int rankDiff = System.Math.Abs(targetRank - rank);
            bool validJump = (fileDiff == 2 && rankDiff == 1) || (fileDiff == 1 && rankDiff == 2);
            if (!validJump) continue;

            int pieceOnTarget = Board.Squares[targetSquare];
            if (Piece.IsColor(pieceOnTarget, friendlyColor)) continue;

            moves.Add(new Move(startSquare, targetSquare));
        }
    }

    static void GenerateKingMoves(int startSquare)
    {
        // King moves in all 8 directions, but only 1 square
        for (int directionIndex = 0; directionIndex < 8; directionIndex++)
        {
            if (PrecomputedMoveData.NumSquaresToEdge[startSquare][directionIndex] == 0) continue;

            int targetSquare = startSquare + PrecomputedMoveData.DirectionOffsets[directionIndex];
            int pieceOnTarget = Board.Squares[targetSquare];

            if (Piece.IsColor(pieceOnTarget, friendlyColor)) continue;

            moves.Add(new Move(startSquare, targetSquare));
        }
    }

    static void GeneratePawnMoves(int startSquare)
    {
        int direction  = (friendlyColor == Piece.White) ? 1 : -1;
        int startRank  = (friendlyColor == Piece.White) ? 1 : 6;
        int file = startSquare % 8;
        int rank = startSquare / 8;

        // One square forward
        int oneForward = startSquare + 8 * direction;
        if (oneForward >= 0 && oneForward < 64 && Board.Squares[oneForward] == Piece.None)
        {
            moves.Add(new Move(startSquare, oneForward));

            // Two squares forward from starting rank
            int twoForward = startSquare + 16 * direction;
            if (rank == startRank && Board.Squares[twoForward] == Piece.None)
                moves.Add(new Move(startSquare, twoForward));
        }

        // Captures diagonally
        int[] captureDirs = { 7 * direction, 9 * direction };
        int[] captureFileDiffs = { -1, 1 }; // left and right

        for (int i = 0; i < 2; i++)
        {
            int targetSquare = startSquare + captureDirs[i];
            int targetFile = targetSquare % 8;

            if (targetSquare < 0 || targetSquare >= 64) continue;
            if (System.Math.Abs(targetFile - file) != 1) continue; // prevent wrap
            if (!Piece.IsColor(Board.Squares[targetSquare], oppositeColor)) continue;

            moves.Add(new Move(startSquare, targetSquare));
        }
    }
}