using System.Collections.Generic;

public static class MoveGeneration
{
    static List<Move> moves = new List<Move>(256);
    static int friendlyColor;
    static int oppositeColor;

    public static List<Move> GenerateLegalMoves()
    {
        List<Move> pseudoLegal = GenerateMoves();
        List<Move> legal = new List<Move>(pseudoLegal.Count);

        foreach (Move move in pseudoLegal)
            if (DoesNotLeaveKingInCheck(move))
                legal.Add(move);

        return legal;
    }

    public static List<Move> GenerateLegalMovesForPiece(int startSquare)
    {
        int piece = Position.Squares[startSquare];
        List<Move> pseudoLegal = GenerateMovesForPiece(startSquare, piece);
        List<Move> legal = new List<Move>(pseudoLegal.Count);

        foreach (Move move in pseudoLegal)
            if (DoesNotLeaveKingInCheck(move))
                legal.Add(move);

        return legal;
    }

    static bool DoesNotLeaveKingInCheck(Move move)
    {
        int movingPiece   = Position.Squares[move.StartSquare];
        int capturedPiece = Position.Squares[move.TargetSquare];

        Position.Squares[move.TargetSquare] = movingPiece;
        Position.Squares[move.StartSquare]  = Piece.None;

        int capturedPawnSquare = -1;
        if (move.EnPassantMove)
        {
            int direction      = Position.colorToMove == Piece.White ? 1 : -1;
            capturedPawnSquare = move.TargetSquare - 8 * direction;
            Position.Squares[capturedPawnSquare] = Piece.None;
        }

        int rookFrom = -1, rookTo = -1, rookPiece = Piece.None;
        if (move.CastlingMove)
        {
            bool isWhite = Position.colorToMove == Piece.White;
            int backRank = isWhite ? 0 : 7;
            if (move.TargetSquare == backRank * 8 + 6)
                { rookFrom = backRank*8+7; rookTo = backRank*8+5; }
            else
                { rookFrom = backRank*8+0; rookTo = backRank*8+3; }
            rookPiece = Position.Squares[rookFrom];
            Position.Squares[rookTo]   = rookPiece;
            Position.Squares[rookFrom] = Piece.None;
        }

        int kingSquare    = Position.FindKing(Position.colorToMove);
        int attackerColor = Position.colorToMove == Piece.White ? Piece.Black : Piece.White;
        bool safe         = kingSquare != -1 && !Position.IsAttacked(kingSquare, attackerColor);

        Position.Squares[move.StartSquare]  = movingPiece;
        Position.Squares[move.TargetSquare] = capturedPiece;

        if (capturedPawnSquare != -1)
        {
            int capturedPawn = Position.colorToMove == Piece.White
                ? Piece.Black | Piece.Pawn
                : Piece.White | Piece.Pawn;
            Position.Squares[capturedPawnSquare] = capturedPawn;
        }

        if (move.CastlingMove && rookFrom != -1)
        {
            Position.Squares[rookFrom] = rookPiece;
            Position.Squares[rookTo]   = Piece.None;
        }

        return safe;
    }

    public static List<Move> GenerateMoves()
    {
        moves = new List<Move>(64);
        friendlyColor = Position.colorToMove;
        oppositeColor = friendlyColor == Piece.White ? Piece.Black : Piece.White;

        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            int piece = Position.Squares[startSquare];
            if (!Piece.IsColor(piece, friendlyColor)) continue;

            if      (Piece.IsSlidingPiece(piece))       GenerateSlidingMoves(startSquare, piece);
            else if (Piece.IsType(piece, Piece.Knight)) GenerateKnightMoves(startSquare);
            else if (Piece.IsType(piece, Piece.King))   GenerateKingMoves(startSquare);
            else if (Piece.IsType(piece, Piece.Pawn))   GeneratePawnMoves(startSquare);
        }

