using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;
using System.Linq;
using DG.Tweening;

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
    [Header("Vanity")]
    public string UnitName;
    [SerializeField] private UnitAnimator unitAnimator;
    [SerializeField] private SpriteRenderer unitSprite;
    private Coroutine parentBlinkCo;
    private Sequence currentBlinkCo;

    private const float BLINK_TIME = 1f;

    [Header("Statistics")]
    public UnitClass Class;
    [SerializedDictionary("Stat","Base Value")] public SerializedDictionary<UnitStat, int> unitStats;
    Dictionary<UnitStat, int> maxStatValues;
    public List<CombatAction> availableActions;

    [Header("Runtime")]
    public CombatAction currentSelectedAction;
    public List<Unit> selectedTargets;

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
        SetupBlinkTween();
    }

    private void SetupBlinkTween()
    {
    }

    // Method to initialize unit
    private void InitializeUnit()
    {
        fight = 0;
        spirit = 0;

        maxStatValues = new Dictionary<UnitStat, int>();

        foreach(KeyValuePair<UnitStat, int> stat in unitStats)
        {
            maxStatValues.Add(stat.Key, stat.Value);
            if(IsStatFightStat(stat.Key)) 
                fight += stat.Value;
            if(IsStatSpiritStat(stat.Key))
                spirit += stat.Value;
        }

        OnFightChanged += CheckUnitStatus;
        OnSpiritChanged += CheckUnitStatus;

        unitAnimator.SetAnimationState(UnitAnimationState.Idle);
        
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

    public int GetMaxValue(HealthType healthType)
    {
        switch(healthType)
        {
            case HealthType.Fight:
            return maxStatValues[UnitStat.Brawn] + maxStatValues[UnitStat.Agility];
            case HealthType.Spirit:
            return maxStatValues[UnitStat.Heart] + maxStatValues[UnitStat.Will];
        }
        return -1;
    }

    public void SetUnitAnimatorState(UnitAnimationState state) => unitAnimator.SetAnimationState(state);

    public void StartBlinking()
    {
        if(currentBlinkCo == null || !currentBlinkCo.IsPlaying())
        {
            float halfBlinkTime = BLINK_TIME/2;
            currentBlinkCo = DOTween.Sequence();
            currentBlinkCo.Append(unitSprite.DOColor(Color.clear, halfBlinkTime).SetEase(Ease.InCubic));
            currentBlinkCo.Append(unitSprite.DOColor(Color.white, halfBlinkTime).SetEase(Ease.OutCubic));
            currentBlinkCo.SetLoops(-1, LoopType.Restart);

            unitSprite.color = Color.white;
            currentBlinkCo.Play();
        }
    }

    public void StopBlinking()
    {
        if(currentBlinkCo != null && currentBlinkCo.IsPlaying()) 
        {
            currentBlinkCo.Kill(true);
            unitSprite.color = Color.white;
        }
    }

    // public override bool Equals(object obj)
    // {
    //     if(obj == null || GetType() != typeof(Unit)) return false;
    //     Unit unit = (Unit) obj;

    //     return true;
    // }
}