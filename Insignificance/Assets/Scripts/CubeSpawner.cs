using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public Transform cube;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaceCubes(float cubeRate, Coord[,] coords) {
        for(int x = 0; x < coords.GetLength(0); x++) {
            for(int y = 0; y < coords.GetLength(1); y++) {
                if(Random.Range(0f, 1.0f) < cubeRate) {
                    PlaceCubeAtCoord(coords[x, y]);
                }
            }
        }
    }

    public void PlaceCubeAtCoord(Coord coord) {
        Instantiate(cube, new Vector3(coord.x, 0.5f, coord.y), Quaternion.identity);
    }
}
