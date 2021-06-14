using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicFade : MonoBehaviour
{
    private static MusicFade instance;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
