using UnityEngine;
using System.Collections.Generic;

public static class Board
{
    public static int[] Squares;
    public static int colorToMove = Piece.White;

    static Board()
    {
        Squares = new int[64];
    }

    public static bool MakeMove(Move move, bool validated)
    {
        if (!MovePieces.IsLegalMove(move))
        {
            Debug.LogError("That move isn't legal");
            return false;
        }

        int movingPiece = Squares[move.StartSquare];

        Squares[move.TargetSquare] = movingPiece;
        Squares[move.StartSquare] = Piece.None;

        if (Piece.IsType(movingPiece, Piece.King))
        {
            int kingIndex = colorToMove == Piece.White ? 4 : 60;

            if (move.TargetSquare == kingIndex + 2) // Kingside
            {
                Squares[kingIndex + 1] = Squares[kingIndex + 3];
                Squares[kingIndex + 3] = Piece.None;
            }
            else if (move.TargetSquare == kingIndex - 2) // Queenside
            {
                Squares[kingIndex - 1] = Squares[kingIndex - 4];
                Squares[kingIndex - 4] = Piece.None;
            }
        }

        colorToMove = colorToMove == Piece.White ? Piece.Black : Piece.White;

        // SpecialMoves.UpdateCastelingRights(move);

        return true;
    }

    public static bool IsSquareAttacked(int squareIndex, int attackerColor)
    {
        int originalColor = Board.colorToMove;
        colorToMove = attackerColor;  // so GenerateMovesForPiece uses correct colors

        bool attacked = false;

        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            int piece = Squares[startSquare];
            if (!Piece.IsColor(piece, attackerColor)) continue;

            List<Move> attacks = MoveGeneration.GenerateMovesForPiece(startSquare, piece, attacksOnly: true);

            foreach (Move move in attacks)
            {
                if (move.TargetSquare == squareIndex)
                {
                    attacked = true;
                    break;
                }
            }
            if (attacked) break;
        }

        colorToMove = originalColor;  // restore
        return attacked;
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

    public static int PositionToIndex(Vector2 worldPos)
    {
        int file = Mathf.RoundToInt(worldPos.x / BoardDrawer.squareSize + 3.5f);
        int rank = Mathf.RoundToInt(worldPos.y / BoardDrawer.squareSize + 3.5f);

        if (file < 0 || file > 7 || rank < 0 || rank > 7) return -1;

        return rank * 8 + file;
    }
}
