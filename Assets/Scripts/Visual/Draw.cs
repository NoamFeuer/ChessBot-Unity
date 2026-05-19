using UnityEngine;

public static class Draw
{
    public static GameObject Square(Vector2 position, Color color, Transform parent, float size = 1f)
    {
        GameObject square = new GameObject("Square");
        square.transform.position = new Vector3(position.x, position.y, 0);
        square.transform.parent = parent;

        SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSquareSprite(size);
        renderer.color = color;
        renderer.sortingOrder = 0;

        return square;
    }

    public static GameObject Piece(Texture2D pieceTexture, Vector2 position, Transform parent, float size = 1f)
    {
        GameObject piece = new GameObject("Piece");
        piece.transform.position = new Vector3(position.x, position.y, 0);
        piece.transform.parent = parent;

        piece.AddComponent<BoxCollider2D>();

        SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(
            pieceTexture,
            new Rect(0, 0, pieceTexture.width, pieceTexture.height),
            new Vector2(0.5f, 0.5f),
            pieceTexture.width / size
        );
        renderer.sortingOrder = 1;

        return piece;
    }

    static Sprite CreateSquareSprite(float size)
    {
        int pixelSize = 10;
        Texture2D texture = new Texture2D(pixelSize, pixelSize);

        Color[] pixels = new Color[pixelSize * pixelSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, pixelSize, pixelSize), new Vector2(0.5f, 0.5f), pixelSize / size);
    }
}