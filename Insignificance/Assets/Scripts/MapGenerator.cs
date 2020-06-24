using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

// Optimize: Combine Meshes, Create separate arrays for coastTileList and mtTileList
public class MapGenerator : MonoBehaviour
{
    public Vector2 mapSize;
    public float noiseMapScale;
    public float fallOffMapPower;
    [Range(0.0f, 1.0f)]
    public float waterRate;

    public Transform waterTile;
    public Transform landTile;

    public Transform CornerTile;
    public Transform SideTile;
    public Transform StairTile;
    public Transform WedgeTile;

    enum Biome { };

    // The GameObject holding all the tiles.
    Transform mapHolder;

    // Array of coordinates of tiles.
    public Coord[,] coordinates;

    // Holds possible tiles.
    List<Compatability>[,] coefficientMatrix;
    // Top cubes.
    public Transform[,] tiles;
    // Prefab List
    static GameObject[] tileList;

    Transform[,] topTiles;

    // Boolean for the WFC completion.
    bool fullyCollapsed;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get Tile Prefabs
        tileList = Resources.LoadAll<GameObject>("Prefabs/Tiles");
        for(int i = 0; i < tileList.Length; i++) {
            PlaceCubeAtCoord(new Coord(i, 60), transform, 1, tileList[i].transform, 90, 0, 0, false);
        }
        
        // Generate Map
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Generates a new map
    public void GenerateMap() {
        // Instantiate the Coefficient Matrix / Arrays
        // Initialize coordinates array.
        tiles = new Transform[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        coordinates = new Coord[tiles.GetLength(0), tiles.GetLength(1)];
        coefficientMatrix = new List<Compatability>[tiles.GetLength(0), tiles.GetLength(1)];
        topTiles = new Transform[tiles.GetLength(0), tiles.GetLength(1)];
        //heightMapArrays = new Transform[5, Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        
        // Reset fully collapsed bool value.
        fullyCollapsed = false;

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
            FindSeaLevelTileWithFewestPossibilities(out minx, out miny);
            // Set the specified tile
            SetTile(minx, miny);
            // Reset possibilities of surrounding tiles
            ResetCoeffecientMatrixSurroundingTile(minx, miny);
            // Set Bool variable
            fullyCollapsed = CheckIfCollapsed();
            // Prevent Endless Loop
            count++;
            if(count > 30000) {
                break;
            }
        }

        // Future Optimization: Create this array beforehand and use it in other methods to minimize .GetComp calls.
        Tile[,] tileComps = new Tile[tiles.GetLength(0), tiles.GetLength(1)];
        for(int x = 0; x < tileComps.GetLength(0); x++) {
            for (int y = 0; y < tileComps.GetLength(1); y++) {
                tileComps[x, y] = tiles[x, y].GetComponent<Tile>();
            }
        }

        // Mountains
        int mountainAttempts = Mathf.RoundToInt(mapSize.x * 0.1f);
        
        for(int i = 0; i < mountainAttempts; i++) {
            // Coordinate
            Vector2Int coords = new Vector2Int(Random.Range(0, tiles.GetLength(0)), Random.Range(0, tiles.GetLength(1)));
            List<Mountain> mts = FindMtsAtCoord(coords, tileComps);
        }
    }
    
    #region Biomes and Texturing
    private void TextureDaMap() {
        Dictionary<Vector2, Biome> bioDic = new Dictionary<Vector2, Biome> {

        };

    }
    
    private float EvalPrecipitation(float x) {
        if(x < 0.25f) {
            return 4 * x;
        }
        else if(x < 0.5f) {
            return -4 * x + 2;
        }
        else if (x < 0.75f) {
            return 4 * x - 2;
        }
        else {
            return -4 * x + 4;
        }
    }
    private float EvalTemperature(float x) {
        if (x < 0.25f) {
            return 2 * x;
        }
        else {
            return -2 * x + 2;
        }
    }
    #endregion
    
    // Mountain Methods
    public List<Mountain> FindMtsAtCoord(Vector2Int coords, Tile[,] tileComps) {
        Mountain[] mountains = MountainData.mountains;

        List<Mountain> mts = new List<Mountain>();
        List<Vector2Int> failedCoords = new List<Vector2Int>();

        for (int i = 0; i < mountains.Length; i++) { // Make Sure to sort mountains by smallest size to greatest size.
            Mountain mt = mountains[i];

            // Quick Invalidations
            if (mt.width + coords.x > tiles.GetLength(0) || mt.length + coords.y > tiles.GetLength(1)) continue;

                bool dq = false;
            // Check if disqualified by previous failed coords:
            for (int coord = 0; coord < failedCoords.Count; coord++) {
                if (mt.width + coords.x >= failedCoords[coord].x && mt.length + coords.y >= failedCoords[coord].y) {
                    dq = true;
                }
            }
            if (dq) continue;

            // Future Optimization: Iterate through the edges as those will more likely be an issue.
            for(int x = coords.x; x < coords.x + mt.width; x++) {
                for (int y = coords.y; y < coords.y + mt.length; y++) {
                    if (tileComps[x, y].CoastOrWater)
                        dq = true;
                }
            }

            if (!dq) {
                mts.Add(mt);
            }
        }

        return mts;
    }

