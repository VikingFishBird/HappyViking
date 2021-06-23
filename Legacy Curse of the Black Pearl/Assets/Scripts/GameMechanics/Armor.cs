using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor
{
    public string name;
    public float bluntReduce;
    public float sharpReduce;
    public float moveSpeed;

    public Armor(string name, float bluntReduce, float sharpReduce, float moveSpeed) {
        this.name = name;
        this.bluntReduce = bluntReduce;
        this.sharpReduce = sharpReduce;
        this.moveSpeed = moveSpeed;
    }
}
