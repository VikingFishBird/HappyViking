using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform followTransform;

    private Camera cam;

    public float fastSpeed;
    public float normalSpeed;
    public float movementSpeed;
    public float dragSpeed;
    public float movementTime;
    public float rotationAmount;

    [Space]
    public float maxZ, minZ, maxY, minY;

    public Vector3 zoomAmount;

    Vector3 newPosition;
    Vector3 newZoom;
    Quaternion newRotation;

    public Vector3 dragStartPosition;
    public Vector3 dragCurrentPosition;
    public Vector3 rotateStartPosition;
    public Vector3 rotateCurrentPosition;

    // Start is called before the first frame update
    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
        cameraTransform.LookAt(transform);
    }

    // Update is called once per frame
    void Update()
    {
        if(followTransform != null) {
            transform.position = followTransform.position;
        }
        else {
            HandleMouseInput();
            HandleMovementInput();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            followTransform = null;
        }
    }

    void HandleMouseInput() {
        if(Input.mouseScrollDelta.y != 0) {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
        }

        //Drag Mechanics
        if (Input.GetMouseButtonDown(0)) {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if(plane.Raycast(ray, out entry)) {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(0)) {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry)) {
                dragCurrentPosition = ray.GetPoint(entry);

                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }
        /*
        if (Input.GetMouseButton(2)) {
            rotateStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2)) {
            rotateCurrentPosition = Input.mousePosition;

            Vector3 difference = rotateStartPosition - rotateCurrentPosition;
            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }*/
    }

    void HandleMovementInput() {
        if (Input.GetKey(KeyCode.LeftShift)) {
            movementSpeed = fastSpeed;
        }
        else {
            movementSpeed = normalSpeed;
        }

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            newPosition += (transform.forward * movementSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            newPosition += (transform.forward * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            newPosition += (transform.right * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            newPosition += (transform.right * movementSpeed);
        }

        if (Input.GetKey(KeyCode.Q)) {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }
        if (Input.GetKey(KeyCode.E)) {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }

        newZoom.y = Mathf.Clamp(newZoom.y, minY, maxY);
        newZoom.z = Mathf.Clamp(newZoom.z, minZ, maxZ);
    
        /*
        // Find point where camera is looking
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float entry;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 point = new Vector3(0,0,0);
        if(plane.Raycast(ray, out entry)) {
            point = ray.GetPoint(entry);
            transform.position = point;
            //transform.RotateAround(point, Vector3.up, rotationAmount * Time.captureDeltaTime);
        }*/
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, 20*Time.deltaTime * movementTime);
    }
}
