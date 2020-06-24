using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainData
{
    public Mountain[] mountains;
    
    public MountainData() {
        // Set mountains array. Add 1 to length when you add a mountain.
        mountains = new Mountain[1];

        // Add a new Mountain
        mountains[0] = new Mountain(2, 5, 5); // Height, Length, Width
        BlockData[,,] bA = mountains[0].blockArray;
        // Add a block to the bloackArray[height, length, width]
        bA[0, 0, 0] = new BlockData(BlockType.Air, 0); // Type, rotation (degrees)
    }
}


