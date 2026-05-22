using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameHandler))]
public class GameHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameHandler handler = (GameHandler)target;

        handler.fen         = EditorGUILayout.TextField("Fen", handler.fen);
        handler.pieceDrawer = (PieceDrawer)EditorGUILayout.ObjectField("Piece Drawer", handler.pieceDrawer, typeof(PieceDrawer), true);
        handler.promotionHandler = (PawnPromotion)EditorGUILayout.ObjectField("Promotion Handler", handler.promotionHandler, typeof(PawnPromotion), true);
        handler.mode        = (GameHandler.Mode)EditorGUILayout.EnumPopup("Mode", handler.mode);

        bool isPerft = handler.mode == GameHandler.Mode.PerftTesting || 
                       handler.mode == GameHandler.Mode.PerftTestingInfo;

        if (isPerft)
            handler.depth = EditorGUILayout.IntField("Depth", handler.depth);
        else
        {
            handler.white = (GameHandler.PlayerType)EditorGUILayout.EnumPopup("White", handler.white);
            handler.black = (GameHandler.PlayerType)EditorGUILayout.EnumPopup("Black", handler.black);
        }

        if (GUI.changed)
            EditorUtility.SetDirty(handler);
    }
}