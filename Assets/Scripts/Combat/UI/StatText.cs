using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statText; 
    public Image statIcon;
    [HideInInspector] public Unit currentUnit;

    public void Initialize(Unit unit, int maxValue, int curValue, UnitStat stat)
    {
        unit.SubscribeToStatChangeEvent(UpdateVisual, stat);
        statText.text = $"{curValue}";
        currentUnit = unit;
    }

    public void UpdateVisual(int newValue)
    {
        statText.text = $"{newValue}";
    }
}
