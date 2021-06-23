using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ListProfile : MonoBehaviour
{
    List<GameObject> list;
    public GameObject listButton;
    // Start is called before the first frame update
    void Start()
    {
        list = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addItem()
    {
        GameObject button = Instantiate(listButton);
        button.transform.parent = transform;
        button.transform.localScale = new Vector3(1, 1, 1);
    }

    void removeItem(GameObject remove)
    {

    }
}