    // Adds all valid matches of a certain tile to the CoefficientMatrix
    private void CheckValid(List<Compatability> matrix, int tileIndex, int x, int y) {
        // Future Optimization: Add all coast tiles at once by type. No need to iterate through 3 coast corners.
        Tile tileComp = tileList[tileIndex].GetComponent<Tile>();
        
        // Tile sides.
        Tile.SideType tileUp = tileComp.upSide;
        Tile.SideType tileDown = tileComp.downSide;
        Tile.SideType tileLeft = tileComp.leftSide;
        Tile.SideType tileRight = tileComp.rightSide;

        // Sides of neighboring tiles. Left = Left of current tile.
        Tile.SideType upSide = Tile.SideType.DNE;
        Tile.SideType downSide = Tile.SideType.DNE;
        Tile.SideType leftSide = Tile.SideType.DNE;
        Tile.SideType rightSide = Tile.SideType.DNE;

        // Set Neighbor sides
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

        // Adjust/Prepare Neighbor sides.
        if (upSide == Tile.SideType.BeachUp) {
            upSide = Tile.SideType.BeachDown;
        }
        else if (upSide == Tile.SideType.BeachDown) {
            upSide = Tile.SideType.BeachUp;
        }

        if (downSide == Tile.SideType.BeachUp) {
            downSide = Tile.SideType.BeachDown;
        }
        else if (downSide == Tile.SideType.BeachDown) {
            downSide = Tile.SideType.BeachUp;
        }

        if (leftSide == Tile.SideType.BeachUp) {
            leftSide = Tile.SideType.BeachDown;
        }
        else if (leftSide == Tile.SideType.BeachDown) {
            leftSide = Tile.SideType.BeachUp;
        }

        if (rightSide == Tile.SideType.BeachUp) {
            rightSide = Tile.SideType.BeachDown;
        }
        else if (rightSide == Tile.SideType.BeachDown) {
            rightSide = Tile.SideType.BeachUp;
        }
        
        
        float rot = 0f;
        for (int i = 0; i < 4; i++) {
            if ((tileUp == upSide || upSide == Tile.SideType.DNE)
            && (tileDown == downSide || downSide == Tile.SideType.DNE)
            && (tileLeft == leftSide || leftSide == Tile.SideType.DNE)
            && (tileRight == rightSide || rightSide == Tile.SideType.DNE)) {
                float tileWeight = tileComp.tileWeight;
                if(upSide == Tile.SideType.DNE && 
                    downSide == Tile.SideType.DNE && 
                    leftSide == Tile.SideType.DNE && 
                    rightSide == Tile.SideType.DNE) {
                    tileWeight *= 0.05f;
                }
                matrix.Add(new Compatability(tileIndex, rot, tileWeight));
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

    // Selects a random valid tile and places it.
    private void SetTile(int x, int y) {
        float sum = 0f;
        for (int i = 0; i < coefficientMatrix[x, y].Count; i++) {
            sum += tileList[coefficientMatrix[x, y][i].index].GetComponent<Tile>().tileWeight;
        }
        float rand = Random.Range(0.0f, sum);
        float cumulativeSum = 0f;
        for (int i = 0; i < coefficientMatrix[x, y].Count; i++) {
            cumulativeSum += tileList[coefficientMatrix[x, y][i].index].GetComponent<Tile>().tileWeight;
            if (rand < cumulativeSum) {
                float rot = coefficientMatrix[x, y][i].rotation;
                tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, 1, tileList[coefficientMatrix[x, y][i].index].transform, rot, x, y, false);
                coefficientMatrix[x, y].Clear();
                return;
            }
        }
    }
    
    #region SeaLevel WFC Methods    
    // Returns the x/y index of the tile with the fewest possibilities.
    private void FindSeaLevelTileWithFewestPossibilities(out int minx, out int miny) {
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
        SeaLevelBackTrack(xZeros, yZeros);
    }  

    // Sets the surrounding tiles (including kitty corners) to null and resets the surrounding co-matrices.
    private void SeaLevelBackTrack(List<int> xZero, List<int> yZero) {
        // Future Optimization: ResetCoefficient Matrix at the end of method.
        // Future Optimization: Make this into a look to add the capability of adjusting the size of the backtrack.
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
    private void InstantiateCoeffecientMatrix() {
        for (int x = 0; x < coefficientMatrix.GetLength(0); x++) {
            for (int y = 0; y < coefficientMatrix.GetLength(1); y++) {
                coefficientMatrix[x, y] = new List<Compatability>();
                if (tiles[x, y] == null) {
                    for (int i = 0; i < tileList.Length; i++) {
                        CheckValid(coefficientMatrix[x, y], i, x, y);
                    }
                }
            }
        }
    }

    // Resets the coefficient matrix of all surrounding tiles (but not the given one).
    private void ResetCoeffecientMatrixSurroundingTile(int x, int y) {
        if(x > 0) {
            if (tiles[x - 1, y] == null) {
                coefficientMatrix[x - 1, y].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[x - 1, y], i, x - 1, y);
                }
            }
        }
        if (x < coefficientMatrix.GetLength(0) - 1) {
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
        if (y < coefficientMatrix.GetLength(1) - 1) {
            if (tiles[x, y + 1] == null) {
                coefficientMatrix[x, y + 1].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[x, y + 1], i, x, y + 1);
                }
            }
        }

    }

