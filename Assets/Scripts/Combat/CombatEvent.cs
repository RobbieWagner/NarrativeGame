using System.Collections;
using UnityEngine;

public enum CombatEventTriggerType
{
    None = -1,
    SetupComplete = 0,
    CombatStarted = 1,
    TurnStarted = 2,
    SelectionPhaseStarted = 3,
    SelectionPhaseEnded = 4,
    ExecutionPhaseStarted = 5,
    ExecutionPhaseEnded = 6,
    TurnEnded = 7,
    CombatResolved = 8,
    CombatWon = 9,
    CombatLost = 10,
    CombatTerminated = 11,
}

public class CombatEvent : MonoBehaviour
{
    [SerializeField] private CombatEventTriggerType eventTrigger;
    [SerializeField] private bool triggersOnce = true;

    protected virtual void Awake()
    {
        SubscribeCombatEvent();
    }

    protected virtual void SubscribeCombatEvent()
    {
        if(ICombatManager.Instance != null)
        {
            switch (eventTrigger)
            {
                case CombatEventTriggerType.SetupComplete:
                ICombatManager.Instance.OnCombatSetupComplete += InvokeEvent;
                break;
                case CombatEventTriggerType.CombatStarted:
                ICombatManager.Instance.OnCombatStarted += InvokeEvent;
                break;
                case CombatEventTriggerType.TurnStarted:
                ICombatManager.Instance.OnCombatTurnStarted += InvokeEvent;
                break;
                case CombatEventTriggerType.SelectionPhaseStarted:
                ICombatManager.Instance.OnActionSelectionStarted += InvokeEvent;
                break;
                case CombatEventTriggerType.SelectionPhaseEnded:
                ICombatManager.Instance.OnActionSelectionComplete += InvokeEvent;
                break;
                case CombatEventTriggerType.ExecutionPhaseStarted:
                ICombatManager.Instance.OnActionExecutionStarted += InvokeEvent;
                break;
                case CombatEventTriggerType.ExecutionPhaseEnded:
                ICombatManager.Instance.OnActionExecutionComplete += InvokeEvent;
                break;
                case CombatEventTriggerType.TurnEnded:
                ICombatManager.Instance.OnTurnEnded += InvokeEvent;
                break;
                case CombatEventTriggerType.CombatResolved:
                ICombatManager.Instance.OnCombatResolved += InvokeEvent;
                break;
                case CombatEventTriggerType.CombatWon:
                ICombatManager.Instance.OnCombatWon += InvokeEvent;
                break;
                case CombatEventTriggerType.CombatLost:
                ICombatManager.Instance.OnCombatLost += InvokeEvent;
                break;
                case CombatEventTriggerType.CombatTerminated:
                ICombatManager.Instance.OnCombatTerminated += InvokeEvent;
                break;
                default:
                break;
            }
        }
    }

    protected virtual void UnsubscribeCombatEvent()
    {
        if(ICombatManager.Instance != null)
        {
            switch (eventTrigger)
            {
                case CombatEventTriggerType.SetupComplete:
                ICombatManager.Instance.OnCombatSetupComplete -= InvokeEvent;
                break;
                case CombatEventTriggerType.CombatStarted:
                ICombatManager.Instance.OnCombatStarted -= InvokeEvent;
                break;
                case CombatEventTriggerType.TurnStarted:
                ICombatManager.Instance.OnCombatTurnStarted -= InvokeEvent;
                break;
                case CombatEventTriggerType.SelectionPhaseStarted:
                ICombatManager.Instance.OnActionSelectionStarted -= InvokeEvent;
                break;
                case CombatEventTriggerType.SelectionPhaseEnded:
                ICombatManager.Instance.OnActionSelectionComplete -= InvokeEvent;
                break;
                case CombatEventTriggerType.ExecutionPhaseStarted:
                ICombatManager.Instance.OnActionExecutionStarted -= InvokeEvent;
                break;
                case CombatEventTriggerType.ExecutionPhaseEnded:
                ICombatManager.Instance.OnActionExecutionComplete -= InvokeEvent;
                break;
                case CombatEventTriggerType.TurnEnded:
                ICombatManager.Instance.OnTurnEnded -= InvokeEvent;
                break;
                case CombatEventTriggerType.CombatResolved:
                ICombatManager.Instance.OnCombatResolved -= InvokeEvent;
                break;
                case CombatEventTriggerType.CombatWon:
                ICombatManager.Instance.OnCombatWon -= InvokeEvent;
                break;
                case CombatEventTriggerType.CombatLost:
                ICombatManager.Instance.OnCombatLost -= InvokeEvent;
                break;
                case CombatEventTriggerType.CombatTerminated:
                ICombatManager.Instance.OnCombatTerminated -= InvokeEvent;
                break;
                default:
                break;
            }
        }
    }

    protected virtual IEnumerator InvokeEvent()
    {
        Debug.Log("event invoked");
        if(triggersOnce) UnsubscribeCombatEvent();
        yield return null;
    }
}