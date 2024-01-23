using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;

public enum UnitClass
{
    None = -1,
    HighBrawn = 0,
    HighAgility = 1,
    HighFocus = 2,
    HighMagic = 3,
    HighHeart = 4,
    HighWill = 5,
    Other = 6
}

public enum UnitStat
{
    Brawn,
    Agility,
    Focus,
    Magic,
    Heart,
    Will
}

public enum HealthType
{
    Fight,
    Mind,
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
            Debug.Log(ToString());
        } 
    }
    public delegate void OnFightChangedDelegate(int fightValue);
    public event OnFightChangedDelegate OnFightChanged;
    
    private int mind;
    public int Mind 
    { 
        get => mind; 
    
        set
        {
            if(value == mind) return;
            mind = value;
            if(mind < 0) mind = 0;
            OnMindChanged?.Invoke(mind);
        } 
    }
    public delegate void OnMindChangedDelegate(int mindValue);
    public event OnMindChangedDelegate OnMindChanged;

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
        mind = 0;
        spirit = 0;

        foreach(KeyValuePair<UnitStat, int> stat in unitStats)
        {
            if(IsStatFightStat(stat.Key)) fight += stat.Value;
            if(IsStatMindStat(stat.Key)) mind += stat.Value;
            if(IsStatSpiritStat(stat.Key)) 
            {
                spirit += stat.Value;
            }
        }

        OnFightChanged += CheckUnitStatus;
        OnMindChanged += CheckUnitStatus;
        OnSpiritChanged += CheckUnitStatus;
    }

    private void CheckUnitStatus(int newStatValue = -1)
    {
        if(Fight <= 0 || Mind <= 0 || Spirit <= 0) 
        {
            isUnitActive = false;
            Debug.Log($"{name} is defeated!");
        }
    }

    private bool IsStatFightStat(UnitStat stat)
    {
        return stat == UnitStat.Brawn || stat == UnitStat.Agility;
    }

    private bool IsStatMindStat(UnitStat stat)
    {
        return stat == UnitStat.Focus || stat == UnitStat.Magic;
    }

    private bool IsStatSpiritStat(UnitStat stat)
    {
        return stat == UnitStat.Heart || stat == UnitStat.Will;
    }

    public override string ToString()
    {
        return $"Name: {name}\nFight: {fight}\nMind: {mind}\nSpirit: {spirit}";
    }

    // public override bool Equals(object obj)
    // {
    //     if(obj == null || GetType() != typeof(Unit)) return false;
    //     Unit unit = (Unit) obj;
        
    //     return true;
    // }
}