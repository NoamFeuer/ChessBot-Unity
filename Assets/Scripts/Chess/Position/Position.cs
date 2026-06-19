using UnityEngine;
using System.Collections.Generic;

public static class Position 
{
    public static Stack<GameHistory> history = new Stack<GameHistory>();
    public static int[] Squares;
    public static int colorToMove   = Piece.White;
    public static int halfMoveClock  = 0;
    public static int fullMoveNumber = 1;

    static Position()
    {
        Squares = new int[64];
    }

    public static int FindKing(int color)
    {
        for (int i = 0; i < 64; i++)
            if (Squares[i] == (color | Piece.King))
                return i;
        return -1;
    }

    public static bool IsKingInCheck(int color)
    {
        int kingSquare = FindKing(color);
        if (kingSquare == -1) return false;
        int attackerColor = color == Piece.White ? Piece.Black : Piece.White;
        return IsAttacked(kingSquare, attackerColor);
    }

    public static bool IsAttacked(int sq, int attackerColor)
    {
        int file = sq % 8;
        int rank = sq / 8;

        int[] knightOffsets = { 17, 15, 10, 6, -6, -10, -15, -17 };
        foreach (int offset in knightOffsets)
        {
            int target = sq + offset;
            if (target < 0 || target >= 64) continue;
            int tf = target % 8, tr = target / 8;
            int fd = System.Math.Abs(tf - file), rd = System.Math.Abs(tr - rank);
            if ((fd == 2 && rd == 1) || (fd == 1 && rd == 2))
            {
                int piece = Squares[target];
                if (Piece.IsColor(piece, attackerColor) && Piece.IsType(piece, Piece.Knight))
                    return true;
            }
        }

        int pawnDir = attackerColor == Piece.White ? -1 : 1;
        int[] pawnFiles = { -1, 1 };
        foreach (int fd in pawnFiles)
        {
            int tf = file + fd, tr = rank + pawnDir;
            if (tf < 0 || tf > 7 || tr < 0 || tr > 7) continue;
            int piece = Squares[tr * 8 + tf];
            if (Piece.IsColor(piece, attackerColor) && Piece.IsType(piece, Piece.Pawn))
                return true;
        }

        int[] kingOffsets = { 1, -1, 8, -8, 9, -9, 7, -7 };
        foreach (int offset in kingOffsets)
        {
            int target = sq + offset;
            if (target < 0 || target >= 64) continue;
            int tf = target % 8, tr = target / 8;
            if (System.Math.Abs(tf - file) > 1 || System.Math.Abs(tr - rank) > 1) continue;
            int piece = Squares[target];
            if (Piece.IsColor(piece, attackerColor) && Piece.IsType(piece, Piece.King))
                return true;
        }

        int[] straightFiles = { 1, -1, 0, 0 };
        int[] straightRanks = { 0, 0, 1, -1 };
        for (int d = 0; d < 4; d++)
        {
            int tf = file + straightFiles[d];
            int tr = rank + straightRanks[d];
            while (tf >= 0 && tf < 8 && tr >= 0 && tr < 8)
            {
                int piece = Squares[tr * 8 + tf];
                if (piece != Piece.None)
                {
                    if (Piece.IsColor(piece, attackerColor) &&
                        (Piece.IsType(piece, Piece.Rook) || Piece.IsType(piece, Piece.Queen)))
                        return true;
                    break;
                }
                tf += straightFiles[d];
                tr += straightRanks[d];
            }
        }

        int[] diagFiles = { 1, -1, 1, -1 };
        int[] diagRanks = { 1, 1, -1, -1 };
        for (int d = 0; d < 4; d++)
        {
            int tf = file + diagFiles[d];
            int tr = rank + diagRanks[d];
            while (tf >= 0 && tf < 8 && tr >= 0 && tr < 8)
            {
                int piece = Squares[tr * 8 + tf];
                if (piece != Piece.None)
                {
                    if (Piece.IsColor(piece, attackerColor) &&
                        (Piece.IsType(piece, Piece.Bishop) || Piece.IsType(piece, Piece.Queen)))
                        return true;
                    break;
                }
                tf += diagFiles[d];
                tr += diagRanks[d];
            }
        }

        return false;
    }

    public static Vector2 IndexToPosition(int i)
    {
        int file = i % 8;
        int rank = i / 8;
        return new Vector2(-3.5f + file, -3.5f + rank) * BoardDrawer.squareSize;
    }

    public static void LoadPositionFromFen(string fen)
    {
        Squares          = new int[64];
        colorToMove      = Piece.White;
        halfMoveClock    = 0;
        fullMoveNumber   = 1;
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
                int type  = char.ToLower(c) switch
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
                    case 'K': SpecialMoves.castlingRights[1][1] = true; break;
                    case 'Q': SpecialMoves.castlingRights[1][0] = true; break;
                    case 'k': SpecialMoves.castlingRights[0][1] = true; break;
                    case 'q': SpecialMoves.castlingRights[0][0] = true; break;
                }
            }
        }

        if (sections.Length > 3 && sections[3] != "-")
        {
            int epFile = sections[3][0] - 'a';
            int epRank = sections[3][1] - '1';
            SpecialMoves.enPassantSquare = epRank * 8 + epFile;
        }

        if (sections.Length > 4) int.TryParse(sections[4], out halfMoveClock);
        if (sections.Length > 5) int.TryParse(sections[5], out fullMoveNumber);
    }

    public static string GetFen()
    {
        string[] pieceChars = { "", "k", "p", "n", "b", "r", "q" };
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int rank = 7; rank >= 0; rank--)
        {
            int empty = 0;
            for (int file = 0; file < 8; file++)
            {
                int sq    = rank * 8 + file;
                int piece = Squares[sq];

                if (piece == Piece.None)
                {
                    empty++;
                }
                else
                {
                    if (empty > 0) { sb.Append(empty); empty = 0; }
                    int type  = Piece.GetType(piece);
                    int color = Piece.GetColor(piece);
                    string ch = pieceChars[type];
                    sb.Append(color == Piece.White ? ch.ToUpper() : ch);
                }
            }
            if (empty > 0) sb.Append(empty);
            if (rank > 0) sb.Append('/');
        }

        sb.Append(colorToMove == Piece.White ? " w " : " b ");

        string castling = "";
        if (SpecialMoves.castlingRights[1][1]) castling += "K";
        if (SpecialMoves.castlingRights[1][0]) castling += "Q";
        if (SpecialMoves.castlingRights[0][1]) castling += "k";
        if (SpecialMoves.castlingRights[0][0]) castling += "q";
        sb.Append(castling.Length > 0 ? castling : "-");

        if (SpecialMoves.enPassantSquare != -1)
        {
            int ep  = SpecialMoves.enPassantSquare;
            char f  = (char)('a' + ep % 8);
            char r  = (char)('1' + ep / 8);
            sb.Append($" {f}{r}");
        }
        else
            sb.Append(" -");

        sb.Append($" {halfMoveClock} {fullMoveNumber}");

        return sb.ToString();
    }

    public static int PositionToIndex(Vector2 worldPos)
    {
        int file = Mathf.RoundToInt(worldPos.x / BoardDrawer.squareSize + 3.5f);
        int rank = Mathf.RoundToInt(worldPos.y / BoardDrawer.squareSize + 3.5f);
        if (file < 0 || file > 7 || rank < 0 || rank > 7) return -1;
        return rank * 8 + file;
    }
}