using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

public class BuildListSelection : MonoBehaviour
{
    public int selectionIndex;
    List<GameObject> buildings;
    public GameObject backdrop;
    public GameObject buildingPreview;
    //public GameObject kingdomManager;

    //desc
    public TextMeshProUGUI description;
    String[] buildingDescriptions;
    //req 1
    public TextMeshProUGUI techRequirementText1;
    String[] techRequirement1;
    //req 2
    public TextMeshProUGUI techRequirementText2;
    String[] techRequirement2;
    //names
    public TextMeshProUGUI buildName;
    String[] buildingNames;
    //buildlimits
    public TextMeshProUGUI buildLimitText;
    String[] buildingLimits;
    //buildTimes
    public TextMeshProUGUI buildTimeText;
    String[] buildingTimes;

    public void setSelectionIndex(int index)
    {
        //if (selectionIndex != -1)
           // buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(false);
        selectionIndex = index;
        buildings[selectionIndex].transform.GetChild(0).gameObject.SetActive(true);

        //kingdomManager.transform.GetComponent<KingdomManagement>().ChangePlayerBuildCount(index);

        if (!backdrop.activeSelf)
            backdrop.SetActive(true);
        //desc
        description.text = buildingDescriptions[selectionIndex];

        techRequirementText1.text = techRequirement1[selectionIndex];

        techRequirementText2.text = techRequirement2[selectionIndex];

        buildName.text = buildingNames[selectionIndex];

        buildLimitText.text = buildingLimits[selectionIndex];

        buildTimeText.text = buildingTimes[selectionIndex];
    }


    

    void Start()
    {
        buildings = new List<GameObject>();
        foreach (Transform child in transform)
        {
            buildings.Add(child.gameObject);
        }
        //desc
        buildingDescriptions = new string[]{
            "The Dwelling is a building that houses x subjects in your kingdom. Building more dwellings can help increase your population.",
            "By building an Estate you can increase your population rapidly. The Estate houses x people.",
            "The Log Hut produces wood for your kingdom. Each Log Hut makes x wood per day, so you might need to make more than one!",
            "The Pig Pen is a great supply of food for your kingdom.The Pig Pen produces x food per day, but it needs x food per day from farms to feed those hungry pigs.",
            "Farms are nessecary food source for your kingdom. They are used to feed your subjects, livestock, and horses.A Farm produces x food per day.",
            "Build a Port to have boat access to nearby water.",
            "The Mine is where your kingdom will dig and refine for stone and iron.This building can be upgraded, to produce higher rates for both stone and iron.",
            "The Clock Tower is a building that boosts the productivity in a radius of x meters by x percent.",
            "The Hospital is a building that replinishes your population by healing the sick. The Hosptital also heals nearby troops with injuries from battle.",
            "The Market allows your kingdom to trade with other nearby kingdoms. No enemy kingdoms allowed of course.",
            "Walls are a great way to protect your kingdom from enemies. Dont forget a gate!",
            "Use barracades to stop artillery and slow down enemy troops.",
            "Watch Towers can be built to help watch your kingdom. They can be armed with troops and even ballistas!",
            "The tavern is a building that boosts the productivity in your kingdom by x percent.",
            "By building the Stables your kingdom now has access to horses for moving artillery, moving goods, and cavelry.",
            "Gates can be built to allow for movement in and out of your kingdoms walls",
            "By building the library, you can research an extra subject at a time.",
            "The Barracks is a building that is used to train troops with stronger armor. The Barracks is also used for constructing artillery.",
            "The Citadel boosts your religions perks by x percent.",
            "Building the Keep is the key to the world of diplomacy and trade. The Keep also increases your kingdoms maximum population.",
            "The Blacksmith helps your kingdom gain access to more advanced weaponary for your troops to use."
        };

        techRequirement1 = new string[]{
            "",
            "Architecture I",
            "Lumber",
            "Animal Husbandry I",
            "Agriculture",
            "Sailing",
            "Architecture I",
            "Architecture II",
            "Architecture II",
            "Trade",
            "",
            "",
            "Architecture I",
            "Culture",
            "Architecture II",
            "Math",
            "Architecture II",
            "Architecture II",
            "Architecture III",
            "Architecture I",
            "Architecture I"
        };

        techRequirement2 = new string[]{
            "",
            "",
            "",
            "",
            "",
            "",
            "Stone Cutting",
            "Math II",
            "Medicine",
            "",
            "",
            "",
            "",
            "",
            "Animal Husbandry II",
            "",
            "math II",
            "Military",
            "Religion",
            "",
            "Weapons II",
        };

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

        buildingLimits = new string[]
        {
            "99",
            "99",
            "4",
            "4",
            "10",
            "6",
            "2",
            "2",
            "1",
            "2",
            "99",
            "99",
            "8",
            "4",
            "2",
            "8",
            "2",
            "3",
            "1",
            "1",
            "1"
        };

        buildingTimes = new string[]
        {
            "99 days",
            "8 hours",
            "1 week",
            "99 days",
            "8 hours",
            "1 week",
            "99 days",
            "8 hours",
            "1 week",
            "99 days",
            "8 hours",
            "1 week",
            "99 days",
            "8 hours",
            "1 week",
            "99 days",
            "8 hours",
            "1 week",
            "99 days",
            "8 hours",
            "1 week",
        };
    }

    // Update is called once per frame
    void Update()
    {

    }
}

