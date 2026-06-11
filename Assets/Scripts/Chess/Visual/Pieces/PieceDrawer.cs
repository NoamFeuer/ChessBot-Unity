using UnityEngine;

public class PieceDrawer : MonoBehaviour
{
    public PieceTextures pieceTextures;
    public Transform parent;
    public float size = 1f;

    public void DrawPieces()
    {
        foreach (var kvp in MovePieces.pieceObjects)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }
        MovePieces.pieceObjects.Clear();

        for (int i = 0; i < Position.Squares.Length; i++)
        {
            int square = Position.Squares[i];
            if (square == Piece.None) continue;

            Texture2D texture = pieceTextures.Get(square);
            if (texture == null) continue;

            Vector2 position = Position.IndexToPosition(i);
            GameObject piece = Draw.Piece(texture, position, transform, size);
            MovePieces.pieceObjects[i] = piece;
        }
    }
}
