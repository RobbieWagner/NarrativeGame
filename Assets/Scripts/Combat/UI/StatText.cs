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
    [HideInInspector] public UnitStat followingStat;

    public void Initialize(Unit unit, int maxValue, int curValue, UnitStat stat)
    {
        unit.SubscribeToStatChangeEvent(UpdateVisual, stat);
        statText.text = $"{curValue}";
        currentUnit = unit;
        followingStat = stat;
    }

    public void UpdateVisual(int newValue)
    {
        statText.text = $"{newValue}";
    }

    public void EnableUI()
    {
        statText.enabled = true;
        statIcon.enabled = true;
        statText.text = currentUnit.GetStatValue(followingStat).ToString();
    }

    public void DisableUI()
    {
        statText.enabled = false;
        statIcon.enabled = false;
    }
}
