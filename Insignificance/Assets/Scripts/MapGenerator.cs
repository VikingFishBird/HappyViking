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
    CubeSpawner cubey;

    // Start is called before the first frame update
    void Start()
    {
        cubey = GetComponent<CubeSpawner>();
        coordinates = new Coord[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        GenerateMap();
        cubey.PlaceCubes(cubeRate, coordinates);
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
                Vector3 tilePosition = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

                newTile.parent = mapHolder;

                coordinates[x,y] = new Coord(-mapSize.x / 2 + 0.5f + x, -mapSize.y / 2 + 0.5f + y);
            }
        }
    }

    
}
