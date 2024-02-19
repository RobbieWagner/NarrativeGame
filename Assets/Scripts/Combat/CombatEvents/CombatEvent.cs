using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public class CombatEvent : EventSequence
    {
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
            if (ICombatManager.Instance != null)
            {
                ICombatManager.Instance.SubscribeEventToCombatEventHandler(this, eventTrigger);
            }
        }

        protected virtual void UnsubscribeCombatEvent()
        {
            if (ICombatManager.Instance != null)
                ICombatManager.Instance.UnsubscribeEventFromCombatEventHandler(this, eventTrigger);
        }

        public IEnumerator InvokeCombatEvent()
        {
            yield return StartCoroutine(InvokeEvent());
        }

        protected override IEnumerator InvokeEvent(bool setToEventGameMode = true)
        {
            ICombatManager.Instance?.DisableControls();
            yield return StartCoroutine(base.InvokeEvent(setToEventGameMode));
            if (triggersOnce) UnsubscribeCombatEvent();
            ICombatManager.Instance.EnableControls();
        }
    }
}