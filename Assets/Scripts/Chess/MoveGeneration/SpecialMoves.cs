using System.Collections.Generic;
using UnityEngine;

public static class SpecialMoves
{
    public static int enPassantSquare = -1;
    public static bool[][] castlingRights = new bool[2][]
    {
        new bool[] { false, false },
        new bool[] { false, false }
    };

    public static void UpdateEnPassant(Move move)
    {
        enPassantSquare = -1;

        if (!Piece.IsType(move.MovingPiece, Piece.Pawn)) return;

        int diff = move.TargetSquare - move.StartSquare;
        if (System.Math.Abs(diff) == 16)
            enPassantSquare = move.StartSquare + diff / 2;
    }

    public static void UpdateCastling(Move move, int capturedPiece)
    {
        int piece = move.MovingPiece;

        // If king moves, revoke both rights.
        if (Piece.IsType(piece, Piece.King))
        {
            int colorIndex = Piece.GetColor(piece) == Piece.White ? 1 : 0;
            castlingRights[colorIndex][0] = false;
            castlingRights[colorIndex][1] = false;
        }

        // If rook moves, revoke rights for that side
        if (Piece.IsType(piece, Piece.Rook))
        {
            if (move.StartSquare == 0)  castlingRights[1][0] = false; // white queenside
            if (move.StartSquare == 7)  castlingRights[1][1] = false; // white kingside
            if (move.StartSquare == 56) castlingRights[0][0] = false; // black queenside
            if (move.StartSquare == 63) castlingRights[0][1] = false; // black kingside
        }

        // Revoke if a rook is captured on its starting square
        if (Piece.IsType(capturedPiece, Piece.Rook))
        {
            if (move.TargetSquare == 0)  castlingRights[1][0] = false;
            if (move.TargetSquare == 7)  castlingRights[1][1] = false;
            if (move.TargetSquare == 56) castlingRights[0][0] = false;
            if (move.TargetSquare == 63) castlingRights[0][1] = false;
        }
    }

    public static List<Move> GetCastlingMoves()
    {
        List<Move> moves = new List<Move>();

        bool isWhite = Position.colorToMove == Piece.White;
        int colorIndex = isWhite ? 1 : 0;
        int backRank = isWhite ? 0 : 7;
        int kingSquare = backRank * 8 + 4;
        int attackColor = isWhite ? Piece.Black : Piece.White;

        // King must actually be on its starting square
        if (!Piece.IsType(Position.Squares[kingSquare], Piece.King) ||
            !Piece.IsColor(Position.Squares[kingSquare], Position.colorToMove))
            return moves;

        // Kingside
        if (castlingRights[colorIndex][1])
        {
            int f = backRank * 8 + 5;
            int g = backRank * 8 + 6;
            int rookSquare = backRank * 8 + 7;

            bool pathClear = Position.Squares[f] == Piece.None &&
                             Position.Squares[g] == Piece.None;
            bool rookPresent = Position.Squares[rookSquare] != Piece.None &&
                               Piece.IsType(Position.Squares[rookSquare], Piece.Rook) &&
                               Piece.IsColor(Position.Squares[rookSquare], Position.colorToMove);
            bool notUnderAttack = !Position.IsAttacked(kingSquare, attackColor) &&
                                  !Position.IsAttacked(f, attackColor) &&
                                  !Position.IsAttacked(g, attackColor);

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

            bool pathClear = Position.Squares[b] == Piece.None &&
                             Position.Squares[c] == Piece.None &&
                                  Position.Squares[d] == Piece.None;
            bool rookPresent = Position.Squares[rookSquare] != Piece.None &&
                               Piece.IsType(Position.Squares[rookSquare], Piece.Rook) &&
                               Piece.IsColor(Position.Squares[rookSquare], Position.colorToMove);
            bool notUnderAttack = !Position.IsAttacked(kingSquare, attackColor) &&
                                  !Position.IsAttacked(d, attackColor) &&
                                  !Position.IsAttacked(c, attackColor);

            if (pathClear && rookPresent && notUnderAttack)
                moves.Add(new Move(kingSquare, c, castlingMove: true));
        }

        return moves;
    }

    public static List<Move> GetEPMoves()
    {
        List<Move> moves = new List<Move>();
        if (enPassantSquare == -1) return moves;

        bool isWhite = Position.colorToMove == Piece.White;
        int direction = isWhite ? -1 : 1;
        int epRank = enPassantSquare / 8;
        int epFile = enPassantSquare % 8;

        int[] fileDeltas = { -1, 1 };
        foreach (int fd in fileDeltas)
        {
            int attackerFile = epFile + fd;
            int attackerRank = epRank + direction;

            if (attackerFile < 0 || attackerFile > 7) continue;
            if (attackerRank < 0 || attackerRank > 7) continue;

            int attackerSquare = attackerRank * 8 + attackerFile;
            int piece = Position.Squares[attackerSquare];

            if (!Piece.IsType(piece, Piece.Pawn)) continue;
            if (!Piece.IsColor(piece, Position.colorToMove)) continue;

            moves.Add(new Move(attackerSquare, enPassantSquare, enPassantMove: true));
        }

        return moves;
    }
}
