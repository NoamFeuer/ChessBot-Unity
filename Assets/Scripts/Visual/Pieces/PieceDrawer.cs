using UnityEngine;

public class PieceDrawer : MonoBehaviour
{
    public PieceTextures pieceTextures;
    public Transform parent;
    public float size = 1f;

    void Start()
    {
        DrawPieces();
    }

   void DrawPieces()
    {
        for (int i = 0; i < Board.Squares.Length; i++)
        {
            int square = Board.Squares[i];
            if (square == Piece.None) continue;

            Texture2D texture = pieceTextures.Get(square);
            if (texture == null) continue;

            Vector2 position = Board.IndexToPosition(i);
            GameObject piece = Draw.Piece(texture, position, transform, size);
            MovePieces.pieceObjects[i] = piece; // register it
        }
    }
}