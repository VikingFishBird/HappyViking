using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

public class ArtilleryListSelection : MonoBehaviour
{
    public int selectionIndex;
    List<GameObject> artillery;
    public GameObject backdrop;
    public GameObject artilleryPreview;

    //desc
    public TextMeshProUGUI description;
    String[] artilleryDescriptions;
    //names
    public TextMeshProUGUI nameText;
    String[] artilleryNames;


    public void setSelectionIndex(int index)
    {
        //if (selectionIndex != -1)
        // buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(false);
        selectionIndex = index;
        artillery[selectionIndex].transform.GetChild(0).gameObject.SetActive(true);

        if (!backdrop.activeSelf)
            backdrop.SetActive(true);
        //desc
        description.text = artilleryDescriptions[selectionIndex];

        nameText.text = artilleryNames[selectionIndex];
    }




    void Start()
    {
        artillery = new List<GameObject>();
        foreach (Transform child in transform)
        {
            artillery.Add(child.gameObject);
        }
        //desc
        artilleryDescriptions = new string[]{
            "Catapults are great for taking out your enemies buildings. \n" +
            "reload time 10 minutes \n" +
            "firing distance 100 meters \n" +
            "damage radius 3 meters",
            "desc 2\n",
            "stick\n",
            "Desc\n"
        };

        artilleryNames = new string[]{
            "Battering Ram",
            "Catapult",
            "Trebuchet",
            "Ballista"
        };

    }

    // Update is called once per frame
    void Update()
    {

    }
}