using UnityEngine;

public static class MoveMaker
{
    public static bool MakeMove(Move move)
    {
        int movingPiece   = Position.Squares[move.StartSquare];
        int capturedPiece = Position.Squares[move.TargetSquare];

        int capturedPawnSquare = -1;

        if (move.EnPassantMove)
        {
            int direction = (Position.colorToMove == Piece.White) ? 1 : -1;
            capturedPawnSquare = move.TargetSquare - 8 * direction;
        }

        Position.history.Push(new GameHistory(
            movingPiece,
            capturedPiece,
            capturedPawnSquare,
            SpecialMoves.enPassantSquare,
            SpecialMoves.castlingRights
        ));

        Position.Squares[move.TargetSquare] = movingPiece;
        Position.Squares[move.StartSquare]  = Piece.None;

        if (move.PromotionType != Piece.None)
            Position.Squares[move.TargetSquare] = Piece.GetColor(movingPiece) | move.PromotionType;

        if (move.EnPassantMove)
            Position.Squares[capturedPawnSquare] = Piece.None;

        if (move.CastlingMove)
        {
            int backRank   = (Position.colorToMove == Piece.White) ? 0 : 7;
            int kingSquare = backRank * 8 + 4;

            if (move.TargetSquare == kingSquare + 2)
            {
                Position.Squares[backRank * 8 + 5] = Position.Squares[backRank * 8 + 7];
                Position.Squares[backRank * 8 + 7] = Piece.None;
            }
            else if (move.TargetSquare == kingSquare - 2)
            {
                Position.Squares[backRank * 8 + 3] = Position.Squares[backRank * 8 + 0];
                Position.Squares[backRank * 8 + 0] = Piece.None;
            }
        }

        SpecialMoves.UpdateEnPassant(move);
        SpecialMoves.UpdateCastling(move, capturedPiece);

        Position.colorToMove = (Position.colorToMove == Piece.White) ? Piece.Black : Piece.White;

        return true;
    }

    public static void UndoMove(Move move)
    {
        if (Position.history.Count == 0) return;

        GameHistory state = Position.history.Pop();

        Position.colorToMove = Position.colorToMove == Piece.White ? Piece.Black : Piece.White;
 
        Position.Squares[move.StartSquare]  = state.movingPiece;
        Position.Squares[move.TargetSquare] = state.capturedPiece;

        if (move.EnPassantMove && state.capturedPawnSquare != -1)
        {
            int capturedPawn = (Position.colorToMove == Piece.White) ? (Piece.Black | Piece.Pawn)
                                                            : (Piece.White | Piece.Pawn);
            Position.Squares[state.capturedPawnSquare] = capturedPawn;
        }

        if (move.CastlingMove)
        {
            int backRank   = (Position.colorToMove == Piece.White) ? 0 : 7;
            int kingSquare = backRank * 8 + 4;

            if (move.TargetSquare == kingSquare + 2)
            {
                Position.Squares[backRank * 8 + 7] = Position.Squares[backRank * 8 + 5];
                Position.Squares[backRank * 8 + 5] = Piece.None;
            }
            else
            {
                Position.Squares[backRank * 8 + 0] = Position.Squares[backRank * 8 + 3];
                Position.Squares[backRank * 8 + 3] = Piece.None;
            }
        }

        SpecialMoves.enPassantSquare = state.enPassantSquare;
        SpecialMoves.castlingRights  = state.castlingRights;
    }
}
