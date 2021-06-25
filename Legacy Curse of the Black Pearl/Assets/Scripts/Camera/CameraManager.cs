using UnityEngine;

public class CameraManager : MonoBehaviour
{
    int MIN_ZOOM;
    int MAX_ZOOM;

    int MIN_POS;
    int MAX_POS;

    public Transform followTransform;
    public Transform cameraTransform;
    public Camera mainCamera;

    public float normalSpeed;
    public float fastSpeed;
    public float movementTime;
    public float rotationAmount;
    public Vector3 zoomAmount;

    public Vector3 newPosition;
    public Quaternion newRotation;
    public Vector3 newZoom;
    float movementSpeed;

    Vector3 dragStartPosition;
    Vector3 dragCurrentPosition;
    Vector3 rotateStartPosition;
    Vector3 rotateCurrentPosition;

    // Start is called before the first frame update
    void Start()
    {
        SetBounds(6012);
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (followTransform != null) {
            transform.position = followTransform.position;
        } else {
            HandleMouseInput();
            HandleMovementInput();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            followTransform = null;
        }
    }

    void SetBounds(int mapSize) {
        MIN_ZOOM = 8;
        MAX_ZOOM = ((mapSize / 10) / 4);

        MIN_POS = 50;
        MAX_POS = (mapSize / 10) - 50;


    }

    void HandleMouseInput() {
        if (Input.mouseScrollDelta.y != 0) {
            if (Input.mouseScrollDelta.y > 0 && newZoom.y > MIN_ZOOM) {
                newZoom += Input.mouseScrollDelta.y * zoomAmount;
            }
            if (Input.mouseScrollDelta.y < 0 && newZoom.y < MAX_ZOOM) {
                newZoom += Input.mouseScrollDelta.y * zoomAmount;
            }
        }

        if (Input.GetMouseButtonDown(0)) {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
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

        
        if (Input.GetMouseButtonDown(2)) {
            rotateStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2)) {
            rotateCurrentPosition = Input.mousePosition;

            Vector3 difference = rotateStartPosition - rotateCurrentPosition;
            rotateStartPosition = rotateCurrentPosition;
            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 10f));
        }
    }

    void HandleMovementInput() {
        // Determine if fast speed is used
        if (Input.GetKey(KeyCode.LeftShift)) {
            movementSpeed = fastSpeed;
        } else {
            movementSpeed = normalSpeed;
        }

        // Move camera based on WASD / arrow keys
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            if (newPosition.z < MAX_POS)
                newPosition += (transform.forward * movementSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            if (newPosition.z > MIN_POS)
                newPosition += (transform.forward * - movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            if (newPosition.x > MIN_POS)
                newPosition += (transform.right * - movementSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            if (newPosition.x < MAX_POS)
                newPosition += (transform.right * movementSpeed);
        }

        if (Input.GetKey(KeyCode.Q)) {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }
        if (Input.GetKey(KeyCode.E)) {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }

        /*if (Input.GetKey(KeyCode.R)) {
            if (newZoom.y > MIN_ZOOM)
                newZoom += zoomAmount / 4;
        }
        if (Input.GetKey(KeyCode.F)) {
            if (newZoom.y < MAX_ZOOM)
                newZoom -= zoomAmount / 4;
        }
        */
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);

    }
}
