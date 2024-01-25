using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;
using System.Linq;

public enum UnitClass
{
    None = -1,
    
    Other = 5
}

public enum UnitStat
{
    Brawn,
    Agility,
    Heart,
    Will
}

public enum HealthType
{
    Fight,
    Spirit
}

public class Unit : MonoBehaviour
{
    // Unit properties
    public string UnitName;
    public UnitClass Class;
    [SerializedDictionary("Stat","Base Value")] public SerializedDictionary<UnitStat, int> unitStats;

    public CombatAction currentSelectedAction;
    public List<CombatAction> availableActions;
    public List<Unit> selectedUnits;

    [HideInInspector] public bool isUnitActive = true;

    // Health stats
    private int fight;
    public int Fight 
    { 
        get => fight; 
    
        set
        {
            if(value == fight) return;
            fight = value;
            if(fight < 0) fight = 0;
            OnFightChanged?.Invoke(fight);
        } 
    }
    public delegate void OnFightChangedDelegate(int fightValue);
    public event OnFightChangedDelegate OnFightChanged;

    private int spirit;
    public int Spirit 
    { 
        get => spirit; 
    
        set
        {
            if(value == spirit) return;
            spirit = value;
            if(spirit < 0) spirit = 0;
            OnSpiritChanged?.Invoke(spirit);
        } 
    }
    public delegate void OnSpiritChangedDelegate(int spiritValue);
    public event OnSpiritChangedDelegate OnSpiritChanged;

    // Unit animator
    //public UnitAnimator unitAnimator;

    // Initialization
    private void Awake()
    {
        InitializeUnit();
    }

    // Method to initialize unit
    private void InitializeUnit()
    {
        fight = 0;
        spirit = 0;

        foreach(KeyValuePair<UnitStat, int> stat in unitStats)
        {
            if(IsStatFightStat(stat.Key)) 
                fight += stat.Value;
            if(IsStatSpiritStat(stat.Key))
                spirit += stat.Value;
        }

        OnFightChanged += CheckUnitStatus;
        OnSpiritChanged += CheckUnitStatus;
        
        OnUnitInitialized?.Invoke();
    }

    public delegate void OnUnitInitializedDelegate();
    public event OnUnitInitializedDelegate OnUnitInitialized;

    private void CheckUnitStatus(int newStatValue = -1)
    {
        if(Fight <= 0 || Spirit <= 0) 
        {
            isUnitActive = false;
            Debug.Log($"{name} is defeated!");
        }
    }

    private bool IsStatFightStat(UnitStat stat)
    {
        return stat == UnitStat.Brawn || stat == UnitStat.Agility;
    }

    private bool IsStatSpiritStat(UnitStat stat)
    {
        return stat == UnitStat.Heart || stat == UnitStat.Will;
    }

    public override string ToString()
    {
        return $"Name: {name}\nFight: {fight}\nSpirit: {spirit}";
    }

    public int GetStatValue(UnitStat stat)
    {
        if(unitStats.ContainsKey(stat)) return unitStats[stat];
        else return -1;
    }


    // public override bool Equals(object obj)
    // {
    //     if(obj == null || GetType() != typeof(Unit)) return false;
    //     Unit unit = (Unit) obj;

    //     return true;
    // }
}