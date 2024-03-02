using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Ink.Runtime;
using UnityEngine;

namespace PsychOutDestined
{
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
    public partial class CombatManagerBase : MonoBehaviour
    {
        public bool canStartNewCombat = true;
        public bool usePartyUnits = true;
        public int unitLimit = 3;
        protected CombatBase currentCombat;
        protected CombatUIBase currentUI;
        [HideInInspector] public List<Unit> allies;
        [HideInInspector] public List<Unit> enemies;
        public CombatPhase currentPhase = CombatPhase.None;
        private int currentTurn;
        public int CurrentTurn => currentTurn;

        protected bool finishedSelectingAction = true;

        List<Unit> unitsInInitiativeOrder;

        public Vector3 UNIT_OFFSET;

        [SerializeField] private CombatBase debugCombat;

        public List<Unit> AllUnitsInCombat
        {
            get { return GetAllUnits(); }
        }

        public List<Unit> ActiveUnitsInCombat
        {
            get { return GetActiveUnits(); }
        }

        public static CombatManagerBase Instance { get; private set; }

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
            InitializeControls();

            #if UNITY_EDITOR
            if(Level.Instance != null && Level.Instance.CurrentCombat != null) StartCoroutine(StartDebugCombat());
            #endif
        }

        public virtual bool StartNewCombat(CombatBase newCombat)
        {
            if (canStartNewCombat && currentPhase == CombatPhase.None && newCombat != null)
            {
                Debug.Log("combat started");
                currentCombat = newCombat;
                StartCoroutine(StartCombatPhase(CombatPhase.CombatSetup));
                return true;
            }
            return false;
        }

        public virtual void TerminateCombat()
        {
            StartCoroutine(ResolveCombat());
            currentPhase = CombatPhase.None;
            currentCombat = null;

            DisableSelectionControls();
        }

