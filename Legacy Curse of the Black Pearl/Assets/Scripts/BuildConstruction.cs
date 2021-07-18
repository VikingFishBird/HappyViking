using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildConstruction : MonoBehaviour
{
    public int buildPhaseSize;
    public GameObject[] buildingPhase;

    public GameObject accessories;

    public int index;

    KingdomManagement kingdomManager;

    

   // public GameObject[] bannerColorSlot;
   // public int[] bannerMatIndex;

   // Material bannerColor;

   // public GameObject buildSmokeObject;

   // ParticleSystem buildSmoke;

    bool start;
    // Start is called before the first frame update
    void Start()
    {
        
        //buildSmoke=buildSmokeObject.transform.GetComponent<ParticleSystem>();
        accessories.SetActive(false);

        for(int i=0; i<buildPhaseSize; ++i){
            buildingPhase[i].SetActive(false);
        }
        //for before building placed
        buildingPhase[0].SetActive(true);
        start=true;

        kingdomManager = GameObject.FindWithTag("KingdomManager").GetComponent<KingdomManagement>();

        //bannerColor = GameObject.FindWithTag("BannerColor").GetComponent<GameSettings>().playerBannerColor;

       // for(int i=0; i<buildPhaseSize; ++i){
        //    bannerColorSlot[i].GetComponent<Renderer>().materials[bannerMatIndex[i]]=bannerColor;
       // }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1) && start){
            StartCoroutine(NextPhase());
            start=false;
        }
    }

    IEnumerator NextPhase(){
        kingdomManager.playerQueueAddBuild(index);

        for(int i=0; i<buildPhaseSize; ++i){
            if(i==0){
                buildingPhase[i].SetActive(true);
            }
            else{
                buildingPhase[i-1].SetActive(false);
                buildingPhase[i].SetActive(true);
                if(i==buildPhaseSize-1){
                    accessories.SetActive(true);

                    kingdomManager.ChangePlayerBuildCount(index);
                    //adding 4 population if dwelling built
                    if(index==0){
                        kingdomManager.playerKingdom.PopulationUpdate(kingdomManager.dwellingPopulation);
                    }
                    if(index==1){
                        kingdomManager.playerKingdom.PopulationUpdate(kingdomManager.estatePopulation);
                    }
                    kingdomManager.playerKingdom.ConsumtionUpdate(kingdomManager.foodCosts[index]);
                }
                //buildSmoke.Play();
            }
            
            yield return new WaitForSeconds(kingdomManager.buildTimes[index]/(buildPhaseSize*1.0f));
        }

    }
}
