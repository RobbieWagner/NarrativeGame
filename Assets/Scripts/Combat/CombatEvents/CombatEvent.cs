using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEvent : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] protected List<SequenceEvent> eventSequence;
    [Space(15)]
    [Header("Settings")]
    public int priority = -1;
    [SerializeField] private CombatEventTriggerType eventTrigger;
    [SerializeField] private bool triggersOnce = true;

    protected virtual void Awake()
    {
        SubscribeCombatEvent();
    }

    public virtual void SubscribeCombatEvent()
    {
        if(ICombatManager.Instance != null)
        {
            ICombatManager.Instance.SubscribeEventToCombatEventHandler(this, eventTrigger);
        }
    }

    protected virtual void UnsubscribeCombatEvent()
    {
        if(ICombatManager.Instance != null)
            ICombatManager.Instance.UnsubscribeEventFromCombatEventHandler(this, eventTrigger);
    }

    public virtual IEnumerator InvokeEvent()
    {
        //Debug.Log("event invoked");
        ICombatManager.Instance?.DisableControls();
        foreach(SequenceEvent e in eventSequence) 
            yield return StartCoroutine(e.InvokeSequenceEvent());

        if(triggersOnce) UnsubscribeCombatEvent();

    }
}