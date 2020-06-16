using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum SideType { Water, Land, BeachUp, BeachDown, Air, MountainUp, MountainDown, DoubleMountain, StairUp, StairDown, DNE };
    // Tile types;
    public SideType upSide;
    public SideType leftSide;
    public SideType downSide;
    public SideType rightSide;

    // How often a tile is placed.
    public float tileWeight;

    // Height of tile.
    public int tileLevel;

    public bool Mountain;
    public bool CoastOrWater;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setAllTiles() {

    }

    public bool checkCompatible(Tile other) {
        return false;
    }
}
