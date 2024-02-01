using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Ink.Parsed;
using UnityEngine;

public enum UnitStat
{
    //Main Stats
    None = -1,
    Brawn,
    Agility,
    Defense,
    Psych,
    Focus,
    Heart,

    //Computed stats
    HP, // Health (10 +  HalfStrength + HalfDefense)
    Mana, // Spell Stamina (5 + Psych + HalfHeart)
    Initiative, // Order in Combat (1d6 + HalfAgility)
    PCrit, // Physical Crit Chance (.001 * (HalfStrength + Agility))
    MCrit, // Mental Crit Chance (.001 * (HalfPsych + Focus))
    BMR, // Bad Mentality Resistance (Focus + Heart + HalfDefense)
}

public partial class Unit : MonoBehaviour
{
    [Header("Statistics")]
    public UnitClass Class;
    private Dictionary<UnitStat, int> unitStats;
    [SerializedDictionary("Stat","Base Value")] private SerializedDictionary<UnitStat, int> maxStatValues;
    public List<CombatAction> availableActions;

    public delegate void OnStatValueChangedDelegate(int value);

    #region stat properties
    private int hp;
    public int HP
    {
        get => hp;
        set
        {
            if (value == hp) return;
            hp = value;
            if(hp < 0) hp = 0;
            if(hp > GetMaxStatValue(UnitStat.HP)) hp = GetMaxStatValue(UnitStat.HP);
            OnHPChanged?.Invoke(hp);
        }
    }
    public event OnStatValueChangedDelegate OnHPChanged;

    private int mana;
    public int Mana
    {
        get => mana;
        set
        {
            if (value == mana) return;
            mana = value;
            if(mana < 0) mana = 0;
            if(mana > GetMaxStatValue(UnitStat.Mana)) mana = GetMaxStatValue(UnitStat.Mana);
            OnManaChanged?.Invoke(mana);
        }
    }
    public event OnStatValueChangedDelegate OnManaChanged;

    private int brawn;
    public int Brawn
    {
        get => brawn;
        set
        {
            if (value == brawn) return;
            brawn = value;
            if(brawn < 0) brawn = 0;
            if(brawn > GetMaxStatValue(UnitStat.Brawn)) brawn = GetMaxStatValue(UnitStat.Brawn);
            OnBrawnChanged?.Invoke(brawn);
        }
    }
    public event OnStatValueChangedDelegate OnBrawnChanged;

    private int agility;
    public int Agility
    {
        get => agility;
        set
        {
            if (value == agility) return;
            agility = value;
            if(agility < 0) agility = 0;
            if(agility > GetMaxStatValue(UnitStat.Agility)) agility = GetMaxStatValue(UnitStat.Agility);
            OnAgilityChanged?.Invoke(agility);
        }
    }
    public event OnStatValueChangedDelegate OnAgilityChanged;

    private int defense;
    public int Defense
    {
        get => defense;
        set
        {
            if (value == defense) return;
            defense = value;
            if(defense < 0) defense = 0;
            if(defense > GetMaxStatValue(UnitStat.Defense)) defense = GetMaxStatValue(UnitStat.Defense);
            OnDefenseChanged?.Invoke(defense);
        }
    }
    public event OnStatValueChangedDelegate OnDefenseChanged;

    private int psych;
    public int Psych
    {
        get => psych;
        set
        {
            if (value == psych) return;
            psych = value;
            if(psych < 0) psych = 0;
            if(psych > GetMaxStatValue(UnitStat.Psych)) psych = GetMaxStatValue(UnitStat.Psych);
            OnPsychChanged?.Invoke(psych);
        }
    }
    public event OnStatValueChangedDelegate OnPsychChanged;

    private int focus;
    public int Focus
    {
        get => focus;
        set
        {
            if (value == focus) return;
            focus = value;
            if(focus < 0) focus = 0;
            if(focus > GetMaxStatValue(UnitStat.Focus)) focus = GetMaxStatValue(UnitStat.Focus);
            OnFocusChanged?.Invoke(focus);
        }
    }
    public event OnStatValueChangedDelegate OnFocusChanged;

    private int heart;
    public int Heart
    {
        get => heart;
        set
        {
            if (value == heart) return;
            heart = value;
            if(heart < 0) heart = 0;
            if(heart > GetMaxStatValue(UnitStat.HP)) heart = GetMaxStatValue(UnitStat.HP);
            OnHeartChanged?.Invoke(heart);
        }
    }
    public event OnStatValueChangedDelegate OnHeartChanged;

    private int pCrit;
    public int PCrit
    {
        get => pCrit;
        set
        {
            if (value == pCrit) return;
            pCrit = value;
            if(pCrit < 0) pCrit = 0;
            if(pCrit > GetMaxStatValue(UnitStat.PCrit)) pCrit = GetMaxStatValue(UnitStat.PCrit);
            OnPCritChanged?.Invoke(pCrit);
        }
    }
    public event OnStatValueChangedDelegate OnPCritChanged;

    private int mCrit;
    public int MCrit
    {
        get => mCrit;
        set
        {
            if (value == mCrit) return;
            mCrit = value;
            if(mCrit < 0) mCrit = 0;
            if(mCrit > GetMaxStatValue(UnitStat.MCrit)) mCrit = GetMaxStatValue(UnitStat.MCrit);
            OnMCritChanged?.Invoke(mCrit);
        }
    }
    public event OnStatValueChangedDelegate OnMCritChanged;

