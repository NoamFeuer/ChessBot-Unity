using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class OpeningBook
{
    static Dictionary<string, Dictionary<string, int>> book;
    static System.Random rng = new System.Random();

    public static void Initialize(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("Opening book not found at: " + path);
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            Debug.Log($"JSON length: {json.Length} chars");
            Debug.Log($"JSON preview: {json.Substring(0, System.Math.Min(200, json.Length))}");

            book = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(json);

            if (book == null)
                Debug.LogError("Book deserialized to null!");
            else
                Debug.Log($"Opening book loaded: {book.Count:N0} positions");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load opening book: {e.Message}");
        }
    }

    public static bool TryGetMove(string fen, out string moveUci)
    {
        moveUci = null;
        if (book == null) return false;

        string[] parts = fen.Split(' ');
        if (parts.Length < 4) return false;

        // Try exact match first (with en passant)
        string fenKey = string.Join(" ", parts[0], parts[1], parts[2], parts[3]);

        // If not found, try without en passant
        if (!book.ContainsKey(fenKey))
            fenKey = string.Join(" ", parts[0], parts[1], parts[2], "-");

        Debug.Log($"Final key: '{fenKey}' exists: {book.ContainsKey(fenKey)}");

        if (!book.TryGetValue(fenKey, out var moves)) return false;
        if (moves.Count == 0) return false;

        int total = 0;
        foreach (var kvp in moves) total += kvp.Value;

        int rand       = rng.Next(total);
        int cumulative = 0;
        foreach (var kvp in moves)
        {
            cumulative += kvp.Value;
            if (rand < cumulative)
            {
                moveUci = kvp.Key;
                return true;
            }
        }

        return false;
    }
}