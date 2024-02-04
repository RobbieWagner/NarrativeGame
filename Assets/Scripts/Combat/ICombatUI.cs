using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ICombatUI : MonoBehaviour
{
    [SerializeField] protected UnitUI unitUIPrefab;
    [SerializeField] protected WorldSpaceStatbar worldSpaceStatbarPrefab;
    [SerializeField] protected LayoutGroup allyUnits;
    [SerializeField] protected LayoutGroup enemyUnits;

    [SerializeField] protected List<UnitUI> alliesUI;
    [SerializeField] protected List<WorldSpaceStatbar> enemiesUI;

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
                ICombatManager.Instance.OnActionSelectionCompleteCo += DestroyUnitActionSelectionUI;

                ICombatManager.Instance.OnEndActionSelection += DisableActionInfo;
                ICombatManager.Instance.OnEndActionSelection += DisableTargetInfo;
                ICombatManager.Instance.OnBeginTargetSelection += CheckToEnableTargetInfo;

                ICombatManager.Instance.OnToggleActionSelectionInfo += ToggleActionSelectionInfo;
                ICombatManager.Instance.OnToggleTargetSelectionInfo += ToggleTargetSelectionInfo;
            }
        }
        catch(NullReferenceException e)
        {
            Debug.LogError("Combat Manager Found Null in Combat UI, Please define a Combat Manager before calling InitializeUI.");
        }
    }

    private void UpdateActionUI(Unit unit, CombatAction action, bool actionIndexIncreased)
    {
        OnUpdateActionUI?.Invoke(unit, action, this, actionIndexIncreased);
    }
    public delegate void OnUpdateActionUIDelegate(Unit unit, CombatAction action, ICombatUI combatUI, bool actionIndexIncreased);
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
        WorldSpaceStatbar newStatbar = Instantiate(worldSpaceStatbarPrefab, WorldSpaceCanvas.transform);
        newStatbar.Initialize(enemy, enemy.GetMaxStatValue(UnitStat.HP), enemy.HP, UnitStat.HP);
        newStatbar.transform.position = enemy.transform.position;
        enemiesUI.Add(newStatbar);

        // UnitUI newUnitUI = Instantiate(unitUIPrefab, enemyUnits.transform);
        // newUnitUI.Unit = enemy;
        // newUnitUI.combatUI = this;
        // newUnitUI.InitializeUnitUI();
        // enemiesUI.Add(newUnitUI);
    }

    protected virtual void StopConsideringTarget(Unit target)
    {
        target?.StopBlinking();
    }

    protected virtual void DisplayConsideredTarget(Unit user, Unit target, CombatAction action)
    {
        target.StartBlinking(); 
    }

    protected virtual void ToggleActionSelectionInfo()
    {
        Debug.Log("toggled");
        bool enable = (ICombatManager.Instance.isSelectingAction || ICombatManager.Instance.isSelectingTargets) && alliesUI.Count > 0 && !alliesUI.FirstOrDefault().statTextParent.enabled;
        SetActionInfoActiveState(enable);
    }

    protected virtual void ToggleTargetSelectionInfo()
    {
        
    }

    public virtual void SetActionInfoActiveState(bool enabled)
    {
        foreach(UnitUI unitUI in alliesUI)
        {
            if(enabled) unitUI.EnableStatUI();
            else unitUI.DisableStatUI();
        }
    }
    public virtual void DisableActionInfo() => SetActionInfoActiveState(false);
    public virtual void EnableActionInfo() => SetActionInfoActiveState(true);

    public virtual void SetTargetInfoActiveState(bool enabled)
    {

    }
    public virtual void DisableTargetInfo() => SetTargetInfoActiveState(false);
    public virtual void EnableTargetInfo() => SetTargetInfoActiveState(true);

    private void CheckToEnableTargetInfo()
    {
        if(alliesUI.Count > 0 && alliesUI.FirstOrDefault().statTextParent.gameObject.activeSelf)
            EnableTargetInfo();
        else
            DisableTargetInfo();

    }
}
