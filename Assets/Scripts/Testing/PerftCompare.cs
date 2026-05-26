using UnityEngine;
using System.Collections.Generic;

public static class PerftCompare
{
    static Dictionary<string, ulong> stockfishRoot = new Dictionary<string, ulong>
    {
        { "a2a3", 94405 }, { "b2b3", 81066 }, { "g2g3", 77468 }, { "d5d6", 79551 },
        { "a2a4", 90978 }, { "g2g4", 75677 }, { "g2h3", 82759 }, { "d5e6", 97464 },
        { "c3b1", 84773 }, { "c3d1", 84782 }, { "c3a4", 91447 }, { "c3b5", 81498 },
        { "e5d3", 77431 }, { "e5c4", 77752 }, { "e5g4", 79912 }, { "e5c6", 83885 },
        { "e5g6", 83866 }, { "e5d7", 93913 }, { "e5f7", 88799 }, { "d2c1", 83037 },
        { "d2e3", 90274 }, { "d2f4", 84869 }, { "d2g5", 87951 }, { "d2h6", 82323 },
        { "e2d1", 74963 }, { "e2f1", 88728 }, { "e2d3", 85119 }, { "e2c4", 84835 },
        { "e2b5", 79739 }, { "e2a6", 69334 }, { "a1b1", 83348 }, { "a1c1", 83263 },
        { "a1d1", 79695 }, { "h1f1", 81563 }, { "h1g1", 84876 }, { "f3d3", 83727 },
        { "f3e3", 92505 }, { "f3g3", 94461 }, { "f3h3", 98524 }, { "f3f4", 90488 },
        { "f3g4", 92037 }, { "f3f5", 104992 }, { "f3h5", 95034 }, { "f3f6", 77838 },
        { "e1d1", 79989 }, { "e1f1", 77887 }, { "e1g1", 86975 }, { "e1c1", 79803 }
    };

    public static void Compare(int depth)
    {
        // First find which top-level moves differ
        List<Move> moveList = MoveGeneration.GenerateLegalMoves();
        List<Move> wrongMoves = new List<Move>();

        foreach (Move move in moveList)
        {
            string name = MoveName(move);

            Board.MakeMove(move);
            ulong nodes = Perft.PerftCheck(depth - 1);
            Board.UndoMove(move);

            if (stockfishRoot.TryGetValue(name, out ulong expected) && nodes != expected)
            {
                Debug.Log($"DIFF {name}: mine={nodes} stockfish={expected} diff={((long)nodes - (long)expected):+#;-#;0}");
                wrongMoves.Add(move);
            }
        }

        if (wrongMoves.Count == 0)
        {
            Debug.Log("All moves match!");
            return;
        }

        // For each wrong move, load the resulting position as FEN and drill down one more level
        foreach (Move wrong in wrongMoves)
        {
            Debug.Log($"--- Drilling into {MoveName(wrong)} ---");

            Board.MakeMove(wrong);
            string fen = BoardToFen();
            Debug.Log($"FEN after {MoveName(wrong)}: {fen}");

            // Now compare at depth-1 within this position
            List<Move> subMoves = MoveGeneration.GenerateLegalMoves();
            foreach (Move sub in subMoves)
            {
                Board.MakeMove(sub);
                ulong nodes = Perft.PerftCheck(depth - 2);
                Board.UndoMove(sub);
                Debug.Log($"  {MoveName(wrong)} {MoveName(sub)}: {nodes}");
            }

            Board.UndoMove(wrong);
        }
    }

    static string BoardToFen()
    {
        string fen = "";
        for (int rank = 7; rank >= 0; rank--)
        {
            int empty = 0;
            for (int file = 0; file < 8; file++)
            {
                int piece = Board.Squares[rank * 8 + file];
                if (piece == Piece.None)
                    empty++;
                else
                {
                    if (empty > 0) { fen += empty; empty = 0; }
                    fen += PieceChar(piece);
                }
            }
            if (empty > 0) fen += empty;
            if (rank > 0) fen += "/";
        }

        fen += Board.colorToMove == Piece.White ? " w " : " b ";

        string castling = "";
        if (SpecialMoves.castlingRights[1][1]) castling += "K";
        if (SpecialMoves.castlingRights[1][0]) castling += "Q";
        if (SpecialMoves.castlingRights[0][1]) castling += "k";
        if (SpecialMoves.castlingRights[0][0]) castling += "q";
        fen += castling.Length > 0 ? castling : "-";

        if (SpecialMoves.enPassantSquare != -1)
        {
            int ep = SpecialMoves.enPassantSquare;
            fen += $" {(char)('a' + ep % 8)}{ep / 8 + 1}";
        }
        else
            fen += " -";

        return fen;
    }

    static char PieceChar(int piece)
    {
        int type = Piece.GetType(piece);
        bool white = Piece.IsColor(piece, Piece.White);
        char c = type switch
        {
            Piece.King   => 'k',
            Piece.Queen  => 'q',
            Piece.Rook   => 'r',
            Piece.Bishop => 'b',
            Piece.Knight => 'n',
            Piece.Pawn   => 'p',
            _            => '?'
        };
        return white ? char.ToUpper(c) : c;
    }

    static string MoveName(Move move)
    {
        return $"{SquareName(move.StartSquare)}{SquareName(move.TargetSquare)}";
    }

    static string SquareName(int index)
    {
        return $"{(char)('a' + index % 8)}{index / 8 + 1}";
    }
}