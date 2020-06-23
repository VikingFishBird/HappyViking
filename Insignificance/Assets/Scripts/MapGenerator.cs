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

    public Transform waterTile;
    public Transform landTile;

    public Transform MountainStair;
    public Transform MountainEndStair;
    public Transform StairCorner;
    public Transform StairWedge;
    public Transform MTS;
    public Transform STM;
    public Transform MTSCorner;
    public Transform STMCorner;
    public Transform MTSWedge;
    public Transform STMWedge;
    //
    public Transform Corner;
    public Transform CornerPath;
    public Transform DubWedge;
    public Transform DubWedgePath;
    public Transform MountainEnd;
    public Transform QuadWedge;
    public Transform TripleWedge;
    public Transform Peak;
    public Transform MountainSide;
    public Transform MountainPath;
    public Transform MountainWedge;
    public Transform STMTS;
    public Transform STMDoubleMountain;
    public Transform MTSDoubleMountain;
    public Transform WedgeToPath;
    public Transform PathToWedge;
    public Transform MirrorWedge;

    enum Biome { };

    // The GameObject holding all the tiles.
    Transform mapHolder;

    // Array of coordinates of tiles.
    public Coord[,] coordinates;

    // Holds possible tiles.
    List<Compatability>[,,] coefficientMatrix;
    // Top cubes.
    public Transform[,,] tiles;
    // Prefab List
    static GameObject[] tileList;

    Transform[,] topTiles;

    // Boolean for the WFC completion.
    bool fullyCollapsed;

    // Mountain Arrays [height, x, y]
    //public Transform[,,] heightMapArrays;

    // Start is called before the first frame update
    void Start()
    {
        // Get Tile Prefabs
        tileList = Resources.LoadAll<GameObject>("Prefabs/Tiles");
        for(int i = 0; i < tileList.Length; i++) {
            PlaceCubeAtCoord(new Coord(i, 60), transform, 1, tileList[i].transform, 90, 0, 0, false);
            print(tileList[i].transform.name);
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
        // Instantiate the Coefficient Matrix / Arrays
        coefficientMatrix = new List<Compatability>[6, Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        tiles = new Transform[6, Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        topTiles = new Transform[tiles.GetLength(1), tiles.GetLength(2)];
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
            SetTile(0, minx, miny);
            // Reset possibilities of surrounding tiles
            ResetCoeffecientMatrixSurroundingTile(0, minx, miny);
            // Set Bool variable
            fullyCollapsed = CheckIfCollapsed();
            // Prevent Endless Loop
            count++;
            if(count > 30000) {
                break;
            }
        }


        // Mountains \\
        count = 0;
        for(int i = 1; i < tiles.GetLength(0); i++) { // i = HeightLevel (mountains only)
            // Find Valid Mountain tiles at level
            bool[,] validMountain = FindValidMountainTiles(i);

            // Set MountainCoefficientMatrix
            SetMountainCoefficientMatrix(i, validMountain);

            string coMatrixStr = "";
            for(int y = 0; y < coefficientMatrix.GetLength(2); y++) {
                for (int x = 0; x < coefficientMatrix.GetLength(1); x++) {
                    coMatrixStr += coefficientMatrix[i, x, y].Count.ToString() + " ";
                }
                coMatrixStr += "\n";
            }
            print(coMatrixStr);

            fullyCollapsed = false;
            while (!fullyCollapsed) {
                // Find the tile with the fewest possibilities
                int minX, minY;
                FindMountainTileWithFewestPossbilities(i, validMountain, out minX, out minY);
                // Set the specified tile
                SetTile(i, minX, minY);
                // Reset possibilities of surrounding tiles
                ResetCoeffecientMatrixSurroundingTile(i, minX, minY, validMountain);
                // Set Bool variable
                fullyCollapsed = CheckIfCollapsed(i, validMountain);
                // Prevent Endless Loop
                count++;
                if (count > 30000) {
                    break;
                }
            }

            // Set Top Tiles
            for(int x = 0; x < tiles.GetLength(1); x++) {
                for (int y = 0; y < tiles.GetLength(2); y++) {
                    for(int height = 0; height < tiles.GetLength(0); height++) {
                        if (tiles[height, x, y] == null) {
                            topTiles[x, y] = tiles[height - 1, x, y];
                            break;
                        }
                    }
                    if (topTiles[x, y] == null)
                        topTiles[x, y] = tiles[tiles.GetLength(0) - 1, x, y];
                }
            }
        }

        fullyCollapsed = false;

        #region OldCode
        /*
        // Iterate through each level
        for(int i = 0; i < heightMapArrays.GetLength(0); i++) {
            // Place Stairs
            PlaceStairCubes(heightMapArrays, i + 2, i);
            // Place Stair-dependent tiles
            bool changes = true;
            count = 0;
            while(changes) {
                changes = PlaceMTSCubes(heightMapArrays, i + 2, i);
                count++;
            }

            // Place Non Stair-Dependent Tiles
            changes = true;
            count = 0;
            while (count < 20) {
                changes = PlaceRemainingMountain(heightMapArrays, i + 2, i);
                count++;
            }
        }*/
        #endregion
    }

    #region Mountain WFC
    // Initializes values of mountain coefficient matrix at height.
    private void SetMountainCoefficientMatrix(int height, bool[,] mountValid) {
        for (int x = 0; x < coefficientMatrix.GetLength(1); x++) {
            for (int y = 0; y < coefficientMatrix.GetLength(2); y++) {
                if (mountValid[x,y] && tiles[height, x, y] == null) {
                    for (int i = 0; i < tileList.Length; i++) {
                        CheckValid(coefficientMatrix[height, x, y], i, height, x, y, mountValid);
                    }
                }
            }
        }
    }
    
    // Returns a boolean array representing valid mountain placements.
    // Tile is valid if tile below exists and is not water/coast/mountain edge. 
    private bool[,] FindValidMountainTiles(int index) {
        bool[,] valid = new bool[tiles.GetLength(1), tiles.GetLength(2)];

        for (int x = 0; x < tiles.GetLength(1); x++) {
            for (int y = 0; y < tiles.GetLength(2); y++) {
                if (tiles[index - 1, x, y] && !tiles[index - 1, x, y].GetComponent<Tile>().CoastOrWater && !tiles[index - 1, x, y].GetComponent<Tile>().MountainEdge) {
                    valid[x, y] = true;
                }
                else {
                    valid[x, y] = false;
                }
            }
        }

        return valid;
    }
    
    // Returns the x/y index of the tile with the fewest possibilities.
    private void FindMountainTileWithFewestPossbilities(int height, bool[,] valid, out int x, out int y) {
        int min = int.MaxValue;
        x = 0;
        y = 0;

        for (int xI = 0; xI < coefficientMatrix.GetLength(1); xI++) {
            for (int yI = 0; yI < coefficientMatrix.GetLength(2); yI++) {
                if (tiles[height, xI, yI] == null && valid[xI, yI]) {
                    if (coefficientMatrix[height, xI, yI].Count > 0 && coefficientMatrix[height, xI, yI].Count < min) {
                        min = coefficientMatrix[height, xI, yI].Count;
                        x = xI;
                        y = yI;
                    }
                }
            }
        }
    }
    #endregion

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
    
    // Adds all valid matches of a certain tile to the CoefficientMatrix
    private void CheckValid(List<Compatability> matrix, int tileIndex, int height, int x, int y, bool[,] mountValid = null) {
        // Future Optimization: Add all coast tiles at once by type. No need to iterate through 3 coast corners.
        Tile tileComp = tileList[tileIndex].GetComponent<Tile>();

        if (mountValid == null && tileComp.Mountain) return;
        else if (mountValid != null && tileComp.CoastOrWater) return;

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
        if (y > 0 && tiles[height, x, y - 1] != null) {
            upSide = tiles[height, x, y - 1].gameObject.GetComponent<Tile>().downSide;
        }
        if (y < Mathf.RoundToInt(mapSize.y) - 1 && tiles[height, x, y + 1] != null) {
            downSide = tiles[height, x, y + 1].gameObject.GetComponent<Tile>().upSide;
        }
        if (x > 0 && tiles[height, x - 1, y] != null) {
            leftSide = tiles[height, x - 1, y].gameObject.GetComponent<Tile>().rightSide;
        }
        if (x < Mathf.RoundToInt(mapSize.x) - 1 && tiles[height, x + 1, y] != null) {
            rightSide = tiles[height, x + 1, y].gameObject.GetComponent<Tile>().leftSide;
        }

        // Adjust/Prepare Neighbor sides.
        if (tileComp.Mountain) {
            if (upSide == Tile.SideType.MountainUp) {
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
            else if(upSide == Tile.SideType.DNE && y > 0 && !mountValid[x,y-1]) {
                upSide = Tile.SideType.Air;
            }

            if (downSide == Tile.SideType.MountainUp) {
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
            else if(downSide == Tile.SideType.DNE && y < tiles.GetLength(2) - 1 && !mountValid[x,y+1]) {
                downSide = Tile.SideType.Air;
            }

            if (leftSide == Tile.SideType.MountainUp) {
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
            else if (leftSide == Tile.SideType.DNE && x > 0 && !mountValid[x-1, y]) {
                leftSide = Tile.SideType.Air;
            }

            if (rightSide == Tile.SideType.MountainUp) {
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
            else if (rightSide == Tile.SideType.DNE && x < tiles.GetLength(1) - 1 && !mountValid[x + 1, y]) {
                rightSide = Tile.SideType.Air;
            }
        }
        else {
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
    private void SetTile(int height, int x, int y) {
        float sum = 0f;
        for (int i = 0; i < coefficientMatrix[height, x, y].Count; i++) {
            sum += tileList[coefficientMatrix[height, x, y][i].index].GetComponent<Tile>().tileWeight;
        }
        float rand = Random.Range(0.0f, sum);
        float cumulativeSum = 0f;
        for (int i = 0; i < coefficientMatrix[height, x, y].Count; i++) {
            cumulativeSum += tileList[coefficientMatrix[height, x, y][i].index].GetComponent<Tile>().tileWeight;
            if (rand < cumulativeSum) {
                float rot = coefficientMatrix[height, x, y][i].rotation;
                tiles[height, x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height + 1, tileList[coefficientMatrix[height, x, y][i].index].transform, rot, x, y, false);
                coefficientMatrix[height, x, y].Clear();
                return;
            }
        }
    }


    /*
    // Optimize: Continue loop after true case.
    private bool PlaceRemainingMountain(Transform[,,] heightLevel, int height, int index) {
        bool changes = false;

        for (int x = 0; x < heightLevel.GetLength(1); x++) {
            for (int y = 0; y < heightLevel.GetLength(2); y++) {

                bool success = false;
                

                // If not mountain, continue.
                if (heightLevel[index, x, y] == null) {
                    continue;
                }
                if (!heightLevel[index, x, y].gameObject.GetComponent<Tile>().Land) {
                    continue;
                }

                // Sides of neighboring tiles. Left = Left of current tile.
                Tile.SideType upSide = Tile.SideType.Air;
                Tile.SideType downSide = Tile.SideType.Air;
                Tile.SideType leftSide = Tile.SideType.Air;
                Tile.SideType rightSide = Tile.SideType.Air;

                if (y > 0 && heightLevel[index, x, y - 1] != null) {
                    upSide = heightLevel[index, x, y - 1].gameObject.GetComponent<Tile>().downSide;
                }
                if (y < Mathf.RoundToInt(mapSize.y) - 1 && heightLevel[index, x, y + 1] != null) {
                    downSide = heightLevel[index, x, y + 1].gameObject.GetComponent<Tile>().upSide;
                }
                if (x > 0 && heightLevel[index, x - 1, y] != null) {
                    leftSide = heightLevel[index, x - 1, y].gameObject.GetComponent<Tile>().rightSide;
                }
                if (x < Mathf.RoundToInt(mapSize.x) - 1 && heightLevel[index, x + 1, y] != null) {
                    rightSide = heightLevel[index, x + 1, y].gameObject.GetComponent<Tile>().leftSide;
                }

                // Set Neighboring tile sides.
                if (upSide == Tile.SideType.MountainUp) {
                    upSide = Tile.SideType.MountainDown;
                }
                else if (upSide == Tile.SideType.MountainDown) {
                    upSide = Tile.SideType.MountainUp;
                }

                if (downSide == Tile.SideType.MountainUp) {
                    downSide = Tile.SideType.MountainDown;
                }
                else if (downSide == Tile.SideType.MountainDown) {
                    downSide = Tile.SideType.MountainUp;
                }

                if (leftSide == Tile.SideType.MountainUp) {
                    leftSide = Tile.SideType.MountainDown;
                }
                else if (leftSide == Tile.SideType.MountainDown) {
                    leftSide = Tile.SideType.MountainUp;
                }

                if (rightSide == Tile.SideType.MountainUp) {
                    rightSide = Tile.SideType.MountainDown;
                }
                else if (rightSide == Tile.SideType.MountainDown) {
                    rightSide = Tile.SideType.MountainUp;
                }

                // Tile sides.
                Tile.SideType tileUp = Corner.GetComponent<Tile>().upSide;
                Tile.SideType tileDown = Corner.GetComponent<Tile>().downSide;
                Tile.SideType tileLeft = Corner.GetComponent<Tile>().leftSide;
                Tile.SideType tileRight = Corner.GetComponent<Tile>().rightSide;

                // Corner Only
                Tile.SideType tileUpCorner = Tile.SideType.Land;
                Tile.SideType tileDownCorner = Tile.SideType.Air;
                Tile.SideType tileLeftCorner = Tile.SideType.Land;
                Tile.SideType tileRightCorner = Tile.SideType.Air;
                // Corner
                float rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide || tileUpCorner == upSide)
                    && (tileDown == downSide || tileDownCorner == downSide)
                    && (tileLeft == leftSide || tileLeftCorner == leftSide)
                    && (tileRight == rightSide || tileRightCorner == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, Corner, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;

                    temp = tileUpCorner;
                    tileUpCorner = tileLeftCorner;
                    tileLeftCorner = tileDownCorner;
                    tileDownCorner = tileRightCorner;
                    tileRightCorner = temp;
                }

                if (success)
                    continue;

                // CornerPath: 
                tileUp = CornerPath.GetComponent<Tile>().upSide;
                tileDown = CornerPath.GetComponent<Tile>().downSide;
                tileLeft = CornerPath.GetComponent<Tile>().leftSide;
                tileRight = CornerPath.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, CornerPath, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // DubWedge: 
                tileUp = DubWedge.GetComponent<Tile>().upSide;
                tileDown = DubWedge.GetComponent<Tile>().downSide;
                tileLeft = DubWedge.GetComponent<Tile>().leftSide;
                tileRight = DubWedge.GetComponent<Tile>().rightSide;
                // DubWedge Only
                Tile.SideType tileUpDubWedge = Tile.SideType.Land;
                Tile.SideType tileDownDubWedge = Tile.SideType.Land;
                Tile.SideType tileLeftDubWedge = Tile.SideType.MountainUp;
                Tile.SideType tileRightDubWedge = Tile.SideType.MountainDown;
                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide || upSide == tileUpDubWedge)
                    && (tileDown == downSide || downSide == tileDownDubWedge)
                    && (tileLeft == leftSide || leftSide == tileLeftDubWedge)
                    && (tileRight == rightSide || rightSide == tileRightDubWedge)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, DubWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;

                    temp = tileUpDubWedge;
                    tileUpDubWedge = tileLeftDubWedge;
                    tileLeftDubWedge = tileDownDubWedge;
                    tileDownDubWedge = tileRightDubWedge;
                    tileRightDubWedge = temp;
                }

                if (success)
                    continue;

                // DubWedgePath:
                tileUp = DubWedgePath.GetComponent<Tile>().upSide;
                tileDown = DubWedgePath.GetComponent<Tile>().downSide;
                tileLeft = DubWedgePath.GetComponent<Tile>().leftSide;
                tileRight = DubWedgePath.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, DubWedgePath, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // MountainEnd:
                tileUp = MountainEnd.GetComponent<Tile>().upSide;
                tileDown = MountainEnd.GetComponent<Tile>().downSide;
                tileLeft = MountainEnd.GetComponent<Tile>().leftSide;
                tileRight = MountainEnd.GetComponent<Tile>().rightSide;

                Tile.SideType tileUpMEnd = Tile.SideType.Air;
                Tile.SideType tileDownMEnd = Tile.SideType.Air;
                Tile.SideType tileLeftMEnd = Tile.SideType.Air;
                Tile.SideType tileRightMEnd = Tile.SideType.Land;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide || upSide == tileUpMEnd)
                    && (tileDown == downSide || downSide == tileDownMEnd)
                    && (tileLeft == leftSide || leftSide == tileLeftMEnd)
                    && (tileRight == rightSide || rightSide == tileRightMEnd)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainEnd, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;

                    temp = tileUpMEnd;
                    tileUpMEnd = tileLeftMEnd;
                    tileLeftMEnd = tileDownMEnd;
                    tileDownMEnd = tileRightMEnd;
                    tileRightMEnd = temp;
                }

                if (success)
                    continue;

                // QuadWedge:
                tileUp = QuadWedge.GetComponent<Tile>().upSide;
                tileDown = QuadWedge.GetComponent<Tile>().downSide;
                tileLeft = QuadWedge.GetComponent<Tile>().leftSide;
                tileRight = QuadWedge.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, QuadWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // TripleWedge: 
                tileUp = TripleWedge.GetComponent<Tile>().upSide;
                tileDown = TripleWedge.GetComponent<Tile>().downSide;
                tileLeft = TripleWedge.GetComponent<Tile>().leftSide;
                tileRight = TripleWedge.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, TripleWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // Peak: 
                tileUp = Peak.GetComponent<Tile>().upSide;
                tileDown = Peak.GetComponent<Tile>().downSide;
                tileLeft = Peak.GetComponent<Tile>().leftSide;
                tileRight = Peak.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, Peak, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // PathToWedge: 
                tileUp = PathToWedge.GetComponent<Tile>().upSide;
                tileDown = PathToWedge.GetComponent<Tile>().downSide;
                tileLeft = PathToWedge.GetComponent<Tile>().leftSide;
                tileRight = PathToWedge.GetComponent<Tile>().rightSide;

                Tile.SideType tileUpPTW = Tile.SideType.Land;
                Tile.SideType tileDownPTW = Tile.SideType.DoubleMountain;
                Tile.SideType tileLeftPTW = Tile.SideType.Air;
                Tile.SideType tileRightPTW = Tile.SideType.MountainDown;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide || upSide == tileUpPTW)
                    && (tileDown == downSide || downSide == tileDownPTW)
                    && (tileLeft == leftSide || leftSide == tileLeftPTW)
                    && (tileRight == rightSide || rightSide == tileRightPTW)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, PathToWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;

                    temp = tileUpPTW;
                    tileUpPTW = tileLeftPTW;
                    tileLeftPTW = tileDownPTW;
                    tileDownPTW = tileRightPTW;
                    tileRightPTW = temp;
                }

                if (success)
                    continue;

                // WedgeToPath: 
                tileUp = WedgeToPath.GetComponent<Tile>().upSide;
                tileDown = WedgeToPath.GetComponent<Tile>().downSide;
                tileLeft = WedgeToPath.GetComponent<Tile>().leftSide;
                tileRight = WedgeToPath.GetComponent<Tile>().rightSide;

                Tile.SideType tileUpWTP = Tile.SideType.Land;
                Tile.SideType tileDownWTP = Tile.SideType.DoubleMountain;
                Tile.SideType tileLeftWTP = Tile.SideType.MountainUp;
                Tile.SideType tileRightWTP = Tile.SideType.Air;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide || upSide == tileUpWTP)
                    && (tileDown == downSide || downSide == tileDownWTP)
                    && (tileLeft == leftSide || leftSide == tileLeftWTP)
                    && (tileRight == rightSide || rightSide == tileRightWTP)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, WedgeToPath, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                    
                    temp = tileUpWTP;
                    tileUpWTP = tileLeftWTP;
                    tileLeftWTP = tileDownWTP;
                    tileDownWTP = tileRightWTP;
                    tileRightWTP = temp;
                }

                if (success)
                    continue;

                // MountainSide: 
                tileUp = MountainSide.GetComponent<Tile>().upSide;
                tileDown = MountainSide.GetComponent<Tile>().downSide;
                tileLeft = MountainSide.GetComponent<Tile>().leftSide;
                tileRight = MountainSide.GetComponent<Tile>().rightSide;

                Tile.SideType tileUpSide = Tile.SideType.Land;
                Tile.SideType tileDownSide = Tile.SideType.Air;
                Tile.SideType tileLeftSide = Tile.SideType.Land;
                Tile.SideType tileRightSide = Tile.SideType.Land;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide || upSide == tileUpSide)
                    && (tileDown == downSide || downSide == tileDownSide)
                    && (tileLeft == leftSide || leftSide == tileLeftSide)
                    && (tileRight == rightSide || rightSide == tileRightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainSide, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;

                    temp = tileUpSide;
                    tileUpSide = tileLeftSide;
                    tileLeftSide = tileDownSide;
                    tileDownSide = tileRightSide;
                    tileRightSide = temp;
                }

                if (success)
                    continue;

                // MountainPath: 
                tileUp = MountainPath.GetComponent<Tile>().upSide;
                tileDown = MountainPath.GetComponent<Tile>().downSide;
                tileLeft = MountainPath.GetComponent<Tile>().leftSide;
                tileRight = MountainPath.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainPath, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // MountainWedge: 
                tileUp = MountainWedge.GetComponent<Tile>().upSide;
                tileDown = MountainWedge.GetComponent<Tile>().downSide;
                tileLeft = MountainWedge.GetComponent<Tile>().leftSide;
                tileRight = MountainWedge.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // STMTS: 
                tileUp = STMTS.GetComponent<Tile>().upSide;
                tileDown = STMTS.GetComponent<Tile>().downSide;
                tileLeft = STMTS.GetComponent<Tile>().leftSide;
                tileRight = STMTS.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, STMTS, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // STMDubMount: 
                tileUp = STMDoubleMountain.GetComponent<Tile>().upSide;
                tileDown = STMDoubleMountain.GetComponent<Tile>().downSide;
                tileLeft = STMDoubleMountain.GetComponent<Tile>().leftSide;
                tileRight = STMDoubleMountain.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, STMDoubleMountain, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // MTSDubMount: 
                tileUp = MTSDoubleMountain.GetComponent<Tile>().upSide;
                tileDown = MTSDoubleMountain.GetComponent<Tile>().downSide;
                tileLeft = MTSDoubleMountain.GetComponent<Tile>().leftSide;
                tileRight = MTSDoubleMountain.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MTSDoubleMountain, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // STMWedge: 
                tileUp = STMWedge.GetComponent<Tile>().upSide;
                tileDown = STMWedge.GetComponent<Tile>().downSide;
                tileLeft = STMWedge.GetComponent<Tile>().leftSide;
                tileRight = STMWedge.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, STMWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // MTSWedge: 
                tileUp = MTSWedge.GetComponent<Tile>().upSide;
                tileDown = MTSWedge.GetComponent<Tile>().downSide;
                tileLeft = MTSWedge.GetComponent<Tile>().leftSide;
                tileRight = MTSWedge.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MTSWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // MTS: 
                tileUp = MTS.GetComponent<Tile>().upSide;
                tileDown = MTS.GetComponent<Tile>().downSide;
                tileLeft = MTS.GetComponent<Tile>().leftSide;
                tileRight = MTS.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MTS, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
                if (success)
                    continue;
                // STM: 
                tileUp = STM.GetComponent<Tile>().upSide;
                tileDown = STM.GetComponent<Tile>().downSide;
                tileLeft = STM.GetComponent<Tile>().leftSide;
                tileRight = STM.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, STM, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
                if (success)
                    continue;
                // MTSCorner: 
                tileUp = MTSCorner.GetComponent<Tile>().upSide;
                tileDown = MTSCorner.GetComponent<Tile>().downSide;
                tileLeft = MTSCorner.GetComponent<Tile>().leftSide;
                tileRight = MTSCorner.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MTSCorner, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // STMCorner: 
                tileUp = STMCorner.GetComponent<Tile>().upSide;
                tileDown = STMCorner.GetComponent<Tile>().downSide;
                tileLeft = STMCorner.GetComponent<Tile>().leftSide;
                tileRight = STMCorner.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, STMCorner, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // MirrorWedge: 
                tileUp = MirrorWedge.GetComponent<Tile>().upSide;
                tileDown = MirrorWedge.GetComponent<Tile>().downSide;
                tileLeft = MirrorWedge.GetComponent<Tile>().leftSide;
                tileRight = MirrorWedge.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MirrorWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
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

        return changes;
    }

    private bool PlaceMTSCubes(Transform[,,] heightLevel, int height, int index) {
        bool changes = false;

        for (int x = 0; x < heightLevel.GetLength(1); x++) {
            for (int y = 0; y < heightLevel.GetLength(2); y++) {
                // If not mountain, continue.
                if (heightLevel[index, x, y] == null) {
                    continue;
                }
                if(!heightLevel[index, x, y].gameObject.GetComponent<Tile>().Land) {
                    continue;
                }

                bool success = false;

                // Sides of neighboring tiles. Left = Left of current tile.
                Tile.SideType upSide = Tile.SideType.Air;
                Tile.SideType downSide = Tile.SideType.Air;
                Tile.SideType leftSide = Tile.SideType.Air;
                Tile.SideType rightSide = Tile.SideType.Air;

                if (y > 0 && heightLevel[index, x, y - 1] != null) {
                    upSide = heightLevel[index, x, y - 1].gameObject.GetComponent<Tile>().downSide;
                }
                if (y < Mathf.RoundToInt(mapSize.y) - 1 && heightLevel[index, x, y + 1] != null) {
                    downSide = heightLevel[index, x, y + 1].gameObject.GetComponent<Tile>().upSide;
                }
                if (x > 0 && heightLevel[index, x - 1, y] != null) {
                    leftSide = heightLevel[index, x - 1, y].gameObject.GetComponent<Tile>().rightSide;
                }
                if (x < Mathf.RoundToInt(mapSize.x) - 1 && heightLevel[index, x + 1, y] != null) {
                    rightSide = heightLevel[index, x + 1, y].gameObject.GetComponent<Tile>().leftSide;
                }

                // Set Neighboring tile sides.
                if (upSide == Tile.SideType.MountainUp) {
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

                if (downSide == Tile.SideType.MountainUp) {
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

                if (leftSide == Tile.SideType.MountainUp) {
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

                if (rightSide == Tile.SideType.MountainUp) {
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

                // Tile sides.
                Tile.SideType tileUp = Tile.SideType.Land;
                Tile.SideType tileDown = Tile.SideType.Air;
                Tile.SideType tileLeft = Tile.SideType.Land;
                Tile.SideType tileRight = Tile.SideType.StairDown;

                // MTS: Bottom Air, Left Land, Top Land, Right Stair
                float rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MTS, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // STM: Bottom Air, Left Stair, Top Land, Right Land
                tileUp = Tile.SideType.Land;
                tileDown = Tile.SideType.Air;
                tileLeft = Tile.SideType.StairUp;
                tileRight = Tile.SideType.Land;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, STM, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // MTS Corner: Bottom Air, Left Stair, Top Land, Right Land
                tileUp = Tile.SideType.Land;
                tileDown = Tile.SideType.Air;
                tileLeft = Tile.SideType.Air;
                tileRight = Tile.SideType.StairDown;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MTSCorner, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // STM Corner: Bottom Air, Left Stair, Top Land, Right Land
                tileUp = Tile.SideType.Land;
                tileDown = Tile.SideType.Air;
                tileLeft = Tile.SideType.StairUp;
                tileRight = Tile.SideType.Air;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, STMCorner, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // MTSWedge
                tileUp = Tile.SideType.Land;
                tileDown = Tile.SideType.Land;
                tileLeft = Tile.SideType.Land;
                tileRight = Tile.SideType.StairDown;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MTSWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // STM Wedge
                tileUp = Tile.SideType.Land;
                tileDown = Tile.SideType.Land;
                tileLeft = Tile.SideType.StairUp;
                tileRight = Tile.SideType.Land;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, STMWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // Stair Corner
                tileUp = Tile.SideType.StairDown;
                tileDown = Tile.SideType.Air;
                tileLeft = Tile.SideType.StairUp;
                tileRight = Tile.SideType.Air;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, StairCorner, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // Stair Wedge
                tileUp = Tile.SideType.Land;
                tileDown = Tile.SideType.StairUp;
                tileLeft = Tile.SideType.Land;
                tileRight = Tile.SideType.StairDown;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, StairWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

                if (success)
                    continue;

                // Stair
                tileUp = Tile.SideType.Land;
                tileDown = Tile.SideType.Air;
                tileLeft = Tile.SideType.StairUp;
                tileRight = Tile.SideType.StairDown;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainStair, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                        success = true;
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

        return changes;

    }

    private void PlaceStairCubes(Transform[,,] heightLevel, int height, int index) {
        for(int x = 0; x < heightLevel.GetLength(1); x++) {
            for (int y = 0; y < heightLevel.GetLength(2); y++) {
                if(heightLevel[index,x,y] == null) {
                    continue;
                }

                // Tile sides.
                Tile.SideType tileUp = Tile.SideType.Land;
                Tile.SideType tileDown = Tile.SideType.Air;
                Tile.SideType tileLeft = Tile.SideType.Land;
                Tile.SideType tileRight = Tile.SideType.Land;

                // Sides of neighboring tiles. Left = Left of current tile.
                Tile.SideType upSide = Tile.SideType.Air;
                Tile.SideType downSide = Tile.SideType.Air;
                Tile.SideType leftSide = Tile.SideType.Air;
                Tile.SideType rightSide = Tile.SideType.Air;

                if (y > 0 && heightLevel[index,x, y - 1] != null) {
                    upSide = heightLevel[index, x, y - 1].gameObject.GetComponent<Tile>().downSide;
                }
                if (y < Mathf.RoundToInt(mapSize.y) - 1 && heightLevel[index, x, y + 1] != null) {
                    downSide = heightLevel[index, x, y + 1].gameObject.GetComponent<Tile>().upSide;
                }
                if (x > 0 && heightLevel[index, x - 1, y] != null) {
                    leftSide = heightLevel[index, x - 1, y].gameObject.GetComponent<Tile>().rightSide;
                }
                if (x < Mathf.RoundToInt(mapSize.x) - 1 && heightLevel[index, x + 1, y] != null) {
                    rightSide = heightLevel[index, x + 1, y].gameObject.GetComponent<Tile>().leftSide;
                }

                // Stair Side: Bottom Air, Left/Top/Right Land
                float rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        if (Random.Range(0.0f, 1.0f) < 0.15f) {
                            Destroy(heightLevel[index,x, y].gameObject);
                            tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainStair, rot, x, y, false);
                            heightLevel[index, x, y] = tiles[x, y];
                        }
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
                // Stair End: Left/Bottom/Right Air, Top Land
                tileUp = Tile.SideType.Land;
                tileDown = Tile.SideType.Air;
                tileLeft = Tile.SideType.Air;
                tileRight = Tile.SideType.Air;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        if(Random.Range(0.0f, 1.0f) < 0.75f) {
                            Destroy(heightLevel[index,x, y].gameObject);
                            tiles[x,y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainEndStair, rot, x, y, false);
                            heightLevel[index, x, y] = tiles[x, y];
                        }
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
    }
    */

    #region SeaLevel Methods    
    // Returns the x/y index of the tile with the fewest possibilities.
    private void FindSeaLevelTileWithFewestPossibilities(out int minx, out int miny) {
        int min = int.MaxValue;
        int xIndex = 0;
        int yIndex = 0;

        List<int> xZeros = new List<int>();
        List<int> yZeros = new List<int>();

        for (int x = 0; x < coefficientMatrix.GetLength(1); x++) {
            for (int y = 0; y < coefficientMatrix.GetLength(2); y++) {
                if (tiles[0, x, y] == null) {
                    if (coefficientMatrix[0, x, y].Count > 0 && coefficientMatrix[0, x, y].Count < min) {
                        min = coefficientMatrix[0, x, y].Count;
                        xIndex = x;
                        yIndex = y;
                    }
                    else if (coefficientMatrix[0, x, y].Count == 0) {
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
            tiles[0, xZero[i], yZero[i]] = null;
            ResetCoeffecientMatrixAtTile(0, xZero[i], yZero[i]);

            //Left
            if (xZero[i] > 0 && tiles[0, xZero[i] - 1, yZero[i]] != null && !tiles[0, xZero[i] - 1, yZero[i]].GetComponent<Tile>().Mountain) {
                tiles[0, xZero[i] - 1, yZero[i]] = null;
                ResetCoeffecientMatrixAtTile(0, xZero[i] - 1, yZero[i]);
                ResetCoeffecientMatrixSurroundingTile(0, xZero[i] - 1, yZero[i]);
            }

            //Right
            if (xZero[i] < Mathf.RoundToInt(mapSize.x) - 1 && tiles[0, xZero[i] + 1, yZero[i]] != null && !tiles[0, xZero[i] + 1, yZero[i]].GetComponent<Tile>().Mountain) {
                tiles[0, xZero[i] + 1, yZero[i]] = null;
                ResetCoeffecientMatrixAtTile(0, xZero[i] + 1, yZero[i]);
                ResetCoeffecientMatrixSurroundingTile(0, xZero[i] + 1, yZero[i]);
            }

            //Above
            if (yZero[i] > 0 && tiles[0, xZero[i], yZero[i] - 1] != null && !tiles[0, xZero[i], yZero[i] - 1].GetComponent<Tile>().Mountain) {
                tiles[0, xZero[i], yZero[i] - 1] = null;
                ResetCoeffecientMatrixAtTile(0, xZero[i], yZero[i] - 1);
                ResetCoeffecientMatrixSurroundingTile(0, xZero[i], yZero[i] - 1);
            }

            // Below
            if (yZero[i] < Mathf.RoundToInt(mapSize.y) - 1 && tiles[0, xZero[i], yZero[i] + 1] != null && !tiles[0, xZero[i], yZero[i] + 1].GetComponent<Tile>().Mountain) {
                tiles[0, xZero[i], yZero[i] + 1] = null;
                ResetCoeffecientMatrixAtTile(0, xZero[i], yZero[i] + 1);
                ResetCoeffecientMatrixSurroundingTile(0, xZero[i], yZero[i] + 1);
            }

            // DiagAboveLeft
            if (yZero[i] > 0 && xZero[i] > 0 && tiles[0, xZero[i] - 1, yZero[i] - 1] != null && !tiles[0, xZero[i] - 1, yZero[i] - 1].GetComponent<Tile>().Mountain) {
                tiles[0, xZero[i] - 1, yZero[i] - 1] = null;
                ResetCoeffecientMatrixAtTile(0, xZero[i] - 1, yZero[i] - 1);
                ResetCoeffecientMatrixSurroundingTile(0, xZero[i] - 1, yZero[i] - 1);
            }

            // DiagAboveRight
            if (yZero[i] > 0 && xZero[i] < Mathf.RoundToInt(mapSize.x) - 1 && tiles[0, xZero[i] + 1, yZero[i] - 1] != null && !tiles[0, xZero[i] + 1, yZero[i] - 1].GetComponent<Tile>().Mountain) {
                tiles[0, xZero[i] + 1, yZero[i] - 1] = null;
                ResetCoeffecientMatrixAtTile(0, xZero[i] + 1, yZero[i] - 1);
                ResetCoeffecientMatrixSurroundingTile(0, xZero[i] + 1, yZero[i] - 1);
            }

            // DiagBelowRight
            if (yZero[i] < Mathf.RoundToInt(mapSize.y) - 1 && tiles[0, xZero[i] + 1, yZero[i] + 1] != null && xZero[i] < Mathf.RoundToInt(mapSize.x) - 1 && !tiles[0, xZero[i] + 1, yZero[i] + 1].GetComponent<Tile>().Mountain) {
                tiles[0, xZero[i] + 1, yZero[i] + 1] = null;
                ResetCoeffecientMatrixAtTile(0, xZero[i] + 1, yZero[i] + 1);
                ResetCoeffecientMatrixSurroundingTile(0, xZero[i] + 1, yZero[i] + 1);
            }

            // DiagBelowLeft
            if (yZero[i] < Mathf.RoundToInt(mapSize.y) - 1 && tiles[0, xZero[i] - 1, yZero[i] + 1] != null && xZero[i] > 0 && !tiles[0, xZero[i] - 1, yZero[i] + 1].GetComponent<Tile>().Mountain) {
                tiles[0, xZero[i] - 1, yZero[i] + 1] = null;
                ResetCoeffecientMatrixAtTile(0, xZero[i] - 1, yZero[i] + 1);
                ResetCoeffecientMatrixSurroundingTile(0, xZero[i] - 1, yZero[i] + 1);
            }
        }
    }
    #endregion

    // Resets the coefficient matrix of all tiles.
    private void InstantiateCoeffecientMatrix() {
        for(int height = 0; height < coefficientMatrix.GetLength(0); height++) {
            for (int x = 0; x < coefficientMatrix.GetLength(1); x++) {
                for (int y = 0; y < coefficientMatrix.GetLength(2); y++) {
                    coefficientMatrix[height, x, y] = new List<Compatability>();
                    if (tiles[height, x, y] == null && height == 0) { // Height == 0 as the mt matrices are dependent on the tiles below.
                        for (int i = 0; i < tileList.Length; i++) {
                            CheckValid(coefficientMatrix[height, x, y], i, height, x, y);
                        }
                    }
                }
            }
        }
    }

    // Resets the coefficient matrix of all surrounding tiles (but not the given one).
    private void ResetCoeffecientMatrixSurroundingTile(int height, int x, int y, bool[,] mountValid = null) {
        if(x > 0) {
            if (tiles[height, x - 1, y] == null) {
                coefficientMatrix[height, x - 1, y].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[height, x - 1, y], i, height, x - 1, y, mountValid);
                }
            }
        }
        if (x < coefficientMatrix.GetLength(1) - 1) {
            if (tiles[height, x + 1, y] == null) {
                coefficientMatrix[height, x + 1, y].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[height, x + 1, y], i, height, x + 1, y, mountValid);
                }
            }
        }
        if (y > 0) {
            if (tiles[height, x, y - 1] == null) {
                coefficientMatrix[height, x, y - 1].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[height, x, y - 1], i, height, x, y - 1, mountValid);
                }
            }
        }
        if (y < coefficientMatrix.GetLength(2) - 1) {
            if (tiles[height, x, y + 1] == null) {
                coefficientMatrix[height, x, y + 1].Clear();
                for (int i = 0; i < tileList.Length; i++) {
                    CheckValid(coefficientMatrix[height, x, y + 1], i, height, x, y + 1, mountValid);
                }
            }
        }

    }

    // Resets the coefficient matrix at a given tile.
    private void ResetCoeffecientMatrixAtTile(int height, int x, int y, bool[,] mountValid = null) {
        coefficientMatrix[height, x, y].Clear();
        for (int i = 0; i < tileList.Length; i++) {
            CheckValid(coefficientMatrix[height, x, y], i, height, x, y, mountValid);
        }
    }

    // Returns true if there are no gaps. False otherwise.
    private bool CheckIfCollapsed() {
        for(int x = 0; x < tiles.GetLength(1); x++) {
            for (int y = 0; y < tiles.GetLength(2); y++) {
                if(tiles[0, x, y] == null) {
                    return false;
                }
            }
        }
        return true;
    }

    // Mountain Overload
    private bool CheckIfCollapsed(int height, bool[,] mtValid) {
        for (int x = 0; x < tiles.GetLength(1); x++) {
            for (int y = 0; y < tiles.GetLength(2); y++) {
                if (mtValid[x,y] && tiles[height, x, y] == null) {
                    return false;
                }
            }
        }
        return true;
    }

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
    private int GetHeightLevelFromPerlin(float val) {
        if (val <= 0.25f)
            return 0;
        else if (val <= 0.65f)
            return 1;
        else if (val <= 0.73f)
            return 2;
        else if (val <= 0.83f)
            return 3;
        else if (val <= 0.88f)
            return 4;
        else if (val <= 0.94f)
            return 5;
        else
            return 6;
    }
    #endregion

    // Places HeightMapCubes ||| Need to change mountains.
    public void PlaceHeightMapCubes(float[,] perlin) {
        // Place Water/Mountain Cubes
        for (int x = 0; x < coordinates.GetLength(0); x++) {
            for (int y = 0; y < coordinates.GetLength(1); y++) {
                int heightLevel = GetHeightLevelFromPerlin(perlin[x, y]);
                if(heightLevel == 0) {
                    tiles[0, x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, heightLevel, waterTile, 0f, x, y, true);
                }
                /*
                else if(heightLevel > 1) {
                    tiles[0, x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, heightLevel, landTile, 0f, x, y, true);
                }*/
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
        /*
        cubey.GetComponent<Tile>().tileLevel = heightLevel;

        // Fill in Land Cubes Below Mountain // Set Mountain Array
        if (heightLevel > 1 && perlin) {
            heightMapArrays[heightLevel - 2,x,y] = cubey;
            for (int j = heightLevel - 1; j >= 1; j--) {
                Transform land = Instantiate(landTile, new Vector3(coord.x, -0.5f + j, coord.y), Quaternion.Euler(new Vector3(0, 0, 0)));
                land.parent = parent;
                land.GetComponent<Tile>().tileLevel = j;
                if (j > 1) {
                    heightMapArrays[j - 2, x, y] = land;
                }
                else {
                    return land;
                }
            }
        }*/

        // Raise Water Cubes
        if (heightLevel == 0) {
            cubey.localPosition = cubey.localPosition + Vector3.up;
            tileComp.tileLevel = heightLevel + 1;
        }
        return cubey;
    }

    #region Unused (For Now) Methods
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

    // Unused but could be useful.
    public void GetIndexFromCoordinates(float xCoord, float yCoord, out int x, out int y) {
        x = 0;
        y = 0;
        for (int i = 0; i < coordinates.GetLength(0); i++) {
            for (int j = 0; j < coordinates.GetLength(1); j++) {
                if(xCoord == coordinates[i,j].x && yCoord == coordinates[i, j].y) {
                    x = i;
                    y = j;
                }
            }
        }
        
    }
    #endregion

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
