using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kingdom
{
    //public List<Soldier> army;
    //public List<Artillery> artillery;
    //public List<Boat> navy;

    public int population;
    public int wood;
    public int stone;
    public int iron;
    public int food;
    public int foodProductionRate;
    public int foodConsumptionRate;
    //public string religion;

    //public List<Building> buildings;
    public int[] buildingCounts;

    //Dictionary<Kingdom, Diplomacy> diplomacy;

    public LinkedList<Task> tasks;
    
    public Kingdom(){
        wood=100;
        stone=100;
        iron=100;
        population=100;
        buildingCounts = new int[21];

        food=100;
        foodConsumptionRate=1;
        foodProductionRate=3;

        tasks= new LinkedList<Task>();
    }

    public void KingdomUpdate(int _wood, int _stone, int _iron, int pigPenFood, int farmFood){
        //wood inc.
        wood+=buildingCounts[2]*_wood;
        stone+=buildingCounts[6]*_stone;
        iron+=buildingCounts[6]*_iron;
        foodProductionRate=(buildingCounts[3]*pigPenFood) + (buildingCounts[4]*farmFood);
        food=food+(foodProductionRate-foodConsumptionRate);
        
    }

    public void PopulationUpdate(int add){
        population+=add;
    }
    public void ConsumtionUpdate(int add){
        foodConsumptionRate+=add;
    }
    public void ProductionUpdate(int add){
        foodProductionRate+=add;
    }


//for reference, needed for index stuff
/*
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
*/

}

public class Task {
    public string ID;
    public string taskName;
    public float time;
    //is a building or research
    public bool isBuild;

    public Task(string name, float t, bool build, int idNumber){
        taskName=name;
        time=t;
        isBuild=build;
        string ID=name+idNumber.ToString();
    }

}
