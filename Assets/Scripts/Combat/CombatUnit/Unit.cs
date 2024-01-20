using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitClass
{
    HighPhysique,
    HighAgility,
    HighSly,
    HighIntuition,
    HighCare,
    HighWill
}

public class Unit : MonoBehaviour
{
    // Unit properties
    public string UnitName;
    public UnitClass Class;

    // Base stats
    public int brawn; //FIGHT: determines damage output for physical attacks
    public int agility; //FIGHT: determines execution order
    public int focus; //PSYCHE: determines hit/crit chance
    public int composure; //PSYCHE: mental defense 
    public int heart; //SPIRIT: determines effectiveness of aiding actions
    public int will; //SPIRIT: determines defense against damage of all types

    // Health stats
    private int fight;
    public int Fight 
    { 
        get => fight; 
    
        set
        {
            if(value == fight) return;
            fight = value;
            OnFightChanged?.Invoke(fight);
        } 
    }
    public delegate void OnFightChangedDelegate(int fightValue);
    public event OnFightChangedDelegate OnFightChanged;
    
    private int psyche;
    public int Psyche 
    { 
        get => psyche; 
    
        set
        {
            if(value == psyche) return;
            psyche = value;
            OnPsycheChanged?.Invoke(psyche);
        } 
    }
    public delegate void OnPsycheChangedDelegate(int witsValue);
    public event OnPsycheChangedDelegate OnPsycheChanged;

    private int spirit;
    public int Spirit 
    { 
        get => spirit; 
    
        set
        {
            if(value == spirit) return;
            spirit = value;
            OnSpiritChanged?.Invoke(spirit);
        } 
    }
    public delegate void OnSpiritChangedDelegate(int spiritValue);
    public event OnSpiritChangedDelegate OnSpiritChanged;

    // Unit actions
    public List<CombatAction> AvailableActions;

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
        fight = brawn + agility;
        psyche = focus + composure;
        spirit = heart + will;
    }

    // Example method for performing an action
    public IEnumerator PerformAction(CombatAction action, List<Unit> targets)
    {
        foreach(Unit target in targets) Debug.Log(target.name);
        yield return null;
    }
}