using UnityEngine;
using System.Collections.Generic;

public static class Position 
{
    public static Stack<GameHistory> history = new Stack<GameHistory>();
    public static int[] Squares;
    public static int colorToMove = Piece.White;

    static Position()
    {
        Squares = new int[64];
    }

    public static int FindKing(int color)
    {
        for (int i = 0; i < 64; i++)
        {
            if (Squares[i] == (color | Piece.King))
                return i;
        }
        return -1;
    }

    public static bool IsKingInCheck(int color)
    {
        int kingSquare = FindKing(color);
        if (kingSquare == -1) return false;

        int attackerColor = (color == Piece.White) ? Piece.Black : Piece.White;
        return IsSquareAttacked(kingSquare, attackerColor);
    }

    public static bool IsSquareAttacked(int squareIndex, int attackerColor)
    {
        int originalColor = colorToMove;
        int savedEP = SpecialMoves.enPassantSquare;
        colorToMove = attackerColor;
        SpecialMoves.enPassantSquare = -1;

        bool attacked = false;

        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            int piece = Squares[startSquare];
            if (!Piece.IsColor(piece, attackerColor)) continue;

            List<Move> attacks = MoveGeneration.GenerateMovesForPiece(startSquare, piece, attacksOnly: true);

            foreach (Move attack in attacks)
            {
                if (attack.TargetSquare == squareIndex)
                {
                    attacked = true;
                    break;
                }
            }
            if (attacked) break;
        }

        colorToMove = originalColor;
        SpecialMoves.enPassantSquare = savedEP;
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
        Squares = new int[64];
        colorToMove = Piece.White;
        SpecialMoves.enPassantSquare = -1;
        history.Clear();
        SpecialMoves.castlingRights = new bool[2][]
        {
            new bool[] { false, false },
            new bool[] { false, false }
        };

        string[] sections = fen.Split(' ');

        int file = 0;
        int rank = 0;
        foreach (char c in sections[0])
        {
            if (c == '/')
            {
                file = 0;
                rank++;
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
                Squares[(7 - rank) * 8 + file] = color | type;
                file++;
            }
        }

        if (sections.Length > 1)
            colorToMove = sections[1] == "b" ? Piece.Black : Piece.White;

        if (sections.Length > 2)
        {
            foreach (char c in sections[2])
            {
                switch (c)
                {
                    case 'K': SpecialMoves.castlingRights[1][1] = true; break; // white kingside
                    case 'Q': SpecialMoves.castlingRights[1][0] = true; break; // white queenside
                    case 'k': SpecialMoves.castlingRights[0][1] = true; break; // black kingside
                    case 'q': SpecialMoves.castlingRights[0][0] = true; break; // black queenside
                }
            }
        }

        if (sections.Length > 3 && sections[3] != "-")
        {
            int epFile = sections[3][0] - 'a';
            int epRank = sections[3][1] - '1';
            SpecialMoves.enPassantSquare = epRank * 8 + epFile;
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
 