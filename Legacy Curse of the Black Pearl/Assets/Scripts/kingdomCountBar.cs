using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class kingdomCountBar : MonoBehaviour
{
    //text
    public TextMeshProUGUI count;

    public Slider kingdomCount;

    // Start is called before the first frame update
    void Start()
    {
        count.text = kingdomCount.value.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void kingdomAmount()
    {
        count.text = kingdomCount.value.ToString();
    }
}
