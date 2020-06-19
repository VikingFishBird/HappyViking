using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MapGenerator : MonoBehaviour
{
    public Vector2 mapSize;
    public float noiseMapScale;

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



    Transform mapHolder;

    public Coord[,] coordinates;

    List<Compatability>[,] coefficientMatrix;
    Transform[,] tiles;
    static GameObject[] tileList;

    bool fullyCollapsed;

    // Mountain Arrays [height, x, y]
    Transform[,,] heightMapArrays;

    // Start is called before the first frame update
    void Start()
    {
        // Get Tile Prefabs
        tileList = Resources.LoadAll<GameObject>("Prefabs/Tiles");
        for(int i = 0; i < tileList.Length; i++) {
            PlaceCubeAtCoord(new Coord(i, 60), transform, 1, tileList[i].transform, 90, 0, 0, false);
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

        heightMapArrays = new Transform[4, Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];

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

        // Mountains \\
        // Iterate through each level
        for(int i = 0; i < heightMapArrays.GetLength(0); i++) {
            string mappyThingString = "";
            for (int x = 0; x < heightMapArrays.GetLength(1); x++) {
                for (int y = 0; y < heightMapArrays.GetLength(2); y++) {
                    if (heightMapArrays[i, x, y] == null)
                        mappyThingString += "N ";
                    else
                        mappyThingString += "M ";
                }
                mappyThingString += '\n';
            }

            print(mappyThingString);
            // Place Stairs
            PlaceStairCubes(heightMapArrays, i + 2, i);
            // Place Stair-dependent tiles
            bool changes = true;
            count = 0;
            while(changes && count < 1) {
                changes = PlaceMTSCubes(heightMapArrays, i + 2, i);
                count++;
            }
            // Place Non Stair-Dependent Tiles
            changes = true;
            changes = PlaceRemainingMountain(heightMapArrays, i + 2, i);
        }
    }
    
    public bool PlaceRemainingMountain(Transform[,,] heightLevel, int height, int index) {
        bool changes = false;

        for (int x = 0; x < heightLevel.GetLength(1); x++) {
            for (int y = 0; y < heightLevel.GetLength(2); y++) {
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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
                // DubWedge: 
                tileUp = DubWedge.GetComponent<Tile>().upSide;
                tileDown = DubWedge.GetComponent<Tile>().downSide;
                tileLeft = DubWedge.GetComponent<Tile>().leftSide;
                tileRight = DubWedge.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, DubWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
                // MountainEnd:
                tileUp = MountainEnd.GetComponent<Tile>().upSide;
                tileDown = MountainEnd.GetComponent<Tile>().downSide;
                tileLeft = MountainEnd.GetComponent<Tile>().leftSide;
                tileRight = MountainEnd.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainEnd, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
                // MountainSide: 
                tileUp = MountainSide.GetComponent<Tile>().upSide;
                tileDown = MountainSide.GetComponent<Tile>().downSide;
                tileLeft = MountainSide.GetComponent<Tile>().leftSide;
                tileRight = MountainSide.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainSide, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
                // MountainWedge: 
                tileUp = MountainWedge.GetComponent<Tile>().upSide;
                tileDown = MountainWedge.GetComponent<Tile>().downSide;
                tileLeft = MountainWedge.GetComponent<Tile>().leftSide;
                tileRight = MountainWedge.GetComponent<Tile>().rightSide;

                rot = 0f;
                for (int i = 0; i < 4; i++) {
                    print((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide));
                    if ((tileUp == upSide)
                    && (tileDown == downSide)
                    && (tileLeft == leftSide)
                    && (tileRight == rightSide)) {
                        Destroy(heightLevel[index, x, y].gameObject);
                        tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height, MountainWedge, rot, x, y, false);
                        heightLevel[index, x, y] = tiles[x, y];
                        changes = true;
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

    public bool PlaceMTSCubes(Transform[,,] heightLevel, int height, int index) {
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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }
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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

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
                    }
                    // Rotate
                    rot += 90f;
                    Tile.SideType temp = tileUp;
                    tileUp = tileLeft;
                    tileLeft = tileDown;
                    tileDown = tileRight;
                    tileRight = temp;
                }

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

    public void PlaceStairCubes(Transform[,,] heightLevel, int height, int index) {
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
                        if (Random.Range(0.0f, 1.0f) < 0.75f) {
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
                tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, coefficientMatrix[x, y][i].heightLevel, tileList[coefficientMatrix[x, y][i].index].transform, rot, x, y, false);
                coefficientMatrix[x, y].Clear();
                return;
            }
        }
    }

    // Sets the surrounding tiles (including kitty corners) to null and resets the surrounding co-matrices.
    public void BackTrack(List<int> xZero, List<int> yZero) {
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
        else if (val <= 0.83f)
            return 2;
        else if (val <= 0.90f)
            return 3;
        else if (val <= 0.96f)
            return 4;
        else
            return 5;
    }

    // Places HeightMapCubes ||| Need to change mountains.
    public void PlaceHeightMapCubes(float[,] perlin) {
        // Place Water/Mountain Cubes
        for (int x = 0; x < coordinates.GetLength(0); x++) {
            for (int y = 0; y < coordinates.GetLength(1); y++) {
                int heightLevel = GetHeightLevelFromPerlin(perlin[x, y]);
                if(heightLevel == 0) {
                    tiles[x,y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, heightLevel, waterTile, 0f, x, y, true);
                }
                else if(heightLevel > 1) {
                    tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, heightLevel, landTile, 0f, x, y, true);
                }
            }
        }
    }

    // Places tile at a coordinate... yep.
    public Transform PlaceCubeAtCoord(Coord coord, Transform parent, int heightLevel, Transform tile, float rot, int x, int y, bool perlin) {
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

        cubey.parent = parent;
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
        }

        // Raise Water Cubes
        if (heightLevel == 0) {
            cubey.localPosition = cubey.localPosition + Vector3.up;
            cubey.GetComponent<Tile>().tileLevel = heightLevel + 1;
        }
        return cubey;
    }

    // Adds all valid matches of a certain tile to the CoefficientMatrix
    private void CheckValid(List<Compatability> matrix, int tileIndex, int x, int y) {
        // Future Optimization: Add all coast tiles at once by type. No need to iterate through 3 coast corners.
        if (tileList[tileIndex].GetComponent<Tile>().Mountain) {
            return;
        }
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
                matrix.Add(new Compatability(tileIndex, rot, 1, tileList[tileIndex].GetComponent<Tile>().tileWeight));
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
        public float tileWeight;

        public Compatability(int ind, float rot, int level, float weight) {
            index = ind;
            rotation = rot;
            heightLevel = level;
            tileWeight = weight;
        }
    }
}
