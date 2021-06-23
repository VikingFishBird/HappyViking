using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

public class bannerScript : MonoBehaviour
{
    public int selectionIndex;
    List<GameObject> glows;


    public void setSelectionIndex(int index)
    {
        //if (selectionIndex != -1)
        // buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(false);
        selectionIndex = index;
        for(int i = 0; i < 12; i++)
        {
            glows[i].GetComponent<Image>().enabled = false;
        }
        glows[selectionIndex].GetComponent<Image>().enabled = true;

    }




    void Start()
    {
        glows = new List<GameObject>();
        foreach (Transform child in transform)
        {
            glows.Add(child.gameObject);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
