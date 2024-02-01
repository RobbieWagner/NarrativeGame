using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatText : MonoBehaviour
{
    [SerializeField] private Slider statSlider;
    [SerializeField] private TextMeshProUGUI statText; 
    public Unit currentUnit;

    public void Initialize(Unit unit, int maxValue, int curValue, UnitStat stat)
    {
        unit.SubscribeToStatChangeEvent(UpdateVisual, stat);
        statText.text = $"{curValue}";
        currentUnit = unit;
    }

    public void UpdateVisual(int newValue)
    {
        statSlider.value = newValue;
        statText.text = $"{newValue}";
    }
}
