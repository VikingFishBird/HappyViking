using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildConstruction : MonoBehaviour
{
    public int buildPhaseSize;
    public GameObject[] buildingPhase;

    public GameObject accessories;

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
        
        for(int i=0; i<buildPhaseSize; ++i){
            if(i==0){
                buildingPhase[i].SetActive(true);
            }
            else{
                buildingPhase[i-1].SetActive(false);
                buildingPhase[i].SetActive(true);
                if(i==buildPhaseSize-1){
                    accessories.SetActive(true);
                }
                //buildSmoke.Play();
            }
            
            yield return new WaitForSeconds(3f);
        }

    }
}
