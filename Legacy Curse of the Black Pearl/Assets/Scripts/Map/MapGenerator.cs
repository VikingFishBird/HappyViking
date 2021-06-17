using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using BinaryHeap;
using BinarySearchTree;
using csDelaunay;

public class MapGenerator : MonoBehaviour {

    public int POLY_COUNT; //200
    public int MAP_SIZE;
    public int LLOYD_ITER;

    // Map Texture Colors
    public Material water;  // (115, 167, 178)

    public Material grass;
    public Material mud;

    public Renderer textureRender;

    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;

    private List<CellInfo> cells;

    // Generates the Entire Map
    public void GenerateMap() {
        // Voronoi nodes
        List<Vector2f> nodes = GenerateNodes();

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
        foreach (KeyValuePair<Vector2f, Site> entry in sites) {
            List<Vector2f> regionVertices = entry.Value.Region(bounds);
            List<Edge> siteEdges = entry.Value.Edges;

            cells.Add(new CellInfo(entry.Key, siteEdges));
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

    // DISPLAY METHODS FOR TESTING

    // Here is a very simple way to display the result using a simple bresenham line algorithm
    private void DisplayVoronoiDiagram() {
        Texture2D tx = new Texture2D(MAP_SIZE, MAP_SIZE);
        foreach (KeyValuePair<Vector2f, Site> kv in sites) {
            tx.SetPixel((int)kv.Key.x, (int)kv.Key.y, Color.red);
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
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;

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

class CellInfo {
    // Values
    public float treeFreq;

    // Shape
    public Vector2f siteCoord;
    public List<Edge> edgeList;
    public MeshInfo meshInfo;
    public Vector2f centroid;

    public CellInfo(Vector2f siteCoord, List<Edge> edgeList) {
        this.edgeList = edgeList;
        this.siteCoord = siteCoord;
        meshInfo = new MeshInfo();

        InitializeMeshInfo();
    }

    private void InitializeMeshInfo() {
        int vIdx = 0; // Incremental index for vertices
        Dictionary<Vector2f, int> vertexIndex = new Dictionary<Vector2f, int>();  // Dictionary to avoid repeating vertice
        vertexIndex.Add(siteCoord, vIdx); // Add site coord to dict
        meshInfo.vertices[vIdx] = new Vector3(siteCoord.x, 0, siteCoord.y); // Add site coord to vertices array;
        vIdx++;  // Increment vertex index for site coord

        // Sums to calculate centroid
        float sumX = 0;
        float sumY = 0;

        // Num Edges == Num Vert == Num Triangles / 3
        meshInfo.vertices = new Vector3[edgeList.Count];
        // If UVs: meshInfo.uvs = new Vector2[edgeList.Count];
        meshInfo.triangles = new int[edgeList.Count * 3];

        foreach (Edge edge in edgeList) {
            Vector2f left = edge.ClippedEnds[LR.LEFT];
            Vector2f right = edge.ClippedEnds[LR.RIGHT];
            
            // Add new vertices to index
            if (!vertexIndex.ContainsKey(left)) {
                vertexIndex.Add(left, vIdx);
                meshInfo.vertices[vIdx] = new Vector3(left.x, 0, left.y);
                sumX += left.x;
                sumY += left.y;
                vIdx++;
            }
            if (!vertexIndex.ContainsKey(right)) {
                vertexIndex.Add(right, vIdx);
                meshInfo.vertices[vIdx] = new Vector3(right.x, 0, right.y);
                sumX += right.x;
                sumY += right.y;
                vIdx++;
            }


            int leftIdx = vertexIndex[left];
            int rightIdx = vertexIndex[right];
            meshInfo.AddTriangle(rightIdx, 0, leftIdx); // 0 is siteCoord vertex index
        }

        centroid = new Vector2f(sumX / vertexIndex.Count, sumY / vertexIndex.Count);
    }

    public class MeshInfo {
        private static int meshCount = 0;

        public Vector3[] vertices;
        public int[] triangles;
        public GameObject gameObject;
        //public Vector2[] uvs;

        int triangleIndex;

        public MeshInfo() {
            triangleIndex = 0;
        }

        public void AddTriangle(int a, int b, int c) {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        public void CreateMesh() {
            gameObject = new GameObject("MapObject: " + meshCount);
            meshCount++;
            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            //mesh.uv = uvs;
            mesh.RecalculateNormals();

            filter.mesh = mesh;
        }
    }
}

/* OLD CODE
Pt2
/*
    // Uses Fortunes Algorithm to initialize edges
    private void VoronoiFortune() {

        beachline = new BinarySearchTree<int, BeachlineItem>();
        //edges = new LinkedList<Edge>();

        // parabola y val
        // directrix - horizontal line with coord yd
        // focus - point with coord xf, yf
        // y = 1 / (2 (yf - yd)) * (x - xf)^2 + (yf + yd) / 2

        while (events.Count() > 0) {
            Event nextEvent = events.Dequeue();

            if (nextEvent.eventType == Event.EventType.Site) {
                // Site Event
                HandleSiteEvent(nextEvent);
            }
            else {
                // Circle Event
                // Find "squeezed" arc
                
                Arc arc = null;
                HandleCircleEvent(arc);
                // Remove the squeezed cell from the beachline
            }
        }

        // Treat Unbounded edges

    }

    // Handles site event for Fortune's Algorithm
    private void HandleSiteEvent(Event nextEvent) {

        // Add the new site to the beachline
        // Split vertical arc at current x
        Arc arc = (Arc) (beachline.SearchForNearest(nextEvent.x));
        
        /// TO-DO: CREATE MinHeap Remove method
        events.Remove(arc.circleEvent.x);
        

        // Check Future Intersection Event
    }

    // Handles circle event for Fortune's Algorithm
    private void HandleCircleEvent(Arc arc) {

        /// TO-DO: CREATE BST Remove method
        //beachline.remove(arc);
        // Update breakpoints?

        /// TO-DO: CREATE MinHeap Remove method
        events.Remove(arc.circleEvent.x);
    }

    private void LloydRelaxtion(int iterations) {
        // Initialize Points using Lloyds
        
    }

    class Node {
        Vector2 coord;

        public Node(Vector2 coord) {
            this.coord = coord;
        }
    }

    // Fortune's Algorithm Event (Site or Circle)
    class Event {
        public enum EventType { Site, Circle };

        public EventType eventType;
        // Event coordinate
        public int x;

        public Event(EventType type) {
            eventType = type;
        }
    }

    // An item in the beachline binary search tree
    abstract class BeachlineItem {
        public enum ItemType { Edge, Arc };

        public ItemType itemType;
    }


    class BeachEdge : BeachlineItem {
        public Vector2 start;
        public Vector2 direction;

        public BeachEdge() {
            itemType = ItemType.Edge;
        }
    }

    class Edge {
        public Vector2 start;
        public Vector2 end;

        public Edge() {

        }
    }

    // Fortune's Algorithm arc.
    class Arc : BeachlineItem {
        public Vector2 focus;
        public Event circleEvent;

        public Arc() {
            itemType = ItemType.Arc;
        }

        // Splits arc at y coordinate
        public Arc[] Split(int y) {
            Arc[] newArcs = new Arc[2];
            return newArcs;
        }
    }

Pt1


    using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

    public const float WATER_LEVEL = 0.3f;
    public const int mapChunkSize = 241; // 241 vertices, 240x240 pixels
    public const int HEIGHT_MAP = 0;
    public const int GRASS_MAP = 1;
    public const int TREE_MAP = 2;

    // Map Texture Colors
    public Color waterColor;  // (115, 167, 178)

    public Color lightGrassColor;  // (91, 201, 120)
    public Color darkGrassColor;  // (59, 84, 66)

    public Color lightMudColor;
    public Color darkMudColor;

    public Renderer textureRender;

    public int mapSize;  // Amount of chunks per row/col
    public bool falloff;
    public bool randomSeed;

    public NoiseMapInfo[] mapInfo;
    TerrainInfo[,] terrainInfo;

    public void GenerateMap() {
        int fullMapSize = mapSize * (mapChunkSize - 1);

        // Create Mesh, Create Texture
        // 1) Get "height map"
        float[,] heightNoiseMap = Noise.GenerateNoiseMap(randomSeed, fullMapSize, fullMapSize,
            mapInfo[HEIGHT_MAP].seed, mapInfo[HEIGHT_MAP].noiseScale, mapInfo[HEIGHT_MAP].octaves, 
            mapInfo[HEIGHT_MAP].persistance, mapInfo[HEIGHT_MAP].lacunarity, mapInfo[HEIGHT_MAP].offset);
        float[,] grassNoiseMap = Noise.GenerateNoiseMap(randomSeed, fullMapSize, fullMapSize,
            mapInfo[GRASS_MAP].seed, mapInfo[GRASS_MAP].noiseScale, mapInfo[GRASS_MAP].octaves,
            mapInfo[GRASS_MAP].persistance, mapInfo[GRASS_MAP].lacunarity, mapInfo[GRASS_MAP].offset);
        float[,] treeNoiseMap = Noise.GenerateNoiseMap(randomSeed, fullMapSize, fullMapSize,
            mapInfo[TREE_MAP].seed, mapInfo[TREE_MAP].noiseScale, mapInfo[TREE_MAP].octaves,
            mapInfo[TREE_MAP].persistance, mapInfo[TREE_MAP].lacunarity, mapInfo[TREE_MAP].offset);


        float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(fullMapSize);

        Color[] colorMap = new Color[fullMapSize * fullMapSize];
        terrainInfo = new TerrainInfo[fullMapSize, fullMapSize];

        Texture2D texture = new Texture2D(fullMapSize, fullMapSize);

        for (int y = 0; y < fullMapSize; y++) {
            for (int x = 0; x < fullMapSize; x++) {
                if (falloff) {
                    heightNoiseMap[x, y] = heightNoiseMap[x, y] - falloffMap[x, y];
                }

                if (heightNoiseMap[x, y] < WATER_LEVEL) {
                    // OCEAN
                    colorMap[y * fullMapSize + x] = waterColor;

                    terrainInfo[x, y].biome = TerrainInfo.Biome.Water;
                    terrainInfo[x, y].placedObject = TerrainInfo.PlacedObject.Empty;
                    terrainInfo[x, y].treeFrequency = 0.0f;
                }
                else {
                    // GRASS
                    terrainInfo[x, y].treeFrequency = treeNoiseMap[x, y];

                    if (treeNoiseMap[x, y] > 0.7f) {
                        terrainInfo[x, y].biome = TerrainInfo.Biome.Forest;
                        colorMap[y * fullMapSize + x] = Color.Lerp(darkMudColor, lightMudColor, grassNoiseMap[x, y]);
                    }
                    else {
                        terrainInfo[x, y].biome = TerrainInfo.Biome.Plains;
                        colorMap[y * fullMapSize + x] = Color.Lerp(darkGrassColor, lightGrassColor, grassNoiseMap[x, y]);
                    }

                    if (terrainInfo[x, y].placedObject != TerrainInfo.PlacedObject.Empty) {

                    } else {
                        terrainInfo[x, y].placedObject = TerrainInfo.PlacedObject.Empty;
                    }
                }
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(fullMapSize, 1, fullMapSize);

        // 2) Set ocean, coasts, land
        // 3) Add Rivers using [banished method](https://www.reddit.com/r/proceduralgeneration/comments/fnglab/how_to_procedurally_generate_rivers_and_small/)
        // 4) Set plains and forest biomes
        // 5) Generate trees/ rocks / etc
    }
}

[System.Serializable]
public struct NoiseMapInfo {
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;
}

public struct TerrainInfo {
    public enum Biome { Water, Plains, Forest};
    public enum PlacedObject { Empty, Tree, Rock, Log };  // Buildings, Rocks, Trees

    public Biome biome;
    public PlacedObject placedObject;
    public float treeFrequency;

}

    */

