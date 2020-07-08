using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum SideType { Water, Land, BeachUp, BeachDown, DNE };
    // Tile types;
    public SideType upSide;
    public SideType leftSide;
    public SideType downSide;
    public SideType rightSide;

    // How often a tile is placed.
    public float tileWeight;

    public bool Mountain;
    public bool CoastOrWater;

    public int grass;
    public int stone;
    public int coast;
    public int water;

    public MapGenerator.Biome biome;

    public Vector2[] potentialTreeLocations;

}
