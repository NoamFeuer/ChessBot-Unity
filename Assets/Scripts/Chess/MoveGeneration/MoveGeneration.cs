using System.Collections.Generic;

public static class MoveGeneration
{
    static List<Move> moves = new List<Move>();
    static int friendlyColor;
    static int oppositeColor;

    static readonly int[] PawnAttackFileDeltas = { -1, 1 };

    // Populated once per GenerateLegalMoves() call by AnalyzeCheckAndPins,
    // then used to filter pseudo-legal moves without a full simulation per move.
    static int CheckerCount;
    static readonly HashSet<int> CheckBlockSquares = new HashSet<int>();
    static readonly Dictionary<int, HashSet<int>> PinRestrictions = new Dictionary<int, HashSet<int>>();

    public static List<Move> GenerateLegalMoves()
    {
        List<Move> pseudoLegal = GenerateMoves();
        List<Move> legal = new List<Move>(pseudoLegal.Count);

        AnalyzeCheckAndPins();

        foreach (Move move in pseudoLegal)
        {
            int piece = Position.Squares[move.StartSquare];

            // King moves, castling, and en passant can't be validated by the pin/check tables alone, so fall back to a full simulation.
            if (Piece.IsType(piece, Piece.King) || move.CastlingMove || move.EnPassantMove)
            {
                if (MoveKeepsKingSafe(move))
                    legal.Add(move);
                continue;
            }

            // Double check: no non-king move can resolve two attackers at once.
            if (CheckerCount >= 2) continue;

            // Single check: the move must capture the checker or block the checking ray.
            if (CheckerCount == 1 && !CheckBlockSquares.Contains(move.TargetSquare)) continue;

            // Pinned piece: it may only move along the line between the king and its pinner.
            if (PinRestrictions.TryGetValue(move.StartSquare, out HashSet<int> allowed) &&!allowed.Contains(move.TargetSquare)) continue;

            legal.Add(move);
        }

        return legal;
    }

    public static List<Move> GenerateLegalMovesForPiece(int startSquare)
    {
        int piece = Position.Squares[startSquare];
        List<Move> pseudoLegal = GenerateMovesForPiece(startSquare, piece);
        List<Move> legal = new List<Move>(pseudoLegal.Count);

        foreach (Move move in pseudoLegal)
        {
            if (MoveKeepsKingSafe(move))
                legal.Add(move);
        }

        return legal;
    }

    // Scans outward from the friendly king to find every current checker and every pinned friendly piece
    static void AnalyzeCheckAndPins()
    {
        CheckerCount = 0;
        CheckBlockSquares.Clear();
        PinRestrictions.Clear();

        int kingSquare = Position.FindKing(friendlyColor);
        if (kingSquare == -1) return;

        FindSlidingChecksAndPins(kingSquare);
        FindKnightChecks(kingSquare);
        FindPawnChecks(kingSquare);
    }

    static void FindSlidingChecksAndPins(int kingSquare)
    {
        for (int dir = 0; dir < 8; dir++)
        {
            int pinnedCandidateSquare = -1;
            List<int> squaresAlongRay = new List<int>();

            for (int n = 0; n < PMD.NumSquaresToEdge[kingSquare][dir]; n++)
            {
                int square = kingSquare + PMD.DirectionOffsets[dir] * (n + 1);
                int pieceOnSquare = Position.Squares[square];

                if (pieceOnSquare == Piece.None)
                {
                    squaresAlongRay.Add(square);
                    continue;
                }

                if (Piece.IsColor(pieceOnSquare, friendlyColor))
                {
                    // First friendly piece on the ray is a pin candidate; a second one means the ray is fully blocked, so no pin is possible.
                    if (pinnedCandidateSquare == -1)
                    {
                        pinnedCandidateSquare = square;
                        squaresAlongRay.Add(square);
                        continue;
                    }
                    break;
                }

                // Enemy piece: only relevant if it actually attacks along this direction
                bool isDiagonal = dir >= 4;
                bool attacksThisDirection =
                    Piece.IsType(pieceOnSquare, Piece.Queen) ||
                    (isDiagonal && Piece.IsType(pieceOnSquare, Piece.Bishop)) ||
                    (!isDiagonal && Piece.IsType(pieceOnSquare, Piece.Rook));

                if (!attacksThisDirection) break;

                squaresAlongRay.Add(square);

                if (pinnedCandidateSquare == -1)
                {
                    // Nothing was blocking the ray, so this is a check; every square from the king up to and including the checker can block or capture it.
                    CheckerCount++;
                    foreach (int s in squaresAlongRay) CheckBlockSquares.Add(s);
                }
                else
                    PinRestrictions[pinnedCandidateSquare] = new HashSet<int>(squaresAlongRay);

                break;
            }
        }
    }

