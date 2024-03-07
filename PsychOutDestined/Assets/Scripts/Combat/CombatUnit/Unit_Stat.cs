using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Ink.Parsed;
using UnityEngine;

namespace PsychOutDestined
{
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
        Stress, // Spell Stamina (5 + Psych + HalfHeart)
        Initiative, // Order in Combat (1d6 + HalfAgility)
        PCrit, // Physical Crit Chance (.001 * (HalfStrength + Agility))
        MCrit, // Mental Crit Chance (.001 * (HalfPsych + Focus))
        BMR, // Bad Mentality Resistance (Focus + Heart + HalfDefense)
    }

    public partial class Unit : MonoBehaviour
    {
        [Header("Statistics")]
        public UnitClass Class;
        [Space(10)]
        private Dictionary<UnitStat, int> unitStats;
        [SerializeField][SerializedDictionary("Stat", "Base Value")] protected SerializedDictionary<UnitStat, int> maxStatValues;
        [Space(10)]
        public List<CombatAction> availableActions;

        public delegate void OnStatValueChangedDelegate(int value);

        public delegate void OnUnitStatChangedDelegate();
        public event OnUnitStatChangedDelegate OnMainStatChanged;
        public event OnUnitStatChangedDelegate OnStatChanged;
        public event OnUnitStatChangedDelegate OnNonMeteredStatChanged;

        #region stat properties
        protected int hp;
        public int HP
        {
            get => hp;
            set
            {
                if (value == hp) return;
                hp = value;
                if (hp < 0) hp = 0;
                if (hp > GetMaxStatValue(UnitStat.HP)) hp = GetMaxStatValue(UnitStat.HP);
                OnHPChanged?.Invoke(hp);
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnHPChanged;

        protected int stress;
        public int Stress
        {
            get => stress;
            set
            {
                if (value == stress) return;
                stress = value;
                if (stress < 0) stress = 0;
                if (stress > GetMaxStatValue(UnitStat.Stress)) stress = GetMaxStatValue(UnitStat.Stress);
                OnStressChanged?.Invoke(stress);
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnStressChanged;

        protected int brawn;
        public int Brawn
        {
            get => brawn;
            set
            {
                if (value == brawn) return;
                brawn = value;
                if (brawn < 0) brawn = 0;
                OnBrawnChanged?.Invoke(brawn);
                OnMainStatChanged?.Invoke();
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnBrawnChanged;

        protected int agility;
        public int Agility
        {
            get => agility;
            set
            {
                if (value == agility) return;
                agility = value;
                if (agility < 0) agility = 0;
                OnAgilityChanged?.Invoke(agility);
                OnMainStatChanged?.Invoke();
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnAgilityChanged;

        protected int defense;
        public int Defense
        {
            get => defense;
            set
            {
                if (value == defense) return;
                defense = value;
                if (defense < 0) defense = 0;
                OnDefenseChanged?.Invoke(defense);
                OnMainStatChanged?.Invoke();
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnDefenseChanged;

        protected int psych;
        public int Psych
        {
            get => psych;
            set
            {
                if (value == psych) return;
                psych = value;
                if (psych < 0) psych = 0;
                OnPsychChanged?.Invoke(psych);
                OnMainStatChanged?.Invoke();
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnPsychChanged;

        protected int focus;
        public int Focus
        {
            get => focus;
            set
            {
                if (value == focus) return;
                focus = value;
                if (focus < 0) focus = 0;
                OnFocusChanged?.Invoke(focus);
                OnMainStatChanged?.Invoke();
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnFocusChanged;

        protected int heart;
        public int Heart
        {
            get => heart;
            set
            {
                if (value == heart) return;
                heart = value;
                if (heart < 0) heart = 0;
                OnHeartChanged?.Invoke(heart);
                OnMainStatChanged?.Invoke();
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnHeartChanged;

        protected int pCrit;
        public int PCrit
        {
            get
            {
                if (pCrit > GetMaxStatValue(UnitStat.PCrit)) return GetMaxStatValue(UnitStat.PCrit);
                return pCrit;
            }
            set
            {
                if (value == pCrit) return;
                pCrit = value;
                if (pCrit < 0) pCrit = 0;
                if (pCrit > GetMaxStatValue(UnitStat.PCrit)) pCrit = GetMaxStatValue(UnitStat.PCrit);
                OnPCritChanged?.Invoke(pCrit);
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnPCritChanged;

        protected int mCrit;
        public int MCrit
        {
            get
            {
                if (mCrit > GetMaxStatValue(UnitStat.MCrit)) return GetMaxStatValue(UnitStat.MCrit);
                return mCrit;
            }
            set
            {
                if (value == mCrit) return;
                mCrit = value;
                if (mCrit < 0) mCrit = 0;
                if (mCrit > GetMaxStatValue(UnitStat.MCrit)) mCrit = GetMaxStatValue(UnitStat.MCrit);
                OnMCritChanged?.Invoke(mCrit);
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnMCritChanged;

        protected int initiative;
        public int Initiative
        {
            get
            {
                if (initiative > GetMaxStatValue(UnitStat.Initiative)) return GetMaxStatValue(UnitStat.Initiative);
                return initiative;
            }
            set
            {
                if (value == initiative) return;
                initiative = value;
                if (initiative < 0) hp = 0;
                if (initiative > GetMaxStatValue(UnitStat.Initiative)) initiative = GetMaxStatValue(UnitStat.Initiative);
                OnInitiativeChanged?.Invoke(initiative);
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnInitiativeChanged;

        protected int badMentRes;
        public int BadMentRes
        {
            get
            {
                if (badMentRes > GetMaxStatValue(UnitStat.BMR)) return GetMaxStatValue(UnitStat.BMR);
                return badMentRes;
            }
            set
            {
                if (value == badMentRes) return;
                badMentRes = value;
                if (badMentRes < 0) badMentRes = 0;
                if (badMentRes > GetMaxStatValue(UnitStat.BMR)) badMentRes = GetMaxStatValue(UnitStat.BMR);
                OnBMRChanged?.Invoke(badMentRes);
                OnNonMeteredStatChanged?.Invoke();
                OnStatChanged?.Invoke();
            }
        }
        public event OnStatValueChangedDelegate OnBMRChanged;
        #endregion

        protected void InitializeStats()
        {
            brawn = GetMaxStatValue(UnitStat.Brawn);
            agility = GetMaxStatValue(UnitStat.Agility);
            defense = GetMaxStatValue(UnitStat.Defense);
            psych = GetMaxStatValue(UnitStat.Psych);
            focus = GetMaxStatValue(UnitStat.Focus);
            heart = GetMaxStatValue(UnitStat.Heart);

            hp = GetMaxStatValue(UnitStat.HP);
            stress = 0;
            pCrit = GetMaxStatValue(UnitStat.PCrit);
            mCrit = GetMaxStatValue(UnitStat.MCrit);
            initiative = GetMaxStatValue(UnitStat.Initiative);
            badMentRes = GetMaxStatValue(UnitStat.BMR);
        }

        public int GetMaxStatValue(UnitStat stat)
        {
            if (maxStatValues.Keys.Contains(stat)) return maxStatValues[stat];
            int returnValue;
            switch (stat)
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
                    returnValue = 10 + maxStatValues[UnitStat.Brawn] / 2 + maxStatValues[UnitStat.Defense] / 2;
                    if (!maxStatValues.ContainsKey(stat)) maxStatValues.Add(stat, returnValue);
                    return returnValue;
                case UnitStat.Stress:
                    returnValue = 5 + maxStatValues[UnitStat.Psych] + maxStatValues[UnitStat.Heart] / 2;
                    if (!maxStatValues.ContainsKey(stat)) maxStatValues.Add(stat, returnValue);
                    return returnValue;
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

        public void SetStatValue(UnitStat stat, int value)
        {
            switch (stat)
            {
                case UnitStat.Brawn:
                    Brawn = value;
                    break;
                case UnitStat.Agility:
                    Agility = value;
                    break;
                case UnitStat.Defense:
                    Defense = value;
                    break;
                case UnitStat.Psych:
                    Psych = value;
                    break;
                case UnitStat.Focus:
                    Focus = value;
                    break;
                case UnitStat.Heart:
                    Heart = value;
                    break;
                case UnitStat.HP:
                    HP = value;
                    break;
                case UnitStat.Stress:
                    Stress = value;
                    break;
                case UnitStat.Initiative:
                    Initiative = value;
                    break;
                case UnitStat.PCrit:
                    PCrit = value;
                    break;
                case UnitStat.MCrit:
                    MCrit = value;
                    break;
                case UnitStat.BMR:
                    BadMentRes = value;
                    break;
                default:
                    break;
            }
        }

        public int GetStatValue(UnitStat stat)
        {
            switch (stat)
            {
                case UnitStat.Brawn:
                    return Brawn;
                case UnitStat.Agility:
                    return Agility;
                case UnitStat.Defense:
                    return Defense;
                case UnitStat.Psych:
                    return Psych;
                case UnitStat.Focus:
                    return Focus;
                case UnitStat.Heart:
                    return Heart;
                case UnitStat.HP:
                    return HP;
                case UnitStat.Stress:
                    return Stress;
                case UnitStat.Initiative:
                    return Initiative;
                case UnitStat.PCrit:
                    return PCrit;
                case UnitStat.MCrit:
                    return MCrit;
                case UnitStat.BMR:
                    return BadMentRes;
                default:
                    return -999;
            }
        }

        public void SubscribeToStatChangeEvent(OnStatValueChangedDelegate action, UnitStat stat)
        {
            switch (stat)
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
                case UnitStat.Stress:
                    OnStressChanged += action;
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
            switch (stat)
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
                case UnitStat.Stress:
                    OnStressChanged -= action;
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

        public void EffectStatValue(UnitStat stat, int delta)
        {
            if (delta == 0) return;

            switch (stat)
            {
                case UnitStat.Brawn:
                    Brawn += delta;
                    break;
                case UnitStat.Agility:
                    Agility += delta;
                    break;
                case UnitStat.Defense:
                    Defense += delta;
                    break;
                case UnitStat.Psych:
                    Psych += delta;
                    break;
                case UnitStat.Focus:
                    Focus += delta;
                    break;
                case UnitStat.Heart:
                    Heart += delta;
                    break;

                case UnitStat.HP:
                    HP += delta;
                    Debug.LogWarning("HP was effected using EffectStatValue. Consider accessing HP directly if possible.");
                    break;
                case UnitStat.Stress:
                    Stress += delta;
                    break;
                case UnitStat.Initiative:
                    Initiative += delta;
                    break;
                case UnitStat.PCrit:
                    PCrit += delta;
                    break;
                case UnitStat.MCrit:
                    MCrit += delta;
                    break;
                case UnitStat.BMR:
                    BadMentRes += delta;
                    break;
                default:
                    Debug.LogWarning("attempt to change stat value unsuccessful (stat was not found)");
                    break;
            }
        }
    }
}