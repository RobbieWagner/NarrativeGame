using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;
using System.Linq;
using DG.Tweening;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Globalization;

namespace PsychOutDestined
{
    public enum UnitClass
    {
        None = -1,

        Other = 5
    }

    public partial class Unit : MonoBehaviour
    {
        // Unit properties
        [Header("Vanity")]
        [Tooltip("If no animator controller path is specified, UnitName/UnitName will be used instead")]
        public string UnitName;
        [Tooltip("The path of the animator controller inside the CombatAnimation Resource folder.\nInclude the file name, but not the extension")]
        public string animatorControllerPath;
        [SerializeField] protected UnitAnimator unitAnimator;
        [SerializeField] protected SpriteRenderer unitSprite;
        protected Sequence currentBlinkCo;

        protected const float BLINK_TIME = 1f;

        [Header("Runtime")]
        public CombatAction selectedAction;
        public List<Unit> selectedTargets;

        [HideInInspector] public bool isUnitActive = true;

        [HideInInspector] public int lastSelectedTurnMenuOptionIndex = 0;
        [HideInInspector] public int lastSelectedActionMenuOptionIndex = 0;
        [HideInInspector] public List<int> lastSelectedTargetIndexes;

        public Sprite headSprite; 

        protected Color BLINK_MIN_COLOR;

        protected MentalityType currentMentailityType = MentalityType.FINE;
        protected Mentality currentMentality;

        // Initialization
        protected virtual void Awake()
        {
            BLINK_MIN_COLOR = new Color(1, 1, 1, .1f);
            InitializeUnit();
        }

        protected virtual void InitializeUnit()
        {
            if (currentMentality == null)
                currentMentality = StaticGameStats.BaseMentality;

            InitializeStats();

            OnHPChanged += CheckUnitStatus;

            unitAnimator.SetAnimationState(UnitAnimationState.Idle);

            OnUnitInitialized?.Invoke();
        }

        public delegate void OnUnitInitializedDelegate();
        public event OnUnitInitializedDelegate OnUnitInitialized;

        protected void CheckUnitStatus(int newStatValue = -1)
        {
            if (HP <= 0)
            {
                isUnitActive = false;
                Debug.Log($"{name} is defeated!");
                OnDeactivateUnit?.Invoke(this);
            }
        }
        public delegate void OnDeactivateUnitDelegate(Unit unit);
        public event OnDeactivateUnitDelegate OnDeactivateUnit;

        public override string ToString()
        {
            string statDetails = string.Join("\n", unitStats.Values.Select(d => d.ToString()));
            return $"Name: {name}\nHP: {HP}\n{statDetails}";
        }

        public void SetUnitAnimatorState(UnitAnimationState state) => unitAnimator.SetAnimationState(state);

        public void StartBlinking()
        {
            if (currentBlinkCo == null || !currentBlinkCo.IsPlaying())
            {
                float halfBlinkTime = BLINK_TIME / 2;
                currentBlinkCo = DOTween.Sequence();
                currentBlinkCo.Append(unitSprite.DOColor(BLINK_MIN_COLOR, halfBlinkTime).SetEase(Ease.InCubic));
                currentBlinkCo.Append(unitSprite.DOColor(Color.white, halfBlinkTime).SetEase(Ease.OutCubic));
                currentBlinkCo.SetLoops(-1, LoopType.Restart);

                unitSprite.color = Color.white;
                currentBlinkCo.Play();
            }
        }

        public void StopBlinking()
        {
            if (currentBlinkCo != null && currentBlinkCo.IsPlaying())
            {
                currentBlinkCo.Kill(true);
                currentBlinkCo = null;
                unitSprite.color = Color.white;
            }
        }

        public void MoveUnit(Vector3 position)
        {
            transform.position = position;
            OnUnitMoved?.Invoke();
        }
        public delegate void OnUnitMovedDelegate();
        public event OnUnitMovedDelegate OnUnitMoved;

        protected void HandleOnUnitInitialized()
        {
            OnUnitInitialized?.Invoke();
        }

        public void SetLastTarget(int listIndex, int target)
        {
            if(lastSelectedTargetIndexes == null) lastSelectedTargetIndexes = new List<int>();
            if(lastSelectedTargetIndexes.Count > listIndex) lastSelectedTargetIndexes[listIndex] = target;
            else lastSelectedTargetIndexes.Add(target);
        }

        public virtual string GetAnimatorResourcePath()
        {
            return string.IsNullOrWhiteSpace(animatorControllerPath) 
                ? $"{StaticGameStats.combatAnimatorFilePath}/{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(UnitName)}/{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(UnitName)}"
                : $"{StaticGameStats.combatAnimatorFilePath}/{animatorControllerPath}";
        }

        public virtual string GetHeadSpriteResourcePath()
        {
            return $"{StaticGameStats.headSpriteFilePath}/{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(UnitName)}";
        }

        public MentalityType GetMentalityType(bool ignorePsychout = false)
        {
            return ignorePsychout || Stress < GetMaxStatValue(UnitStat.Stress) ? currentMentailityType : MentalityType.PSYCHED_OUT;
        }

        public bool SetMentality(MentalityType mentalityType, Mentality mentality)
        {
            if (GetMentalityType() != MentalityType.PSYCHED_OUT)
            {
                currentMentailityType = mentalityType;
                if(currentMentality != null && !currentMentality.RemoveMentalityEffects(this))
                {
                    Debug.LogWarning($"Failed to remove mentality {currentMentailityType} from {UnitName}");
                    return false;
                }
                currentMentality = mentality;
                if(!currentMentality.ApplyMentalityEffects(this))
                {
                    Debug.LogWarning($"Failed to apply mentality {currentMentailityType} to {UnitName}");
                    return false;
                }
                Debug.Log(ToString());

                OnMentalityChanged?.Invoke(mentality, mentalityType);
                return true;
            }

            return false;
        }
        public delegate void OnMentalityChangedDelegate(Mentality mentality, MentalityType mentalityType);
        public event OnMentalityChangedDelegate OnMentalityChanged;
    }
}