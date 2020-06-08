using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh};
    public DrawMode drawmode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public float meshHeightMulti;
    public AnimationCurve meshHeightCurve;

    public Vector2 offset;

    public bool autoUpdate;

    public TerrainType[] regions;

    public void GenerateMap() {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapWidth * mapHeight];

        for(int y = 0; y < mapHeight; y++) {
            for(int x = 0; x < mapWidth; x++) {
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++) {
                    if(currentHeight <= regions[i].height) {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawmode == DrawMode.NoiseMap) {
            display.drawTexture(TextureGenerator.textureFromHeightMap(noiseMap));
        }
        else if(drawmode == DrawMode.ColorMap) {
            display.drawTexture(TextureGenerator.textureFromColorMap(colorMap, mapWidth, mapHeight));
        }
        else if (drawmode == DrawMode.Mesh) {
            display.drawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMulti, meshHeightCurve), TextureGenerator.textureFromColorMap(colorMap, mapWidth, mapHeight));
        }
    }

    // Keep Editor values above 1
    private void OnValidate() {
        if(mapWidth < 1) {
            mapWidth = 1;
        }
        if(mapHeight < 1) {
            mapHeight = 1;
        }
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }
    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}
