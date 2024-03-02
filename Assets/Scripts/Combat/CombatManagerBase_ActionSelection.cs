using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public partial class CombatManagerBase : MonoBehaviour
    {
        [SerializeField] private TurnMenu turnMenu;
        private MenuControls selectionControls;
        [HideInInspector] public bool isSelectingAction = false;
        [HideInInspector] public bool isSelectingTargets = false;
        public CombatAction passTurn;

        private Unit currentActingUnit;
        private int currentUnitIndex;

        private Unit currentTarget;
        private int currentTargetIndex;

        private List<Unit> actionTargets;
        private List<Unit> selectedTargets; //TODO: Allow for multi target selection

        protected virtual void InitializeControls()
        {
            selectionControls = new MenuControls();
        }

        public virtual void EnableSelectionControls() => InputManager.Instance.RegisterActionCollection(selectionControls);

        public virtual void DisableSelectionControls() => InputManager.Instance.DeregisterActionCollection(selectionControls);

        protected virtual IEnumerator HandleActionSelectionPhase()
        {
            yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.SelectionPhaseStarted));
            Debug.Log("Handling Action Selection...");
            OnBeginActionSelection?.Invoke();

            currentActingUnit = unitsInInitiativeOrder[0];
            if(currentActingUnit.isUnitActive)
            {
                if(enemies.Contains(currentActingUnit))
                {
                    currentActingUnit.selectedAction = SelectAnAction(currentActingUnit, currentActingUnit.availableActions);
                    if (currentActingUnit.selectedAction != null) currentActingUnit.selectedTargets = SelectTargetsForSelectedAction(currentActingUnit);
                    else currentActingUnit.selectedAction = passTurn;
                    finishedSelectingAction = true;
                }
                else StartActionSelection();

                while (!finishedSelectingAction) yield return null;

                OnEndActionSelection?.Invoke();
                yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.SelectionPhaseEnded));
            }
        }
        public delegate void OnToggleActionSelectionStateDelegate();
        public event OnToggleActionSelectionStateDelegate OnBeginActionSelection;
        public event OnToggleActionSelectionStateDelegate OnEndActionSelection;

        protected void BeginActionSelection()
        {
            Debug.Log("Action Selection Begun");
            finishedSelectingAction = false;

            selectionControls.UIInput.Navigate.performed += NavigateTargets;
            selectionControls.UIInput.Select.performed += SelectTarget;
            selectionControls.UIInput.Cancel.performed += CancelPreviousSelection;
            selectionControls.UIInput.Info.performed += ToggleTargetSelectionInfo;
        }

        protected void StartActionSelection(bool loadLastSelection = false)
        {
            BeginActionSelection();
            Debug.Log($"Action Selection Begun for unit: {currentActingUnit.name}");
            isSelectingAction = true;
            isSelectingTargets = false;

            if (currentActingUnit.availableActions.Count == 0)
            {
                currentActingUnit.selectedAction = passTurn;
                EndActionSelection();
            }
            else
            {
                StartCoroutine(CombatCamera.Instance?.MoveCamera(Vector3.MoveTowards(CombatCamera.Instance.defaultPosition + CombatCamera.Instance.transform.parent.position,
                                                                                     currentActingUnit.transform.position,
                                                                                     1f)));

                selectionControls.Disable();

                if(loadLastSelection)
                    OnReturnToUnitsActionSelectionMenu(currentActingUnit);
                else
                    OnStartActionSelectionForUnit?.Invoke(currentActingUnit);
                    
            }
        }
        public delegate void OnStartActionSelectionForUnitDelegate(Unit unit);
        public event OnStartActionSelectionForUnitDelegate OnStartActionSelectionForUnit;
        public delegate void OnReturnToUnitsActionSelectionMenuDelegate(Unit unit);
        public event OnReturnToUnitsActionSelectionMenuDelegate OnReturnToUnitsActionSelectionMenu;

        protected void CancelPreviousSelection(InputAction.CallbackContext context)
        {
            if (isSelectingTargets)
            {
                currentTarget.StopBlinking();
                StartActionSelection(true);
            }
        }

        public void MakeActionSelectionForCurrentUnit(CombatAction action)
        {
            currentActingUnit.selectedAction = action;
            StartTargetSelection(currentActingUnit.selectedAction);
        }

        protected void StartTargetSelection(CombatAction selectedAction)
        {
            Debug.Log($"start target selection for {selectedAction.name}");
            actionTargets = new List<Unit>();
            currentActingUnit.selectedTargets = new List<Unit>();
            isSelectingAction = false;
            isSelectingTargets = true;

            StartCoroutine(CombatCamera.Instance?.ResetCameraPosition());

            if (selectedAction.canTargetSelf) actionTargets.Add(currentActingUnit);
            if (selectedAction.canTargetAllies) actionTargets.AddRange(GetActiveAlliesOfUnit(currentActingUnit));
            if (selectedAction.canTargetEnemies) actionTargets.AddRange(GetActiveEnemiesOfUnit(currentActingUnit));

            if (actionTargets.Count == 0) EndActionSelection();

            else
            {
                InputManager.Instance.RegisterActionCollection(selectionControls);
                
                if(currentActingUnit.lastSelectedTargetIndexes.Any())
                    currentTargetIndex = currentActingUnit.lastSelectedTargetIndexes[0] % Math.Clamp(actionTargets.Count, 1, int.MaxValue); 
                else
                    currentTargetIndex = 0;

                ConsiderTarget(actionTargets[currentTargetIndex]);
            }

            OnBeginTargetSelection?.Invoke();
        }
        public delegate void OnBeginTargetSelectionDelegate();
        public event OnBeginTargetSelectionDelegate OnBeginTargetSelection;

        protected void ConsiderTarget(Unit unit)
        {
            //Debug.Log($"Target is {unit.name}");
            OnStopConsideringTarget?.Invoke(currentTarget);
            currentTarget?.StopBlinking();
            currentTarget = unit;
            currentTarget.StartBlinking();
            OnConsiderTarget?.Invoke(currentActingUnit, currentTarget, currentActingUnit.selectedAction);
        }
        public delegate void OnStopConsideringTargetDelegate(Unit target);
        public event OnStopConsideringTargetDelegate OnStopConsideringTarget;

        public delegate void OnConsiderTargetDelegate(Unit user, Unit target, CombatAction action);
        public event OnConsiderTargetDelegate OnConsiderTarget;

        protected void NavigateTargets(InputAction.CallbackContext context)
        {
            float direction = context.ReadValue<float>();

            int newTarget = direction > 0 ? currentTargetIndex + 1 : currentTargetIndex - 1;
            if (newTarget == currentTargetIndex || newTarget >= actionTargets.Count || newTarget < 0) return;
            currentTargetIndex = newTarget;
            ConsiderTarget(actionTargets[currentTargetIndex]);
        }

        protected void SelectTarget(InputAction.CallbackContext context)
        {
            currentActingUnit.selectedTargets.Add(currentTarget);
            currentTarget.StopBlinking();
            currentActingUnit.SetLastTarget(0, currentTargetIndex);
            EndActionSelection();
        }

        protected void EndActionSelection()
        {
            Debug.Log($"Action selection complete");
            //InputManager.Instance.DeregisterActionMap(selectionControls.UIInput);
            DisableSelectionControls();

            finishedSelectingAction = true;
        }

        protected void ToggleActionSelectionInfo(InputAction.CallbackContext context)
        {
            if (isSelectingAction) OnToggleActionSelectionInfo?.Invoke();
        }
        public delegate void OnToggleInfoDelegate();
        public event OnToggleInfoDelegate OnToggleActionSelectionInfo;

        protected void ToggleTargetSelectionInfo(InputAction.CallbackContext context)
        {
            if (isSelectingTargets) OnToggleTargetSelectionInfo?.Invoke();
        }
        public event OnToggleInfoDelegate OnToggleTargetSelectionInfo;
    }
}