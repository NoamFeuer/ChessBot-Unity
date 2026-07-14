using System;
using UnityEngine;

public class PMD
{
    public static readonly int[] DirectionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
    public static readonly int[] knightJumps = { 15, 17, -15, -17, 6, 10, -6, -10 };
    public static readonly int[][] NumSquaresToEdge;

    public static readonly int[] blackRookIndexes =  { 0, 7 };
    public static readonly int[] whiteRookIndexes = { 56, 63 };

    public static int whiteKingIndex = 60;
    public static int blackKingIndex = 4;

    static PMD()
    {
        NumSquaresToEdge = new int[64][];

        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                int numNorth = 7 - rank;
                int numSouth = rank;
                int numWest = file;
                int numEast = 7 - file;

                int squareIndex = rank * 8 + file;

                NumSquaresToEdge[squareIndex] = new int[]
                {
                    numNorth,
                    numSouth,
                    numWest,
                    numEast,
                    Math.Min(numNorth, numWest),
                    Math.Min(numSouth, numEast),
                    Math.Min(numNorth, numEast),
                    Math.Min(numSouth, numWest)
                };
            }
        }
    }
}
