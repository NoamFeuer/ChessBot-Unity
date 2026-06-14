using UnityEngine;
using System.Collections.Generic;

public static class Perft
{
    public static ulong PerftCheck(int depth)
    {
        if (depth == 0)
            return 1UL;

        List<Move> moves = MoveGeneration.GenerateLegalMoves();

        if (moves.Count == 0)
            return 0UL;

        ulong nodes = 0;
        foreach (Move move in moves)
        {
            MoveMaker.MakeMove(move);
            nodes += PerftCheck(depth - 1);
            MoveMaker.UndoMove(move);
        }

        return nodes;
    }

    public static void PerftDivide(int depth, string filterMove = "")
    {
        List<Move> moveList = MoveGeneration.GenerateLegalMoves();
        ulong total = 0;

        foreach (Move move in moveList)
        {
            string from = SquareName(move.StartSquare);
            string to = SquareName(move.TargetSquare);
            string name = $"{from}{to}";

            if (filterMove != "" && name != filterMove) continue;

            MoveMaker.MakeMove(move);
            ulong nodes = PerftCheck(depth - 1);
            MoveMaker.UndoMove(move);

            Debug.Log($"{name}: {nodes}");
            total += nodes;
        }

        Debug.Log($"Total: {total}");
    }

    static string SquareName(int index)
    {
        int file = index % 8;
        int rank = index / 8;
        return $"{(char)('a' + file)}{rank + 1}";
    }
}