        private IEnumerator RunCombatPhases()
        {
            Debug.Log("starting combat");
            yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.CombatStarted));
            while (currentPhase != CombatPhase.CombatEnd)
            {
                if (currentPhase == CombatPhase.TurnEnd) currentTurn++;
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
                    yield return StartCoroutine(HandleActionSelectionPhase());
                    break;

                case CombatPhase.ActionExecution:
                    Debug.Log("Action Execution Phase");
                    yield return StartCoroutine(ExecuteUnitAction());
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
            if (CheckForCombatEnd()) return CombatPhase.CombatEnd;
            if (currentPhase == CombatPhase.ActionExecution && unitsInInitiativeOrder.Count > 0) return CombatPhase.ActionSelection;
            return (CombatPhase)(((int)currentPhase + 1) % 4);
        }

        #region Combat Phases
        protected virtual IEnumerator SetupCombat()
        {
            yield return new WaitForSeconds(.2f);
            currentTurn = 1;

            yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.SetupComplete));
            //TODO: start combat run somewhere else
            StartCoroutine(RunCombatPhases());
        }

        protected virtual IEnumerator StartTurn()
        {
            unitsInInitiativeOrder = new List<Unit>();
            unitsInInitiativeOrder.AddRange(allies);
            unitsInInitiativeOrder.AddRange(enemies);

            unitsInInitiativeOrder = unitsInInitiativeOrder.OrderBy(u => u.Initiative).Where(u => u.isUnitActive).ToList();

            foreach (Unit enemy in enemies) enemy.selectedAction = null;
            foreach (Unit ally in allies) ally.selectedAction = null;
            yield return new WaitForSeconds(.2f);
            yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.TurnStarted));
        }

        protected virtual IEnumerator ExecuteUnitAction()
        {
            yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.ExecutionPhaseStarted));
            Debug.Log("Executing Actions...");

            Debug.Log($"{currentActingUnit.name} is acting");
            if (currentActingUnit.isUnitActive && currentActingUnit.selectedAction != null)
            {
                List<Unit> intendedTargets = currentActingUnit.selectedAction.GetTargetUnits(currentActingUnit.selectedTargets);
                OnStartActionExecution?.Invoke(currentActingUnit, intendedTargets);
                yield return StartCoroutine(CombatCamera.Instance?.MoveCamera(Vector3.MoveTowards(CombatCamera.Instance.defaultPosition + CombatCamera.Instance.transform.parent.position,
                                                                                    currentActingUnit.transform.position,
                                                                                    1f)));
                //show UI for action
                StartCoroutine(CombatCamera.Instance?.ResetCameraPosition(.75f));
                yield return StartCoroutine(currentActingUnit.selectedAction?.ExecuteAction(currentActingUnit,intendedTargets));
                OnEndActionExecution?.Invoke(currentActingUnit, intendedTargets);
                yield return new WaitForSeconds(.25f);
            }
            else if (!currentActingUnit.isUnitActive) Debug.Log($"{currentActingUnit.name} defeated, action cancelled");
            currentActingUnit.selectedAction = null;

            //foreach(Unit unit in unitsInInitiativeOrder) Debug.Log(unit.ToString());

            unitsInInitiativeOrder.Remove(currentActingUnit);
            yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.ExecutionPhaseEnded));
        }

        public delegate void ActionExecutionDelegate(Unit user, List<Unit> targets);
        public event ActionExecutionDelegate OnStartActionExecution;
        public event ActionExecutionDelegate OnEndActionExecution;

        protected virtual IEnumerator EndTurn()
        {
            //Debug.Log("End Turn");
            yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.TurnEnded));
        }

        protected virtual IEnumerator ResolveCombat()
        {
            Debug.Log("End of Combat Reached");
            yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.CombatResolved));
            if (allies.Select(a => a.isUnitActive).Any())
                yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.CombatWon));
            else
                yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.CombatLost));

            //TODO: add end combat screen flashes, then tear down combat
            yield return new WaitForSeconds(1f);
            StartCoroutine(TerminateCombatScene());
        }
        #endregion

        protected virtual IEnumerator TerminateCombatScene()
        {
            Debug.Log("Tearing Down Combat");
            yield return StartCoroutine(InvokeCombatEventHandler(CombatEventTriggerType.CombatTerminated));
            OnCombatTerminated?.Invoke();
        }
        public delegate void OnCombatTerminatedDelegate();
        public event OnCombatTerminatedDelegate OnCombatTerminated;

        protected virtual bool TryAddAllyToCombat(Unit ally)
        {
            if (allies.Count < 3)
            {
                Unit instantiatedUnit = Instantiate(ally, transform);
                allies.Add(instantiatedUnit);
                instantiatedUnit.SetUnitAnimatorState(UnitAnimationState.CombatIdleRight);
                OnAddNewAlly?.Invoke(instantiatedUnit);
                return true;
            }
            else return false;
        }

        protected void InvokeOnAddNewAlly(Unit unit) => OnAddNewAlly?.Invoke(unit);
        public event UnitEventHandler OnAddNewAlly;

        protected virtual bool TryAddEnemyToCombat(Unit enemy)
        {
            if (enemies.Count < 3)
            {
                Unit instantiatedUnit = Instantiate(enemy, transform);
                enemies.Add(instantiatedUnit);
                instantiatedUnit.SetUnitAnimatorState(UnitAnimationState.CombatIdleLeft);
                OnAddNewEnemy?.Invoke(instantiatedUnit);
                return true;
            }
            else return false;
        }
        public event UnitEventHandler OnAddNewEnemy;
        public delegate void UnitEventHandler(Unit unit);

        protected virtual CombatAction SelectAnAction(Unit unit, List<CombatAction> actions)
        {
            if (actions != null && actions.Count > 0)
                return actions[UnityEngine.Random.Range(0, actions.Count)];
            return null;
        }

        protected virtual List<Unit> SelectTargetsForSelectedAction(Unit unit)
        {
            CombatAction action = unit.selectedAction;
            HashSet<Unit> targetOptions = new HashSet<Unit>();
            if (action.canTargetSelf) targetOptions.Add(unit);
            if ((action.canTargetAllies && allies.Contains(unit)) || (action.canTargetEnemies && enemies.Contains(unit)))
            {
                targetOptions.UnionWith(allies);
            }
            if ((action.canTargetAllies && enemies.Contains(unit)) || (action.canTargetEnemies && allies.Contains(unit)))
            {
                targetOptions.UnionWith(enemies);
            }

            if (targetOptions.Count == 0) return new List<Unit>();
            return new List<Unit>() { targetOptions.ElementAt(UnityEngine.Random.Range(0, targetOptions.Count)) };
        }

        protected IEnumerator CheckForCombatInterruption()
        {
            if (UnityEngine.Random.Range(0, 50) == 0)
            {
                Debug.Log("combat interrupted");
                isInterrupted = true;
                yield return new WaitForSeconds(.5f);
            }

            isInterrupted = false;
        }

        protected virtual bool CheckForCombatEnd()
        {
            if (currentTurn > 20 || IsAllySideDefeated() || IsEnemySideDefeated()) return true;
            return false;
        }

        protected virtual bool IsAllySideDefeated()
        {
            foreach (Unit ally in allies) if (ally.isUnitActive) return false;
            return true;
        }

        protected virtual bool IsEnemySideDefeated()
        {
            foreach (Unit enemy in enemies) if (enemy.isUnitActive) return false;
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

        protected List<Unit> GetActiveAlliesOfUnit(Unit unit)
        {
            if (enemies.Contains(unit)) return enemies.Where(x => !x.Equals(unit)).ToList();
            else return allies.Where(x => !x.Equals(unit)).ToList();
        }

        protected List<Unit> GetActiveEnemiesOfUnit(Unit unit)
        {
            if (allies.Contains(unit)) return enemies.Where(x => !x.Equals(unit) && x.isUnitActive).ToList();
            else return allies.Where(x => !x.Equals(unit) && x.isUnitActive).ToList();
        }

        protected void OnDestroy() => Instance = null;

#if UNITY_EDITOR
        public IEnumerator StartDebugCombat()
        {
            yield return null;

            //DEBUG ONLY! COMMENT OUT IF NOT USING
            if (debugCombat != null)
                StartNewCombat(debugCombat);
        }

        public IEnumerator ForceTerminateCombat()
        {
            yield return StartCoroutine(StartCombatPhase(CombatPhase.CombatEnd));
        }
#endif
    }
}