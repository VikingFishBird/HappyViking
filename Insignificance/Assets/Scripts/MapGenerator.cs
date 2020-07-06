using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

// Optimize: Combine Meshes
public class MapGenerator : MonoBehaviour
{
    [Header("Map Details")]
    public int mapSize;
    public float noiseMapScale;
    public float fallOffMapPower;
    [Range(0.0f, 1.0f)]
    public float waterRate;
    public float mtRate;
    public int chunkSize;

    [Space]
    [Header("Tile Prefabs")]
    public Transform waterTile;
    public Transform landTile;

    public Transform[] Corner;
    public Transform[] Side;
    public Transform[] Wedge;
    public Transform[] Peak;
    public Transform[] MTLand;

    [Space]
    [Header("Shared Materials")]
    public Material SnowForestGrass;
    public Material SnowForestStone;
    public Material ForestGrass;
    public Material ForestStone;
    public Material PlainGrass;
    public Material PlainStone;
    public Material DesertGrass;
    public Material DesertStone;
    public Material Water;
    public Material Coast;

    public Material RainForestGrass;
    public Material RainForestStone;
    public Material SavannaGrass;
    public Material SavannaStone;
    public Material TundraGrass;
    public Material TundraStone;

    [Space]
    [Header("Trees")]
    public Transform[] spruce;
    public Transform[] oak;
    public Transform[] desert;

    [Space]
    [Header("-- Map Variables --")]
    // Array of coordinates of tiles.       CLEAN THIS UP
    public Coord[,] coordinates;
    public Transform[,] tiles;      // Floor Tiles
    Tile[,] tileComps;              // Tile Object attached to tiles.
    Transform[,,] mtTiles;          // Mountain Tiles
    Transform[,] topTiles;          // Top Tiles
    Chunk[,] chunks;                 // List of chunks. Goes left to right then up to down.

    // The GameObject holding all the tiles.
    Transform mapHolder;
    // Holds all Mountains
    MountainData mtData;
    // Holds possible tiles.
    List<Compatability>[,] coefficientMatrix;
    // Prefab List
    static GameObject[] tileList;
    
    // Boolean for the WFC completion.
    bool fullyCollapsed;

    // BIOME OBJECTS
    public enum Biome { SnowForest, Forest, Plains, Desert, Tundra, RainForest, Savanna, Water, Coast};
    Biome[,] BiomeMap = { { Biome.Tundra, Biome.Tundra, Biome.Tundra, Biome.Tundra, Biome.Desert, Biome.Desert, Biome.Desert, Biome.Desert, Biome.Desert, Biome.Desert },
                          { Biome.Tundra, Biome.Tundra, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Desert, Biome.Desert, Biome.Desert },
                          { Biome.Tundra, Biome.Tundra, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Desert, Biome.Desert },
                          { Biome.Tundra, Biome.Tundra, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Savanna, Biome.Savanna },
                          { Biome.Tundra, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Savanna, Biome.Savanna },
                          { Biome.SnowForest, Biome.SnowForest, Biome.Plains, Biome.Plains, Biome.Plains, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Savanna, Biome.Savanna },
                          { Biome.SnowForest, Biome.SnowForest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Savanna, Biome.Savanna },
                          { Biome.SnowForest, Biome.SnowForest, Biome.SnowForest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.RainForest, Biome.RainForest },
                          { Biome.SnowForest, Biome.SnowForest, Biome.SnowForest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.RainForest, Biome.RainForest },
                          { Biome.SnowForest, Biome.SnowForest, Biome.SnowForest, Biome.SnowForest, Biome.Forest, Biome.Forest, Biome.Forest, Biome.RainForest, Biome.RainForest, Biome.RainForest }};
    Dictionary<Biome, float> treeRate = new Dictionary<Biome, float>() {
        { Biome.SnowForest, 0.08f},
        { Biome.Forest, 0.08f},
        { Biome.Desert, 0.01f},
        { Biome.Plains, 0.03f},
        { Biome.Tundra, 0.01f},
        { Biome.RainForest, 0.15f},
        { Biome.Savanna, 0.03f}
    };
    Dictionary<Biome, Transform[]> treeType; // Returns the tree type of a biome.
    Dictionary<Biome, int> biomeIDs = new Dictionary<Biome, int>() {
        { Biome.Plains, 0 },
        { Biome.Forest, 1 },
        { Biome.SnowForest, 2 },
        { Biome.RainForest, 3 },
        { Biome.Tundra, 4 },
        { Biome.Desert, 5 },
        { Biome.Savanna, 6 },
        { Biome.Water, 7 },
        { Biome.Coast, 8 },
    };
    Dictionary<Biome, Material> biomeMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // Get Tile Prefabs
        tileList = Resources.LoadAll<GameObject>("Prefabs/Tiles");
        mtData = new MountainData();

