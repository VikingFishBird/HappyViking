using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using csDelaunay;

public class MapGenerator : MonoBehaviour {

    // Map Creation Variables
    public int POLY_COUNT; //200
    public int MAP_SIZE;
    public int LLOYD_ITER;

    // Noise Map Values
    public NoiseMapInfo[] noiseMapInfo;
    const int HEIGHT_MAP = 0;
    const int TREE_MAP = 1;
    const bool RANDOM_SEED = true;
    const int RELATIVE_NOISE_SIZE = 4;

    const float WATER_LEVEL = 0.3f;
    const float FOREST_LEVEL = 0.7f;

    // Map Texture Colors
    public Material water;  // (115, 167, 178)
    public Material grass;
    public Material mud;

    // Texture Render for Plane
    public Renderer textureRender;

    // CellInfo
    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;

    private List<CellInfo> cells;

    // Generates the Entire Map
    public void GenerateMap() {
        // Destroy any children and set MapGen 
        // object as parent for meshes.
        ClearMap();
        CellInfo.parent = transform;

        // Voronoi nodes
        List<Vector2f> nodes = GenerateNodes();
        cells = new List<CellInfo>();

        // Places bounds for voronoi map
        // Bounds are equal to the map borders
        Rectf bounds = new Rectf(0, 0, MAP_SIZE, MAP_SIZE);

        // Initialize Voronoi Diagram
        Voronoi voronoi = new Voronoi(nodes, bounds, LLOYD_ITER);

        // Retrieve voronoi info
        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;

        DisplayVoronoiDiagram();

        // Create Meshes
        SetCellList(bounds);

        SetTileBiomes();
    }

    // View Tiles disappear
    public IEnumerator SetInactive() {
        yield return new WaitForSeconds(3.0f);
        for (int i = 0; i < cells.Count; i++) {
            cells[i].gameObject.SetActive(false);
            print(cells[i].siteCoord);
            yield return new WaitForSeconds(0.7f);
        }
    }

    // Creates Meshes and sets the CellInfo list.
    private void SetCellList(Rectf bounds) {
        // Create Meshes
        foreach (KeyValuePair<Vector2f, Site> entry in sites) {
            List<Vector2f> regionVertices = entry.Value.Region(bounds);
            List<Edge> siteEdges = entry.Value.Edges;

            // Convert from Vector2f List to Vector2[]/Vector3[] arrays
            Vector2[] vertices2d = new Vector2[regionVertices.Count];
            Vector3[] vertices3d = new Vector3[regionVertices.Count];
            for (int i = 0; i < regionVertices.Count; i++) {
                vertices2d[i] = new Vector2(regionVertices[i].x, regionVertices[i].y);
                vertices3d[i] = new Vector3(regionVertices[i].x, 0, regionVertices[i].y);
            }

            // Triangulate polygon
            Triangulator triangulator = new Triangulator(vertices2d);
            int[] triangles = triangulator.Triangulate();

            // Add complete cell with mesh
            cells.Add(new CellInfo(entry.Key, siteEdges, vertices3d, triangles));
            cells[cells.Count - 1].SetMaterial(grass);

        }
    }

    // Sets each tile's biome information
    private void SetTileBiomes() {
        int noiseMapSize = MAP_SIZE / RELATIVE_NOISE_SIZE;

        float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(noiseMapSize);

        float[,] heightMap = Noise.GenerateNoiseMap(RANDOM_SEED, noiseMapSize, noiseMapSize, 0,
            noiseMapInfo[HEIGHT_MAP].scale, noiseMapInfo[HEIGHT_MAP].octaves,
            noiseMapInfo[HEIGHT_MAP].persistance, noiseMapInfo[HEIGHT_MAP].lacunarity,
            noiseMapInfo[HEIGHT_MAP].offset);
        float[,] treeMap = Noise.GenerateNoiseMap(RANDOM_SEED, noiseMapSize, noiseMapSize, 0,
            noiseMapInfo[TREE_MAP].scale, noiseMapInfo[TREE_MAP].octaves,
            noiseMapInfo[TREE_MAP].persistance, noiseMapInfo[TREE_MAP].lacunarity,
            noiseMapInfo[TREE_MAP].offset);

        for (int i = 0; i < cells.Count; i++) {
            int noiseX = (int)((cells[i].siteCoord.x / MAP_SIZE) * noiseMapSize);
            int noiseY = (int)((cells[i].siteCoord.y / MAP_SIZE) * noiseMapSize);

            if (WATER_LEVEL > heightMap[noiseX, noiseY] - falloffMap[noiseX, noiseY]) {
                cells[i].biome = CellInfo.Biome.Water;
                cells[i].treeFreq = 0.0f;
                cells[i].SetMaterial(water);
            } else {
                cells[i].treeFreq = treeMap[noiseX, noiseY];
                if (cells[i].treeFreq > FOREST_LEVEL) {
                    cells[i].biome = CellInfo.Biome.Forest;
                    cells[i].SetMaterial(mud);
                } else {
                    cells[i].biome = CellInfo.Biome.Plains;
                    cells[i].SetMaterial(grass);
                }
            }
        }
    }

