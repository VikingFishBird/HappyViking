using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    public GameObject glow;

    // Start is called before the first frame update
    void Start()
    {
        //glow.SetActive(false);
        Vector2 size = transform.GetComponent<RectTransform>().sizeDelta;
        glow.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x * 1.6f, size.y * 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
