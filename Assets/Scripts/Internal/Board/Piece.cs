using UnityEngine;

public static class Piece
{
    public const int None = 0;
    public const int King = 1;
    public const int Pawn = 2;
    public const int Knight = 3;
    public const int Bishop = 4;
    public const int Rook = 5;
    public const int Queen = 6;

    public const int White = 8;
    public const int Black = 16;


    public static bool IsSlidingPiece(int piece)
    {
        int type = piece & 0b00111;
        return type == Bishop || type == Rook || type == Queen;
    }

    public static bool IsColor(int piece, int color)
    {
        return (piece & 0b11000) == color;
    }

    public static bool IsType(int piece, int type)
    {
        return (piece & 0b00111) == type;
    }
}
