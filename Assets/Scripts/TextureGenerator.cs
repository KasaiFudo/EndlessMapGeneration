using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator 
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHightMap(float[,] hightMap)
    {
        int wight = hightMap.GetLength(0);
        int hight = hightMap.GetLength(1);

        Color[] colourMap = new Color[wight * hight];

        for (int y = 0; y < hight; y++)
        {
            for (int x = 0; x < wight; x++)
            {
                colourMap[y * wight + x] = Color.Lerp(Color.black, Color.white, hightMap[x, y]);
            }
        }
        return TextureFromColourMap(colourMap, wight, hight);
    }
}
