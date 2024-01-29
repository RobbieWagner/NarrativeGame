using System;
using UnityEngine;
using UnityEngine.UI;

public class ActionSelectionUI : MonoBehaviour
{
    public Image prevActionImage;
    public Image curActionImage;
    public Image nextActionImage;

    public void EnableActionUI()
    {
        prevActionImage.enabled = true;
        curActionImage.enabled = true;
        nextActionImage.enabled = true;
    }
}