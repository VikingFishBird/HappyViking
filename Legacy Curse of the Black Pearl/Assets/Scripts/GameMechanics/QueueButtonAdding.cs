using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QueueButtonAdding : MonoBehaviour
{
    public GameObject buildSlot;
    public GameObject queueParent;
    //public GameObject researchSlot;
    public LinkedList<GameObject> listQueue;
    // Start is called before the first frame update
    void Start()
    {
        listQueue = new LinkedList<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddBuildSlot(Task newTask)
    {
        GameObject button = Instantiate(buildSlot);
        button.transform.SetParent(queueParent.transform);
        button.transform.localScale = new Vector3(.25f, .25f, .25f);
        button.transform.GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text=newTask.taskName;
        button.transform.GetChild(2).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text=CalculateStringTime(newTask);
        listQueue.AddLast(button);

    }

    public string CalculateStringTime(Task taskTime){
        float time= taskTime.time;
        if(time<60){
            return ((int)time).ToString() + " minutes remaining";
        }
        if(time<600){
            return ((int)(time/60)).ToString() + " hours remaining";
        }
        return ((int)(time/600)).ToString() + " days remaining";
    }
    //void removeCompleted()
    //{
    //    foreach (GameObject slot in listQueue)
     //   {
    //        if(slot.time)
    //    }
   // }
}
