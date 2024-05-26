using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobbieWagnerGames;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Security.Cryptography;

namespace PsychOutDestined
{
    public enum EventTrigger
    {
        NONE = -1,
        TRIGGER_COLLISION,
        COLLISION
    }

    public class ExplorationEvent : EventSequence
    {
        [Header("General")]
        [SerializeField] protected string triggerSaveDataName;
        [SerializeField] protected bool triggersOnce = true;
        [SerializeField] protected List<EventTrigger> triggers;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (triggers.Contains(EventTrigger.TRIGGER_COLLISION) && other.gameObject.CompareTag("Player"))
                StartCoroutine(InvokeEvent());
        }

        public override IEnumerator InvokeEvent(bool setToEventGameMode = true)
        {
            if (!GameSession.Instance.explorationData.GetSavedExplorationEventData(triggerSaveDataName, out var data) || (!data.triggered || !triggersOnce))
            {
                GameSession.Instance.SetEventTriggered(triggerSaveDataName, triggersOnce);
                yield return StartCoroutine(base.InvokeEvent(setToEventGameMode));
            }
        }
    }
}