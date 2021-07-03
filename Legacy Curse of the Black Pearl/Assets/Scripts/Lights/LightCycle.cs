using UnityEngine;
using System.Collections;

public class LightCycle : MonoBehaviour
{
    
    public float dayLength;

    public Color dayColor;
    public Color sunsetColor;
    public Color nightColor;
    public Color sunriseColor;

    public float dayIntensity;
    public float nightIntensity;

    public bool sunSet=false;

    public bool sunRise=false;

    //nature noises, non-area related
    public AudioSource wind;
    public AudioSource morningNoise;
    public AudioSource nightNoise;

    public float windVol;
    public float morningVol;
    public float nightVol;

    Light lt;

    void Start()
    {
        lt = GetComponent<Light>();
        

        StartCoroutine(dayCycle());
        

    }

    void Update()
    {
        
        
    }

    IEnumerator dayCycle() {
        while(true){
            float timeElapsed = 0;
            yield return new WaitForSeconds(dayLength*.52f);

            //StartCoroutine(FadeIn(wind, 5f, windVol));

            //yield return new WaitForSeconds(dayLength*.26f);

            //StartCoroutine(FadeOut(wind, 5f));

            sunSet=true;
            sunRise=false;
            
            //day to sunset color
            while(timeElapsed < dayLength*.04f){
                lt.color=Color.Lerp(dayColor,sunsetColor, timeElapsed/(dayLength*.04f));
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            //sunset to night
            timeElapsed=0;
            while(timeElapsed < dayLength*.04f){
                lt.color=Color.Lerp(sunsetColor,nightColor, timeElapsed/(dayLength*.04f));
                lt.intensity = Mathf.Lerp(dayIntensity,nightIntensity, timeElapsed/(dayLength*.04f));
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            //music fade in
            StartCoroutine(FadeIn(nightNoise, 5f, nightVol));

            yield return new WaitForSeconds(dayLength*.32f);

            StartCoroutine(FadeOut(nightNoise, 5f));

            sunRise=true;
            sunSet=false;
            //night to sunrise
            timeElapsed=0;
            while(timeElapsed < dayLength*.04f){
                lt.color=Color.Lerp(nightColor,sunriseColor, timeElapsed/(dayLength*.04f));
                lt.intensity = Mathf.Lerp(nightIntensity,dayIntensity, timeElapsed/(dayLength*.04f));
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            StartCoroutine(FadeIn(morningNoise, 5f, morningVol));

            //sunrise to day
            timeElapsed=0;
            while(timeElapsed < dayLength*.04f){
                lt.color=Color.Lerp(sunriseColor,dayColor, timeElapsed/(dayLength*.04f));
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            
            StartCoroutine(FadeOut(morningNoise, 5f));
        }
    }

    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;
 
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
 
            yield return null;
        }
 
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
 
    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime, float vol)
    {
        float startVolume = 0.2f;
 
        audioSource.volume = 0;
        audioSource.Play();
 
        while (audioSource.volume < vol)
        {
            audioSource.volume += startVolume * Time.deltaTime / FadeTime;
 
            yield return null;
        }
 
        audioSource.volume = vol;
    }
    
}
