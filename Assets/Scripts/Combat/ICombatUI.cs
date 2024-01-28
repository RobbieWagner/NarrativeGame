using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ICombatUI : MonoBehaviour
{
    [SerializeField] protected UnitUI unitUIPrefab;
    [SerializeField] protected LayoutGroup allyUnits;
    [SerializeField] protected LayoutGroup enemyUnits;

    [SerializeField] protected List<UnitUI> alliesUI;
    [SerializeField] protected List<UnitUI> enemiesUI;

    [SerializeField] protected Canvas ScreenSpaceCanvas;
    [SerializeField] protected Canvas WorldSpaceCanvas;

    public virtual IEnumerator InitializeUI()
    {
        yield return null;

        try
        {
            if(ICombatManager.Instance != null)
            {
                ICombatManager.Instance.OnAddNewAlly += AddAllyUI;
                ICombatManager.Instance.OnAddNewEnemy += AddEnemyUI;

                ICombatManager.Instance.OnStopConsideringTarget += StopConsideringTarget;
                ICombatManager.Instance.OnConsiderTarget += DisplayConsideredTarget;
            }
        }
        catch(NullReferenceException e)
        {
            Debug.LogError("Combat Manager Found Null in Combat UI, Please define a Combat Manager before calling InitializeUI.");
        }
    }

    protected virtual void AddAllyUI(Unit ally)
    {
        UnitUI newUnitUI = Instantiate(unitUIPrefab, allyUnits.transform);
        newUnitUI.Unit = ally;
        alliesUI.Add(newUnitUI);
    }

    protected virtual void AddEnemyUI(Unit enemy)
    {
        UnitUI newUnitUI = Instantiate(unitUIPrefab, enemyUnits.transform);
        newUnitUI.Unit = enemy;
        enemiesUI.Add(newUnitUI);
    }

    protected virtual void StopConsideringTarget(Unit target)
    {
        target?.StopBlinking();
    }

    protected virtual void DisplayConsideredTarget(Unit user, Unit target, CombatAction action)
    {
        target.StartBlinking(); 
    }

    protected virtual void SetupActionSelection(Unit unit)
    {

    }

    protected virtual void SetDisplayedAction(Unit unit, int action)
    {
        CombatAction curAction = unit.availableActions[action];
        CombatAction prevAction;
        CombatAction nextAction;

        if(action != 0) prevAction = unit.availableActions[action - 1];
        if(action < unit.availableActions.Count - 1) nextAction = unit.availableActions[action + 1];

        
    }
}
