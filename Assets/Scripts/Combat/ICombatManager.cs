using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum CombatPhase
{
    None = -2,
    CombatSetup = -1,
    TurnStart = 0,
    ActionSelection = 1,
    ActionExecution = 2,
    TurnEnd = 3,
    CombatEnd = 4
}

// Base class for the combat system manager
public class ICombatManager : MonoBehaviour
{
    public bool canStartNewCombat = true;
    protected ICombat currentCombat;
    protected ICombatUI currentUI;
    protected Transform unitParent;
    [HideInInspector] public List<Unit> allies;
    [HideInInspector] public List<Unit> enemies;
    public CombatPhase currentPhase = CombatPhase.None;
    private int currentTurn;
    public int CurrentTurn => currentTurn;

    private bool isInterrupted = false;
    private Coroutine currentInterruptionCoroutine;

    [SerializeField] private ICombat debugCombat;

    public List<Unit> AllUnitsInCombat
    {
        get { return GetAllUnits(); } 
    }

    public List<Unit> ActiveUnitsInCombat
    {
        get { return GetActiveUnits();}
    }

    public static ICombatManager Instance {get; private set;}

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
        } 
        else 
        { 
            Instance = this; 
        } 
        //DEBUG ONLY! COMMENT OUT IF NOT USING
        StartNewCombat(debugCombat);
    }

    public virtual bool StartNewCombat(ICombat newCombat)
    {
        //Debug.Log("attempting to start new combat");
        if(canStartNewCombat && currentPhase == CombatPhase.None && newCombat != null) 
        {
            //Debug.Log("combat started");
            currentCombat = newCombat;
            StartCoroutine(StartCombatPhase(CombatPhase.CombatSetup));
            return true;
        }
        return false;
    }

    public virtual void EndCombat()
    {
        currentPhase = CombatPhase.None;
        currentCombat = null;
    }

    private IEnumerator RunCombatPhases()
    {
        while (currentPhase != CombatPhase.CombatEnd)
        {
            if(currentPhase == CombatPhase.TurnEnd) currentTurn++;
            currentPhase = GetNextPhase();
            yield return StartCoroutine(StartCombatPhase(currentPhase));
            yield return CheckForCombatInterruption();
        }
        yield return StartCoroutine(CheckForCombatInterruption());
    }

    protected virtual IEnumerator StartCombatPhase(CombatPhase phase)
    {
        switch (phase)
        {
            case CombatPhase.CombatSetup:
                currentPhase = CombatPhase.CombatSetup;
                //Debug.Log("Combat Setup Phase");
                yield return StartCoroutine(SetupCombat());
                break;

            case CombatPhase.TurnStart:
                //Debug.Log("Turn Start Phase");
                yield return StartCoroutine(StartTurn());
                break;

            case CombatPhase.ActionSelection:
                Debug.Log("Action Selection Phase");
                yield return StartCoroutine(HandleActionSelection());
                break;

            case CombatPhase.ActionExecution:
                Debug.Log("Action Execution Phase");
                yield return StartCoroutine(ExecuteActions());
                break;

            case CombatPhase.TurnEnd:
                //Debug.Log("Turn End Phase");
                yield return StartCoroutine(EndTurn());
                break;

            case CombatPhase.CombatEnd:
                //Debug.Log("end combat phase");
                yield return StartCoroutine(ResolveCombat());
                break;

            default:
                Debug.LogError("Unknown Combat Phase");
                break;
        }
    }

    protected virtual CombatPhase GetNextPhase()
    {
        if(CheckForCombatEnd()) return CombatPhase.CombatEnd;
        return (CombatPhase)(((int)currentPhase + 1) % 4);
    }

    #region Combat Phases
    protected virtual IEnumerator SetupCombat()
    {
        yield return new WaitForSeconds(.2f);
        //Debug.Log("Combat Set up!");
        currentTurn = 1;
        StartCoroutine(RunCombatPhases());
    }

    protected virtual IEnumerator StartTurn()
    {
        foreach(Unit enemy in enemies) enemy.currentSelectedAction = null;
        foreach(Unit ally in allies) ally.currentSelectedAction = null;
        Debug.Log($"Turn {currentTurn}");
        yield return new WaitForSeconds(.2f);
    }

    protected virtual IEnumerator HandleActionSelection()
    {
        Debug.Log("Handling Action Selection...");
        foreach(Unit enemy in enemies)
        { 
            if(enemy.isUnitActive) enemy.currentSelectedAction = SelectAnAction(enemy, enemy.availableActions);
            if(enemy.currentSelectedAction != null) enemy.selectedUnits = SelectTargetsForSelectedAction(enemy);
        }

        //TODO: Replace this with ally unit selection setup
        foreach(Unit ally in allies)
        {
            if(ally.isUnitActive) ally.currentSelectedAction = SelectAnAction(ally, ally.availableActions);
            if(ally.currentSelectedAction != null) ally.selectedUnits = SelectTargetsForSelectedAction(ally);
        }

        while(!UnitsAllHaveSelectedActions()) yield return null;

        yield return new WaitForSeconds(.2f); 
    }

    protected virtual IEnumerator ExecuteActions()
    {
        Debug.Log("Executing Actions...");

        List<Unit> unitsInInitiativeOrder = new List<Unit>();
        unitsInInitiativeOrder.AddRange(allies);
        unitsInInitiativeOrder.AddRange(enemies);

        unitsInInitiativeOrder = unitsInInitiativeOrder.OrderBy(u => u.unitStats[UnitStat.Agility]).ToList();

        foreach(Unit unit in unitsInInitiativeOrder)
        {
            //Debug.Log($"{unit.name} is acting");
            if(unit.isUnitActive && unit.currentSelectedAction != null)
                yield return StartCoroutine(unit.currentSelectedAction?.ExecuteAction(
                    unit, 
                    unit.currentSelectedAction.GetTargetUnits(unit.selectedUnits)));
            else if(!unit.isUnitActive) Debug.Log($"{unit.name} defeated, action cancelled");
            unit.currentSelectedAction = null;
        }

        //foreach(Unit unit in unitsInInitiativeOrder) Debug.Log(unit.ToString());

        yield return new WaitForSeconds(.2f); 
    }

    protected virtual IEnumerator EndTurn()
    {
        //Debug.Log("End Turn");
        yield return new WaitForSeconds(.2f);
    }

    protected virtual IEnumerator ResolveCombat()
    {
        Debug.Log("End of Combat Reached");
        yield return new WaitForSeconds(.2f);
    }
    #endregion

    protected virtual bool TryAddAllyToCombat(Unit ally)
    {
        if(allies.Count < 3)
        {
            Unit instantiatedUnit = Instantiate(ally, unitParent);
            allies.Add(instantiatedUnit);
            return true;
        }
        else return false;
    }

    protected virtual bool TryAddEnemyToCombat(Unit enemy)
    {
        if(enemies.Count < 3)
        {
            Unit instantiatedUnit = Instantiate(enemy, unitParent);
            enemies.Add(instantiatedUnit);
            return true;
        }
        else return false;
    }

    protected virtual CombatAction SelectAnAction(Unit unit, List<CombatAction> actions)
    {
        if(actions != null && actions.Count > 0)
            return actions[UnityEngine.Random.Range(0, actions.Count)];
        return null;
    }

    protected virtual List<Unit> SelectTargetsForSelectedAction(Unit unit)
    {
        CombatAction action = unit.currentSelectedAction;
        HashSet<Unit> targetOptions = new HashSet<Unit>();
        if(action.canTargetSelf) targetOptions.Add(unit);
        if((action.canTargetAllies && allies.Contains(unit)) || (action.canTargetEnemies && enemies.Contains(unit))) 
        {
            targetOptions.AddRange(allies);
        }
        if((action.canTargetAllies && enemies.Contains(unit)) || (action.canTargetEnemies && allies.Contains(unit))) 
        {
            targetOptions.AddRange(enemies);
        }

        if(targetOptions.Count == 0) return new List<Unit>();
        return new List<Unit>() { targetOptions.ElementAt(UnityEngine.Random.Range(0, targetOptions.Count)) };
    }

    private bool UnitsAllHaveSelectedActions()
    {
        foreach(Unit ally in allies) if(!ally.currentSelectedAction && ally.isUnitActive) return false;
        return true;
    }

    protected IEnumerator CheckForCombatInterruption()
    {
        if(UnityEngine.Random.Range(0, 50) == 0)
        {
            Debug.Log("combat interrupted");
            isInterrupted = true;
            yield return new WaitForSeconds(.5f);
        }

        isInterrupted = false;
    }

    protected virtual bool CheckForCombatEnd()
    {
        if(currentTurn > 20 || IsAllySideDefeated() || IsEnemySideDefeated()) return true;
        return false;
    }

    protected virtual bool IsAllySideDefeated()
    {
        foreach(Unit ally in allies) if(ally.isUnitActive) return false;
        return true;
    }

    protected virtual bool IsEnemySideDefeated()
    {
        foreach(Unit enemy in enemies) if(enemy.isUnitActive) return false;
        return true;
    }

    protected virtual List<Unit> GetAllUnits()
    {
        List<Unit> returnValue = new List<Unit>();
        returnValue.AddRange(allies);
        returnValue.AddRange(enemies);
        return returnValue;
    }

    protected virtual List<Unit> GetActiveUnits()
    {
        List<Unit> returnValue = new List<Unit>();
        returnValue.AddRange(allies.Where(u => u.isUnitActive));
        returnValue.AddRange(enemies.Where(u => u.isUnitActive));
        return returnValue;
    }
}