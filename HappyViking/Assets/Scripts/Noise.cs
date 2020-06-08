using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class  Noise {
    
    // Scale is used as a parameter so that the same map isn't produced each time.
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        // [,] means it's a multidimensional array
        float[,] noiseMap = new float[mapWidth, mapHeight];

        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++) {
            float offsetX = Random.Range(-100000, 100000) + offset.x;
            float offsetY = Random.Range(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        // Avoid divide by zero error.
        if(scale <= 0) {
            scale = 0.00001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // Loop through coordinates of the map.
        for(int y = 0; y < mapHeight; y++) {
            for(int x = 0; x < mapWidth; x++) {

                // Maximum/Minimum Height
                float amplitude = 1;
                // Freq of height changes
                float frequency = 1;
                float noiseHeight = 0;

                for(int i = 0; i < octaves; i++) {
                    float samepleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float samepleY = (y - halfWidth) / scale * frequency + octaveOffsets[i].y;

                    // Perlin Noise has gradual changes. * 2 -1 is used to make the range [-1, 1]
                    float perlinValue = Mathf.PerlinNoise(samepleX, samepleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    // Persistance = Change of amplitude between octaves
                    // Lacunarity = Change of frequency between octaves
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // Get Max and Min Noise Height
                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;
                noiseMap[x, y] = noiseHeight;
            }
        }

        // Place values on range of 0-1
        for(int y = 0; y < mapHeight; y++) {
            for(int x = 0; x < mapWidth; x++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
    
}