    static void FindKnightChecks(int kingSquare)
    {
        foreach (int jump in PMD.knightJumps)
        {
            int targetSquare = kingSquare + jump;
            if (targetSquare < 0 || targetSquare >= 64) continue;
            if (!IsValidKnightMove(kingSquare, targetSquare)) continue;

            int pieceOnSquare = Position.Squares[targetSquare];
            if (Piece.IsColor(pieceOnSquare, oppositeColor) && Piece.IsType(pieceOnSquare, Piece.Knight))
            {
                CheckerCount++;
                CheckBlockSquares.Add(targetSquare); // knight checks can only be captured, not blocked
            }
        }
    }

    static void FindPawnChecks(int kingSquare)
    {
        // A pawn attacks diagonally, so checking the squares a friendly pawn on the
        foreach (int checkSquare in GetPawnAttackSquares(kingSquare, friendlyColor))
        {
            int pieceOnSquare = Position.Squares[checkSquare];
            if (Piece.IsColor(pieceOnSquare, oppositeColor) && Piece.IsType(pieceOnSquare, Piece.Pawn))
            {
                CheckerCount++;
                CheckBlockSquares.Add(checkSquare);
            }
        }
    }

    static bool MoveKeepsKingSafe(Move move)
    {
        int mover = Position.colorToMove;

        MoveMaker.MakeMove(move);

        int kingSquare = Position.FindKing(mover);
        bool safe = kingSquare != -1 && !Position.IsAttacked(kingSquare, Position.colorToMove);

        MoveMaker.UndoMove(move);

        return safe;
    }

    // Generates every pseudo-legal move for the side to move (no check/pin filtering).
    public static List<Move> GenerateMoves()
    {
        moves = new List<Move>(64);
        friendlyColor = Position.colorToMove;
        oppositeColor = friendlyColor == Piece.White ? Piece.Black : Piece.White;

        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            int piece = Position.Squares[startSquare];
            if (!Piece.IsColor(piece, friendlyColor)) continue;

            if (Piece.IsSlidingPiece(piece)) GenerateSlidingMoves(startSquare, piece);
            else if (Piece.IsType(piece, Piece.Knight)) GenerateKnightMoves(startSquare);
            else if (Piece.IsType(piece, Piece.King)) GenerateKingMoves(startSquare);
            else if (Piece.IsType(piece, Piece.Pawn)) GeneratePawnMoves(startSquare);
        }

