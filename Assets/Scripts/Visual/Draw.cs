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