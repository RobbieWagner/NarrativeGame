using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AYellowpaper.SerializedCollections;

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

public partial class ICombatManager : MonoBehaviour
{
    [Space(10)]
    [Header("Combat Events")]
    [SerializeField][SerializedDictionary("Trigger","Handler")] private SerializedDictionary<CombatEventTriggerType, CombatEventHandler> combatEventHandlers;
    [Space(10)]

    private bool isInterrupted = false;
    private Coroutine currentInterruptionCoroutine;
    public delegate IEnumerator CombatCoroutineEventHandler();

    public void SubscribeEventToCombatEventHandler(CombatEvent combatEvent, CombatEventTriggerType triggerType)
    {
        Debug.Log("attempt to subscribe");
        if(combatEventHandlers.Keys.Contains(triggerType))
            combatEventHandlers[triggerType].Subscribe(combatEvent, combatEvent.priority);
        else Debug.LogWarning($"Trigger type {triggerType} not found, please ensure that trigger type is valid for combat event");
    }

    public void UnsubscribeEventFromCombatEventHandler(CombatEvent combatEvent, CombatEventTriggerType triggerType)
    {
        if(combatEventHandlers.Keys.Contains(triggerType))
            combatEventHandlers[triggerType].Unsubscribe(combatEvent);
        else Debug.LogWarning($"Trigger type {triggerType} not found, please ensure that trigger type is valid for combat event");
    }

    public IEnumerator InvokeCombatEventHandler(CombatEventTriggerType triggerType)
    {
        if(combatEventHandlers.Keys.Contains(triggerType))
            yield return StartCoroutine(combatEventHandlers[triggerType].Invoke());
        else Debug.LogWarning($"Trigger type {triggerType} not found, please ensure that trigger type is valid for combat event");
    }

    protected virtual IEnumerator InvokeCombatEvent(CombatCoroutineEventHandler handler, bool yield = true)
    {
        if(handler != null)
        {
            if(yield) foreach(CombatCoroutineEventHandler invocation in handler?.GetInvocationList()) yield return StartCoroutine(invocation?.Invoke());
            else foreach(CombatCoroutineEventHandler invocation in handler?.GetInvocationList()) StartCoroutine(invocation?.Invoke());
        }
    }
}