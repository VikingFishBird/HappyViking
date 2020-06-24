using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainData
{
    public Mountain[] mountains;
    
    public MountainData() {
        // Set mountains array. Add 1 to length when you add a mountain.
        mountains = new Mountain[1];
        #region Mountain1
        // Add a new Mountain
        mountains[0] = new Mountain(2, 4, 4); // Height, Length, Width
        BlockData[,,] bA = mountains[0].blockArray;
        // Add a block to the bloackArray[height, length, width]
        bA[0, 0, 0] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[0, 0, 1] = new BlockData(BlockType.Corner, 180); // Type, rotation (degrees)
        bA[0, 0, 2] = new BlockData(BlockType.Side, 180); // Type, rotation (degrees)
        bA[0, 0, 3] = new BlockData(BlockType.Corner, 270); // Type, rotation (degrees)
        bA[0, 1, 0] = new BlockData(BlockType.Corner, 180); // Type, rotation (degrees)
        bA[0, 1, 1] = new BlockData(BlockType.Wedge, 180); // Type, rotation (degrees)
        bA[0, 1, 2] = new BlockData(BlockType.Land, 0); // Type, rotation (degrees)
        bA[0, 1, 3] = new BlockData(BlockType.Side, 270); // Type, rotation (degrees)
        bA[0, 2, 0] = new BlockData(BlockType.Side, 90); // Type, rotation (degrees)
        bA[0, 2, 1] = new BlockData(BlockType.Wedge, 0); // Type, rotation (degrees)
        bA[0, 2, 2] = new BlockData(BlockType.Side, 0); // Type, rotation (degrees)
        bA[0, 2, 3] = new BlockData(BlockType.Corner, 0); // Type, rotation (degrees)
        bA[0, 3, 0] = new BlockData(BlockType.Corner, 90); // Type, rotation (degrees)
        bA[0, 3, 1] = new BlockData(BlockType.Corner, 0); // Type, rotation (degrees)
        bA[0, 3, 2] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[0, 3, 3] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 0, 0] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 0, 1] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 0, 2] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 0, 3] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 1, 0] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 1, 1] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 1, 2] = new BlockData(BlockType.Peak, 0); // Type, rotation (degrees)
        bA[1, 1, 3] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 2, 0] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 2, 1] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 2, 2] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 2, 3] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 3, 0] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 3, 1] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 3, 2] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
        bA[1, 3, 3] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)

        #endregion

    }
}


