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
        if(prevActionImage != null) prevActionImage.enabled = true;
        curActionImage.enabled = true;
        if(prevActionImage != null) nextActionImage.enabled = true;
    }
}