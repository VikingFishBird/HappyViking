using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menuButtonScript : MonoBehaviour
{
    public GameObject menuHud;

    public void activateMenuHud()
    {
        if (menuHud.activeSelf)
        {
            menuHud.SetActive(false);
        }
        else
            menuHud.SetActive(true);
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            if (menuHud.activeSelf)
            {
                menuHud.SetActive(false);
            }
    }
}
