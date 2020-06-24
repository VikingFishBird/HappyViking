using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType { Side, Stair, Corner, Wedge, Peak, Land, Air };

public class Mountain
{
    
    public BlockData[,,] blockArray;

    public Mountain(int height, int length, int width) {
        blockArray = new BlockData[height, length, width];
    }
}

public class BlockData {
    public BlockType blockType;
    public float rotation;

    public BlockData(BlockType bT, float rot) {
        blockType = bT;
        rotation = rot;
    }
}