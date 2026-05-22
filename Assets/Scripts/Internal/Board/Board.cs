using UnityEngine;
using System.Collections.Generic;

public static class Board
{
    public static Stack<GameHistory> history = new Stack<GameHistory>();
    public static int[] Squares;
    public static int colorToMove = Piece.White;

    static Board()
    {
        Squares = new int[64];
    }

    public static bool MakeMove(Move move)
    {
        int movingPiece   = Squares[move.StartSquare];
        int capturedPiece = Squares[move.TargetSquare];

        int capturedPawnSquare = -1;

        if (move.EnPassantMove)
        {
            int direction = (colorToMove == Piece.White) ? 1 : -1;
            capturedPawnSquare = move.TargetSquare - 8 * direction;
        }

        // Save moving piece in history before modifying the board
        history.Push(new GameHistory(
            movingPiece,
            capturedPiece,
            capturedPawnSquare,
            SpecialMoves.enPassantSquare,
            SpecialMoves.castlingRights
        ));

        Squares[move.TargetSquare] = movingPiece;
        Squares[move.StartSquare]  = Piece.None;

        if (move.PromotionType != Piece.None)
            Squares[move.TargetSquare] = Piece.GetColor(movingPiece) | move.PromotionType;

        if (move.EnPassantMove)
            Squares[capturedPawnSquare] = Piece.None;

        if (move.CastlingMove)
        {
            int backRank   = (colorToMove == Piece.White) ? 0 : 7;
            int kingSquare = backRank * 8 + 4;

            if (move.TargetSquare == kingSquare + 2)
            {
                Squares[backRank * 8 + 5] = Squares[backRank * 8 + 7];
                Squares[backRank * 8 + 7] = Piece.None;
            }
            else if (move.TargetSquare == kingSquare - 2)
            {
                Squares[backRank * 8 + 3] = Squares[backRank * 8 + 0];
                Squares[backRank * 8 + 0] = Piece.None;
            }
        }

        SpecialMoves.UpdateEnPassant(move);
        SpecialMoves.UpdateCastling(move, capturedPiece);

        colorToMove = (colorToMove == Piece.White) ? Piece.Black : Piece.White;

        return true;
    }

    public static void UndoMove(Move move)
    {
        if (history.Count == 0) return;

        GameHistory state = history.Pop();

        colorToMove = colorToMove == Piece.White ? Piece.Black : Piece.White;

        // Restore the moving piece from history instead of reading target square
        Squares[move.StartSquare]  = state.movingPiece;
        Squares[move.TargetSquare] = state.capturedPiece;

        if (move.EnPassantMove && state.capturedPawnSquare != -1)
        {
            int capturedPawn = (colorToMove == Piece.White) ? (Piece.Black | Piece.Pawn)
                                                            : (Piece.White | Piece.Pawn);
            Squares[state.capturedPawnSquare] = capturedPawn;
        }

        if (move.CastlingMove)
        {
            int backRank   = (colorToMove == Piece.White) ? 0 : 7;
            int kingSquare = backRank * 8 + 4;

            if (move.TargetSquare == kingSquare + 2)
            {
                Squares[backRank * 8 + 7] = Squares[backRank * 8 + 5];
                Squares[backRank * 8 + 5] = Piece.None;
            }
            else
            {
                Squares[backRank * 8 + 0] = Squares[backRank * 8 + 3];
                Squares[backRank * 8 + 3] = Piece.None;
            }
        }

        SpecialMoves.enPassantSquare = state.enPassantSquare;
        SpecialMoves.castlingRights  = state.castlingRights;
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
        colorToMove = attackerColor;

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

        int file = 0;
        int rank = 0;

        foreach (char c in fen)
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
    }

    public static int PositionToIndex(Vector2 worldPos)
    {
        int file = Mathf.RoundToInt(worldPos.x / BoardDrawer.squareSize + 3.5f);
        int rank = Mathf.RoundToInt(worldPos.y / BoardDrawer.squareSize + 3.5f);

        if (file < 0 || file > 7 || rank < 0 || rank > 7) return -1;

        return rank * 8 + file;
    }
}
 