    private int initiative;
    public int Initiative
    {
        get => initiative;
        set
        {
            if (value == initiative) return;
            initiative = value;
            if(initiative < 0) hp = 0;
            if(initiative > GetMaxStatValue(UnitStat.Initiative)) initiative = GetMaxStatValue(UnitStat.Initiative);
            OnInitiativeChanged?.Invoke(initiative);
        }
    }
    public event OnStatValueChangedDelegate OnInitiativeChanged;

    private int badMentRes;
    public int BadMentRes
    {
        get => badMentRes;
        set
        {
            if (value == badMentRes) return;
            badMentRes = value;
            if(badMentRes < 0) badMentRes = 0;
            if(badMentRes > GetMaxStatValue(UnitStat.BMR)) badMentRes = GetMaxStatValue(UnitStat.BMR);
            OnBMRChanged?.Invoke(badMentRes);
        }
    }
    public event OnStatValueChangedDelegate OnBMRChanged;
    #endregion

    private void InitializeStats()
    {
        brawn = GetMaxStatValue(UnitStat.Brawn);
        agility = GetMaxStatValue(UnitStat.Agility);
        defense = GetMaxStatValue(UnitStat.Defense);
        psych = GetMaxStatValue(UnitStat.Psych);
        focus = GetMaxStatValue(UnitStat.Focus);
        heart = GetMaxStatValue(UnitStat.Heart);

        hp = GetMaxStatValue(UnitStat.HP);
        mana = GetMaxStatValue(UnitStat.Mana);
        pCrit = GetMaxStatValue(UnitStat.PCrit);
        mCrit = GetMaxStatValue(UnitStat.MCrit);
        initiative = GetMaxStatValue(UnitStat.Initiative);
        badMentRes = GetMaxStatValue(UnitStat.BMR);
    }

    public int GetMaxStatValue(UnitStat stat)
    {
        if(maxStatValues.Keys.Contains(stat)) return maxStatValues[stat];
        switch(stat)
        {
            case UnitStat.None:
            return -1;
            case UnitStat.Brawn:
            return -1;
            case UnitStat.Agility:
            return -1;
            case UnitStat.Defense:
            return -1;
            case UnitStat.Psych:
            return -1;
            case UnitStat.Focus:
            return -1;
            case UnitStat.Heart:
            return -1;

            case UnitStat.HP:
            return 10 + maxStatValues[UnitStat.Brawn] / 2 + maxStatValues[UnitStat.Agility] / 2;
            case UnitStat.Mana:
            return 5 + maxStatValues[UnitStat.Psych] + maxStatValues[UnitStat.Heart] / 2;
            case UnitStat.Initiative:
            return Random.Range(1, 7) + maxStatValues[UnitStat.Agility];
            case UnitStat.PCrit:
            return maxStatValues[UnitStat.Brawn] / 2 + maxStatValues[UnitStat.Agility];
            case UnitStat.MCrit:
            return maxStatValues[UnitStat.Psych] / 2 + maxStatValues[UnitStat.Focus];
            case UnitStat.BMR:
            return maxStatValues[UnitStat.Defense] + maxStatValues[UnitStat.Focus] + maxStatValues[UnitStat.Heart];
        }
        return -1;
    }

    public void SubscribeToStatChangeEvent(OnStatValueChangedDelegate action, UnitStat stat)
    {
        switch(stat)
        {
            case UnitStat.Brawn:
            OnBrawnChanged += action;
            break;
            case UnitStat.Agility:
            OnAgilityChanged += action;
            break;
            case UnitStat.Defense:
            OnDefenseChanged += action;
            break;
            case UnitStat.Psych:
            OnPsychChanged += action;
            break;
            case UnitStat.Focus:
            OnFocusChanged += action;
            break;
            case UnitStat.Heart:
            OnHeartChanged += action;
            break;

            case UnitStat.HP:
            OnHPChanged += action;
            break;
            case UnitStat.Mana:
            OnManaChanged += action;
            break;
            case UnitStat.Initiative:
            OnInitiativeChanged += action;
            break;
            case UnitStat.PCrit:
            OnPCritChanged += action;
            break;
            case UnitStat.MCrit:
            OnMCritChanged += action;
            break;
            case UnitStat.BMR:
            OnBMRChanged += action;
            break;
            default:
            Debug.LogWarning("attempt to subscribe to stat change failed (stat was not found)");
            break;
        }
    }

    public void UnsubscribeToStatChangeEvent(OnStatValueChangedDelegate action, UnitStat stat)
    {
        switch(stat)
        {
            case UnitStat.Brawn:
            OnBrawnChanged -= action;
            break;
            case UnitStat.Agility:
            OnAgilityChanged -= action;
            break;
            case UnitStat.Defense:
            OnDefenseChanged -= action;
            break;
            case UnitStat.Psych:
            OnPsychChanged -= action;
            break;
            case UnitStat.Focus:
            OnFocusChanged -= action;
            break;
            case UnitStat.Heart:
            OnHeartChanged -= action;
            break;

            case UnitStat.HP:
            OnHPChanged -= action;
            break;
            case UnitStat.Mana:
            OnManaChanged -= action;
            break;
            case UnitStat.Initiative:
            OnInitiativeChanged -= action;
            break;
            case UnitStat.PCrit:
            OnPCritChanged -= action;
            break;
            case UnitStat.MCrit:
            OnMCritChanged -= action;
            break;
            case UnitStat.BMR:
            OnBMRChanged -= action;
            break;
            default:
            Debug.LogWarning("attempt to unsubscribe to stat change failed (stat was not found)");
            break;
        }
    }
}