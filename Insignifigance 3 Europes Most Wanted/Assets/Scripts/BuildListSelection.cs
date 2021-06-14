using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

public class BuildListSelection : MonoBehaviour
{
    public int selectionIndex;
    List<GameObject> buildings;
    public GameObject backdrop;
    public GameObject buildingPreview;

    //desc
    public TextMeshProUGUI description;
    String[] buildingDescriptions;
    

    public void setSelectionIndex(int index)
    {
        //if (selectionIndex != -1)
           // buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(false);
        selectionIndex = index;
        buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(true);

        if (!backdrop.activeSelf)
            backdrop.SetActive(true);
        //desc
        description.text = buildingDescriptions[selectionIndex];
    }


    

    void Start()
    {
        buildings = new List<GameObject>();
        foreach (Transform child in transform)
        {
            buildings.Add(child.gameObject);
        }
        //desc
        buildingDescriptions = new string[]{
            "poop\n",
            "raid\n",
            "stick\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n",
            "Desc\n"
        };
        
       

    }

    // Update is called once per frame
    void Update()
    {

    }
}

