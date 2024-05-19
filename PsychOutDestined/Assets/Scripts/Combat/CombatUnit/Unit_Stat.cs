using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Ink.Parsed;
using UnityEngine;

namespace PsychOutDestined
{
    public partial class Unit : MonoBehaviour
    {
        [Header("Statistics")]
        public UnitClass Class;
        [Space(10)]
        private Dictionary<UnitStat, UnitStatDetails> unitStats;
        [SerializeField][SerializedDictionary("Stat", "Base Value")] protected SerializedDictionary<UnitStat, int> maxStatValues;
        [Space(10)]
        public List<CombatAction> availableActions;

        public delegate void OnStatValueChangedDelegate(int value);

        public delegate void OnUnitStatChangedDelegate();
        public event OnUnitStatChangedDelegate OnStatChanged;

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

       

        protected void InitializeStats()
        {
            unitStats = new Dictionary<UnitStat, UnitStatDetails>
            {
                {UnitStat.Brawn, new UnitStatDetails(UnitStat.Brawn, GetMaxStatValue(UnitStat.Brawn))},
                {UnitStat.Agility, new UnitStatDetails(UnitStat.Agility, GetMaxStatValue(UnitStat.Agility))},
                {UnitStat.Defense, new UnitStatDetails(UnitStat.Defense, GetMaxStatValue(UnitStat.Defense))},
                {UnitStat.Psych, new UnitStatDetails(UnitStat.Psych, GetMaxStatValue(UnitStat.Psych))},
                {UnitStat.Focus, new UnitStatDetails(UnitStat.Focus, GetMaxStatValue(UnitStat.Focus))},
                {UnitStat.Heart, new UnitStatDetails(UnitStat.Heart, GetMaxStatValue(UnitStat.Heart))},

                {UnitStat.PCrit, new UnitStatDetails(UnitStat.PCrit, GetMaxStatValue(UnitStat.PCrit))},
                {UnitStat.MCrit, new UnitStatDetails(UnitStat.MCrit, GetMaxStatValue(UnitStat.MCrit))},
                {UnitStat.Initiative, new UnitStatDetails(UnitStat.Initiative, GetMaxStatValue(UnitStat.Initiative))},
                {UnitStat.Resistance, new UnitStatDetails(UnitStat.Resistance, GetMaxStatValue(UnitStat.Resistance))}
            };

            hp = GetMaxStatValue(UnitStat.HP);
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
                    returnValue = 100;//5 + maxStatValues[UnitStat.Psych] + maxStatValues[UnitStat.Heart] / 2;
                    if (!maxStatValues.ContainsKey(stat)) maxStatValues.Add(stat, returnValue);
                    return returnValue;
                case UnitStat.Initiative:
                    return Random.Range(1, 7) + maxStatValues[UnitStat.Agility];
                case UnitStat.PCrit:
                    return maxStatValues[UnitStat.Brawn] / 2 + maxStatValues[UnitStat.Agility];
                case UnitStat.MCrit:
                    return maxStatValues[UnitStat.Psych] / 2 + maxStatValues[UnitStat.Focus];
                case UnitStat.Resistance:
                    return maxStatValues[UnitStat.Defense] + maxStatValues[UnitStat.Focus] + maxStatValues[UnitStat.Heart];
            }
            return -1;
        }

        public bool ModifyStatValue(UnitStat stat, int raise_reduction)
        {
            if (unitStats.ContainsKey(stat))
            {
                unitStats[stat].RaiseReduction += raise_reduction;
                return true;
            }
            if (stat == UnitStat.HP)
            {
                HP += raise_reduction;
                return true;
            }
            if (stat == UnitStat.Stress)
            {
                Stress += raise_reduction;
                return true;
            }

            Debug.LogWarning($"Could not set stat {stat}: Stat not modifyable, or not found.");
            return false;
        }

        public bool ModifyStatValue(UnitStat stat, float multiplier, bool replacesMultiplier = false)
        {
            if (unitStats.ContainsKey(stat))
            {
                if(replacesMultiplier)
                    unitStats[stat].Multiplier = multiplier;
                else
                    unitStats[stat].Multiplier *= multiplier;
            }

            Debug.LogWarning($"Could not set stat {stat}: Stat not modifyable, or not found.");
            return false;
        }

        public int GetStatValue(UnitStat stat)
        {            
            if (stat == UnitStat.HP)
                return HP;
            if (stat == UnitStat.Stress)
                return Stress;
            UnitStatDetails statDetails;
            if (unitStats.TryGetValue(stat, out statDetails))
                return statDetails.StatValue;

            Debug.LogWarning($"Could not get stat {stat}: stat not found.");
            return -1;
        }

        public bool SubscribeToStatChangeEvent(OnStatValueChangedDelegate action, UnitStat stat)
        {
            if (stat == UnitStat.HP)
            {
                OnHPChanged += action;
                return true;
            }
            if (stat == UnitStat.Stress)
            {
                OnStressChanged += action;
                return true;
            }
            UnitStatDetails statDetails;
            if (unitStats.TryGetValue(stat, out statDetails))
            {
                UnitStatDetails.OnStatChangedDelegate statChangeAction = new UnitStatDetails.OnStatChangedDelegate(action);
                statDetails.OnStatChanged += statChangeAction;
                return true;
            }

            Debug.LogWarning($"attempt to subscribe to stat change failed (stat {stat} was not found)");
            return false;
        }

        public bool UnsubscribeToStatChangeEvent(OnStatValueChangedDelegate action, UnitStat stat)
        {
            if (stat == UnitStat.HP)
            {
                OnHPChanged -= action;
                return true;
            }
            if (stat == UnitStat.Stress)
            {
                OnStressChanged -= action;
                return true;
            }
            UnitStatDetails statDetails;
            if (unitStats.TryGetValue(stat, out statDetails))
            {
                UnitStatDetails.OnStatChangedDelegate statChangeAction = new UnitStatDetails.OnStatChangedDelegate(action);
                statDetails.OnStatChanged -= statChangeAction;
                return true;
            }

            Debug.LogWarning($"attempt to unsubscribe from stat change failed (stat {stat} was not found)");
            return false;
        }
    }
}