using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MapGenerator : MonoBehaviour
{
    public Vector2 mapSize;

    public Transform waterTile;
    public Transform landTile;

    Transform mapHolder;

    public Coord[,] coordinates;
    public float noiseMapScale;

    List<Compatability>[,] coefficientMatrix;
    Transform[,] tiles;
    static GameObject[] tileList;

    bool fullyCollapsed;

    // Start is called before the first frame update
    void Start()
    {
        // Get Tile Prefabs
        tileList = Resources.LoadAll<GameObject>("Prefabs/Tiles");
        for(int i = 0; i < tileList.Length; i++) {
            PlaceCubeAtCoord(new Coord(i, 60), transform, 1, tileList[i].transform, 90);
        }
        // Initialize coordinates array.
        coordinates = new Coord[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        // Generate Map
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Generates a new map
    public void GenerateMap() {
        // Instantiate the Coefficient Matrix
        coefficientMatrix = new List<Compatability>[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        tiles = new Transform[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];

        // Set coordinates array values
        for (int x = 0; x < mapSize.x; x++) {
            for(int y = 0; y < mapSize.y; y++) {
                coordinates[x,y] = new Coord(-mapSize.x / 2 + 0.5f + x, mapSize.y / 2 - 0.5f - y);             
            }
        }

        // Make sure the heirarchy isn't cluttered and delete existing map blocks.
        string holderName = "Object Holder";
        if (transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // Get Noise Map
        float[,] noiseMap = GenerateNoiseMap(noiseMapScale);
        // Set Water / Mountains
        PlaceHeightMapCubes(noiseMap);
        // Set possibilities for Wave Function Collpase Algorithm
        InstantiateCoeffecientMatrix();

        int count = 0;
        while (!fullyCollapsed) {
            int minx, miny;
            // Find the tile with the fewest possibilities
            FindTileWithFewestPossibilities(out minx, out miny);
            // Set the specified tile
            SetTile(minx, miny);
            // Reset possibilities of surrounding tiles
            ResetCoeffecientMatrixSurroundingTile(minx, miny);
            // Set Bool variable
            fullyCollapsed = CheckIfCollapsed();
            // Prevent Endless Loop
            count++;
            if(count > 10000) {
                break;
            }
        }
        // Reset fully collapsed bool value.
        fullyCollapsed = false;
    }

    // Finds which tile has the fewest valid potential tiles.
    public void FindTileWithFewestPossibilities(out int minx, out int miny) {
        int min = int.MaxValue;
        int xIndex = 0;
        int yIndex = 0;

        List<int> xZeros = new List<int>();
        List<int> yZeros = new List<int>();

        for (int x = 0; x < coefficientMatrix.GetLength(0); x++) {
            for (int y = 0; y < coefficientMatrix.GetLength(1); y++) {
                if (tiles[x, y] == null) {
                    if (coefficientMatrix[x, y].Count > 0 && coefficientMatrix[x, y].Count < min) {
                        min = coefficientMatrix[x, y].Count;
                        xIndex = x;
                        yIndex = y;
                    }
                    else if (coefficientMatrix[x, y].Count == 0) {
                        xZeros.Add(x);
                        yZeros.Add(y);
                    }
                }
            }
        }
        minx = xIndex;
        miny = yIndex;
        BackTrack(xZeros, yZeros);
    }

    // Selects a random valid tile and places it.
    public void SetTile(int x, int y) {
        float sum = 0f;
        for(int i = 0; i < coefficientMatrix[x,y].Count; i++) {
            sum += tileList[coefficientMatrix[x, y][i].index].GetComponent<Tile>().tileWeight;
        }
        float rand = Random.Range(0.0f, sum);
        float cumulativeSum = 0f;
        for (int i = 0; i < coefficientMatrix[x, y].Count; i++) {
            cumulativeSum += tileList[coefficientMatrix[x, y][i].index].GetComponent<Tile>().tileWeight;
            if(rand < cumulativeSum) {
                float rot = coefficientMatrix[x, y][i].rotation;
                tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, coefficientMatrix[x, y][i].heightLevel, tileList[coefficientMatrix[x, y][i].index].transform, rot);
                coefficientMatrix[x, y].Clear();
                return;
            }
        }
    }

    // Sets the surrounding tiles (including kitty corners) to null and resets the surrounding co-matrices.
    public void BackTrack(List<int> xZero, List<int> yZero) {
        // Future Optimization: ResetCoefficient Matrix at the end of method.
        for(int i = 0; i < xZero.Count; i++) {
            // Tile
            tiles[xZero[i], yZero[i]] = null;
            ResetCoeffecientMatrixAtTile(xZero[i], yZero[i]);

            //Left
            if (xZero[i] > 0 && tiles[xZero[i] - 1, yZero[i]] != null && !tiles[xZero[i] - 1, yZero[i]].GetComponent<Tile>().Mountain) {
                tiles[xZero[i] - 1, yZero[i]] = null;
                ResetCoeffecientMatrixAtTile(xZero[i] - 1, yZero[i]);
                ResetCoeffecientMatrixSurroundingTile(xZero[i] - 1, yZero[i]);
            }

            //Right
            if (xZero[i] < Mathf.RoundToInt(mapSize.x) - 1 && tiles[xZero[i] + 1, yZero[i]] != null && !tiles[xZero[i] + 1, yZero[i]].GetComponent<Tile>().Mountain) {
                tiles[xZero[i] + 1, yZero[i]] = null;
                ResetCoeffecientMatrixAtTile(xZero[i] + 1, yZero[i]);
                ResetCoeffecientMatrixSurroundingTile(xZero[i] + 1, yZero[i]);
            }

            //Above
            if (yZero[i] > 0 && tiles[xZero[i], yZero[i] - 1] != null && !tiles[xZero[i], yZero[i] - 1].GetComponent<Tile>().Mountain) {
                tiles[xZero[i], yZero[i] - 1] = null;
                ResetCoeffecientMatrixAtTile(xZero[i], yZero[i] - 1);
                ResetCoeffecientMatrixSurroundingTile(xZero[i], yZero[i] - 1);
            }

            // Below
            if (yZero[i] < Mathf.RoundToInt(mapSize.y) - 1 && tiles[xZero[i], yZero[i] + 1] != null && !tiles[xZero[i], yZero[i] + 1].GetComponent<Tile>().Mountain) {
                tiles[xZero[i], yZero[i] + 1] = null;
                ResetCoeffecientMatrixAtTile(xZero[i], yZero[i] + 1);
                ResetCoeffecientMatrixSurroundingTile(xZero[i], yZero[i] + 1);
            }

            // DiagAboveLeft
            if (yZero[i] > 0 && xZero[i] > 0 && tiles[xZero[i] - 1, yZero[i] - 1] != null && !tiles[xZero[i] - 1, yZero[i] - 1].GetComponent<Tile>().Mountain) {
                tiles[xZero[i] - 1, yZero[i] - 1] = null;
                ResetCoeffecientMatrixAtTile(xZero[i] - 1, yZero[i] - 1);
                ResetCoeffecientMatrixSurroundingTile(xZero[i] - 1, yZero[i] - 1);
            }

            // DiagAboveRight
            if (yZero[i] > 0 && xZero[i] < Mathf.RoundToInt(mapSize.x) - 1 && tiles[xZero[i] + 1, yZero[i] - 1] != null && !tiles[xZero[i] + 1, yZero[i] - 1].GetComponent<Tile>().Mountain) {
                tiles[xZero[i] + 1, yZero[i] - 1] = null;
                ResetCoeffecientMatrixAtTile(xZero[i] + 1, yZero[i] - 1);
                ResetCoeffecientMatrixSurroundingTile(xZero[i] + 1, yZero[i] - 1);
            }

            // DiagBelowRight
            if (yZero[i] < Mathf.RoundToInt(mapSize.y) - 1 && tiles[xZero[i] + 1, yZero[i] + 1] != null && xZero[i] < Mathf.RoundToInt(mapSize.x) - 1 && !tiles[xZero[i] + 1, yZero[i] + 1].GetComponent<Tile>().Mountain) {
                tiles[xZero[i] + 1, yZero[i] + 1] = null;
                ResetCoeffecientMatrixAtTile(xZero[i] + 1, yZero[i] + 1);
                ResetCoeffecientMatrixSurroundingTile(xZero[i] + 1, yZero[i] + 1);
            }

            // DiagBelowLeft
            if (yZero[i] < Mathf.RoundToInt(mapSize.y) - 1 && tiles[xZero[i] - 1, yZero[i] + 1] != null && xZero[i] > 0 && !tiles[xZero[i] - 1, yZero[i] + 1].GetComponent<Tile>().Mountain) {
                tiles[xZero[i] - 1, yZero[i] + 1] = null;
                ResetCoeffecientMatrixAtTile(xZero[i] - 1, yZero[i] + 1);
                ResetCoeffecientMatrixSurroundingTile(xZero[i] - 1, yZero[i] + 1);
            }
        }
    }

    // Resets the coefficient matrix of all tiles.
    public void InstantiateCoeffecientMatrix() {
        for(int x = 0; x < coefficientMatrix.GetLength(0); x++) {
            for (int y = 0; y < coefficientMatrix.GetLength(1); y++) {
                coefficientMatrix[x, y] = new List<Compatability>();
                if (tiles[x,y] == null) {
                    for(int i = 0; i < tileList.Length; i++) {
                        CheckValid(coefficientMatrix[x, y], i, x, y);
                    }
                }
            }
        }
    }

    // Resets the coefficient matrix of all surrounding tiles (but not the given one).
    public void ResetCoeffecientMatrixSurroundingTile(int x, int y) {
        if(x > 0) {
            if (tiles[x - 1, y] == null) {
                coefficientMatrix[x - 1, y].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[x - 1, y], i, x - 1, y);
                }
            }
        }
        if (x < Mathf.RoundToInt(mapSize.x) - 1) {
            if (tiles[x + 1, y] == null) {
                coefficientMatrix[x + 1, y].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[x + 1, y], i, x + 1, y);
                }
            }
        }
        if (y > 0) {
            if (tiles[x, y - 1] == null) {
                coefficientMatrix[x, y - 1].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[x, y - 1], i, x, y - 1);
                }
            }
        }
        if (y < Mathf.RoundToInt(mapSize.y) - 1) {
            if (tiles[x, y + 1] == null) {
                coefficientMatrix[x, y + 1].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[x, y + 1], i, x, y + 1);
                }
            }
        }

    }

    // Resets the coefficient matrix at a given tile.
    public void ResetCoeffecientMatrixAtTile(int x, int y) {
        coefficientMatrix[x, y].Clear();
        for (int i = 0; i < tileList.Length; i++) {
            CheckValid(coefficientMatrix[x, y], i, x, y);
        }
    }

    // Returns true if there are no gaps. False otherwise.
    public bool CheckIfCollapsed() {
        for(int x = 0; x < tiles.GetLength(0); x++) {
            for (int y = 0; y < tiles.GetLength(1); y++) {
                if(tiles[x,y] == null) {
                    return false;
                }
            }
        }
        return true;
    }

    // Creates Noise Map... yeah
    private float[,] GenerateNoiseMap(float scale) {
        float[,] noiseMap = new float[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        float offSetX = Random.Range(-100000, 100000);
        float offSetY = Random.Range(-100000, 100000);

        float halfWidth = mapSize.x / 2f;
        float halfHeight = mapSize.y / 2f;

        for (int x = 0; x < mapSize.x; x++) {

            for (int y = 0; y < mapSize.y; y++) {
                float sampleX = (x - halfWidth + offSetX) / scale;
                float sampleY = (y - halfWidth + offSetY) / scale;

                float perlinValue = Mathf.Clamp(Mathf.PerlinNoise(sampleX, sampleY), 0, 1);
                
                noiseMap[x, y] = perlinValue;
            }
        }

        return noiseMap;

    }

    // Adjust Values in method for mountain perlin heights.
    private int GetHeightLevelFromPerlin(float val) {
        if (val <= 0.25f)
            return 0;
        else if (val <= 0.65f)
            return 1;
        else if (val <= 0.85f)
            return 2;
        else if (val <= 0.95f)
            return 3;
        else
            return 4;
    }

    // Places HeightMapCubes ||| Need to change mountains.
    public void PlaceHeightMapCubes(float[,] perlin) {
        // Place Water/Mountain Cubes
        for (int x = 0; x < coordinates.GetLength(0); x++) {
            for (int y = 0; y < coordinates.GetLength(1); y++) {
                int heightLevel = GetHeightLevelFromPerlin(perlin[x, y]);
                if(heightLevel == 0) {
                    tiles[x,y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, heightLevel, waterTile, 0f);
                }
                else if(heightLevel > 1) {
                    tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, heightLevel, landTile, 0f);
                }
            }
        }
    }

    // Places tile at a coordinate... yep.
    public Transform PlaceCubeAtCoord(Coord coord, Transform parent, int heightLevel, Transform tile, float rot) {
        Transform cubey = Instantiate(tile, new Vector3(coord.x, -0.5f + heightLevel, coord.y), Quaternion.Euler(new Vector3(0, rot, 0)));

        float i = 0f;
        while(i < rot) {
            Tile.SideType temp = cubey.gameObject.GetComponent<Tile>().upSide;
            cubey.gameObject.GetComponent<Tile>().upSide = cubey.gameObject.GetComponent<Tile>().leftSide;
            cubey.gameObject.GetComponent<Tile>().leftSide = cubey.gameObject.GetComponent<Tile>().downSide;
            cubey.gameObject.GetComponent<Tile>().downSide = cubey.gameObject.GetComponent<Tile>().rightSide;
            cubey.gameObject.GetComponent<Tile>().rightSide = temp;
            i += 90f;
        }

        cubey.GetComponent<Tile>().tileLevel = heightLevel;

        if (heightLevel == 0) {
            cubey.localPosition = cubey.localPosition + Vector3.up;
            cubey.GetComponent<Tile>().tileLevel = heightLevel + 1;
        }
        cubey.parent = parent;
        return cubey;
    }

    // Adds all valid matches of a certain tile to the CoefficientMatrix
    private void CheckValid(List<Compatability> matrix, int tileIndex, int x, int y) {
        // Future Optimization: Add all coast tiles at once by type. No need to iterate through 3 coast corners.

        // Tile sides.
        Tile.SideType tileUp = tileList[tileIndex].GetComponent<Tile>().upSide;
        Tile.SideType tileDown = tileList[tileIndex].GetComponent<Tile>().downSide;
        Tile.SideType tileLeft = tileList[tileIndex].GetComponent<Tile>().leftSide;
        Tile.SideType tileRight = tileList[tileIndex].GetComponent<Tile>().rightSide;

        // Sides of neighboring tiles. Left = Left of current tile.
        Tile.SideType upSide = Tile.SideType.DNE;
        Tile.SideType downSide = Tile.SideType.DNE;
        Tile.SideType leftSide = Tile.SideType.DNE;
        Tile.SideType rightSide = Tile.SideType.DNE;

        if (y > 0 && tiles[x, y - 1] != null) {
            upSide = tiles[x, y - 1].gameObject.GetComponent<Tile>().downSide;
        }
        if (y < Mathf.RoundToInt(mapSize.y) - 1 && tiles[x, y + 1] != null) {
            downSide = tiles[x, y + 1].gameObject.GetComponent<Tile>().upSide;
        }
        if (x > 0 && tiles[x - 1, y] != null) {
            leftSide = tiles[x - 1, y].gameObject.GetComponent<Tile>().rightSide;
        }
        if (x < Mathf.RoundToInt(mapSize.x) - 1 && tiles[x + 1, y] != null) {
            rightSide = tiles[x + 1, y].gameObject.GetComponent<Tile>().leftSide;
        }

        // Set Neighboring tile sides.
        if (upSide == Tile.SideType.BeachUp) {
            upSide = Tile.SideType.BeachDown;
        }
        else if (upSide == Tile.SideType.BeachDown) {
            upSide = Tile.SideType.BeachUp;
        }
        else if (upSide == Tile.SideType.MountainUp) {
            upSide = Tile.SideType.MountainDown;
        }
        else if (upSide == Tile.SideType.MountainDown) {
            upSide = Tile.SideType.MountainUp;
        }
        else if (upSide == Tile.SideType.StairUp) {
            upSide = Tile.SideType.StairDown;
        }
        else if (upSide == Tile.SideType.StairDown) {
            upSide = Tile.SideType.StairUp;
        }

        if (downSide == Tile.SideType.BeachUp) {
            downSide = Tile.SideType.BeachDown;
        }
        else if (downSide == Tile.SideType.BeachDown) {
            downSide = Tile.SideType.BeachUp;
        }
        else if (downSide == Tile.SideType.MountainUp) {
            downSide = Tile.SideType.MountainDown;
        }
        else if (downSide == Tile.SideType.MountainDown) {
            downSide = Tile.SideType.MountainUp;
        }
        else if (downSide == Tile.SideType.StairUp) {
            downSide = Tile.SideType.StairDown;
        }
        else if (downSide == Tile.SideType.StairDown) {
            downSide = Tile.SideType.StairUp;
        }

        if (leftSide == Tile.SideType.BeachUp) {
            leftSide = Tile.SideType.BeachDown;
        }
        else if (leftSide == Tile.SideType.BeachDown) {
            leftSide = Tile.SideType.BeachUp;
        }
        else if (leftSide == Tile.SideType.MountainUp) {
            leftSide = Tile.SideType.MountainDown;
        }
        else if (leftSide == Tile.SideType.MountainDown) {
            leftSide = Tile.SideType.MountainUp;
        }
        else if (leftSide == Tile.SideType.StairUp) {
            leftSide = Tile.SideType.StairDown;
        }
        else if (leftSide == Tile.SideType.StairDown) {
            leftSide = Tile.SideType.StairUp;
        }

        if (rightSide == Tile.SideType.BeachUp) {
            rightSide = Tile.SideType.BeachDown;
        }
        else if (rightSide == Tile.SideType.BeachDown) {
            rightSide = Tile.SideType.BeachUp;
        }
        else if (rightSide == Tile.SideType.MountainUp) {
            rightSide = Tile.SideType.MountainDown;
        }
        else if (rightSide == Tile.SideType.MountainDown) {
            rightSide = Tile.SideType.MountainUp;
        }
        else if (rightSide == Tile.SideType.StairUp) {
            rightSide = Tile.SideType.StairDown;
        }
        else if (rightSide == Tile.SideType.StairDown) {
            rightSide = Tile.SideType.StairUp;
        }

        float rot = 0f;
        for(int height = 1; height < 4; height++) {
            if (tileList[tileIndex].GetComponent<Tile>().Mountain) {
                continue;
            }
            if (tileList[tileIndex].GetComponent<Tile>().CoastOrWater && height > 1) {
                break;
            }

            for (int i = 0; i < 4; i++) {
                if ((tileUp == upSide || upSide == Tile.SideType.DNE)
                && (tileDown == downSide || downSide == Tile.SideType.DNE)
                && (tileLeft == leftSide || leftSide == Tile.SideType.DNE)
                && (tileRight == rightSide || rightSide == Tile.SideType.DNE)) {
                    matrix.Add(new Compatability(tileIndex, rot, height));
                }
                // Rotate
                rot += 90f;
                Tile.SideType temp = tileUp;
                tileUp = tileLeft;
                tileLeft = tileDown;
                tileDown = tileRight;
                tileRight = temp;
            }
        }
    }

    // Unused but could be useful.
    private int GetTileIndexFromName(string name) {
        for (int i = 0; i < tileList.Length; i++) {
            if (tileList[i].name.Equals(name)) {
                return i;
            }
        }
        print("Could not find tile: " + name);
        return 0;
    }

    // Stores the data for each match in the Coefficient Matrix.
    struct Compatability {
        public int index;
        public float rotation;
        public int heightLevel;

        public Compatability(int ind, float rot, int level) {
            index = ind;
            rotation = rot;
            heightLevel = level;
        }
    }
}
