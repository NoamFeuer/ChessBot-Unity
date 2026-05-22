using UnityEngine;

[CreateAssetMenu(fileName = "PieceTextures", menuName = "Chess/Piece Textures")]
public class PieceTextures : ScriptableObject
{
    [Header("White Pieces")]
    public Texture2D whiteKing;
    public Texture2D whiteQueen;
    public Texture2D whiteRook;
    public Texture2D whiteBishop;
    public Texture2D whiteKnight;
    public Texture2D whitePawn;

    [Header("Black Pieces")]
    public Texture2D blackKing;
    public Texture2D blackQueen;
    public Texture2D blackRook;
    public Texture2D blackBishop;
    public Texture2D blackKnight;
    public Texture2D blackPawn;

    public Texture2D Get(int piece)
    {
        return piece switch
        {
            var p when p == (Piece.White | Piece.King)   => whiteKing,
            var p when p == (Piece.White | Piece.Queen)  => whiteQueen,
            var p when p == (Piece.White | Piece.Rook)   => whiteRook,
            var p when p == (Piece.White | Piece.Bishop) => whiteBishop,
            var p when p == (Piece.White | Piece.Knight) => whiteKnight,
            var p when p == (Piece.White | Piece.Pawn)   => whitePawn,
            var p when p == (Piece.Black | Piece.King)   => blackKing,
            var p when p == (Piece.Black | Piece.Queen)  => blackQueen,
            var p when p == (Piece.Black | Piece.Rook)   => blackRook,
            var p when p == (Piece.Black | Piece.Bishop) => blackBishop,
            var p when p == (Piece.Black | Piece.Knight) => blackKnight,
            var p when p == (Piece.Black | Piece.Pawn)   => blackPawn,
            _ => null
        };
    }
}
