using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public Transform cube;

    public void PlaceCubes(Coord[,] coords, Transform parent, float[,] perlin) {
        string holderName = "Object Holder";
        if (parent.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = parent;

        for (int x = 0; x < coords.GetLength(0); x++) {
            for(int y = 0; y < coords.GetLength(1); y++) {
                int heightLevel = GetHeightLevelFromPerlin(perlin[x, y]);
                PlaceCubeAtCoord(coords[x, y], mapHolder, heightLevel);
            }
        }
    }

    public void PlaceCubeAtCoord(Coord coord, Transform parent, int heightLevel) {
        Transform cubey = Instantiate(cube, new Vector3(coord.x, -0.5f + heightLevel, coord.y), Quaternion.Euler(Vector3.right * -90));
        cubey.localScale = new Vector3(50f, 50f, 50f);
        cubey.parent = parent;
    }

    public int GetHeightLevelFromPerlin(float val) {
        if(val <= 0.3f)
            return 0;
        else if(val <= 0.75f)
            return 1;
        else if (val <= 0.9f)
            return 2;
        else
            return 3;
    }
}