        treeType = new Dictionary<Biome, Transform[]>() {
            { Biome.SnowForest, spruce},
            { Biome.Forest, spruce},
            { Biome.Desert, desert},
            { Biome.Plains, oak},
            { Biome.Tundra, oak},
            { Biome.RainForest, spruce},
            { Biome.Savanna, oak}
        };
        biomeMaterial = new Dictionary<Biome, Material>() {
            { Biome.Plains, PlainGrass},
            { Biome.Desert, DesertGrass},
            { Biome.Forest, ForestGrass},
            { Biome.RainForest, RainForestGrass},
            { Biome.SnowForest, SnowForestGrass},
            { Biome.Tundra, TundraGrass},
            { Biome.Savanna, SavannaGrass},
            { Biome.Water, Water},
            { Biome.Coast, Coast}
        };


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
        tiles = new Transform[mapSize, mapSize];
        coordinates = new Coord[mapSize, mapSize];
        coefficientMatrix = new List<Compatability>[mapSize, mapSize];
        topTiles = new Transform[mapSize, mapSize];
        mtTiles = new Transform[5, mapSize, mapSize];
        tileComps = new Tile[mapSize, mapSize];

        int chunksPerRow = mapSize / chunkSize;
        chunks = new Chunk[chunksPerRow, chunksPerRow];


        // Set COORDINATES ARRAY values
        for (int x = 0; x < mapSize; x++) {
            for(int y = 0; y < mapSize; y++) {
                coordinates[x,y] = new Coord(-mapSize / 2 + 0.5f + x, mapSize / 2 - 0.5f - y);             
            }
        }

        // Reset values and make sure the heirarchy isn't cluttered and delete existing map blocks.
        string holderName = "Object Holder";
        if (transform.Find(holderName)) {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < chunks.GetLength(0); x++) {
            for (int y = 0; y < chunks.GetLength(1); y++) {
                chunks[x, y] = new Chunk(mapHolder);
            }
        }

        fullyCollapsed = false;
        
        // Get Noise Map
        float[,] noiseMap = GenerateNoiseMap(noiseMapScale, true);
        // Set Water / Mountains
        PlaceHeightMapCubes(noiseMap);
        // Set possibilities for Wave Function Collpase Algorithm
        InstantiateCoeffecientMatrix();

        int count = 0;
        while (!fullyCollapsed) {
            int minx, miny;
            // Find the tile with the fewest possibilities
            FindSeaLevelTileWithFewestPossibilities(out minx, out miny, out fullyCollapsed, out count);
            if (fullyCollapsed)
                break;

            // Set the specified tile
            SetTile(minx, miny);
            // Reset possibilities of surrounding tiles
            ResetCoeffecientMatrixSurroundingTile(minx, miny);
            // Prevent Endless Loop
            count++;
            if(count > 30000) {
                break;
            }
        }

        // Mountains
        int mountainAttempts = Mathf.RoundToInt(mapSize * mtRate);
        Dictionary<BlockType, Transform[]> mtDic = new Dictionary<BlockType, Transform[]>() {
            { BlockType.Air, null },
            { BlockType.Side, Side },
            { BlockType.Wedge, Wedge },
            { BlockType.Corner, Corner },
            { BlockType.Land, MTLand },
            { BlockType.Peak, Peak }
        };

