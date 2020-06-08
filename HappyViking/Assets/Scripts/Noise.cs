using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class  Noise {
    
    // Scale is used as a parameter so that the same map isn't produced each time.
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale) {
        // [,] means it's a multidimensional array
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Avoid divide by zero error.
        if(scale <= 0) {
            scale = 0.00001f;
        }

        // Loop through coordinates of the map.
        for(int y = 0; y < mapHeight; y++) {
            for(int x = 0; x < mapWidth; x++) {
                float samepleX = x / scale;
                float samepleY = y / scale;

                // Perlin Noise has gradual changes.
                float perlinValue = Mathf.PerlinNoise(samepleX, samepleY);
                noiseMap[x, y] = perlinValue;
            }
        }

        return noiseMap;
    }
    
}