    // Initializes Nodes for Voronoi diagram
    private List<Vector2f> GenerateNodes() {

        List<Vector2f> newSites = new List<Vector2f>();

        // Generate initial voronoi nodes
        for (int i = 0; i < POLY_COUNT; i++) {
            int x = Random.Range(0, MAP_SIZE);
            int y = Random.Range(0, MAP_SIZE);
            newSites.Add(new Vector2f(x, y));
        }

        return newSites;
    }

    private void ClearMap() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
    // DISPLAY METHODS FOR TESTING
    // Here is a very simple way to display the result using a simple bresenham line algorithm
    private void DisplayVoronoiDiagram() {
        Texture2D tx = new Texture2D(MAP_SIZE, MAP_SIZE);
        foreach (KeyValuePair<Vector2f, Site> kv in sites) {
            tx.SetPixel(MAP_SIZE - (int)kv.Key.x, MAP_SIZE - (int) (kv.Key.y), Color.red);
        }
        foreach (Edge edge in edges) {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;

            DrawLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], tx, Color.black);
        }
        tx.Apply();

        textureRender.material.mainTexture = tx;
    }

    // Bresenham line algorithm
    private void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, int offset = 0) {
        int x0 = MAP_SIZE - (int)p0.x;
        int y0 = MAP_SIZE - (int)p0.y;
        int x1 = MAP_SIZE - (int)p1.x;
        int y1 = MAP_SIZE - (int)p1.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true) {
            tx.SetPixel(x0 + offset, y0 + offset, c);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }
    }

}

public class CellInfo {

    public static Transform parent;
    private static int cellCount = 0;

    public enum Biome { Water, Plains, Forest }

    // Values
    public float treeFreq;
    public Biome biome;

    // Shape
    public Vector2f siteCoord;
    public List<Edge> edgeList;
    public Vector2f centroid;

    public GameObject gameObject;
    public MeshRenderer renderer;
    public Mesh mesh;

    public CellInfo(Vector2f siteCoord, List<Edge> edgeList, Vector3[] vert, int[] tri) {
        this.edgeList = edgeList;
        this.siteCoord = siteCoord;

        // Calculate Centroid
        float xSum = 0, ySum = 0;
        for(int i = 0; i < vert.Length; i++) {
            xSum += vert[i].x;
            ySum += vert[i].z;
        }
        centroid = new Vector2f(xSum / vert.Length, ySum / vert.Length);

        // Create Mesh
        InitializeMeshInfo(vert, tri);
    }

    public void SetMaterial(Material mat) {
        renderer.material = mat;
    }

    private void InitializeMeshInfo(Vector3[] vert, int[] tri) {
        // Set mesh values
        mesh = new Mesh();
        mesh.vertices = vert;
        mesh.triangles = tri;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Create gameObject and add components
        gameObject = new GameObject("MapObject: " + cellCount);
        gameObject.transform.parent = CellInfo.parent;
        cellCount++;
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        renderer = gameObject.AddComponent<MeshRenderer>();
        
        filter.mesh = mesh;
    }
}

[System.Serializable]
public struct NoiseMapInfo {
    public string name;  // Map Name

    public float scale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public Vector2 offset;
}