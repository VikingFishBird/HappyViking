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
    public Sprite[] buildingImages;
    public GameObject buildingPreview;
    public GameObject req1Preview;
    public GameObject req2Preview;
    Image imageComponent;
    //desc
    public TextMeshProUGUI description;
    String[] buildingDescriptions;
    //req1
    Image req1Component;
    public Sprite[] req1Images;
    //req2
    Image req2Component;
    public Sprite[] req2Images;
    //req1Desc
    public TextMeshProUGUI req1description;
    String[] req1Descriptions;
    //req2Desc
    public TextMeshProUGUI req2description;
    String[] req2Descriptions;

    public void setSelectionIndex(int index){
        if(selectionIndex != -1)
            buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(false);
        selectionIndex = index;
        buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(true);

        if (!backdrop.active)
            backdrop.SetActive(true);
        //image
        imageComponent.sprite = buildingImages[selectionIndex];
        //desc
        description.text = buildingDescriptions[selectionIndex];
        //req1
        req1Component.sprite = req1Images[selectionIndex];
        //req2
        req2Component.sprite = req2Images[selectionIndex];
        //req1Desc
        req1description.text = req1Descriptions[selectionIndex];
        //req2Desc
        req2description.text = req2Descriptions[selectionIndex];

        if (req2Component.sprite == null)
            req2Component.gameObject.SetActive(false);
        else
            req2Component.gameObject.SetActive(true);

    }


    public void resetHud()
    {
        if(selectionIndex != -1)
        {
            buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(false);
            backdrop.SetActive(false);
        }    
    }


    void Start()
    {
        buildings = new List<GameObject>();
        foreach(Transform child in transform)
        {
            buildings.Add(child.gameObject);
        }
        selectionIndex = -1;
        //image
        imageComponent = buildingPreview.GetComponent<Image>();
        //req1
        req1Component = req1Preview.GetComponent<Image>();
        //req2
        req2Component = req2Preview.GetComponent<Image>();
        //desc
        buildingDescriptions = new string[]{
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
            "Desc\n",
            "Desc\n"
        };
        req1Descriptions = new string[]{
            "Architecture 1",
            "Architecture 2",
            "Architecture 3",
            "Architecture 1",
            "Architecture 1",
            "Architecture 1",
            "Architecture 2",
            "Architecture 2",
            "Architecture 3",
            "Architecture 2",
            "Architecture 1",
            "Architecture 1",
            "Architecture 1",
            "Architecture 2",
            "Architecture 2",
            "Architecture 3",
            "Architecture 2",
            "Architecture 2",
            "Architecture 1",
            "Architecture 3",
            "Architecture 3"
        };
        req2Descriptions = new string[]{
            " ",
            " ",
            " ",
            " ",
            " ",
            " ",
            " ",
            " ",
            " ",
            " ",
            " ",
            "Agriculture 1",
            "Animal Care 1",
            "Animal Care 1",
            " ",
            " ",
            " ",
            " ",
            "Medicine",
            "Religion",
            " "
        };

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
