using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public TextMeshProUGUI fightValueText;
    public TextMeshProUGUI spiritValueText;
    [Space(10)]
    public TextMeshProUGUI brawnValueText;
    public TextMeshProUGUI agilityValueText;
    public TextMeshProUGUI heartValueText;
    public TextMeshProUGUI willValueText; 

    public Image backgroundImage;

    private Unit unit;
    public Unit Unit
    {
        get {return unit;}
        set
        {
            if(unit != null && unit.Equals(value)) return;
            if(unit != null) Unsubscribe();
            unit = value;
            if(unit != null) InitializeUnitUI(unit);
        }
    }

    private void Unsubscribe()
    {
        unit.OnFightChanged -= UpdateFightText;
        unit.OnSpiritChanged -= UpdateSpiritText;
    }

    private void Subscribe()
    {
        unit.OnFightChanged += UpdateFightText;
        unit.OnSpiritChanged += UpdateSpiritText;

        unit.OnUnitInitialized += UpdateStatTexts;
    }

    private void InitializeUnitUI(Unit unit)
    {
        UpdateStatTexts();

        brawnValueText.text = unit.GetStatValue(UnitStat.Brawn).ToString();
        agilityValueText.text = unit.GetStatValue(UnitStat.Agility).ToString();
        heartValueText.text = unit.GetStatValue(UnitStat.Heart).ToString();
        willValueText.text = unit.GetStatValue(UnitStat.Will).ToString();

        Subscribe();
    }

    public void UpdateStatTexts()
    {
        fightValueText.text = unit.Fight.ToString();
        spiritValueText.text = unit.Spirit.ToString();
    }

    private void UpdateFightText(int fightValue)
    {
        fightValueText.text = fightValue.ToString();
    }

    private void UpdateSpiritText(int spiritValue)
    {
        spiritValueText.text = spiritValue.ToString();
    }
}