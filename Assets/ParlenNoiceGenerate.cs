using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public int mDivisions;
    public float mSize;
    public float mHight;

    public void Start()
    {
        CreateTerrain();
    }
    private void CreateTerrain()
    {
        int divsPlusOne = mDivisions + 1;
        int vertCount = divsPlusOne * divsPlusOne;
        Vector3[] verts = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] tris = new int[mDivisions * mDivisions * 6];

        float halfSize = mSize * 0.5f;
        float divisionSize = mSize / mDivisions;

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int triOffset = 0;

        for(int i = 0; i <= mDivisions; i++)
        {
            for(int j = 0; j <= mDivisions; j++)
            {

                float xCoord = (float)j / mDivisions;
                float yCoord = (float)i / mDivisions;
                float hight = Mathf.PerlinNoise(xCoord * 4, yCoord * 4) + 0.5f * Mathf.PerlinNoise(xCoord * 8, yCoord * 8) + 0.25f * Mathf.PerlinNoise(xCoord * 16, yCoord * 16);

                hight = Mathf.Pow(hight, 2);

                hight *= mHight; 

                verts[i * divsPlusOne + j] = new Vector3(-halfSize + j * divisionSize, hight, halfSize - i * divisionSize);
                uvs[i * divsPlusOne + j] = new Vector2((float)j/ mDivisions, (float)i / mDivisions);

                if(i < mDivisions && j < mDivisions)
                {
                    int topLeft = i * divsPlusOne + j;
                    int bottomLeft = (i+1) * divsPlusOne + j;

                    tris[triOffset] = topLeft;
                    tris[triOffset + 1] = topLeft + 1;
                    tris[triOffset + 2] = bottomLeft + 1;

                    tris[triOffset + 3] = topLeft;
                    tris[triOffset + 4] = bottomLeft + 1;
                    tris[triOffset + 5] = bottomLeft;

                    triOffset += 6;
                }
            }
        }

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
