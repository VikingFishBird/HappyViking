using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaternLighting : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject lantern;

    static LightCycle sun;
    
    bool startedNightCycle = false;
    bool startedDayCycle = false;

    void Start()
    {
        sun = GameObject.FindWithTag("SunLight").GetComponent<LightCycle>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!startedNightCycle && sun.sunSet){
            StartCoroutine(LightLantern());
            startedNightCycle=true;
            startedDayCycle=false;
        }
        else if(!startedDayCycle && sun.sunRise){
            StartCoroutine(Extinguish());
            startedDayCycle=true;
            startedNightCycle=false;
        }
    }

    IEnumerator LightLantern(){
        float timeElapsed=0;
        
        while(timeElapsed < sun.dayLength*0.08f && lantern.transform.GetChild(1).gameObject.activeSelf == false){

            float num = UnityEngine.Random.Range(0.0f,1.0f);

            if(timeElapsed/(sun.dayLength*0.08f) >= num){
                lantern.transform.GetChild(1).gameObject.SetActive(true);
            }
            timeElapsed +=Time.deltaTime;
            
            yield return null;
        }
    }

    IEnumerator Extinguish(){
        float timeElapsed=0;

        while(timeElapsed < sun.dayLength*0.08f && lantern.transform.GetChild(1).gameObject.activeSelf == true){
            float num = UnityEngine.Random.Range(0.0f,1.0f);

            if(timeElapsed/(sun.dayLength*0.08f) >= num){
                lantern.transform.GetChild(1).gameObject.SetActive(false);
            }
            timeElapsed +=Time.deltaTime;
            
            yield return null;
        }

    }
}
