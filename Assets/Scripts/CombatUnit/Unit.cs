using System.Collections;
using System.Collections.Generic;
using RobbieWagnerGames;
using RobbieWagnerGames.StrategyCombat;
using UnityEngine;

public enum BaseStatType
{
    None = 0,
    Strength = 1,
    Agility = 2,
    Cunning = 3,
    Intuition = 4,
    Care = 5,
    Will = 6
}

public enum ComputedStatType
{
    None = 0,
    Fight = 1,
    Wits = 2,
    Spirit = 3
}

public enum UnitClass
{
    None = 0,
    HighStatA = 1,
    HighStatB = 2,
    HighStatC = 3,
    HighStatD = 4,
    HighStatE = 5,
    HighStatF = 6,
    Other = 7
}

public partial class Unit : MonoBehaviour
{
    [SerializeField] public string unitName;
    [SerializeField] public UnitClass unitClass = UnitClass.None;
    [SerializeField] public UnitAnimator unitAnimator;
    [SerializeField] public List<CombatAction> unitActions;
}
