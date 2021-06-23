using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kingdom
{
    public List<Soldier> army;
    public List<Artillery> artillery;
    public List<Boat> navy;

    public int population;
    public int wood;
    public int stone;
    public int iron;
    public int food;
    public int foodProductionRate;
    public int foodConsumptionRate;
    public string religion;

    //public List<Building> buildings;
    Dictionary<string, int> buildingCounts;

    //Dictionary<Kingdom, Diplomacy> diplomacy;

    //LinkedList<?> tasks;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
