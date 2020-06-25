using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType { Side, Corner, Wedge, Peak, Land, Air };

public class Mountain
{
    public int height, length, width;
    public float weight;
    public BlockData[,,] blockArray;

    public Mountain(int _height, int _length, int _width, float _weight) {
        height = _height;
        length = _length;
        width = _width;
        weight = _weight;
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