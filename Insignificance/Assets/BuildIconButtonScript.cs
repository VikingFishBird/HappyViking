using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildIconButtonScript : MonoBehaviour
{
    public GameObject buildhud;
    public GameObject buildListSelection;
    BuildListSelection buildListSelectionScript;

    public void activateBuildHud(){
        if (buildhud.active)
        {
            buildhud.SetActive(false);
            buildListSelectionScript.resetHud();
        }
        else
            buildhud.SetActive(true);
    }
    void Start()
    {
        buildListSelectionScript = buildListSelection.GetComponent<BuildListSelection>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            if (buildhud.active)
            {
                buildhud.SetActive(false);
                buildListSelectionScript.resetHud();
            }
    }
}
