using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TechBranch { Agriculture, Math, Lumber, Military, Culture };
public enum TechStatus { Researched, Researching, Idle, Locked };

public class TechTree : MonoBehaviour
{
    public TechTreeItem[] items;

    public Sprite activeResearchButton;
    public Sprite idleButton;
    public Sprite lockedButton;

    public Sprite lockedLine;
    public Sprite dimLine;
    public Sprite brightLine;

    public Sprite tempSprite;

    public Sprite AgricultureButton;
    public Sprite MathButton;
    public Sprite LumberButton;
    public Sprite MilitaryButton;
    public Sprite CultureButton;

    public Sprite AgricultureLine;
    public Sprite MathLine;
    public Sprite LumberLine;
    public Sprite MilitaryLine;
    public Sprite CultureLine;

    public void UpdateTechTreeColors() {
        for (int i = 0; i < items.Length; i++) {
            if (items[i].gameObject == null)
                continue;
            items[i].gameObject.GetComponent<Image>().sprite = tempSprite;
            for (int j = 0; j < items[i].childLines.Length; j++) {
                items[i].childLines[j].GetComponent<Image>().sprite = tempSprite;
            }
        }


        for (int i = 0; i < items.Length; i++) {
            if (items[i].gameObject == null)
                continue;

            if (items[i].status == TechStatus.Researched) {
                Sprite itemSprite = GetTechSprite(items[i].branch, true);
                items[i].gameObject.GetComponent<Image>().sprite = itemSprite;
                for (int j = 0; j < items[i].childLines.Length; j++) {
                    Sprite lineSprite = GetTechSprite(items[i].branch, false);
                    items[i].childLines[j].GetComponent<Image>().sprite = lineSprite;
                }
            } else if (items[i].status == TechStatus.Researching) {
                items[i].gameObject.GetComponent<Image>().sprite = activeResearchButton;
                for (int j = 0; j < items[i].childLines.Length; j++) {
                    Image lineImage = items[i].childLines[j].GetComponent<Image>();
                    if (!(lineImage.sprite == AgricultureLine || lineImage.sprite == MathLine ||
                        lineImage.sprite == LumberLine || lineImage.sprite == MilitaryLine ||
                        lineImage.sprite == CultureLine)) {
                        lineImage.sprite = dimLine;
                    }
                }
            } else if (items[i].status == TechStatus.Idle) {
                items[i].gameObject.GetComponent<Image>().sprite = idleButton;
                for (int j = 0; j < items[i].childLines.Length; j++) {
                    Image lineImage = items[i].childLines[j].GetComponent<Image>();
                    if (!(lineImage.sprite == AgricultureLine || lineImage.sprite == MathLine ||
                        lineImage.sprite == LumberLine || lineImage.sprite == MilitaryLine ||
                        lineImage.sprite == CultureLine)) {
                        lineImage.sprite = dimLine;
                    }
                }
            } else if (items[i].status == TechStatus.Locked) {
                items[i].gameObject.GetComponent<Image>().sprite = lockedButton;
                for (int j = 0; j < items[i].childLines.Length; j++) {
                    Image lineImage = items[i].childLines[j].GetComponent<Image>();
                    if (lineImage.sprite == tempSprite) {
                        lineImage.sprite = lockedLine;
                    }  
                }
            }
        }
    }

    private Sprite GetTechSprite(TechBranch branch, bool isButton) {
        if (isButton) {
            if (branch == TechBranch.Agriculture) {
                return AgricultureButton;
            } else if (branch == TechBranch.Math) {
                return MathButton;
            } else if (branch == TechBranch.Lumber) {
                return LumberButton;
            } else if (branch == TechBranch.Military) {
                return MilitaryButton;
            } else {
                return CultureButton;
            }
        } else {
            if (branch == TechBranch.Agriculture) {
                return AgricultureLine;
            } else if (branch == TechBranch.Math) {
                return MathLine;
            } else if (branch == TechBranch.Lumber) {
                return LumberLine;
            } else if (branch == TechBranch.Military) {
                return MilitaryLine;
            } else {
                return CultureLine;
            }
        }
    }

}

[System.Serializable]
public struct TechTreeItem {
    public string name;
    public string description;
    public GameObject gameObject;
    public TechBranch branch;
    public TechStatus status;
    public GameObject[] childLines;
}