using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class ICombatManager : MonoBehaviour
{
    private MenuControls actionSelectionControls;
    private MenuControls targetSelectionControls;
    [HideInInspector] public bool isSelectingAction = false;
    [HideInInspector] public bool isSelectingTargets = false;
    [SerializeField] private CombatAction passTurn;

    private Unit currentUnit;
    private int currentUnitIndex;

    private CombatAction currentConsideredAction;
    private int consideredActionIndex;

    private Unit currentTarget;
    private int currentTargetIndex;

    private List<Unit> actionTargets;
    private List<Unit> selectedTargets;
    
    protected virtual void InitializeControls()
    {
        actionSelectionControls = new MenuControls();
        targetSelectionControls = new MenuControls();
        OnBeginActionSelection += BeginActionSelection;
    }

    public virtual void EnableControls()
    {
        actionSelectionControls.Enable();
        targetSelectionControls.Enable();
    }

    public virtual void DisableControls()
    {
        actionSelectionControls.Disable();
        targetSelectionControls.Disable();
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
            actionSelectionControls.UIInput.Info.performed += ToggleActionSelectionInfo;
            targetSelectionControls.UIInput.Navigate.performed += NavigateTargets;
            targetSelectionControls.UIInput.Select.performed += SelectTarget;
            targetSelectionControls.UIInput.Cancel.performed += CancelPreviousSelection;
            targetSelectionControls.UIInput.Info.performed += ToggleTargetSelectionInfo;

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
        else
        {
            //display ui
            //unit.StartBlinking();
            StartCoroutine(CombatCamera.Instance?.MoveCamera(Vector3.MoveTowards(CombatCamera.Instance.transform.position, //TODO: Use transform of parent + default position instead
                                                                                 unit.transform.position, 
                                                                                 1f)));

            targetSelectionControls.Disable();
            actionSelectionControls.Enable();

            currentUnit = unit;
            ConsiderAction(0);
        }
    }

    private void CancelPreviousSelection(InputAction.CallbackContext context)
    {
        if(isSelectingAction && currentUnitIndex > 0) 
        {
            currentUnit.StopBlinking();
            currentUnitIndex--;
            OpenActionSelectionForUnit(allies[currentUnitIndex]);
        }
        else if(isSelectingTargets) 
        {
            currentTarget.StopBlinking();
            OpenActionSelectionForUnit(currentUnit);
        }
    }

    private void NavigateActions(InputAction.CallbackContext context)
    {
        float direction = context.ReadValue<float>();
        if (direction > 0) ConsiderAction(consideredActionIndex + 1);
        else ConsiderAction(consideredActionIndex - 1);
    }

    private void ConsiderAction(int index)
    {
        bool actionIndexIncreased = index > consideredActionIndex;
        int actionIndex = index % currentUnit.availableActions.Count; 
        if(actionIndex < 0) actionIndex = currentUnit.availableActions.Count - 1;
        consideredActionIndex = actionIndex;
        currentConsideredAction = currentUnit.availableActions[consideredActionIndex];
        OnConsiderAction?.Invoke(currentUnit, currentConsideredAction, actionIndexIncreased);
    }
    public delegate void OnConsiderActionDelegate(Unit unit, CombatAction action, bool actionIndexIncreased);
    public event OnConsiderActionDelegate OnConsiderAction;

    private void SelectAction(InputAction.CallbackContext context)
    {
        currentUnit.currentSelectedAction = currentConsideredAction;
        StartTargetSelection(currentUnit.currentSelectedAction);
    }

    private void StartActionSelectionForNextUnit()
    {
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

        //currentUnit.StopBlinking();
        StartCoroutine(CombatCamera.Instance?.ResetCameraPosition());

        if(currentSelectedAction.canTargetSelf) actionTargets.Add(currentUnit);
        if(currentSelectedAction.canTargetAllies) actionTargets.AddRange(GetActiveAlliesOfUnit(currentUnit));
        if(currentSelectedAction.canTargetEnemies) actionTargets.AddRange(GetActiveEnemiesOfUnit(currentUnit));

        if(actionTargets.Count == 0) StartActionSelectionForNextUnit();

        else
        {
            targetSelectionControls.UIInput.Enable();
            actionSelectionControls.UIInput.Disable();

            currentTargetIndex = 0;
            ConsiderTarget(actionTargets[0]);
        }

        OnBeginTargetSelection?.Invoke();
    }
    public delegate void OnBeginTargetSelectionDelegate();
    public event OnBeginTargetSelectionDelegate OnBeginTargetSelection;

    private void ConsiderTarget(Unit unit)
    {
        //Debug.Log($"Target is {unit.name}");
        OnStopConsideringTarget?.Invoke(currentTarget);
        currentTarget?.StopBlinking();
        currentTarget = unit;
        currentTarget.StartBlinking();
        OnConsiderTarget?.Invoke(currentUnit, currentTarget, currentUnit.currentSelectedAction);
    }
    public delegate void OnStopConsideringTargetDelegate(Unit target);
    public event OnStopConsideringTargetDelegate OnStopConsideringTarget;

    public delegate void OnConsiderTargetDelegate(Unit user, Unit target, CombatAction action);
    public event OnConsiderTargetDelegate OnConsiderTarget;

    private void NavigateTargets(InputAction.CallbackContext context)
    {
        float direction = context.ReadValue<float>();

        int newTarget = direction > 0 ? currentTargetIndex + 1 : currentTargetIndex - 1;
        if(newTarget == currentTargetIndex || newTarget >= actionTargets.Count || newTarget < 0) return;
        currentTargetIndex = newTarget;
        ConsiderTarget(actionTargets[currentTargetIndex]);
    }

    private void SelectTarget(InputAction.CallbackContext context)
    {
        currentUnit.selectedTargets.Add(currentTarget);
        currentTarget.StopBlinking();
        StartActionSelectionForNextUnit();
    }

    private void EndActionSelection()
    {
        Debug.Log($"Action selection complete");
        actionSelectionControls.Disable();
        targetSelectionControls.Disable();

        finishedSelectingActions = true;
    }

    private void ToggleActionSelectionInfo(InputAction.CallbackContext context)
    {
        if(isSelectingAction) OnToggleActionSelectionInfo?.Invoke();
    }
    public delegate void OnToggleInfoDelegate();
    public event OnToggleInfoDelegate OnToggleActionSelectionInfo;

    private void ToggleTargetSelectionInfo(InputAction.CallbackContext context)
    {
        if(isSelectingTargets) OnToggleTargetSelectionInfo?.Invoke();
    }
    public event OnToggleInfoDelegate OnToggleTargetSelectionInfo;
}