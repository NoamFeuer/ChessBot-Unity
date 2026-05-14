using System.Collections.Generic;

public static class MoveGeneration
{
    static List<Move> moves;
    static int friendlyColor;
    static int oppositeColor;

    public static List<Move> GenerateLegalMoves()
    {
        List<Move> pseudoLegal = GenerateMoves();
        List<Move> legal = new List<Move>();

        foreach (Move move in pseudoLegal)
        {
            if (DoesNotLeaveKingInCheck(move))
                legal.Add(move);
        }

        return legal;
    }

    public static List<Move> GenerateLegalMovesForPiece(int startSquare)
    {
        int piece = Board.Squares[startSquare];
        List<Move> pseudoLegal = GenerateMovesForPiece(startSquare, piece);
        List<Move> legal = new List<Move>();

        foreach (Move move in pseudoLegal)
        {
            if (DoesNotLeaveKingInCheck(move))
                legal.Add(move);
        }

        return legal;
    }

    static bool DoesNotLeaveKingInCheck(Move move)
    {
        int movingPiece   = Board.Squares[move.StartSquare];
        int capturedPiece = Board.Squares[move.TargetSquare];

        Board.Squares[move.TargetSquare] = movingPiece;
        Board.Squares[move.StartSquare]  = Piece.None;

        int attackerColor = (Board.colorToMove == Piece.White) ? Piece.Black : Piece.White;
        int kingSquare    = FindKing(Board.colorToMove);
        bool safe         = kingSquare != -1 && !Board.IsSquareAttacked(kingSquare, attackerColor);

        Board.Squares[move.StartSquare]  = movingPiece;
        Board.Squares[move.TargetSquare] = capturedPiece;

        return safe;
    }

    public static int FindKing(int color)
    {
        for (int i = 0; i < 64; i++)
        {
            if (Board.Squares[i] == (color | Piece.King))
                return i;
        }
        return -1;
    }

    public static bool IsKingInCheck(int color)
    {
        int kingSquare = FindKing(color);
        if (kingSquare == -1) return false;

        int attackerColor = (color == Piece.White) ? Piece.Black : Piece.White;
        return Board.IsSquareAttacked(kingSquare, attackerColor);
    }

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

    public static List<Move> GenerateMovesForPiece(int startSquare, int piece, bool attacksOnly = false)
    {
        List<Move> previousMoves = moves;
        moves = new List<Move>();

        friendlyColor = Board.colorToMove;
        oppositeColor = (friendlyColor == Piece.White) ? Piece.Black : Piece.White;

        if (Piece.IsType(piece, Piece.Pawn))
            GeneratePawnMoves(startSquare);
        else if (Piece.IsType(piece, Piece.Knight))
            GenerateKnightMoves(startSquare);
        else if (Piece.IsType(piece, Piece.Bishop) ||
                 Piece.IsType(piece, Piece.Queen)  ||
                 Piece.IsType(piece, Piece.Rook))
            GenerateSlidingMoves(startSquare, piece);
        else if (Piece.IsType(piece, Piece.King))
            GenerateKingMoves(startSquare, attacksOnly);

        List<Move> pieceMoves = moves;
        moves = previousMoves;

        return pieceMoves;
    }

    static void GenerateSlidingMoves(int startSquare, int piece)
    {
        int startDirIndex = Piece.IsType(piece, Piece.Bishop) ? 4 : 0;
        int endDirIndex   = Piece.IsType(piece, Piece.Rook)   ? 4 : 8;

        for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
        {
            for (int n = 0; n < PMD.NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + PMD.DirectionOffsets[directionIndex] * (n + 1);
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

        foreach (int jump in PMD.knightJumps)
        {
            int targetSquare = startSquare + jump;

            if (targetSquare < 0 || targetSquare >= 64) continue;

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

    static void GenerateKingMoves(int startSquare, bool attacksOnly = false)
    {
        for (int directionIndex = 0; directionIndex < 8; directionIndex++)
        {
            if (PMD.NumSquaresToEdge[startSquare][directionIndex] == 0) continue;

            int targetSquare = startSquare + PMD.DirectionOffsets[directionIndex];
            int pieceOnTarget = Board.Squares[targetSquare];

            if (Piece.IsColor(pieceOnTarget, friendlyColor)) continue;

            moves.Add(new Move(startSquare, targetSquare));
        }

        // if (!attacksOnly)
        // {
        //     foreach (Move move in SpecialMoves.GetCastlingMoves())
        //         moves.Add(move);
        // }
    }

    static void GeneratePawnMoves(int startSquare)
    {
        int direction = (friendlyColor == Piece.White) ? 1 : -1;
        int startRank = (friendlyColor == Piece.White) ? 1 : 6;
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
        int[] fileDeltas = { -1, 1 };
        foreach (int fd in fileDeltas)
        {
            int targetFile = file + fd;
            int targetRank = rank + direction;

            if (targetFile < 0 || targetFile > 7) continue;
            if (targetRank < 0 || targetRank > 7) continue;

            int targetSquare = targetRank * 8 + targetFile;
            if (!Piece.IsColor(Board.Squares[targetSquare], oppositeColor)) continue;

            moves.Add(new Move(startSquare, targetSquare));
        }
    }
}