        return moves;
    }

    public static List<Move> GenerateMovesForPiece(int startSquare, int piece, bool attacksOnly = false)
    {
        List<Move> savedMoves = moves;
        int savedFriendly     = friendlyColor;
        int savedOpposite     = oppositeColor;

        moves         = new List<Move>(32);
        friendlyColor = Piece.GetColor(piece);
        oppositeColor = friendlyColor == Piece.White ? Piece.Black : Piece.White;

        if      (Piece.IsType(piece, Piece.Pawn))    GeneratePawnMoves(startSquare);
        else if (Piece.IsType(piece, Piece.Knight))  GenerateKnightMoves(startSquare);
        else if (Piece.IsType(piece, Piece.Bishop) ||
                 Piece.IsType(piece, Piece.Queen)  ||
                 Piece.IsType(piece, Piece.Rook))    GenerateSlidingMoves(startSquare, piece);
        else if (Piece.IsType(piece, Piece.King))    GenerateKingMoves(startSquare, attacksOnly);

        List<Move> result = moves;

        moves         = savedMoves;
        friendlyColor = savedFriendly;
        oppositeColor = savedOpposite;

        return result;
    }

    static void GenerateSlidingMoves(int startSquare, int piece)
    {
        int startDirIndex = Piece.IsType(piece, Piece.Bishop) ? 4 : 0;
        int endDirIndex   = Piece.IsType(piece, Piece.Rook)   ? 4 : 8;

        for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
        {
            for (int n = 0; n < PMD.NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare        = startSquare + PMD.DirectionOffsets[directionIndex] * (n + 1);
                int pieceOnTargetSquare = Position.Squares[targetSquare];

                if (Piece.IsColor(pieceOnTargetSquare, friendlyColor)) break;

                moves.Add(new Move(startSquare, targetSquare));

                if (Piece.IsColor(pieceOnTargetSquare, oppositeColor)) break;
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
            int fileDiff   = System.Math.Abs(targetFile - file);
            int rankDiff   = System.Math.Abs(targetRank - rank);

            if (!((fileDiff == 2 && rankDiff == 1) || (fileDiff == 1 && rankDiff == 2))) continue;

            int pieceOnTarget = Position.Squares[targetSquare];
            if (Piece.IsColor(pieceOnTarget, friendlyColor)) continue;

            moves.Add(new Move(startSquare, targetSquare));
        }
    }

    static void GenerateKingMoves(int startSquare, bool attacksOnly = false)
    {
        for (int directionIndex = 0; directionIndex < 8; directionIndex++)
        {
            if (PMD.NumSquaresToEdge[startSquare][directionIndex] == 0) continue;

            int targetSquare  = startSquare + PMD.DirectionOffsets[directionIndex];
            int pieceOnTarget = Position.Squares[targetSquare];

            if (Piece.IsColor(pieceOnTarget, friendlyColor)) continue;

            moves.Add(new Move(startSquare, targetSquare));
        }

        if (!attacksOnly)
            foreach (Move move in SpecialMoves.GetCastlingMoves())
                moves.Add(move);
    }

    static void GeneratePawnMoves(int startSquare)
    {
        int direction     = friendlyColor == Piece.White ? 1 : -1;
        int startRank     = friendlyColor == Piece.White ? 1 : 6;
        int promotionRank = friendlyColor == Piece.White ? 7 : 0;
        int file          = startSquare % 8;
        int rank          = startSquare / 8;

        int oneForward = startSquare + 8 * direction;
        if (oneForward >= 0 && oneForward < 64 && Position.Squares[oneForward] == Piece.None)
        {
            if (rank + direction == promotionRank)
                AddPromotionMoves(startSquare, oneForward);
            else
            {
                moves.Add(new Move(startSquare, oneForward));
                int twoForward = startSquare + 16 * direction;
                if (rank == startRank && Position.Squares[twoForward] == Piece.None)
                    moves.Add(new Move(startSquare, twoForward));
            }
        }

        int[] fileDeltas = { -1, 1 };
        foreach (int fd in fileDeltas)
        {
            int targetFile = file + fd;
            int targetRank = rank + direction;

            if (targetFile < 0 || targetFile > 7) continue;
            if (targetRank < 0 || targetRank > 7) continue;

            int targetSquare = targetRank * 8 + targetFile;
            if (!Piece.IsColor(Position.Squares[targetSquare], oppositeColor)) continue;

            if (targetRank == promotionRank)
                AddPromotionMoves(startSquare, targetSquare);
            else
                moves.Add(new Move(startSquare, targetSquare));
        }

        foreach (Move epMove in SpecialMoves.GetEPMoves())
            if (epMove.StartSquare == startSquare)
                moves.Add(epMove);
    }

    static void AddPromotionMoves(int startSquare, int targetSquare)
    {
        moves.Add(new Move(startSquare, targetSquare, promotionType: Piece.Queen));
        moves.Add(new Move(startSquare, targetSquare, promotionType: Piece.Rook));
        moves.Add(new Move(startSquare, targetSquare, promotionType: Piece.Bishop));
        moves.Add(new Move(startSquare, targetSquare, promotionType: Piece.Knight));
    }
}