using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public bannerScript bannerPicker;
    public Material playerBannerColor;
    // Start is called before the first frame update
    
    void Start()
    {
        playerBannerColor = bannerPicker.bannerMaterials[bannerPicker.selectionIndex];
        DontDestroyOnLoad(this.gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        playerBannerColor = bannerPicker.bannerMaterials[bannerPicker.selectionIndex];
    }
}
