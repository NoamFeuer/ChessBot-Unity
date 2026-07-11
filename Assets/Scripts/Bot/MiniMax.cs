using System.Collections.Generic;

public static class MiniMax
{
    const int TT_SIZE = 1 << 20;

    struct TTEntry
    {
        public ulong Key;
        public int   Depth;
        public int   Score;
        public byte  Flag;
        public Move  BestMove;
    }

    static TTEntry[] tt          = new TTEntry[TT_SIZE];
    static Move[,]   killers     = new Move[64, 2];
    static int[,]    history     = new int[64, 64];

    const byte EXACT      = 0;
    const byte LOWERBOUND = 1;
    const byte UPPERBOUND = 2;

    static int nodeCount = 0;

    static readonly ulong[,,] ZobristTable;
    static readonly ulong     ZobristBlackToMove;

    static MiniMax()
    {
        System.Random rng = new System.Random(12345);
        ZobristTable = new ulong[64, 7, 2];

        for (int sq = 0; sq < 64; sq++)
            for (int pt = 0; pt < 7; pt++)
                for (int c  = 0; c  < 2; c++)
                    ZobristTable[sq, pt, c] = NextUlong(rng);

        ZobristBlackToMove = NextUlong(rng);
    }

    static ulong NextUlong(System.Random rng)
    {
        byte[] buf = new byte[8];
        rng.NextBytes(buf);
        return System.BitConverter.ToUInt64(buf, 0);
    }

    static ulong ComputeZobrist()
    {
        ulong hash = 0;
        for (int sq = 0; sq < 64; sq++)
        {
            int piece = Position.Squares[sq];
            if (piece == Piece.None) continue;
            int type  = Piece.GetType(piece);
            int color = Piece.GetColor(piece) == Piece.White ? 0 : 1;
            hash ^= ZobristTable[sq, type, color];
        }
        if (Position.colorToMove == Piece.Black)
            hash ^= ZobristBlackToMove;
        return hash;
    }

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

    static int CheapEval()
    {
        int score = 0;
        for (int sq = 0; sq < 64; sq++)
        {
            int piece = Position.Squares[sq];
            if (piece == Piece.None) continue;
            int value = PieceValue(piece);
            score += Piece.GetColor(piece) == Piece.White ? value : -value;
        }
        return Position.colorToMove == Piece.White ? score : -score;
    }

    static int MoveScore(Move move, int ply, Move ttMove)
    {
        if (move.StartSquare  == ttMove.StartSquare &&
            move.TargetSquare == ttMove.TargetSquare)
            return 10_000_000;

        if (move.CaptureMove)
        {
            int victim    = PieceValue(Position.Squares[move.TargetSquare]);
            int aggressor = PieceValue(move.MovingPiece);
            return 1_000_000 + victim * 10 - aggressor;
        }

        if (move.IsPromotion)
            return 900_000 + PieceValue(move.PromotionType);

        if (move.StartSquare  == killers[ply, 0].StartSquare &&
            move.TargetSquare == killers[ply, 0].TargetSquare)
            return 800_000;

        if (move.StartSquare  == killers[ply, 1].StartSquare &&
            move.TargetSquare == killers[ply, 1].TargetSquare)
            return 700_000;

        return history[move.StartSquare, move.TargetSquare];
    }

