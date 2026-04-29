using Unity.VisualScripting;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public enum PlayerType { Human, Bot }

    public string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public PieceDrawer pieceDrawer;
    public PlayerType white;
    public PlayerType black;

    void Start()
    {
        Board.LoadPositionFromFen(fen);

        pieceDrawer.DrawPieces();
    }
}