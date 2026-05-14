// using System.Collections.Generic;
// using UnityEngine;

// public static class SpecialMoves
// {
//     static bool whiteCastleKingside = true;
//     static bool whiteCastleQueenside = true;
//     static bool blackCastleKingside = true;
//     static bool blackCastleQueenside = true;

//     public static List<Move> GetCastlingMoves()
//     {
//         List<Move> castlingMoves = new List<Move>();

//         if (Board.colorToMove == Piece.White)
//         {
//             if (whiteCastleKingside &&
//                 Board.Squares[PMD.whiteKingIndex] == (Piece.White | Piece.King) &&
//                 Board.Squares[PMD.whiteRookIndexes[1]] == (Piece.White | Piece.Rook) &&
//                 Board.Squares[PMD.whiteKingIndex + 1] == Piece.None &&
//                 Board.Squares[PMD.whiteKingIndex + 2] == Piece.None &&
//                 !Board.IsSquareAttacked(PMD.whiteKingIndex) &&
//                 !Board.IsSquareAttacked(PMD.whiteKingIndex + 1) &&
//                 !Board.IsSquareAttacked(PMD.whiteKingIndex + 2))
//             {
//                 castlingMoves.Add(
//                     new Move(
//                         PMD.whiteKingIndex,
//                         PMD.whiteKingIndex + 2,
//                         castelingMove: true
//                     )
//                 );
//             }

//             if (whiteCastleQueenside &&
//                 Board.Squares[PMD.whiteKingIndex] == (Piece.White | Piece.King) &&
//                 Board.Squares[PMD.whiteRookIndexes[0]] == (Piece.White | Piece.Rook) &&
//                 Board.Squares[PMD.whiteKingIndex - 1] == Piece.None &&
//                 Board.Squares[PMD.whiteKingIndex - 2] == Piece.None &&
//                 Board.Squares[PMD.whiteKingIndex - 3] == Piece.None &&
//                 !Board.IsSquareAttacked(PMD.whiteKingIndex) &&
//                 !Board.IsSquareAttacked(PMD.whiteKingIndex - 1) &&
//                 !Board.IsSquareAttacked(PMD.whiteKingIndex - 2))
//             {
//                 castlingMoves.Add(
//                     new Move(
//                         PMD.whiteKingIndex,
//                         PMD.whiteKingIndex - 2,
//                         castelingMove: true
//                     )
//                 );
//             }
//         }
//         else
//         {
//             if (blackCastleKingside &&
//                 Board.Squares[PMD.blackKingIndex] == (Piece.Black | Piece.King) &&
//                 Board.Squares[PMD.blackRookIndexes[1]] == (Piece.Black | Piece.Rook) &&
//                 Board.Squares[PMD.blackKingIndex + 1] == Piece.None &&
//                 Board.Squares[PMD.blackKingIndex + 2] == Piece.None &&
//                 !Board.IsSquareAttacked(PMD.blackKingIndex) &&
//                 !Board.IsSquareAttacked(PMD.blackKingIndex + 1) &&
//                 !Board.IsSquareAttacked(PMD.blackKingIndex + 2))
//             {
//                 castlingMoves.Add(
//                     new Move(
//                         PMD.blackKingIndex,
//                         PMD.blackKingIndex + 2,
//                         castelingMove: true
//                     )
//                 );
//             }

//             if (blackCastleQueenside &&
//                 Board.Squares[PMD.blackKingIndex] == (Piece.Black | Piece.King) &&
//                 Board.Squares[PMD.blackRookIndexes[0]] == (Piece.Black | Piece.Rook) &&
//                 Board.Squares[PMD.blackKingIndex - 1] == Piece.None &&
//                 Board.Squares[PMD.blackKingIndex - 2] == Piece.None &&
//                 Board.Squares[PMD.blackKingIndex - 3] == Piece.None &&
//                 !Board.IsSquareAttacked(PMD.blackKingIndex) &&
//                 !Board.IsSquareAttacked(PMD.blackKingIndex - 1) &&
//                 !Board.IsSquareAttacked(PMD.blackKingIndex - 2))
//             {
//                 castlingMoves.Add(
//                     new Move(
//                         PMD.blackKingIndex,
//                         PMD.blackKingIndex - 2,
//                         castelingMove: true
//                     )
//                 );
//             }
//         }

//         return castlingMoves;
//     }

//     public static void UpdateCastelingRights(Move move)
//     {
//         if (Board.colorToMove == Piece.White)
//         {
//             if (Piece.IsType(move.MovingPiece, Piece.King))
//             {
//                 whiteCastleKingside = false;
//                 whiteCastleQueenside = false;
//             }

//             if (move.StartSquare == PMD.whiteRookIndexes[0] ||
//                 move.TargetSquare == PMD.whiteRookIndexes[0])
//                 whiteCastleQueenside = false;

//             if (move.StartSquare == PMD.whiteRookIndexes[1] ||
//                 move.TargetSquare == PMD.whiteRookIndexes[1])
//                 whiteCastleKingside = false;

//             if (move.TargetSquare == PMD.blackRookIndexes[0])
//                 blackCastleQueenside = false;

//             if (move.TargetSquare == PMD.blackRookIndexes[1])
//                 blackCastleKingside = false;
//         }
//         else
//         {
//             if (Piece.IsType(move.MovingPiece, Piece.King))
//             {
//                 blackCastleKingside = false;
//                 blackCastleQueenside = false;
//             }

//             if (move.StartSquare == PMD.blackRookIndexes[0] ||
//                 move.TargetSquare == PMD.blackRookIndexes[0])
//                 blackCastleQueenside = false;

//             if (move.StartSquare == PMD.blackRookIndexes[1] ||
//                 move.TargetSquare == PMD.blackRookIndexes[1])
//                 blackCastleKingside = false;

//             if (move.TargetSquare == PMD.whiteRookIndexes[0])
//                 whiteCastleQueenside = false;

//             if (move.TargetSquare == PMD.whiteRookIndexes[1])
//                 whiteCastleKingside = false;
//         }
//     }

//     public static void ResetCastlingRights()
//     {
//         whiteCastleKingside = true;
//         whiteCastleQueenside = true;
//         blackCastleKingside = true;
//         blackCastleQueenside = true;
//     }
// }