        return moves;
    }

    public static List<Move> GenerateMovesForPiece(int startSquare, int piece, bool excludeCastling = false)
    {
        List<Move> savedMoves = moves;
        int savedFriendly = friendlyColor;
        int savedOpposite = oppositeColor;

        moves = new List<Move>(32);
        friendlyColor = Piece.GetColor(piece);
        oppositeColor = friendlyColor == Piece.White ? Piece.Black : Piece.White;

        if (Piece.IsType(piece, Piece.Pawn)) GeneratePawnMoves(startSquare);
        else if (Piece.IsType(piece, Piece.Knight)) GenerateKnightMoves(startSquare);
        else if (Piece.IsType(piece, Piece.Bishop) ||
                 Piece.IsType(piece, Piece.Queen)  ||
                 Piece.IsType(piece, Piece.Rook)) GenerateSlidingMoves(startSquare, piece);
        else if (Piece.IsType(piece, Piece.King)) GenerateKingMoves(startSquare, excludeCastling);

        List<Move> result = moves;

        moves = savedMoves;
        friendlyColor = savedFriendly;
        oppositeColor = savedOpposite;

        return result;
    }

    static void GenerateSlidingMoves(int startSquare, int piece)
    {
        int startDirIndex = Piece.IsType(piece, Piece.Bishop) ? 4 : 0;
        int endDirIndex = Piece.IsType(piece, Piece.Rook) ? 4 : 8;

        for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
        {
            for (int n = 0; n < PMD.NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + PMD.DirectionOffsets[directionIndex] * (n + 1);
                int pieceOnTargetSquare = Position.Squares[targetSquare];

                if (Piece.IsColor(pieceOnTargetSquare, friendlyColor)) break;

                moves.Add(new Move(startSquare, targetSquare));

                // Ray stops after a capture: can't slide past an enemy piece.
                if (Piece.IsColor(pieceOnTargetSquare, oppositeColor)) break;
            }
        }
    }

    static void GenerateKnightMoves(int startSquare)
    {
        foreach (int jump in PMD.knightJumps)
        {
            int targetSquare = startSquare + jump;
            if (targetSquare < 0 || targetSquare >= 64) continue;
            if (!IsValidKnightMove(startSquare, targetSquare)) continue;

            int pieceOnTarget = Position.Squares[targetSquare];
            if (Piece.IsColor(pieceOnTarget, friendlyColor)) continue;

            moves.Add(new Move(startSquare, targetSquare));
        }
    }

    static void GenerateKingMoves(int startSquare, bool excludeCastling = false)
    {
        for (int directionIndex = 0; directionIndex < 8; directionIndex++)
        {
            if (PMD.NumSquaresToEdge[startSquare][directionIndex] == 0) continue;

            int targetSquare = startSquare + PMD.DirectionOffsets[directionIndex];
            int pieceOnTarget = Position.Squares[targetSquare];

            if (Piece.IsColor(pieceOnTarget, friendlyColor)) continue;

            moves.Add(new Move(startSquare, targetSquare));
        }

        // excludeCastling is used by callers checking attacked squares, where treating castling as a normal king move would be meaningless
        if (!excludeCastling)
        {
            foreach (Move move in SpecialMoves.GetCastlingMoves())
            {
                moves.Add(move);
            }
        }
    }

    static void GeneratePawnMoves(int startSquare)
    {
        int direction = friendlyColor == Piece.White ? 1 : -1;
        int startRank = friendlyColor == Piece.White ? 1 : 6;
        int promotionRank = friendlyColor == Piece.White ? 7 : 0;
        int rank = startSquare / 8;

        int oneForward = startSquare + 8 * direction;
        if (oneForward >= 0 && oneForward < 64 && Position.Squares[oneForward] == Piece.None)
        {
            if (rank + direction == promotionRank)
                AddPromotionMoves(startSquare, oneForward);
            else
            {
                moves.Add(new Move(startSquare, oneForward));
                int twoForward = startSquare + 16 * direction;

                // Double push is only legal from the starting rank, and only if both squares ahead are empty.
                if (rank == startRank && Position.Squares[twoForward] == Piece.None)
                    moves.Add(new Move(startSquare, twoForward));
            }
        }

        foreach (int targetSquare in GetPawnAttackSquares(startSquare, friendlyColor))
        {
            if (!Piece.IsColor(Position.Squares[targetSquare], oppositeColor)) continue;

            int targetRank = targetSquare / 8;
            if (targetRank == promotionRank)
                AddPromotionMoves(startSquare, targetSquare);
            else
                moves.Add(new Move(startSquare, targetSquare));
        }

        foreach (Move epMove in SpecialMoves.GetEPMoves())
        {
            if (epMove.StartSquare == startSquare)
                moves.Add(epMove);
        }   
    }

    // A pawn move to the back rank always comes with a choice of piece to promote to.
    static void AddPromotionMoves(int startSquare, int targetSquare)
    {
        moves.Add(new Move(startSquare, targetSquare, promotionType: Piece.Queen));
        moves.Add(new Move(startSquare, targetSquare, promotionType: Piece.Rook));
        moves.Add(new Move(startSquare, targetSquare, promotionType: Piece.Bishop));
        moves.Add(new Move(startSquare, targetSquare, promotionType: Piece.Knight));
    }

    static bool IsValidKnightMove(int fromSquare, int toSquare)
    {
        int fileDiff = System.Math.Abs(toSquare % 8 - fromSquare % 8);
        int rankDiff = System.Math.Abs(toSquare / 8 - fromSquare / 8);

        return (fileDiff == 2 && rankDiff == 1) || (fileDiff == 1 && rankDiff == 2);
    }

    // Squares a pawn of the given color standing on `square` would attack diagonally.
    static IEnumerable<int> GetPawnAttackSquares(int square, int color)
    {
        int direction = color == Piece.White ? 1 : -1;
        int file = square % 8;
        int rank = square / 8;

        foreach (int fd in PawnAttackFileDeltas)
        {
            int targetFile = file + fd;
            int targetRank = rank + direction;

            if (targetFile < 0 || targetFile > 7) continue;
            if (targetRank < 0 || targetRank > 7) continue;

            yield return targetRank * 8 + targetFile;
        }
    }
}
