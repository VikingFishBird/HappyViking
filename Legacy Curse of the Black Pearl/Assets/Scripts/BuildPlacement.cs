using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 *          "Dwelling",
            "Estate",
            "Log Hut",
            "Pig Pen",
            "Farm",
            "Port",
            "Mine",
            "Clock Tower",
            "Hospital",
            "market",
            "Wall",
            "barracade",
            "Tower",
            "Tavern",
            "stables",
            "gate",
            "library",
            "barracks",
            "citadel",
            "the keep",
            "blacksmith"
*/
public class BuildPlacement : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] buildings;

    public BuildListSelection BuildListSelectionObj;

    KingdomManagement kingdomManager;

    bool move;

    Transform newBuilding;
    Vector3 placecoords;
    void Start()
    {
        move = false;

        kingdomManager = GameObject.FindWithTag("KingdomManager").GetComponent<KingdomManagement>();

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

                newBuilding.position = new Vector3(hitInfo.point.x, 0.01f, hitInfo.point.z);
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
        if(kingdomManager.playerKingdom.wood < kingdomManager.woodCosts[BuildListSelectionObj.selectionIndex]){
            return;
        }
        //material subtraction
        kingdomManager.playerKingdom.wood -= kingdomManager.woodCosts[BuildListSelectionObj.selectionIndex];
        
        move = true;

        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo))
        {
            newBuilding = Instantiate(buildings[BuildListSelectionObj.selectionIndex].transform, new Vector3(hitInfo.point.x, 0.01f, hitInfo.point.z), Quaternion.Euler(new Vector3(0, 0, 0)));
        }
    }
}
