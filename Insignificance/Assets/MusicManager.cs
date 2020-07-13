using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] clips;
    public float[] clipVolume;
    private AudioSource audioSource;

    //public Slider music;

    // Start is called before the first frame update
    void Start() {
        audioSource = this.gameObject.GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.volume = 0.25f;//music.value;
        audioSource.Play();
    }

    public void setMusicVol() {
        audioSource.volume = 0.25f;//music.value;
    }

    // Update is called once per frame
    void Update() {
        if (!audioSource.isPlaying) {
            int rand = Random.Range(0, clips.Length - 1);
            audioSource.clip = clips[rand];
            audioSource.volume = clipVolume[rand];
            audioSource.Play();
        }
    }
}
