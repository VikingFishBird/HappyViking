using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CubeSpawner))]
public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Vector2 mapSize;

    public Coord[,] coordinates;
    [Range(0.0f, 1.0f)]
    public float cubeRate;
    public float noiseMapScale;
    CubeSpawner cubey;

    // Start is called before the first frame update
    void Start()
    {
        cubey = GetComponent<CubeSpawner>();
        coordinates = new Coord[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMap() {
        string holderName = "Generated Map";
        if (transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < mapSize.x; x++) {
            for(int y = 0; y < mapSize.y; y++) {
                //Vector3 tilePosition = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
                //Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

                //newTile.parent = mapHolder;

                coordinates[x,y] = new Coord(-mapSize.x / 2 + 0.5f + x, -mapSize.y / 2 + 0.5f + y);
                
            }
        }

        float[,] noiseMap = GenerateNoiseMap(noiseMapScale);
        cubey.PlaceCubes(coordinates, transform, noiseMap);

    }

    public float[,] GenerateNoiseMap(float scale) {
        float[,] noiseMap = new float[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        float offSetX = Random.Range(-100000, 100000);
        float offSetY = Random.Range(-100000, 100000);

        float halfWidth = mapSize.x / 2f;
        float halfHeight = mapSize.y / 2f;

        for (int x = 0; x < mapSize.x; x++) {
            //List<string> perlinNoiseThings = new List<string>();

            for (int y = 0; y < mapSize.y; y++) {
                float sampleX = (x - halfWidth + offSetX) / scale;
                float sampleY = (y - halfWidth + offSetY) / scale;

                float perlinValue = Mathf.Clamp(Mathf.PerlinNoise(sampleX, sampleY), 0, 1);
                
                noiseMap[x, y] = perlinValue;
                //perlinNoiseThings.Add(Mathf.PerlinNoise(sampleX, sampleY).ToString());
            }

            //print(string.Join(" | ", perlinNoiseThings));
            //perlinNoiseThings = new List<string>();
        }

        return noiseMap;

    }   
}
