using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Statbar : MonoBehaviour
{
    [SerializeField] private Slider statSlider;
    [SerializeField] private TextMeshProUGUI statText; 
    [SerializeField] private TextMeshProUGUI statNameText;
    public Image statIcon;
    public Image sliderFill;

    public void Initialize(Unit unit, int maxValue, int curValue, UnitStat stat)
    {
        unit.SubscribeToStatChangeEvent(UpdateVisual, stat);
        statSlider.maxValue = maxValue;
        statSlider.value = curValue;
        statText.text = $"{statSlider.value}/{statSlider.maxValue}";
        statNameText.text = stat.ToString().ToUpper();
    }

    public void UpdateVisual(int newValue)
    {
        statSlider.value = newValue;
        statText.text = $"{statSlider.value}/{statSlider.maxValue}";
    }
}
