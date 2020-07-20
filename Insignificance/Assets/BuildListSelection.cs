using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildListSelection : MonoBehaviour
{
    int selectionIndex;
    List<GameObject> buildings;
    public GameObject backdrop;
    public Sprite[] buildingImages;
    public GameObject buildingPreview;
    public GameObject req1Preview;
    Image imageComponent;

    public TextMeshProUGUI description;
    String[] buildingDescriptions;

    Image req1Component;
    public Sprite[] req1Images;

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

        req1Component.sprite = req1Images[selectionIndex];
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
        imageComponent = buildingPreview.GetComponent<Image>();
        req1Component = req1Preview.GetComponent<Image>();

        buildingDescriptions = new string[]{
        "100 log\n100 Stone",
        "200 log\n200 Stone\n300 Iron"
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
