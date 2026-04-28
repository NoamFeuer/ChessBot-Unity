using UnityEngine;

public static class Board
{
    public static int[] Squares;
    public static int colorToMove = Piece.White;

    static Board()
    {
        Squares = new int[64];

        LoadPositionFromFen("r1bqk2r/pppp1ppp/2n2n2/2b1p3/2B1P3/2N2N2/PPPP1PPP/R1BQK2R");
    }

    public static Vector2 IndexToPosition(int i)
    {
        int file = i % 8;
        int rank = i / 8;

        return new Vector2(-3.5f + file, -3.5f + rank) * BoardDrawer.squareSize;
    }

    public static void LoadPositionFromFen(string fen)
    {
        int file = 0;
        int rank = 7;

        foreach (char c in fen)
        {
            if (c == '/')
            {
                file = 0;
                rank--;
            }
            else if (char.IsDigit(c))
                file += (int)char.GetNumericValue(c);
            else
            {
                int color = char.IsUpper(c) ? Piece.White : Piece.Black;
                int type = char.ToLower(c) switch
                {
                    'k' => Piece.King,
                    'q' => Piece.Queen,
                    'r' => Piece.Rook,
                    'b' => Piece.Bishop,
                    'n' => Piece.Knight,
                    'p' => Piece.Pawn,
                    _   => Piece.None
                };

                Squares[rank * 8 + file] = color | type;
                file++;
            }
        }
    }
}