    // Resets the coefficient matrix at a given tile.
    private void ResetCoeffecientMatrixAtTile(int x, int y) {
        coefficientMatrix[x, y].Clear();
        for (int i = 0; i < tileList.Length; i++) {
            CheckValid(coefficientMatrix[x, y], i, x, y);
        }
    }

    // Returns true if there are no gaps. False otherwise.
    private bool CheckIfCollapsed() {
        for(int x = 0; x < tiles.GetLength(0); x++) {
            for (int y = 0; y < tiles.GetLength(1); y++) {
                if(tiles[x, y] == null) {
                    return false;
                }
            }
        }
        return true;
    }
    #endregion

    #region Noise
    // Creates Noise Map... yeah
    private float[,] GenerateNoiseMap(float scale) {
        float[,] noiseMap = new float[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        float offSetX = Random.Range(-100000, 100000);
        float offSetY = Random.Range(-100000, 100000);

        float halfWidth = mapSize.x / 2f;
        float halfHeight = mapSize.y / 2f;

        float[,] falloff = GenerateFalloffMap();

        for (int x = 0; x < mapSize.x; x++) {

            for (int y = 0; y < mapSize.y; y++) {
                float sampleX = (x - halfWidth + offSetX) / scale;
                float sampleY = (y - halfWidth + offSetY) / scale;

                float perlinValue = Mathf.Clamp(Mathf.PerlinNoise(sampleX, sampleY), 0, 1);

                noiseMap[x, y] = Mathf.Clamp01(perlinValue - falloff[x,y]);
            }
        }

        return noiseMap;

    }

    // Create Falloff Map
    private float[,] GenerateFalloffMap() {
        float[,] map = new float[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];

        for(int i = 0; i < map.GetLength(0); i++) {
            for (int j = 0; j < map.GetLength(1); j++) {
                float x = i / mapSize.x * 2 - 1;
                float y = j / mapSize.y * 2 - 1;

                float val = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i,j] = EvalWithCurve(val);
            }
        }

        return map;
    }

    // Falloff Map eval curve (it adjusts how quickly it falls of)
    private float EvalWithCurve(float val) {
        float a = 3;
        float b = fallOffMapPower;

        return Mathf.Pow(val, a) / (Mathf.Pow(val, a) + Mathf.Pow(b-b*val, a));
    }
    
    // Adjust Values in method for mountain perlin heights.
    private bool GetHeightLevelFromPerlin(float val) {
        if (val <= waterRate)
            return true;
        else
            return false;
    }
    #endregion

    // Places HeightMapCubes ||| Need to change mountains.
    public void PlaceHeightMapCubes(float[,] perlin) {
        // Place Water/Mountain Cubes
        for (int x = 0; x < coordinates.GetLength(0); x++) {
            for (int y = 0; y < coordinates.GetLength(1); y++) {
                if(GetHeightLevelFromPerlin(perlin[x, y])) {
                    tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, 1, waterTile, 0f, x, y, true);
                }
            }
        }
    }

    // Places tile at a coordinate... yep.
    public Transform PlaceCubeAtCoord(Coord coord, Transform parent, int heightLevel, Transform tile, float rot, int x, int y, bool perlin) {
        Transform cubey = Instantiate(tile, new Vector3(coord.x, -0.5f + heightLevel, coord.y), Quaternion.Euler(new Vector3(0, rot, 0)));

        Tile tileComp = cubey.GetComponent<Tile>();
        float i = 0f;
        while(i < rot) {
            Tile.SideType temp = tileComp.upSide;
            tileComp.upSide = tileComp.leftSide;
            tileComp.leftSide = tileComp.downSide;
            tileComp.downSide = tileComp.rightSide;
            tileComp.rightSide = temp;
            i += 90f;
        }
        cubey.parent = parent;
       
        return cubey;
    }

    // Stores the data for each match in the Coefficient Matrix.
    struct Compatability {
        public int index;
        public float rotation;
        public float tileWeight;

        public Compatability(int ind, float rot, float weight) {
            index = ind;
            rotation = rot;
            tileWeight = weight;
        }
    }
}
