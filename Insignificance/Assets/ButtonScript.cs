using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    public GameObject glow;
    public float xscale;
    public float yscale;
    // Start is called before the first frame update
    void Start()
    {
        //glow.SetActive(false);
        Vector2 size = transform.GetComponent<RectTransform>().sizeDelta;
        glow.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x *xscale, size.y *yscale);

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
