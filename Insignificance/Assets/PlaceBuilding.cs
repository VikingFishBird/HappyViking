using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class PlaceBuilding : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] buildings;

    public BuildListSelection BuildListSelectionObj;
    bool move;
    
    Transform newBuilding;
    Vector3 placecoords;
    void Start()
    {
        move = false;
    }

    // Update is called once per frame
    void Update()
    {
       
        if (move)
        {
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hitInfo))
            {
               
                newBuilding.position = new Vector3(hitInfo.point.x, 1, hitInfo.point.z);
            }
            if (Input.GetMouseButtonDown(1))
            {
                move = false;
            }
            if (Input.GetKeyDown(KeyCode.R))
                newBuilding.Rotate(45 * Vector3.up);
        }
    }

   
    public void SetVariableActive()
    {
        move = true;
        

        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo))
        {
            newBuilding = Instantiate(buildings[BuildListSelectionObj.selectionIndex].transform, hitInfo.point, Quaternion.Euler(new Vector3(0, 0, 0)));
        }
    }
}