        for(int i = 0; i < mountainAttempts; i++) {
            // Coordinate
            Vector2Int coords = new Vector2Int(Random.Range(0, tiles.GetLength(0)), Random.Range(0, tiles.GetLength(1)));
            List<Mountain> mts = FindMtsAtCoord(coords, tileComps);
            Mountain mt = SetMT(mts);
            if (mt != null) {
                PlaceMT(mt, coords, mtDic);
            }
        }

        /*
        // BiomeMeshArrays
        List<Mesh> desertLand = new List<Mesh>();
        List<Mesh> desertStone = new List<Mesh>();
        List<Mesh> plainsLand = new List<Mesh>();
        List<Mesh> plainsStone = new List<Mesh>();
        List<Mesh> forestLand = new List<Mesh>();
        List<Mesh> forestStone = new List<Mesh>();
        List<Mesh> snowForestLand = new List<Mesh>();
        List<Mesh> snowForestStone = new List<Mesh>();
        List<Mesh> savannaLand = new List<Mesh>();
        List<Mesh> savannaStone = new List<Mesh>();
        List<Mesh> tundraLand = new List<Mesh>();
        List<Mesh> tundraStone = new List<Mesh>();
        List<Mesh> rainForestLand = new List<Mesh>();
        List<Mesh> rainForestStone = new List<Mesh>();*/

        // Biomes
        Biome[,] biomes = GenerateBiomesArray();
        for(int x = 0; x < biomes.GetLength(0); x++) {
            for (int y = 0; y < biomes.GetLength(1); y++) {
                MeshRenderer meshRenderer = tiles[x, y].GetChild(0).GetComponent<MeshRenderer>();
                Material[] mats = meshRenderer.materials;
                Tile tileComp = tileComps[x, y];
                //Material grass;
                //Material stone;
                //List<Mesh> grassMeshList;
                //List<Mesh> stoneMeshList;

                int chunkX = x / chunkSize;
                int chunkY = y / chunkSize;
                int chunkBiomeIndex = biomeIDs[biomes[x, y]];

                tiles[x, y].parent = chunks[chunkX, chunkY].gameObjects[chunkBiomeIndex].transform;
                chunks[chunkX, chunkY].gameObjects[chunkBiomeIndex].GetComponent<MeshRenderer>().material = biomeMaterial[biomes[x, y]];

                #region OldBiomeCode
                /*
                if(biomes[x,y] == Biome.SnowForest) {
                    grass = SnowForestGrass;
                    stone = SnowForestStone;

                    grassMeshList = snowForestLand;
                    stoneMeshList = snowForestStone;
                }
                else if (biomes[x, y] == Biome.Forest) {
                    grass = ForestGrass;
                    stone = ForestStone;

                    grassMeshList = forestLand;
                    stoneMeshList = forestStone;
                }
                else if (biomes[x, y] == Biome.Plains) {
                    grass = PlainGrass;
                    stone = PlainStone;

                    grassMeshList = plainsLand;
                    stoneMeshList = plainsStone;
                }
                else if (biomes[x, y] == Biome.Savanna) {
                    grass = SavannaGrass;
                    stone = SavannaStone;

                    grassMeshList = savannaLand;
                    stoneMeshList = savannaStone;
                }
                else if (biomes[x, y] == Biome.RainForest) {
                    grass = RainForestGrass;
                    stone = RainForestStone;

                    grassMeshList = rainForestLand;
                    stoneMeshList = rainForestStone;
                }
                else if (biomes[x, y] == Biome.Tundra) {
                    grass = TundraGrass;
                    stone = TundraStone;

                    grassMeshList = tundraLand;
                    stoneMeshList = tundraStone;
                }
                else {
                    grass = DesertGrass;
                    stone = DesertStone;

                    grassMeshList = desertLand;
                    stoneMeshList = desertStone;
                }

                if (tileComp.grass != -1) {
                    mats[tileComp.grass] = grass;
                    grassMeshList.Add(tiles[x, y].GetChild(0).GetComponent<MeshFilter>().mesh.GetSubmesh(tileComp.grass));
                }
                if (tileComp.stone != -1) {
                    mats[tileComp.stone] = stone;
                    stoneMeshList.Add(tiles[x, y].GetChild(0).GetComponent<MeshFilter>().mesh.GetSubmesh(tileComp.stone));

                }

                tileComp.biome = biomes[x, y];
                meshRenderer.materials = mats;
                
                // Mts
                for(int height = 0; height < 5; height++) {
                    if(mtTiles[height, x, y] != null) {
                        MeshRenderer mtMeshRenderer = mtTiles[height, x, y].GetChild(0).GetComponent<MeshRenderer>();
                        Material[] matsMT = mtMeshRenderer.materials;
                        Tile tileCompMT = mtTiles[height, x, y].GetComponent<Tile>();

                        if (tileComp.grass != -1) {
                            matsMT[tileCompMT.grass] = grass;
                        }
                        if (tileComp.stone != -1) {
                            matsMT[tileCompMT.stone] = stone;
                        }

                        tileCompMT.biome = biomes[x, y];
                        mtMeshRenderer.materials = matsMT;

                    }
                    else if(height == 0) {
                        float chance = treeRate[tileComp.biome];
                        Transform[] _treeType = treeType[tileComp.biome];
                        for (int i = 0; i < tileComp.potentialTreeLocations.Length; i++) {
                            if (Random.Range(0.0f, 1.0f) < chance) {
                                Transform tree = Instantiate(_treeType[Random.Range(0, _treeType.Length)], new Vector3(coordinates[x, y].x + tileComp.potentialTreeLocations[i].x, 1f, coordinates[x, y].y + tileComp.potentialTreeLocations[i].y), Quaternion.Euler(new Vector3(0, Random.Range(0.0f, 360.0f), 0)));
                                tree.parent = tiles[x, y];
                            }
                        }
                    }
                }*/
                #endregion
            }
        }

