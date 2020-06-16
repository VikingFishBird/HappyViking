using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MapGenerator : MonoBehaviour
{
    public Vector2 mapSize;

    public Transform waterTile;
    public Transform landTile;

    public Coord[,] coordinates;
    public float noiseMapScale;

    List<int>[,] coefficientMatrix;
    Transform[,] tiles;
    static GameObject[] tileList;

    bool fullyCollapsed;

    // Start is called before the first frame update
    void Start()
    {
        // Get Tile Prefabs
        tileList = Resources.LoadAll<GameObject>("Prefabs/Tiles");
        for(int i = 0; i < tileList.Length; i++) {
            PlaceCubeAtCoord(new Coord(i, 50), transform, 1, tileList[i].transform, 90);
        }
        // Initialize coordinates array.
        coordinates = new Coord[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMap() {
        // Instantiate the Coefficient Matrix
        coefficientMatrix = new List<int>[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];
        tiles = new Transform[Mathf.RoundToInt(mapSize.x), Mathf.RoundToInt(mapSize.y)];

        // Set coordinates array values
        for (int x = 0; x < mapSize.x; x++) {
            for(int y = 0; y < mapSize.y; y++) {
                coordinates[x,y] = new Coord(-mapSize.x / 2 + 0.5f + x, -mapSize.y / 2 + 0.5f + y);             
            }
        }

        // Get Noise Map
        float[,] noiseMap = GenerateNoiseMap(noiseMapScale);
        // Set Water / Mountains
        PlaceHeightMapCubes(noiseMap);
        // Set possibilities for Wave Function Collpase Algorithm
        InstantiateCoeffecientMatrix();

        int count = 0;
        while (!fullyCollapsed) {
            int minx, miny;
            FindTileWithFewestPossibilities(out minx, out miny);
            SetTile(minx, miny);
            ResetCoeffecientMatrixSurroundingTile(minx, miny);
            fullyCollapsed = CheckIfCollapsed();
            count++;
            if(count > 1) {
                break;
            }
        }

        /*
        for (int x = 0; x < coefficientMatrix.GetLength(0); x = x + 5) {
            for (int y = 0; y < coefficientMatrix.GetLength(1); y = y +5) {
                List<string> tileStrings = new List<string>();
                for(int i = 0; i < coefficientMatrix[x,y].Count; i++) {
                    tileStrings.Add(tileList[coefficientMatrix[x, y][i]].name);
                }
                CreateWorldText(transform, string.Join(" | ", tileStrings), new Vector3(coordinates[x, y].x, 0, coordinates[x, y].y), 2, Color.black, TextAnchor.MiddleCenter, TextAlignment.Center, new Vector3(-90, 0, 0));
            }
        }*/
    }

    public void SetTile(int x, int y) {
        int tileIndex = 0;

        float sum = 0f;
        for(int i = 0; i < coefficientMatrix[x,y].Count; i++) {
            sum += tileList[coefficientMatrix[x, y][i]].GetComponent<Tile>().tileWeight;
        }
        float rand = Random.Range(0.0f, sum);
        float cumulativeSum = 0f;
        for (int i = 0; i < coefficientMatrix[x, y].Count; i++) {
            cumulativeSum += tileList[coefficientMatrix[x, y][i]].GetComponent<Tile>().tileWeight;
            if(rand < cumulativeSum) {
                tileIndex = i;
                float rot = GetRotation(tileList[i].GetComponent<Tile>(), x, y);
                tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], transform, -1, tileList[i].transform, rot);
                coefficientMatrix[x, y].Clear();
                return;
            }
        }
    }

    public void FindTileWithFewestPossibilities(out int minx, out int miny) {
        int min = int.MaxValue;
        int xIndex = 0;
        int yIndex = 0;

        for (int x = 0; x < coefficientMatrix.GetLength(0); x++) {
            for (int y = 0; y < coefficientMatrix.GetLength(1); y++) {
                if(tiles[x,y] == null) {
                    if (coefficientMatrix[x, y].Count > 0 && coefficientMatrix[x, y].Count < min) {
                        min = coefficientMatrix[x, y].Count;
                        xIndex = x;
                        yIndex = y;
                    }
                }
            }
        }
        minx = xIndex;
        miny = yIndex;
        for(int i = 0; i < coefficientMatrix[minx, miny].Count; i++) {
            print(tileList[coefficientMatrix[minx, miny][i]].name);
        }
    }

    public void InstantiateCoeffecientMatrix() {
        for(int x = 0; x < coefficientMatrix.GetLength(0); x++) {
            for (int y = 0; y < coefficientMatrix.GetLength(1); y++) {
                coefficientMatrix[x, y] = new List<int>();
                if (tiles[x,y] == null) {
                    for(int i = 0; i < tileList.Length; i++) {
                        if(CheckValid(tileList[i].GetComponent<Tile>(), x, y)) {
                            coefficientMatrix[x, y].Add(i);
                        }
                    }
                }
            }
        }
    }

    public void ResetCoeffecientMatrixSurroundingTile(int x, int y) {
        if(x > 0) {
            if (tiles[x - 1, y] == null) {
                coefficientMatrix[x - 1, y] = new List<int>();
                for (int i = 0; i < tileList.Length; i++) {
                    if (CheckValid(tileList[i].GetComponent<Tile>(), x - 1, y)) {
                        coefficientMatrix[x - 1, y].Add(i);
                    }
                }
            }
        }
        if (x < Mathf.RoundToInt(mapSize.x) - 1) {
            if (tiles[x + 1, y] == null) {
                coefficientMatrix[x + 1, y] = new List<int>();
                for (int i = 0; i < tileList.Length; i++) {
                    if (CheckValid(tileList[i].GetComponent<Tile>(), x + 1, y)) {
                        coefficientMatrix[x + 1, y].Add(i);
                    }
                }
            }
        }
        if (y > 0) {
            if (tiles[x, y - 1] == null) {
                coefficientMatrix[x, y - 1] = new List<int>();
                for (int i = 0; i < tileList.Length; i++) {
                    if (CheckValid(tileList[i].GetComponent<Tile>(), x, y - 1)) {
                        coefficientMatrix[x, y - 1].Add(i);
                    }
                }
            }
        }
        if (y < Mathf.RoundToInt(mapSize.y) - 1) {
            if (tiles[x, y + 1] == null) {
                coefficientMatrix[x, y + 1] = new List<int>();
                for (int i = 0; i < tileList.Length; i++) {
                    if (CheckValid(tileList[i].GetComponent<Tile>(), x, y + 1)) {
                        coefficientMatrix[x, y + 1].Add(i);
                    }
                }
            }
        }

    }

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
        if (val <= 0.3f)
            return 0;
        else if (val <= 0.75f)
            return 1;
        else if (val <= 0.9f)
            return 2;
        else
            return 3;
    }

    public void PlaceHeightMapCubes(float[,] perlin) {
        // Make sure the heirarchy isn't cluttered:
        string holderName = "Object Holder";
        if (transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

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

        // Refine Mountains
    }

    public Transform PlaceCubeAtCoord(Coord coord, Transform parent, int heightLevel, Transform tile, float rot) {
        Transform cubey = Instantiate(tile, new Vector3(coord.x, -0.5f + heightLevel, coord.y), Quaternion.Euler(new Vector3(0, rot, 0)));
        if(heightLevel == 0) {
            cubey.localPosition = cubey.localPosition + Vector3.up;
        }
        cubey.parent = parent;
        return cubey;
    }

    private int GetTileIndexFromName(string name) {
        for(int i = 0; i < tileList.Length; i++) {
            if (tileList[i].name.Equals(name)) {
                return i;
            }
        }
        print("Could not find tile: " + name);
        return 0;
    }

    private float GetRotation(Tile tile, int x, int y) {
        // Tile sides.
        Tile.SideType tileUp = tile.upSide;
        Tile.SideType tileDown = tile.downSide;
        Tile.SideType tileLeft = tile.leftSide;
        Tile.SideType tileRight = tile.rightSide;

        // Sides of neighboring tiles. Left = Left of current tile.
        Tile.SideType upSide = Tile.SideType.DNE;
        Tile.SideType downSide = Tile.SideType.DNE;
        Tile.SideType leftSide = Tile.SideType.DNE;
        Tile.SideType rightSide = Tile.SideType.DNE;

        // Set Neighboring tile sides.


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

        float rot = 0f;
        for (int i = 0; i < 4; i++) {
            if ((tileUp == upSide || upSide == Tile.SideType.DNE)
            && (tileDown == downSide || downSide == Tile.SideType.DNE)
            && (tileLeft == leftSide || leftSide == Tile.SideType.DNE)
            && (tileRight == rightSide || rightSide == Tile.SideType.DNE)) {
                print(rot);
                return rot;
            }
            // Rotate
            rot += 90;
            Tile.SideType temp = tileUp;
            tileUp = tileRight;
            tileRight = tileDown;
            tileDown = tileLeft;
            tileLeft = temp;
        }
        
        return rot;
    }

    private bool CheckValid(Tile tile, int x, int y) {
        // Tile sides.
        Tile.SideType tileUp = tile.upSide;
        Tile.SideType tileDown = tile.downSide;
        Tile.SideType tileLeft = tile.leftSide;
        Tile.SideType tileRight = tile.rightSide;

        // Sides of neighboring tiles. Left = Left of current tile.
        Tile.SideType upSide = Tile.SideType.DNE;
        Tile.SideType downSide = Tile.SideType.DNE;
        Tile.SideType leftSide = Tile.SideType.DNE;
        Tile.SideType rightSide = Tile.SideType.DNE;

        // Set Neighboring tile sides.


        if (y > 0 && tiles[x, y - 1] != null) {
            upSide = tiles[x, y - 1].gameObject.GetComponent<Tile>().downSide;
        }
        if (y < Mathf.RoundToInt(mapSize.y) - 1 && tiles[x, y + 1] != null) {
            downSide = tiles[x, y + 1].gameObject.GetComponent<Tile>().upSide;
        }
        if (x > 0 && tiles[x-1, y] != null) {
            leftSide = tiles[x - 1, y].gameObject.GetComponent<Tile>().rightSide;
        }
        if (x < Mathf.RoundToInt(mapSize.x) - 1 && tiles[x + 1, y] != null) {
            rightSide = tiles[x + 1, y].gameObject.GetComponent<Tile>().leftSide;
        }

        for (int i = 0; i < 4; i++) {
            if ((tileUp == upSide || upSide == Tile.SideType.DNE)
            && (tileDown == downSide || downSide == Tile.SideType.DNE)
            && (tileLeft == leftSide || leftSide == Tile.SideType.DNE)
            && (tileRight == rightSide || rightSide == Tile.SideType.DNE)) {
                return true;
            }
            // Rotate
            Tile.SideType temp = tileUp;
            tileUp = tileRight;
            tileRight = tileDown;
            tileDown = tileLeft;
            tileLeft = temp;
        }

        return false;
    }

    public TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, Vector3 rotation) {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.transform.localRotation = Quaternion.Euler(rotation);
        return textMesh;
    }
}
