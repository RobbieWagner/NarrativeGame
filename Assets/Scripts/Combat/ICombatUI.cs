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

    [SerializeField] public Canvas ScreenSpaceCanvas;
    [SerializeField] public Canvas WorldSpaceCanvas;

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

                ICombatManager.Instance.OnConsiderAction += UpdateActionUI;
                ICombatManager.Instance.OnActionSelectionComplete += DestroyUnitActionSelectionUI;
            }
        }
        catch(NullReferenceException e)
        {
            Debug.LogError("Combat Manager Found Null in Combat UI, Please define a Combat Manager before calling InitializeUI.");
        }
    }

    private void UpdateActionUI(Unit unit, CombatAction action)
    {
        OnUpdateActionUI?.Invoke(unit, action, this);
    }
    public delegate void OnUpdateActionUIDelegate(Unit unit, CombatAction action, ICombatUI combatUI);
    public event OnUpdateActionUIDelegate OnUpdateActionUI;

    private IEnumerator DestroyUnitActionSelectionUI()
    {
        foreach(UnitUI unitUI in alliesUI)
        {
            unitUI.EndActionSelectionDisplay();
        }
        yield return null;
    }

    protected virtual void AddAllyUI(Unit ally)
    {
        UnitUI newUnitUI = Instantiate(unitUIPrefab, allyUnits.transform);
        newUnitUI.Unit = ally;
        newUnitUI.combatUI = this;
        newUnitUI.InitializeUnitUI();
        alliesUI.Add(newUnitUI);
    }

    protected virtual void AddEnemyUI(Unit enemy)
    {
        UnitUI newUnitUI = Instantiate(unitUIPrefab, enemyUnits.transform);
        newUnitUI.Unit = enemy;
        newUnitUI.combatUI = this;
        newUnitUI.InitializeUnitUI();
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
}
