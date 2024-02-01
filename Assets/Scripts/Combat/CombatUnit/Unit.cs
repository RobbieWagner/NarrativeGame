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

public partial class Unit : MonoBehaviour
{
    // Unit properties
    [Header("Vanity")]
    public string UnitName;
    [SerializeField] private UnitAnimator unitAnimator;
    [SerializeField] private SpriteRenderer unitSprite;
    private Coroutine parentBlinkCo;
    private Sequence currentBlinkCo;

    private const float BLINK_TIME = 1f;

    [Header("Runtime")]
    public CombatAction currentSelectedAction;
    public List<Unit> selectedTargets;

    [HideInInspector] public bool isUnitActive = true;

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

        unitStats = new Dictionary<UnitStat, int>();
        InitializeStats();
        

        unitAnimator.SetAnimationState(UnitAnimationState.Idle);
        
        OnUnitInitialized?.Invoke();
    }

    public delegate void OnUnitInitializedDelegate();
    public event OnUnitInitializedDelegate OnUnitInitialized;

    private void CheckUnitStatus(int newStatValue = -1)
    {
        if(unitStats[UnitStat.HP] <= 0) 
        {
            isUnitActive = false;
            Debug.Log($"{name} is defeated!");
        }
    }

    public override string ToString()
    {
        return $"Name: {name}\nHP: {unitStats[UnitStat.HP]}";
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
}