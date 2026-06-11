using System.Collections.Generic;
using UnityEngine;

public static class SpecialMoves
{
    public static bool[][] castlingRights = new bool[2][]
    {
        new bool[] { true, true },
        new bool[] { true, true }
    };

    public static int enPassantSquare = -1;

    public static void UpdateEnPassant(Move move)
    {
        if (Piece.IsType(move.MovingPiece, Piece.Pawn) && System.Math.Abs(move.TargetSquare - move.StartSquare) == 16)
            enPassantSquare = (move.StartSquare + move.TargetSquare) / 2;
        else
            enPassantSquare = -1;
    }

    public static List<Move> GetEPMoves()
    {
        List<Move> epMoves = new List<Move>();

        if (enPassantSquare == -1) return epMoves; 

        int direction = (Position.colorToMove == Piece.White) ? 1 : -1;
        int epFile = enPassantSquare % 8;
        int epRank = enPassantSquare / 8;

        int[] fileDeltas = { -1, 1 };
        foreach (int fd in fileDeltas)
        {
            int attackerFile = epFile + fd;
            int attackerRank = epRank - direction;

            if (attackerFile < 0 || attackerFile > 7) continue;
            if (attackerRank < 0 || attackerRank > 7) continue;

            int attackerSquare = attackerRank * 8 + attackerFile;
            int piece = Position.Squares[attackerSquare];

            if (!Piece.IsType(piece, Piece.Pawn)) continue;
            if (!Piece.IsColor(piece, Position.colorToMove)) continue;

            epMoves.Add(new Move(attackerSquare, enPassantSquare, enPassantMove: true));
        }

        return epMoves;
    }

    public static void UpdateCastling(Move move, int capturedPiece)
    {
        int piece = move.MovingPiece;

        // King moves, revoke both sides
        if (Piece.IsType(piece, Piece.King))
        {
            int colorIndex = Piece.IsColor(piece, Piece.White) ? 1 : 0;
            castlingRights[colorIndex][0] = false;
            castlingRights[colorIndex][1] = false;
        }

        // Rook moves, revoke that side only
        if (Piece.IsType(piece, Piece.Rook))
        {
            int start = move.StartSquare;
            if (start == 7)  castlingRights[1][1] = false; // white kingside  (h1)
            if (start == 0)  castlingRights[1][0] = false; // white queenside (a1)
            if (start == 63) castlingRights[0][1] = false; // black kingside  (h8)
            if (start == 56) castlingRights[0][0] = false; // black queenside (a8)
        }

        // Rook captured, revoke that side too (use passed-in capturedPiece)
        if (Piece.IsType(capturedPiece, Piece.Rook))
        {
            int target = move.TargetSquare;
            if (target == 7)  castlingRights[1][1] = false;
            if (target == 0)  castlingRights[1][0] = false;
            if (target == 63) castlingRights[0][1] = false;
            if (target == 56) castlingRights[0][0] = false;
        }
    }

    public static List<Move> GetCastlingMoves()
    {
        List<Move> moves = new List<Move>();

        bool isWhite    = Position.colorToMove == Piece.White;
        int colorIndex  = isWhite ? 1 : 0;
        int backRank    = isWhite ? 0 : 7;
        int kingSquare  = backRank * 8 + 4;
        int attackColor = isWhite ? Piece.Black : Piece.White;

        // Kingside
        if (castlingRights[colorIndex][1])
        {
            int f = backRank * 8 + 5;
            int g = backRank * 8 + 6;
            int rookSquare = backRank * 8 + 7;

            bool pathClear      = Position.Squares[f] == Piece.None && Position.Squares[g] == Piece.None;
            bool rookPresent    = Piece.IsType(Position.Squares[rookSquare], Piece.Rook);
            bool notUnderAttack = !Position.IsSquareAttacked(kingSquare, attackColor)
                            && !Position.IsSquareAttacked(f, attackColor)
                            && !Position.IsSquareAttacked(g, attackColor);

            if (pathClear && rookPresent && notUnderAttack)
                moves.Add(new Move(kingSquare, g, castlingMove: true));
        }

        // Queenside
        if (castlingRights[colorIndex][0])
        {
            int b = backRank * 8 + 1;
            int c = backRank * 8 + 2;
            int d = backRank * 8 + 3;
            int rookSquare = backRank * 8 + 0;

            bool pathClear      = Position.Squares[b] == Piece.None && Position.Squares[c] == Piece.None && Position.Squares[d] == Piece.None;
            bool rookPresent    = Piece.IsType(Position.Squares[rookSquare], Piece.Rook);
            bool notUnderAttack = !Position.IsSquareAttacked(kingSquare, attackColor)
                            && !Position.IsSquareAttacked(d, attackColor)
                            && !Position.IsSquareAttacked(c, attackColor);

            if (pathClear && rookPresent && notUnderAttack)
                moves.Add(new Move(kingSquare, c, castlingMove: true));
        }

        return moves;
    }

    public static void ExecuteCastling(Move move)
    {
        bool isWhite = Position.colorToMove == Piece.White;
        int backRank = isWhite ? 0 : 7;

        int kingFrom = backRank * 8 + 4;
        int target   = move.TargetSquare;

        Position.Squares[target]   = Position.Squares[kingFrom];
        Position.Squares[kingFrom] = Piece.None;

        if (target % 8 == 6)
        {
            int rookFrom = backRank * 8 + 7;
            int rookTo   = backRank * 8 + 5;
            Position.Squares[rookTo]   = Position.Squares[rookFrom];
            Position.Squares[rookFrom] = Piece.None;
        }
        else
        {
            int rookFrom = backRank * 8 + 0;
            int rookTo   = backRank * 8 + 3;
            Position.Squares[rookTo]   = Position.Squares[rookFrom];
            Position.Squares[rookFrom] = Piece.None;
        }
    }
}
