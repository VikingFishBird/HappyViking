using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    bool flipped;
    Camera cameraToLookAt;
    float cameraAngle;
    void Start(){
        flipped = false;
        cameraToLookAt = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        cameraAngle = GameObject.FindWithTag("CameraRotator").transform.rotation.y;
    }
    void Update()
    {
        cameraAngle = GameObject.FindWithTag("CameraRotator").transform.rotation.y;
        transform.LookAt(cameraToLookAt.transform);
        transform.Rotate(-30,0,0);
        if(cameraAngle>0 && !flipped){
            
            GetComponent<SpriteRenderer>().flipX=true;
            flipped=true;
        }else if(cameraAngle<=0 && flipped){
            GetComponent<SpriteRenderer>().flipX=false;
            flipped=false;
        }
    }

}