        for(int x = 0; x < chunks.GetLength(0); x++) {
            for (int y = 0; y < chunks.GetLength(1); y++) {
                for(int i = 0; i < chunks[x,y].gameObjects.Length; i++) {
                    GameObject obj = chunks[x, y].gameObjects[i];
                    MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
                    //Tile[] tileComps = obj.GetComponentsInChildren<Tile>();
                    CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                    for(int j = 0; j < meshFilters.Length; j++) {
                        combine[j].mesh = meshFilters[j].mesh.GetSubmesh(0);
                        combine[j].transform = meshFilters[j].transform.localToWorldMatrix;
                        meshFilters[j].gameObject.SetActive(false);
                    }

                    obj.transform.GetComponent<MeshFilter>().mesh = new Mesh();
                    obj.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
                    obj.SetActive(true);
                }
            }
        }
        for (int x = 0; x < chunks.GetLength(0); x++) {
            for (int y = 0; y < chunks.GetLength(1); y++) {
                for (int i = 0; i < chunks[x, y].gameObjects.Length; i++) {
                    if(chunks[x, y].gameObjects[i].transform.childCount > 0) {

                    }
                    else {
                        Destroy(chunks[x, y].gameObjects[i]);
                    }
                }
            }
        }
    }
    
    #region Biomes
    // Generates an array of Biomes
    private Biome[,] GenerateBiomesArray() {
        float[,] precipnoiseMap = GenerateNoiseMap(4, false);
        float[,] tempnoiseMap = GenerateNoiseMap(4, false);
        Biome[,] biomes = new Biome[precipnoiseMap.GetLength(0), precipnoiseMap.GetLength(1)];
        for (int y = 0; y < biomes.GetLength(1); y++) {
            float basePrec = EvalPrecipitation((float)y / biomes.GetLength(1));
            float baseTemp = (float)y / biomes.GetLength(1);
            for (int x = 0; x < biomes.GetLength(0); x++) {
                int prec = Mathf.FloorToInt(Mathf.Clamp(0.2f * (precipnoiseMap[x, y] - 0.5f) + basePrec, 0.0f, 0.999f) * 10);
                int temp = Mathf.FloorToInt(Mathf.Clamp(0.2f * (tempnoiseMap[x, y] - 0.5f) + baseTemp, 0.0f, 0.999f) *10);
                biomes[x, y] = BiomeMap[prec, temp];
            }
        }

        return biomes;

    }
    
    private float EvalPrecipitation(float x) {
        if(x < 0.5f) {
            return 2 * x;
        }
        else {
            return -2 * x + 2;
        }
    }
    #endregion

    #region Mountains
    // Mountain Methods
    public List<Mountain> FindMtsAtCoord(Vector2Int coords, Tile[,] tileComps) {
        Mountain[] mountains = mtData.mountains;

        List<Mountain> mts = new List<Mountain>();
        List<Vector2Int> failedCoords = new List<Vector2Int>();

        for (int i = 0; i < mountains.Length; i++) { // Make Sure to sort mountains by smallest size to greatest size.
            Mountain mt = mountains[i];

            // Quick Invalidations
            if (mt.width + coords.x > tiles.GetLength(0) || mt.length + coords.y > tiles.GetLength(1)) {
                continue;
            }

            bool dq = false;
            // Check if disqualified by previous failed coords:
            for (int coord = 0; coord < failedCoords.Count; coord++) {
                if (mt.width + coords.x >= failedCoords[coord].x && mt.length + coords.y >= failedCoords[coord].y) {
                    dq = true;
                }
            }

            if (dq) {
                continue;
            }

            // Future Optimization: Iterate through the edges as those will more likely be an issue.
            for(int x = coords.x; x < coords.x + mt.width; x++) {
                for (int y = coords.y; y < coords.y + mt.length; y++) {
                    if (tileComps[x, y].CoastOrWater || topTiles[x,y].GetComponent<Tile>().Mountain)
                        dq = true;
                }
            }

            if (!dq) {
                mts.Add(mt);
            }
        }

        return mts;
    }

    private Mountain SetMT(List<Mountain> mts) {
        // Calculate the total sum of the mtsArray weights.
        float sum = 0f;
        for(int i = 0; i < mts.Count; i++) {
            sum += mts[i].weight;
        }
        float rand = Random.Range(0.0f, sum);
        float cumSum = 0.0f;
        // Select random MT (weighted).
        for (int i = 0; i < mts.Count; i++) {
            cumSum += mts[i].weight;
            if(rand < cumSum) {
                return mts[i];
            }
        }

        // This should not happen if mts.Count > 0.
        return null;
    }
    
    private void PlaceMT(Mountain mt, Vector2Int coords, Dictionary<BlockType, Transform[]> dic) {
        for(int height = 0; height < mt.height; height++) {
            for (int y = coords.y; y < mt.length + coords.y; y++) {
                for (int x = coords.x; x < mt.width + coords.x; x++) {
                    Transform[] block = dic[mt.blockArray[height, y - coords.y, x - coords.x].blockType];
                    if(block != null) {
                        mtTiles[height, x ,y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, height + 2, block[Random.Range(0, block.Length)], mt.blockArray[height, y - coords.y, x - coords.x].rotation, true);
                        topTiles[x, y] = mtTiles[height, x, y];
                    }
                }
            }
        }
    }
    #endregion

    #region SeaLevel WFC Methods    
    // Adds all valid matches of a certain tile to the CoefficientMatrix
    private void CheckValid(List<Compatability> matrix, int tileIndex, int x, int y) {
        // Future Must-Do Optimization: Add all coast tiles at once by type. No need to iterate through 3 coast corners.
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
            upSide = tileComps[x, y - 1].downSide;
        }
        if (y < mapSize - 1 && tiles[x, y + 1] != null) {
            downSide = tileComps[x, y + 1].upSide;
        }
        if (x > 0 && tiles[x - 1, y] != null) {
            leftSide = tileComps[x - 1, y].rightSide;
        }
        if (x < mapSize - 1 && tiles[x + 1, y] != null) {
            rightSide = tileComps[x + 1, y].leftSide;
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
    // Optimization: Add a tileListComps Array. Is it possible to merge for loops? Or calculate the sum in another place.
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
                tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, 1, tileList[coefficientMatrix[x, y][i].index].transform, rot);
                topTiles[x, y] = tiles[x, y];
                tileComps[x, y] = tiles[x, y].GetComponent<Tile>();
                coefficientMatrix[x, y].Clear();
                return;
            }
        }
    }
    
    // Returns the x/y index of the tile with the fewest possibilities.
    private void FindSeaLevelTileWithFewestPossibilities(out int minx, out int miny, out bool fullyCollapsed, out int count) {
        int min = int.MaxValue;
        int xIndex = 0;
        int yIndex = 0;
        count = 0;

        fullyCollapsed = true;

        List<int> xZeros = new List<int>();
        List<int> yZeros = new List<int>();

        for (int x = 0; x < coefficientMatrix.GetLength(0); x++) {
            for (int y = 0; y < coefficientMatrix.GetLength(1); y++) {
                if (tiles[x, y] == null) {
                    fullyCollapsed = false;
                    count++;
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
            if (xZero[i] < mapSize - 1 && tiles[xZero[i] + 1, yZero[i]] != null && !tiles[xZero[i] + 1, yZero[i]].GetComponent<Tile>().Mountain) {
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
            if (yZero[i] < mapSize - 1 && tiles[xZero[i], yZero[i] + 1] != null && !tiles[xZero[i], yZero[i] + 1].GetComponent<Tile>().Mountain) {
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
            if (yZero[i] > 0 && xZero[i] < mapSize - 1 && tiles[xZero[i] + 1, yZero[i] - 1] != null && !tiles[xZero[i] + 1, yZero[i] - 1].GetComponent<Tile>().Mountain) {
                tiles[xZero[i] + 1, yZero[i] - 1] = null;
                ResetCoeffecientMatrixAtTile(xZero[i] + 1, yZero[i] - 1);
                ResetCoeffecientMatrixSurroundingTile(xZero[i] + 1, yZero[i] - 1);
            }

            // DiagBelowRight
            if (yZero[i] < mapSize - 1 && tiles[xZero[i] + 1, yZero[i] + 1] != null && xZero[i] < mapSize - 1 && !tiles[xZero[i] + 1, yZero[i] + 1].GetComponent<Tile>().Mountain) {
                tiles[xZero[i] + 1, yZero[i] + 1] = null;
                ResetCoeffecientMatrixAtTile(xZero[i] + 1, yZero[i] + 1);
                ResetCoeffecientMatrixSurroundingTile(xZero[i] + 1, yZero[i] + 1);
            }

            // DiagBelowLeft
            if (yZero[i] < mapSize - 1 && tiles[xZero[i] - 1, yZero[i] + 1] != null && xZero[i] > 0 && !tiles[xZero[i] - 1, yZero[i] + 1].GetComponent<Tile>().Mountain) {
                tiles[xZero[i] - 1, yZero[i] + 1] = null;
                ResetCoeffecientMatrixAtTile(xZero[i] - 1, yZero[i] + 1);
                ResetCoeffecientMatrixSurroundingTile(xZero[i] - 1, yZero[i] + 1);
            }
        }
    }

    // Resets the coefficient matrix of all tiles.
    // Optimization: Is it possible to avoid another loop?
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
    // Optimization: Create separate methods for Removing options and resetting.
    private void ResetCoeffecientMatrixAtTile(int x, int y) {
        coefficientMatrix[x, y].Clear();
        for (int i = 0; i < tileList.Length; i++) {
            CheckValid(coefficientMatrix[x, y], i, x, y);
        }
    }

    #endregion

    #region Noise
    // Creates Noise Map... yeah
    private float[,] GenerateNoiseMap(float scale, bool falloffBool) {
        float[,] noiseMap = new float[mapSize, mapSize];
        float offSetX = Random.Range(-100000, 100000);
        float offSetY = Random.Range(-100000, 100000);

        float halfWidth = mapSize / 2f;
        float halfHeight = mapSize / 2f;
        float[,] falloff = new float[noiseMap.GetLength(0), noiseMap.GetLength(1)];
        if (falloffBool) {
            falloff = GenerateFalloffMap();
        }
        for (int x = 0; x < mapSize; x++) {

            for (int y = 0; y < mapSize; y++) {
                float sampleX = (x - halfWidth + offSetX) / scale;
                float sampleY = (y - halfWidth + offSetY) / scale;

                float perlinValue = Mathf.Clamp(Mathf.PerlinNoise(sampleX, sampleY), 0, 1);

                if (falloffBool)
                    noiseMap[x, y] = Mathf.Clamp01(perlinValue - falloff[x, y]);
                else
                    noiseMap[x, y] = perlinValue;
            }
        }

        return noiseMap;

    }

    // Create Falloff Map
    private float[,] GenerateFalloffMap() {
        float[,] map = new float[mapSize, mapSize];

        for(int i = 0; i < map.GetLength(0); i++) {
            for (int j = 0; j < map.GetLength(1); j++) {
                float x = i / (float) mapSize * 2 - 1;
                float y = j / (float) mapSize * 2 - 1;

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

    #region Initial Height Cubes
    // Places HeightMapCubes ||| Need to change mountains.
    public void PlaceHeightMapCubes(float[,] perlin) {
        // Place Water/Mountain Cubes
        for (int x = 0; x < coordinates.GetLength(0); x++) {
            for (int y = 0; y < coordinates.GetLength(1); y++) {
                if(GetHeightLevelFromPerlin(perlin[x, y])) {
                    tiles[x, y] = PlaceCubeAtCoord(coordinates[x, y], mapHolder, 1, waterTile, 0f);
                    topTiles[x, y] = tiles[x, y];
                    tileComps[x, y] = tiles[x, y].GetComponent<Tile>();
                }
            }
        }
    }
    #endregion

    // Places tile at a coordinate... yep.
    public Transform PlaceCubeAtCoord(Coord coord, Transform parent, int heightLevel, Transform tile, float rot, bool mt = false) {
        Transform cubey = Instantiate(tile, new Vector3(coord.x, -0.5f + heightLevel, coord.y), Quaternion.Euler(new Vector3(0, rot, 0)));

        if (!mt) {
            Tile tileComp = cubey.GetComponent<Tile>();
            float i = 0f;
            while (i < rot) {
                Tile.SideType temp = tileComp.upSide;
                tileComp.upSide = tileComp.leftSide;
                tileComp.leftSide = tileComp.downSide;
                tileComp.downSide = tileComp.rightSide;
                tileComp.rightSide = temp;
                i += 90f;
            }
        }
        cubey.parent = parent;
       
        return cubey;
    }

    // Returns the chunk from the x,y value. OPTIMIZE: Do you have to loop?
    private int GetChunkIndexFromXY(int x, int y) {
        int chunkX = 0;
        bool chunkXFound = false;
        int chunkY = 0;
        bool chunkYFound = false;
        for (int index = chunkSize; index <= mapSize; index += chunkSize) {
            if(!chunkXFound && !(x < index)) {
                chunkX++;
            }
            else {
                chunkXFound = true;
            }
            if (!chunkXFound && !(y < index)) {
                chunkY++;
            }
            else {
                chunkYFound = true;
            }
            if(chunkXFound && chunkYFound) {
                break;
            }
        }

        int firstDimensionIndex = chunkY * (mapSize / chunkSize) + chunkX;

        return firstDimensionIndex;
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


    class Chunk {
        public GameObject[] gameObjects;
        public GameObject chunkObject;

        public Chunk(Transform parent) {
            chunkObject = new GameObject("Chunk");
            chunkObject.transform.parent = parent;

            gameObjects = new GameObject[] { new GameObject("PlainsCombined"),
                                             new GameObject("ForestCombined"),
                                             new GameObject("SnowForestCombined"),
                                             new GameObject("RainForestCombined"),
                                             new GameObject("TundraCombined"),
                                             new GameObject("DesertCombined"),
                                             new GameObject("SavannaCombined"),
                                             new GameObject("WaterCombined"),
                                             new GameObject("CoastCombined")};
            for(int i = 0; i < gameObjects.Length; i++) {
                gameObjects[i].transform.parent = chunkObject.transform;
                gameObjects[i].AddComponent<MeshFilter>();
                gameObjects[i].AddComponent<MeshRenderer>();
            }
        }
    }
}