    static int Search(int depth, int alpha, int beta, int ply)
    {
        nodeCount++;

        if (depth <= 0)
            return Quiescence(alpha, beta);

        ulong key   = ComputeZobrist();
        int ttIndex = (int)(key % TT_SIZE);
        TTEntry ttEntry = tt[ttIndex];
        Move ttMove = default;

        if (ttEntry.Key == key)
        {
            ttMove = ttEntry.BestMove;
            if (ttEntry.Depth >= depth)
            {
                int ttScore = ttEntry.Score;
                if (ttEntry.Flag == EXACT)                          return ttScore;
                if (ttEntry.Flag == LOWERBOUND && ttScore >= beta)  return ttScore;
                if (ttEntry.Flag == UPPERBOUND && ttScore <= alpha) return ttScore;
            }
        }

        List<Move> moves = MoveGeneration.GenerateLegalMoves();

        if (moves.Count == 0)
            return CheapEval();

        moves.Sort((a, b) => MoveScore(b, ply, ttMove).CompareTo(MoveScore(a, ply, ttMove)));

        int originalAlpha = alpha;
        Move bestMove     = default;
        int bestScore     = -999999999;

        for (int i = 0; i < moves.Count; i++)
        {
            Move move = moves[i];
            MoveMaker.MakeMove(move);
            int eval = -Search(depth - 1, -beta, -alpha, ply + 1);
            MoveMaker.UndoMove(move);

            if (eval > bestScore)
            {
                bestScore = eval;
                bestMove  = move;
            }

            if (eval >= beta)
            {
                if (!move.CaptureMove)
                {
                    killers[ply, 1] = killers[ply, 0];
                    killers[ply, 0] = move;
                    history[move.StartSquare, move.TargetSquare] += depth * depth;
                }

                tt[ttIndex] = new TTEntry {
                    Key = key, Depth = depth, Score = eval,
                    Flag = LOWERBOUND, BestMove = move
                };
                return beta;
            }

            if (eval > alpha) alpha = eval;
        }

        byte flag = bestScore <= originalAlpha ? UPPERBOUND :
                    bestScore >= beta           ? LOWERBOUND : EXACT;

        tt[ttIndex] = new TTEntry {
            Key = key, Depth = depth, Score = bestScore,
            Flag = flag, BestMove = bestMove
        };

        return bestScore;
    }

    static int Quiescence(int alpha, int beta)
    {
        int standPat = CheapEval();

        if (standPat >= beta)  return beta;
        if (standPat > alpha)  alpha = standPat;

        List<Move> moves = MoveGeneration.GenerateLegalMoves();
        moves.RemoveAll(m => !m.CaptureMove && !m.IsPromotion);
        moves.Sort((a, b) => MoveScore(b, 0, default).CompareTo(MoveScore(a, 0, default)));

        foreach (Move move in moves)
        {
            MoveMaker.MakeMove(move);
            int eval = -Quiescence(-beta, -alpha);
            MoveMaker.UndoMove(move);

            if (eval >= beta)  return beta;
            if (eval > alpha)  alpha = eval;
        }

        return alpha;
    }

    public static Move GetBestMove(int depth)
    {
        killers  = new Move[64, 2];
        history  = new int[64, 64];
        nodeCount = 0;

        Move bestMove = default;
        int alpha     = -999999999;
        int beta      =  999999999;

        var (_, policy) = ChessEvaluator.EvaluateWithPolicy();

        ulong key   = ComputeZobrist();
        int ttIndex = (int)(key % TT_SIZE);
        TTEntry ttEntry = tt[ttIndex];
        Move ttMove = ttEntry.Key == key ? ttEntry.BestMove : default;

        List<Move> moves = MoveGeneration.GenerateLegalMoves();

        moves.Sort((a, b) =>
        {
            float scoreA = ChessEvaluator.GetMoveScore(policy, a) * 100f + MoveScore(a, 0, ttMove);
            float scoreB = ChessEvaluator.GetMoveScore(policy, b) * 100f + MoveScore(b, 0, ttMove);
            return scoreB.CompareTo(scoreA);
        });

        foreach (Move move in moves)
        {
            MoveMaker.MakeMove(move);
            int eval = -Search(depth - 1, -beta, -alpha, 1);
            MoveMaker.UndoMove(move);

            if (eval > alpha)
            {
                alpha    = eval;
                bestMove = move;
            }
        }

        tt[ttIndex] = new TTEntry {
            Key = key, Depth = depth, Score = alpha,
            Flag = EXACT, BestMove = bestMove
        };

        UnityEngine.Debug.Log($"Nodes: {nodeCount}");

        return bestMove;
    }
}