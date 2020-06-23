using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClicker : MonoBehaviour
{
    public GameObject map;
    Transform[,,] heightMap;
    float currentTime = 0;
    float step = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetMouseButtonDown(0)) {
            heightMap = map.GetComponent<MapGenerator>().heightMapArrays;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f)) {
                if (hit.transform != null) {
                    hit.transform.localPosition += Vector3.up * -1;
                    int x, y;
                    map.GetComponent<MapGenerator>().GetIndexFromCoordinates(hit.transform.localPosition.x, hit.transform.localPosition.z, out x, out y);
                    print(x + " " + y);

                    //StartCoroutine(TimerRoutine());

                    for (int i = 0; i < heightMap.GetLength(0); i++) {
                        print(hit.transform.gameObject + " " + i);
                        if (x > 0 && heightMap[i, x - 1, y] != null) {
                            print(heightMap[i, x - 1, y].name + " " + heightMap[i, x - 1, y].GetComponent<Tile>().rightSide);
                            heightMap[i, x - 1, y].localPosition += Vector3.up * 2;
                        }
                        if (x < heightMap.GetLength(1) - 1 && heightMap[i, x + 1, y] != null) {
                            print(heightMap[i, x + 1, y].name + " " + heightMap[i, x + 1, y].GetComponent<Tile>().leftSide);
                            heightMap[i, x + 1, y].localPosition += Vector3.up * 3;
                        }
                        if (y > 0 && heightMap[i, x, y - 1] != null) {
                            print(heightMap[i, x, y - 1].name + " " + heightMap[i, x, y - 1].GetComponent<Tile>().downSide);
                            heightMap[i, x, y - 1].localPosition += Vector3.up * 4;
                        }
                        if (y < heightMap.GetLength(2) - 1 && heightMap[i, x, y + 1] != null) {
                            print(heightMap[i, x, y + 1].name + " " + heightMap[i, x, y + 1].GetComponent<Tile>().upSide);
                            heightMap[i, x, y + 1].localPosition += Vector3.up * 5;
                        }
                    }
                }
            }
        }*/

    }

    IEnumerator TimerRoutine() {
        for (int j = 0; j < heightMap.GetLength(2); j++) {
            for (int i = 0; i < heightMap.GetLength(1); i++) {
                if (heightMap[0, i, j]) {
                    heightMap[0, i, j].localPosition += Vector3.up * 10;
                    yield return new WaitForSeconds(step);
                    currentTime += step;
                }
                print("Hi");
            }
        }
        
        
    }
}
