using System.Collections.Generic;

public static class NegaMax
{
    static int Search(int depth, int alpha, int beta)
    {
        if (depth <= 0)
            return (int)ChessEvaluator.Evaluate();

        List<Move> moves = MoveGeneration.GenerateLegalMoves();

        if (moves.Count == 0)
            return (int)ChessEvaluator.Evaluate();

        foreach (Move move in moves)
        {
            Board.MakeMove(move);
            int eval = -Search(depth - 1, -beta, -alpha);
            Board.UndoMove(move);

            if (eval >= beta)
                return beta;

            if (eval > alpha)
                alpha = eval;
        }

        return alpha;
    }

    public static Move GetBestMove(int depth)
    {
        Move bestMove = default;
        int alpha     = -999999999;
        int beta      =  999999999;

        List<Move> moves = MoveGeneration.GenerateLegalMoves();
        foreach (Move move in moves)
        {
            Board.MakeMove(move);
            int eval = -Search(depth - 1, -beta, -alpha);
            Board.UndoMove(move);

            if (eval > alpha)
            {
                alpha    = eval;
                bestMove = move;
            }
        }

        return bestMove;
    }
}