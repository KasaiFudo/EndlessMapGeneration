using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settigns, Vector2 sampleCenter)
    {
        float[,] values = Noise.GenerateNoiseMap(width,height, settigns.noiseSettings, sampleCenter);

        AnimationCurve heightCurve_threadSave = new AnimationCurve(settigns.heightCurve.keys);

        float minValue = float.MinValue;
        float maxValue = float.MaxValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] *= heightCurve_threadSave.Evaluate(values[i,j]) * settigns.heightMultiplier;

                if (values[i, j] > maxValue)
                    maxValue = values[i, j];
                if (values[i, j] < minValue)
                    minValue = values[i, j];
            }
        }
        return new HeightMap(values, minValue, maxValue);
    }
}

public struct HeightMap
{
    public readonly float[,] values;

    public readonly float minValue;
    public readonly float maxValue;
    public HeightMap(float[,] heightMap, float minValue, float maxValue)
    {
        this.values = heightMap;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}