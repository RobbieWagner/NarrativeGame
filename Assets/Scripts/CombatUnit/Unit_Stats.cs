using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

[System.Serializable]
public class BaseStat
{
    [SerializeField] public ComputedStatType computedStat;
    public int value;
    [SerializeField] private int baseValue;
    [SerializeField] private int boost;

    public int GetBaseValue() { return baseValue; } 
    public int GetBoost() { return boost; }
}

[System.Serializable]
public class ComputedStat
{
    public int value {get; private set;}
}

public partial class Unit : MonoBehaviour
{
    [SerializedDictionary("Computed Stat", "Stat Info")] public SerializedDictionary<ComputedStatType, ComputedStat> computedStats;
    [SerializedDictionary("Base Stat", "Stat Info")] public SerializedDictionary<BaseStatType, BaseStat> baseStats;

    public void InitializeStats()
    {
        
    }
}
