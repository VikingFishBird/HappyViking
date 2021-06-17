using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsHudCheck : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject optionHud;

    void start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape) && optionHud.activeSelf)
            if (optionHud.activeSelf)
            {
                optionHud.SetActive(false);
            }
       
    }
}
