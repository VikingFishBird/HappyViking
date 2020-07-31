using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class BuildButton : MonoBehaviour
{
    public Sprite buttonUnclicked;
    public Sprite buttonClicked;
    Image imageComp;
    bool clicked = true;
    public GameObject buildingPlacement;

    void Start()
    {
        imageComp = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (clicked)
        {
            if (Input.GetMouseButtonDown(1))
            {
                imageComp.sprite = buttonUnclicked;
                clicked = false;
                
            }
        }
    }

    public void ClickedButton()
    {
        imageComp.sprite = buttonClicked;
        clicked = true;
    }
}
