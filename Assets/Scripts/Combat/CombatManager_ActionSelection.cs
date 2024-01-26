using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class CombatManager : ICombatManager
{
    private MenuControls actionSelectionControls;
    private MenuControls targetSelectionControls;
    protected bool isSelectingAction = false;
    protected bool isSelectingTargets = false;
    [SerializeField] private CombatAction passTurn;

    private Unit currentUnit;
    private int currentUnitIndex;

    private CombatAction currentConsideredAction;
    private int consideredActionIndex;

    private Unit currentTarget;
    private int currentTargetIndex;

    private List<Unit> actionTargets;
    private List<Unit> selectedTargets;
    
    protected override void Awake()
    {
        Debug.Log("awake");
        base.Awake();
        actionSelectionControls = new MenuControls();
        targetSelectionControls = new MenuControls();
        OnBeginActionSelection += BeginActionSelection;
    }

    private void BeginActionSelection()
    {
        Debug.Log("Action Selection Begun");
        finishedSelectingActions = false;
        actionSelectionControls.Enable();
        currentUnitIndex = 0;
        if(allies?.Count > 0) 
        {
            actionSelectionControls.UIInput.Navigate.performed += NavigateActions;
            actionSelectionControls.UIInput.Select.performed += SelectAction;
            actionSelectionControls.UIInput.Cancel.performed += CancelPreviousSelection;
            targetSelectionControls.UIInput.Navigate.performed += NavigateTargets;
            targetSelectionControls.UIInput.Select.performed += SelectTarget;
            targetSelectionControls.UIInput.Cancel.performed += CancelPreviousSelection;

            OpenActionSelectionForUnit(allies[currentUnitIndex]);
        }
        else EndActionSelection();
    }

    private void OpenActionSelectionForUnit(Unit unit)
    {
        Debug.Log($"Action Selection Begun for unit: {unit.name}");
        isSelectingAction = true;
        isSelectingTargets = false;
        
        if(unit.availableActions.Count < 1)
        {
            unit.currentSelectedAction = passTurn;
            StartActionSelectionForNextUnit();
        }

        targetSelectionControls.Disable();
        actionSelectionControls.Enable();

        currentUnit = unit;
        ConsiderAction(0);
        //open UI
    }

    private void CancelPreviousSelection(InputAction.CallbackContext context)
    {
        Debug.Log($"Going back to last action");
        if(isSelectingAction && currentUnitIndex > 0) OpenActionSelectionForUnit(allies[currentUnitIndex - 1]);
        else if(isSelectingTargets) OpenActionSelectionForUnit(currentUnit);
    }

    private void NavigateActions(InputAction.CallbackContext context)
    {
        Debug.Log($"Navigating Actions");
        float direction = context.ReadValue<float>();
        if (direction > 0) ConsiderAction(consideredActionIndex + 1);
        else ConsiderAction(consideredActionIndex - 1);
    }

    private void ConsiderAction(int index)
    {
        Debug.Log($"considering action {index}");
        int actionIndex = index % currentUnit.availableActions.Count; 
        if(actionIndex < 0) actionIndex = currentUnit.availableActions.Count - 1;
        consideredActionIndex = actionIndex;
        currentConsideredAction = currentUnit.availableActions[consideredActionIndex];
        OnConsiderAction?.Invoke(currentUnit, currentConsideredAction);
    }
    public delegate void OnConsiderActionDelegate(Unit unit, CombatAction action);
    public event OnConsiderActionDelegate OnConsiderAction;

    private void SelectAction(InputAction.CallbackContext context)
    {
        Debug.Log($"selecting action");
        currentUnit.currentSelectedAction = currentConsideredAction;
        StartTargetSelection(currentUnit.currentSelectedAction);
    }

    private void StartActionSelectionForNextUnit()
    {
        Debug.Log($"start action selection for the next unit");
        currentUnitIndex++;
        if(currentUnitIndex >= allies.Count)
            EndActionSelection();
        else
            OpenActionSelectionForUnit(allies[currentUnitIndex]);
    }

    private void StartTargetSelection(CombatAction currentSelectedAction)
    {
        Debug.Log($"start target selection for {currentSelectedAction.name}");
        actionTargets = new List<Unit>();
        currentUnit.selectedTargets = new List<Unit>();
        isSelectingAction = false;
        isSelectingTargets = true;

        if(currentSelectedAction.canTargetSelf) actionTargets.Add(currentUnit);
        if(currentSelectedAction.canTargetAllies) actionTargets.AddRange(GetActiveAlliesOfUnit(currentUnit));
        if(currentSelectedAction.canTargetEnemies) actionTargets.AddRange(GetActiveEnemiesOfUnit(currentUnit));

        if(actionTargets.Count == 0) StartActionSelectionForNextUnit();
        else if(actionTargets.Count == 1)
        {
            currentUnit.selectedTargets.AddRange(actionTargets);
            StartActionSelectionForNextUnit();
        }

        else
        {
            targetSelectionControls.UIInput.Enable();
            actionSelectionControls.UIInput.Disable();

            DisplayTarget(0);
        }
    }

    private void DisplayTarget(int unit)
    {
        Debug.Log($"Target is {unit}");
        currentTargetIndex = unit % actionTargets.Count;
        if(currentTargetIndex < 0) currentTargetIndex = actionTargets.Count - 1;
        currentTarget = actionTargets[currentTargetIndex]; 
    }

    private void NavigateTargets(InputAction.CallbackContext context)
    {
        Debug.Log($"navigating targets");
        float direction = context.ReadValue<float>();

        if(direction > 0) DisplayTarget(currentTargetIndex + 1);
        else DisplayTarget(currentTargetIndex - 1);
    }

    private void SelectTarget(InputAction.CallbackContext context)
    {
        Debug.Log($"Selecting Target");
        currentUnit.selectedTargets.Add(currentTarget);
        StartActionSelectionForNextUnit();
    }

    private void EndActionSelection()
    {
        Debug.Log($"Action selection complete");
        actionSelectionControls.Disable();

        finishedSelectingActions = true;
    }
}