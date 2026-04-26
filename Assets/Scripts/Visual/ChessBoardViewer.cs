using UnityEngine;

public class ChessBoardViewer : MonoBehaviour
{
    public Color lightCol;
    public Color darkCol;
    public float squareSize = 1.5f;

    void Start()
    {
        CreateGraphicalBoard();
    }

    void CreateGraphicalBoard()
    {
        for (int file = 0; file < 8; file++)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                bool lightSquare = (file + rank) % 2 != 0;

                Color squareColor = (lightSquare) ? lightCol : darkCol;
                Vector2 position = new Vector2((-3.5f + file) * squareSize, (-3.5f + rank) * squareSize);

                Draw.Square(position, squareColor, transform, squareSize);
            }
        }
    }
}