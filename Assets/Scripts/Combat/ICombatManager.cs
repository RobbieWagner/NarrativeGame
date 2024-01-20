using System;
using System.Collections;
using RobbieWagnerGames.CombatSystem;
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
public class CombatManager : MonoBehaviour
{
    public bool canStartNewCombat = true;
    [HideInInspector] public ICombat currentCombat;
    public CombatPhase currentPhase = CombatPhase.None;
    private int currentTurn;
    public int CurrentTurn => currentTurn;

    private bool isInterrupted = false;
    private Coroutine currentInterruptionCoroutine;

    [SerializeField] private ICombat debugCombat;

    protected virtual void Awake()
    {
        //DEBUG ONLY! COMMENT OUT IF NOT USING
        StartNewCombat(debugCombat);
    }

    public virtual bool StartNewCombat(ICombat newCombat)
    {
        Debug.Log("attempting to start new combat");
        if(canStartNewCombat && currentPhase == CombatPhase.None && newCombat != null) 
        {
            Debug.Log("combat started");
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
        yield return StartCoroutine(StartCombatPhase(CombatPhase.CombatEnd));
    }

    protected virtual IEnumerator StartCombatPhase(CombatPhase phase)
    {
        switch (phase)
        {
            case CombatPhase.CombatSetup:
                Debug.Log("Combat Setup Phase");
                yield return StartCoroutine(SetupCombat());
                break;

            case CombatPhase.TurnStart:
                Debug.Log("Turn Start Phase");
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
                Debug.Log("Turn End Phase");
                yield return StartCoroutine(EndTurn());
                break;

            case CombatPhase.CombatEnd:
                Debug.Log("end combat phase");
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

    protected virtual bool CheckForCombatEnd()
    {
        if(currentTurn > 20) return true;
        return false;
    }

    #region Combat Phases
    protected virtual IEnumerator SetupCombat()
    {
        currentPhase = CombatPhase.CombatSetup;
        yield return new WaitForSeconds(.2f);
        Debug.Log("Combat Set up!");
        currentTurn = 1;
        StartCoroutine(RunCombatPhases());
    }

    protected virtual IEnumerator StartTurn()
    {
        Debug.Log($"Turn {currentTurn}");
        yield return new WaitForSeconds(.2f);
    }

    protected virtual IEnumerator HandleActionSelection()
    {
        Debug.Log("Handling Action Selection...");
        yield return new WaitForSeconds(.2f); 
    }

    protected virtual IEnumerator ExecuteActions()
    {
        Debug.Log("Executing Actions...");
        yield return new WaitForSeconds(.2f); 
    }

    protected virtual IEnumerator EndTurn()
    {
        Debug.Log("End Turn");
        yield return new WaitForSeconds(.2f);
    }

    protected virtual IEnumerator ResolveCombat()
    {
        Debug.Log("End of Combat Reached");
        yield return new WaitForSeconds(.2f);
    }
    #endregion

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
}