using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

    public const float waterLevel = 0.3f;
    public const int mapChunkSize = 241;

    // Map Texture Colors
    public static Color waterColor = new Color(115, 167, 178);

    public static Color lightGrassColor = new Color(91, 201, 120);
    public static Color darkGrassColor = new Color(59, 84, 66);

    MapDisplay display;

    public int mapSize;  // Amount of chunks per row/col
    [Range(0, 6)]
    public int editorPreviewLOD;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    void Start() {

    }

    void Update() {

    }

    public void GenerateMap() {
        display = FindObjectOfType<MapDisplay>();
        int fullMapSize = mapSize * (mapChunkSize - 1) + 1;

        // Create Mesh, Create Texture
        // 1) Get "height map"
        float[,] heightNoiseMap = Noise.GenerateNoiseMap(fullMapSize, fullMapSize,
            seed, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] grassNoiseMap = Noise.GenerateNoiseMap(fullMapSize, fullMapSize,
            seed, noiseScale, octaves, persistance, lacunarity, offset);
        Color[] colorMap = new Color[fullMapSize * fullMapSize];

        for (int y = 0; y < fullMapSize; y++) {
            for (int x = 0; x < fullMapSize; x++) {
                if (heightNoiseMap[x, y] < waterLevel) {
                    // OCEAN
                    colorMap[y * fullMapSize + x] = waterColor;
                } else {
                    // GRASS
                    int r = Mathf.RoundToInt((lightGrassColor.r - darkGrassColor.r) * grassNoiseMap[x, y] + darkGrassColor.r);
                    int g = Mathf.RoundToInt((lightGrassColor.g - darkGrassColor.g) * grassNoiseMap[x, y] + darkGrassColor.g);
                    int b = Mathf.RoundToInt((lightGrassColor.b - darkGrassColor.b) * grassNoiseMap[x, y] + darkGrassColor.b);

                    colorMap[y * fullMapSize + x] = new Color(r, g, b);
                }
            }
        }

        //Texture2D texture = TextureGenerator.TextureFromColorMap(colorMap);

        display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, fullMapSize, fullMapSize));

        // 2) Set ocean, coasts, land
        // 3) Add Rivers using [banished method](https://www.reddit.com/r/proceduralgeneration/comments/fnglab/how_to_procedurally_generate_rivers_and_small/)
        // 4) Set plains and forest biomes
        // 5) Generate trees/ rocks / etc
    }

    Mesh GenerateQuad() {
        Mesh mapMesh = new Mesh();

        Vector3[] vertices = new Vector3[4] {
            new Vector3(-mapSize, 0, -mapSize),
            new Vector3(-mapSize, 0, mapSize),
            new Vector3(mapSize, 0, -mapSize),
            new Vector3(mapSize, 0, mapSize)
        };

        mapMesh.vertices = vertices;

        int[] tris = new int[6]
        {
            // lower left triangle
            0, 1, 2,
            // upper right triangle
            1, 3, 2
        };
        mapMesh.triangles = tris;

        
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mapMesh.uv = uv;

        return mapMesh;
    }

    void OnValidate() {
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }
    }
}
