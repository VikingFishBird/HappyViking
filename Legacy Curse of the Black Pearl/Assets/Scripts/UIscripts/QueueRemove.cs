using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QueueRemove : MonoBehaviour
{

    public GameObject removeButton;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ShowRemove(){
        if(removeButton.activeSelf){
            removeButton.SetActive(false);
        }else{
            removeButton.SetActive(true);
        }

        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
