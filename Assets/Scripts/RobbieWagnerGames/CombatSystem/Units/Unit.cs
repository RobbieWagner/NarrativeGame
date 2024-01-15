using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobbieWagnerGames;

namespace RobbieWagnerGames.StrategyCombat
{
    [Serializable]
    public enum UnitClass
    {
        Classless = 0, 
    }

    [Serializable]
    public enum UnitStat
    {
        Strength = 0,
        Defense = 1,
        Agility = 2,
        Brain = 3,
        Care = 4,
        Magic = 5,
    }

    public class Unit : MonoBehaviour
    {
        [SerializeField] protected string unitName;
        [SerializeField] protected UnitClass unitClass;
        [SerializeField] public UnitAnimator unitAnimator;

        [SerializeField] protected int baseHP = 35;
        protected int maxHP;
        
        #region Unit Stats
        public UnitStatInfo strength;
        [SerializeField] public int baseStrength = 10;
        public UnitStatInfo defense;
        [SerializeField] public int baseDefense = 10;
        public UnitStatInfo agility;
        [SerializeField] public int baseAgility = 10;
        public UnitStatInfo brain;
        [SerializeField] public int baseBrain = 10;
        public UnitStatInfo care;
        [SerializeField] public int baseCare = 10;
        public UnitStatInfo magic;
        [SerializeField] public int baseMagic = 10;
        #endregion

        [HideInInspector] public bool isUnitFighting = true;

        [SerializeField] public List<CombatAction> unitActions;
        [HideInInspector] public CombatAction currentAction;
        [HideInInspector] public List<Unit> selectedTargets;

        [SerializeField] protected SpriteRenderer spriteRenderer;

        protected int hp;
        public int HP
        {
            get {return hp;}
            set 
            {
                if(value == hp) 
                {
                    OnHPLowered?.Invoke(0, this, Color.gray);
                    return;
                }

                int difference = Math.Abs(value - hp);
                if (value < hp) 
                {
                    OnHPLowered?.Invoke(difference * -1, this, Color.red);
                }
                else
                {
                    OnHPRaised?.Invoke(difference, this, Color.green);
                }

                hp = value;
                if(hp > maxHP) hp = maxHP;
                if(hp < 0) hp = 0;

                OnHPChanged?.Invoke(hp);
            }
        }

        public delegate void OnHPChangedDelegate(int hp);
        public event OnHPChangedDelegate OnHPChanged;

        public delegate void OnHPRaisedDelegate(int hpDifference, Unit unit, Color color);
        public event OnHPRaisedDelegate OnHPRaised;

        public delegate void OnHPLoweredDelegate(int hpDifference, Unit unit, Color color);
        public event OnHPLoweredDelegate OnHPLowered;

        [SerializeField] protected int baseMP = 10;
        protected int maxMP;

        protected int mp;
        public int MP
        {
            get {return mp;}
            set 
            {
                if(value == mp) return;    
                mp = value;
                if(mp > maxMP) mp = maxMP;
                if(mp < 0) mp = 0;

                OnMPChanged?.Invoke(mp);
            }
        }

        public delegate void OnMPChangedDelegate(int hp);
        public event OnMPChangedDelegate OnMPChanged;

        protected virtual void Awake()
        {
            if(spriteRenderer == null)
            {
                spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            }

            SetupUnit();
        }

        public virtual void SetupUnit()
        {
            strength.init(this, baseStrength);
            agility.init(this, baseAgility);
            defense.init(this, baseDefense);
            brain.init(this, baseBrain);
            care.init(this, baseCare);
            magic.init(this, baseMagic);

            InitializeMaxHP();
            InitializeMaxMP();
        }

        public void DealDamage(int amount)
        {
            if(amount > 0) HP -= amount;
        }

        public void Heal(int amount)
        {
            HP += amount;
        }

        public void ChangeStatValue(UnitStat stat, int amount)
        {
            if(stat == UnitStat.Strength) strength.StatValue += amount;
            else if(stat == UnitStat.Defense) defense.StatValue += amount;
            else if(stat == UnitStat.Agility) agility.StatValue += amount;
            else if(stat == UnitStat.Brain) brain.StatValue += amount;
            else if(stat == UnitStat.Care) care.StatValue += amount;
            else if(stat == UnitStat.Magic) magic.StatValue += amount;
        }

        public virtual IEnumerator DownUnit()
        {
            isUnitFighting = false;

            yield return null;
            if(spriteRenderer != null)
            {
                yield return spriteRenderer.DOColor(Color.clear, .3f).SetEase(Ease.Linear).WaitForCompletion();
            }

            StopCoroutine(DownUnit());
        }

        #region Action Selection
        public void SelectAction(int index)
        {
            currentAction = unitActions[index];
            OnActionSelected(this, currentAction);
        }

        public delegate void OnActionSelectedDelegate(Unit user, CombatAction action);
        public event OnActionSelectedDelegate OnActionSelected;

        public void UnselectAction()
        {
            currentAction = null;
            //CombatManager.Instance.SelectingAlly--;
        }

        public void SelectTargets(List<Unit> targets)
        {
            selectedTargets = targets;
            OnTargetSelected();
        }
        public delegate void OnTargetSelectedDelegate();
        public event OnTargetSelectedDelegate OnTargetSelected;
        #endregion

        #region action execution
        public void DodgeAttack()
        {
            OnAttackDodged("miss", this, Color.gray);
        }
        public delegate void OnAttackDodgedDelegate(string text, Unit unit, Color color);
        public event OnAttackDodgedDelegate OnAttackDodged;
        #endregion

        #region statGetters
        #region base stat getters
        public int GetDamageBoost() {return strength.StatValue;}
        public int InitializeMaxHP()
        {
            maxHP = baseHP + defense.StatValue;
            return maxHP;
        }
        public int GetMaxHP() {return maxHP;}
        public int GetInitiativeBoost() {return agility.StatValue;}
        public int GetAccuracyBoost() {return brain.StatValue;}
        public int GetBoonBoost() {return care.StatValue;}
        public int InitializeMaxMP() 
        {
            maxMP = baseMP + magic.StatValue;
            return care.StatValue;
        }
        public int GetMaxMP() {return maxMP;}

        #region derived stat getters;
        public int GetCritChance() {return (strength.StatValue + agility.StatValue)/2;}
        public int GetItemPotency() {return (defense.StatValue + magic.StatValue)/2;}
        public int GetBaneBoost() {return (brain.StatValue + care.StatValue)/2;}
        #endregion
        #endregion
        #endregion

        public string GetName()
        {
            if(unitName.Equals("^NAME^"))
            {
                List<char> allowList = new List<char>() {' ', '-', '\'', ',', '.'};
                string name = SaveDataManager.LoadString("name", "Lux");

                bool nameAllowed = true;
                foreach(char c in name)
                {
                    if(!Char.IsLetterOrDigit(c) && !allowList.Contains(c))
                    {
                        nameAllowed = false;
                        break;
                    }
                }

                if(!nameAllowed)
                {
                    name = "Lux";
                    SaveDataManager.SaveString("name", "Lux");
                }

                return name; 
            }

            return unitName;
        }
    }
}