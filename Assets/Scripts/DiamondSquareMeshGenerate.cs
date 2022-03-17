using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = System.Random;

public class DiamondSquareMeshGenerate : MonoBehaviour
{
    [SerializeField] private int terrainSeperation;
    [SerializeField] private float terrainSize;
    [SerializeField] private float terrainAltitude;

    private Color[] colors;
    public Gradient colorGradient;

    private Vector3[] terrainVertices;
    private int terrainVerticesCount;
    private float terrainAltitudeMax;
    private float terrainAltitudeMin;
    
    public int TerrainDissection
    {
        get { return terrainSeperation; }
        set { terrainSeperation = value; }
    }

    public float TerrainSize
    {
        get { return terrainSize; }
        set { terrainSize = value; }
    }

    public float TerrainAltitude
    {
        get { return terrainAltitude; }
        set { terrainAltitude = value; }
    }

    private void Start()
    {
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        terrainAltitude = UnityEngine.Random.Range(3, 6);
        terrainVerticesCount = (int) Mathf.Pow(terrainSeperation + 1, 2); //To calculate the vertex count
        terrainVertices = new Vector3[terrainVerticesCount]; //Initialize vertex array
        int[] triangles = new int[((int)Mathf.Pow(terrainSeperation, 2)) * 6]; //triangles required for each terrain
        Vector2[] uvs = new Vector2[terrainVerticesCount]; //Creates UVs for vertex array
        
        float terrainHalfSize = terrainSize / 2f;
        float seperationSize = terrainSize / terrainSeperation;

        Mesh terrainMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = terrainMesh;

        int triangleOffset = 0;

        for (int i = 0; i <= terrainSeperation; i++)
        {
            for (int j = 0; j <= terrainSeperation; j++)
            {
                terrainVertices[(i * (terrainSeperation + 1)) + j] = new Vector3(-terrainHalfSize+j*seperationSize,0.0f, terrainHalfSize-i*seperationSize);
                uvs[i * (terrainSeperation + 1) + j] = new Vector2((float) i / terrainSeperation, (float) j / terrainSeperation);

                if (i < terrainSeperation && j < terrainSeperation)
                {
                    int topLeftVertex = i * (terrainSeperation + 1) + j;
                    int bottomLeftVertex = (i + 1) * (terrainSeperation + 1) + j;

                    triangles[triangleOffset] = topLeftVertex;
                    triangles[triangleOffset + 1] = topLeftVertex + 1;
                    triangles[triangleOffset + 2] = bottomLeftVertex + 1; //Creates first set of triangle in square

                    triangles[triangleOffset + 3] = topLeftVertex;
                    triangles[triangleOffset + 4] = bottomLeftVertex + 1;
                    triangles[triangleOffset + 5] = bottomLeftVertex; //Creates second set of triangle in square

                    triangleOffset += 6;
                }
            }
        }

        //Setting corner points to initial value
        terrainVertices[0].y = UnityEngine.Random.Range(-terrainAltitude, terrainAltitude);
        terrainVertices[terrainSeperation].y = UnityEngine.Random.Range(-terrainAltitude, terrainAltitude);
        terrainVertices[(terrainVertices.Length) - 1].y = UnityEngine.Random.Range(-terrainAltitude, terrainAltitude);
        terrainVertices[((terrainVertices.Length) - 1) - terrainSeperation].y = UnityEngine.Random.Range(-terrainAltitude, terrainAltitude);

        int iterations = (int) Mathf.Log(terrainSeperation, 2);
        int numberOfSquares = 1;
        int squareSize = terrainSeperation;
        for (int i = 0; i < iterations; i++)
        {
            int row = 0;
            for (int j = 0; j < numberOfSquares; j++)
            {
                int column = 0;
                for (int k = 0; k < numberOfSquares; k++)
                {
                    DiamondSquareAlgorithm(row, column, squareSize, terrainAltitude);
                    
                    column += squareSize;
                }

                row += squareSize;
            }
            
            numberOfSquares *= 2;
            squareSize /= 2;
            terrainAltitude /= 2f;
        }
        
        //Set a custom range for vertices to apply the gradient
        terrainAltitudeMax = -terrainAltitude;
        terrainAltitudeMin = terrainAltitude;
        for (int i = 0; i < terrainVerticesCount; i++)
        {
            if (terrainVertices[i].y > terrainAltitudeMax) terrainAltitudeMax = terrainVertices[i].y;
            if (terrainVertices[i].y < terrainAltitudeMin) terrainAltitudeMin = terrainVertices[i].y;
        }

        colors = new Color[terrainVerticesCount];
        for (int i = 0; i < terrainVerticesCount; i++)
        {
            float vertexAltitude = Mathf.InverseLerp(terrainAltitudeMax, terrainAltitudeMin, terrainVertices[i].y);
            colors[i] = colorGradient.Evaluate(vertexAltitude);
        }

        terrainMesh.vertices = terrainVertices;
        terrainMesh.uv = uvs;
        terrainMesh.triangles = triangles;
        terrainMesh.colors = colors;
        
        terrainMesh.RecalculateBounds();
        terrainMesh.RecalculateNormals();
    }

    void DiamondSquareAlgorithm(int row, int column, int size, float offset)
    {
        int halfSize = (int) (size / 2f);
        int topLeft = row * (terrainSeperation + 1) + column;
        int bottomLeft = (row + size) * (terrainSeperation + 1) + column;

        int midPoint = (int) (row + halfSize) * (terrainSeperation + 1) + (int) (column + halfSize);
        terrainVertices[midPoint].y = (terrainVertices[topLeft].y + terrainVertices[topLeft + size].y + terrainVertices[bottomLeft].y + terrainVertices[bottomLeft + size].y)*0.25f + UnityEngine.Random.Range(-offset, offset);
        terrainVertices[topLeft + halfSize].y = (terrainVertices[topLeft].y + terrainVertices[topLeft + size].y + terrainVertices[midPoint].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        terrainVertices[midPoint - halfSize].y = (terrainVertices[topLeft].y + terrainVertices[bottomLeft].y + terrainVertices[midPoint].y)/3 + UnityEngine.Random.Range(-offset, offset);
        terrainVertices[midPoint + halfSize].y = (terrainVertices[topLeft + size].y + terrainVertices[bottomLeft + size].y + terrainVertices[midPoint].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        terrainVertices[bottomLeft + halfSize].y = (terrainVertices[bottomLeft].y + terrainVertices[bottomLeft + size].y + terrainVertices[midPoint].y) / 3 + UnityEngine.Random.Range(-offset, offset);
    }
}
