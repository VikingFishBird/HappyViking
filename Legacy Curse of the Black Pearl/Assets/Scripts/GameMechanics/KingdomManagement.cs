using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KingdomManagement : MonoBehaviour
{
    public Kingdom playerKingdom;
    public float[] buildTimes;

    public int[] woodCosts;
    public int[] stoneCosts;
    public int[] ironCosts;

    public int[] foodCosts;
    

    public TextMeshProUGUI logCount;
    public TextMeshProUGUI stoneCount;
    public TextMeshProUGUI ironCount;
    public TextMeshProUGUI populationCount;

    //Food UI
    public TextMeshProUGUI foodTotal;
    public TextMeshProUGUI foodRatio;
    public TextMeshProUGUI foodDesc;
    public GameObject minusSign;

    public string[] buildingNames;

    //case tweaks
    public int dwellingPopulation;
    public int estatePopulation;
    
    public int logHutProduction;
    public int stoneProduction;
    public int ironProduction;
    public int farmProduction;
    public int pigPenProduction;

    //List<Kingdom> AIKingdoms;

    //player queue
    public QueueButtonAdding playerQueue;

    public int x=0;
    
    // Start is called before the first frame update
    void Start()
    {
        

        playerKingdom = new Kingdom();
        //materials
        logCount.text=playerKingdom.wood.ToString();
        stoneCount.text=playerKingdom.stone.ToString();
        ironCount.text=playerKingdom.iron.ToString();
        //population
        populationCount.text=playerKingdom.population.ToString();
        //food
        foodTotal.text=playerKingdom.food.ToString();
        if(playerKingdom.foodProductionRate-playerKingdom.foodConsumptionRate>=0){
                    foodRatio.text=Mathf.Abs(playerKingdom.foodProductionRate-playerKingdom.foodConsumptionRate).ToString();
                    minusSign.SetActive(false);

        }else{
            foodRatio.text=Mathf.Abs(playerKingdom.foodProductionRate-playerKingdom.foodConsumptionRate).ToString();
            minusSign.SetActive(true);
        }
        foodDesc.text="producing " + playerKingdom.foodProductionRate.ToString() + "\n using " + playerKingdom.foodConsumptionRate.ToString();
        

        StartCoroutine(KingdomClock());

        buildingNames = new string[]
        {
            "Dwelling",
            "Estate",
            "Log Hut",
            "Pig Pen",
            "Farm",
            "Port",
            "Mine",
            "Clock Tower",
            "Hospital",
            "market",
            "Wall",
            "barracade",
            "Tower",
            "Tavern",
            "stables",
            "gate",
            "library",
            "barracks",
            "citadel",
            "the keep",
            "blacksmith"
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //only updates once building is finished
    public void ChangePlayerBuildCount(int index){
        playerKingdom.buildingCounts[index]++;
    }

    //Clock used for build time

    int clockTime=0;
    IEnumerator KingdomClock(){
        while(true){
            yield return new WaitForSeconds(.1f);
            clockTime++;
            //10, meaning 1 second interval update
            if(clockTime%10==0){
                
                

                //Queue update
                if(playerKingdom.tasks.Count!=0){
                    LinkedListNode<GameObject> buttonSlot = playerQueue.listQueue.First;
                    for(LinkedListNode<Task> slot= playerKingdom.tasks.First; slot!=null; slot=slot.Next){
                        slot.Value.time-=1f;
                        if(slot.Value.time <= 0){
                            playerKingdom.tasks.Remove(slot);
                            Destroy(buttonSlot.Value);
                            playerQueue.listQueue.Remove(buttonSlot);
                        }
                        buttonSlot = buttonSlot.Next;
                    }

                    buttonSlot = playerQueue.listQueue.First;
                    foreach(Task slot in playerKingdom.tasks){
                        buttonSlot.Value.transform.GetChild(2).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text=playerQueue.CalculateStringTime(slot);
                    }
                }
            }
            //this update will bidaily, change to 3600 (every 6 minutes)
            if(clockTime%60==0){
                playerKingdom.KingdomUpdate(logHutProduction,stoneProduction,ironProduction,pigPenProduction,farmProduction);
                

            }
            
            //UI updates
            logCount.text=playerKingdom.wood.ToString();
            stoneCount.text=playerKingdom.stone.ToString();
            ironCount.text=playerKingdom.iron.ToString();
            populationCount.text=playerKingdom.population.ToString();

            if(playerKingdom.food<0){
                foodTotal.text="0";
            }else{
                foodTotal.text=playerKingdom.food.ToString();
            }
            
            if(playerKingdom.foodProductionRate-playerKingdom.foodConsumptionRate>=0){
                foodRatio.text=Mathf.Abs(playerKingdom.foodProductionRate-playerKingdom.foodConsumptionRate).ToString();
                minusSign.SetActive(false);

            }else{
                foodRatio.text=Mathf.Abs(playerKingdom.foodProductionRate-playerKingdom.foodConsumptionRate).ToString();
                minusSign.SetActive(true);
            }
                
            foodDesc.text="producing " + playerKingdom.foodProductionRate.ToString() + "\n using " + playerKingdom.foodConsumptionRate.ToString();
        }
    }

    //for queue
    public void playerQueueAddBuild(int selectionIndex){
        Task temp= new Task(buildingNames[selectionIndex], buildTimes[selectionIndex], true, clockTime);
        playerKingdom.tasks.AddLast(temp);
        playerQueue.AddBuildSlot(temp);
    }
    
}
