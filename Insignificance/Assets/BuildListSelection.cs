using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BuildListSelection : MonoBehaviour
{
    int selectionIndex;
    List<GameObject> buildings;
    public void setSelectionIndex(int index){
        if(selectionIndex != -1)
            buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(false);
        selectionIndex = index;
        buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(true);

    }

    void Start()
    {
        buildings = new List<GameObject>();
        foreach(Transform child in transform)
        {
            // if (child.tag == "Tag")
            buildings.Add(child.gameObject);
        }
        selectionIndex = -1;
        for(int i= 0; i<buildings.Count; i++)
        {
            print(buildings[i].name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
