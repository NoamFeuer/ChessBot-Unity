using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ChessEvaluator
{
    static InferenceSession session;
    const int NUM_MOVES = 64 * 64 * 5;

    static int GetPlane(int piece)
    {
        int type  = piece & 0b00111;
        int color = piece & 0b11000;

        int typeIndex = type switch
        {
            Piece.Pawn   => 0,
            Piece.Knight => 1,
            Piece.Bishop => 2,
            Piece.Rook   => 3,
            Piece.Queen  => 4,
            Piece.King   => 5,
            _            => -1
        };

        if (typeIndex == -1) return -1;
        return color == Piece.White ? typeIndex : typeIndex + 6;
    }

    public static void Initialize(string modelPath)
    {
        session = new InferenceSession(modelPath);
        Debug.Log("Chess evaluator loaded.");
    }

    public static float Evaluate()
    {
        var (value, _) = RunModel(EncodeFromBoard());
        return Position.colorToMove == Piece.White ? value : -value;
    }

    public static (float value, float[] policy) EvaluateWithPolicy()
    {
        var (value, policy) = RunModel(EncodeFromBoard());
        float adjustedValue = Position.colorToMove == Piece.White ? value : -value;
        return (adjustedValue, policy);
    }

    public static float GetMoveScore(float[] policy, Move move)
    {
        int index = MoveToIndex(move);
        return policy[index];
    }

    public static int MoveToIndex(Move move)
    {
        int from = move.StartSquare;
        int to   = move.TargetSquare;

        int promoOffset = 0;
        if (move.IsPromotion)
        {
            promoOffset = move.PromotionType switch
            {
                Piece.Queen  => 1,
                Piece.Rook   => 2,
                Piece.Bishop => 3,
                Piece.Knight => 4,
                _            => 0
            } * 64 * 64;
        }

        return promoOffset + from * 64 + to;
    }

    static float[,,] EncodeFromBoard()
    {
        float[,,] planes = new float[19, 8, 8];

        for (int sq = 0; sq < 64; sq++)
        {
            int piece = Position.Squares[sq];
            if (piece == Piece.None) continue;
            int plane = GetPlane(piece);
            if (plane == -1) continue;
            planes[plane, sq / 8, sq % 8] = 1f;
        }

        for (int sq = 0; sq < 64; sq++)
        {
            if (Position.IsAttacked(sq, Piece.White)) planes[12, sq / 8, sq % 8] = 1f;
            if (Position.IsAttacked(sq, Piece.Black)) planes[13, sq / 8, sq % 8] = 1f;
        }

        int ep = SpecialMoves.enPassantSquare;
        if (ep != -1) planes[14, ep / 8, ep % 8] = 1f;

        if (SpecialMoves.castlingRights[1][1]) Fill(planes, 15);
        if (SpecialMoves.castlingRights[1][0]) Fill(planes, 16);
        if (SpecialMoves.castlingRights[0][1]) Fill(planes, 17);
        if (SpecialMoves.castlingRights[0][0]) Fill(planes, 18);

        return planes;
    }

    static (float value, float[] policy) RunModel(float[,,] planes)
    {
        DenseTensor<float> tensor = new DenseTensor<float>(new[] { 1, 19, 8, 8 });

        for (int p = 0; p < 19; p++)
            for (int r = 0; r < 8; r++)
                for (int f = 0; f < 8; f++)
                    tensor[0, p, r, f] = planes[p, r, f];

        List<NamedOnnxValue> inputs = new List<NamedOnnxValue> {
            NamedOnnxValue.CreateFromTensor("board", tensor)
        };

        using var results = session.Run(inputs);

        float value = results.First(r => r.Name == "value").AsEnumerable<float>().First() * 1500f;
        float[] policy = results.First(r => r.Name == "policy").AsEnumerable<float>().ToArray();

        return (value, policy);
    }

    static void Fill(float[,,] planes, int planeIdx)
    {
        for (int r = 0; r < 8; r++)
            for (int f = 0; f < 8; f++)
                planes[planeIdx, r, f] = 1f;
    }

    public static void Shutdown() => session?.Dispose();
}