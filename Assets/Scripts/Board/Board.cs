using UnityEngine;

public static class Board
{
    public static int[] Squares;

    static Board()
    {
        Squares = new int[64];

        Squares[0] = Piece.White | Piece.Bishop;
        Squares[63] = Piece.Black | Piece.Queen;
        Squares[7] = Piece.Black | Piece.Knight;
    }

    public static Vector2 IndexToPosition(int i)
    {
        int file = i % 8;
        int rank = i / 8;

        return new Vector2(-3.5f + file, -3.5f + rank) * BoardDrawer.squareSize;
    }
}
