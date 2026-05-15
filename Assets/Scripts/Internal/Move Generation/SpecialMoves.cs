using System.Collections.Generic;

public static class SpecialMoves
{
    public static int enPassantSquare = -1;

    public static void UpdateEnPassant(Move move)
    {
        int piece = Board.Squares[move.TargetSquare];

        if (Piece.IsType(piece, Piece.Pawn) && System.Math.Abs(move.TargetSquare - move.StartSquare) == 16)
            enPassantSquare = (move.StartSquare + move.TargetSquare) / 2;
        else
            enPassantSquare = -1;
    }

    public static List<Move> GetEPMoves()
    {
        List<Move> epMoves = new List<Move>();

        if (enPassantSquare == -1) return epMoves;

        int direction = (Board.colorToMove == Piece.White) ? 1 : -1;
        int epFile    = enPassantSquare % 8;
        int epRank    = enPassantSquare / 8;

        int[] fileDeltas = { -1, 1 };
        foreach (int fd in fileDeltas)
        {
            int attackerFile = epFile + fd;
            int attackerRank = epRank - direction;

            if (attackerFile < 0 || attackerFile > 7) continue;
            if (attackerRank < 0 || attackerRank > 7) continue;

            int attackerSquare = attackerRank * 8 + attackerFile;
            int piece = Board.Squares[attackerSquare];

            if (!Piece.IsType(piece, Piece.Pawn)) continue;
            if (!Piece.IsColor(piece, Board.colorToMove)) continue;

            epMoves.Add(new Move(attackerSquare, enPassantSquare, enPassantMove: true));
        }

        return epMoves;
    }
}