using UnityEngine;
using System.Collections.Generic;

public static class Perft
{
    public static ulong PerftCheck(int depth)
    {
        if (depth == 0) return 1UL;

        List<Move> moveList = MoveGeneration.GenerateLegalMoves();

        if (depth == 1)
            foreach (Move move in moveList)
                Debug.Log($"{move.StartSquare} -> {move.TargetSquare}");

        ulong nodes = 0;
        foreach (Move move in moveList)
        {
            Board.MakeMove(move);
            nodes += PerftCheck(depth - 1);
            Board.UndoMove(move);
        }

        return nodes;
    }
}
