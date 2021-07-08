using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessoryPlacement : MonoBehaviour
{
    public GameObject[] addOns;
    public int size;
    public float[] chance;
    

    // Start is called before the first frame update
    void Start()
    {

        float num = UnityEngine.Random.Range(0.0f,1.0f);

        for(int i=0; i<size; i++)
        {
            if(chance[i] < num)
            {
                addOns[i].SetActive(false);
            }
        }
    }

}
