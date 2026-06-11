using System.Collections.Generic;

public static class NegaMax
{
    static int PieceValue(int piece)
    {
        return Piece.GetType(piece) switch
        {
            Piece.Pawn   => 100,
            Piece.Knight => 320,
            Piece.Bishop => 330,
            Piece.Rook   => 500,
            Piece.Queen  => 900,
            Piece.King   => 20000,
            _            => 0
        };
    }

    static int MoveScore(Move move)
    {
        int score = 0;

        // Captures — MVV-LVA
        if (move.CaptureMove)
        {
            int victim    = PieceValue(Position.Squares[move.TargetSquare]);
            int aggressor = PieceValue(move.MovingPiece);
            score += 10000 + victim - aggressor;
        }

        // Promotions
        if (move.IsPromotion)
            score += 9000 + PieceValue(move.PromotionType);

        return score;
    }

    static int Search(int depth, int alpha, int beta)
    {
        if (depth <= 0)
            return Quiescence(alpha, beta);  // ← quiescence instead of static eval

        List<Move> moves = MoveGeneration.GenerateLegalMoves();

        if (moves.Count == 0)
            return (int)ChessEvaluator.Evaluate();

        // Move ordering
        moves.Sort((a, b) => MoveScore(b).CompareTo(MoveScore(a)));

        foreach (Move move in moves)
        {
            MoveMaker.MakeMove(move);
            int eval = -Search(depth - 1, -beta, -alpha);
            MoveMaker.UndoMove(move);

            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;
        }

        return alpha;
    }

    // Quiescence search — keeps searching captures until quiet position
    // Prevents "horizon effect" where engine misses captures just beyond search depth
    static int Quiescence(int alpha, int beta)
    {
        int standPat = (int)ChessEvaluator.Evaluate();

        if (standPat >= beta) return beta;
        if (standPat > alpha) alpha = standPat;

        List<Move> moves = MoveGeneration.GenerateLegalMoves();

        // Only look at captures
        moves.RemoveAll(m => !m.CaptureMove && !m.IsPromotion);
        moves.Sort((a, b) => MoveScore(b).CompareTo(MoveScore(a)));

        foreach (Move move in moves)
        {
            MoveMaker.MakeMove(move);
            int eval = -Quiescence(-beta, -alpha);
            MoveMaker.UndoMove(move);

            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;
        }

        return alpha;
    }

    public static Move GetBestMove(int depth)
    {
        Move bestMove = default;
        int alpha     = -999999999;
        int beta      =  999999999;

        List<Move> moves = MoveGeneration.GenerateLegalMoves();

        // Move ordering at root
        moves.Sort((a, b) => MoveScore(b).CompareTo(MoveScore(a)));

        foreach (Move move in moves)
        {
            MoveMaker.MakeMove(move);
            int eval = -Search(depth - 1, -beta, -alpha);
            MoveMaker.UndoMove(move);

            if (eval > alpha)
            {
                alpha    = eval;
                bestMove = move;
            }
        }

        return bestMove;
    }
}