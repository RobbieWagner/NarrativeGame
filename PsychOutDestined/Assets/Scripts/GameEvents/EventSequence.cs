using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public class EventSequence : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] protected List<SequenceEvent> eventSequence;

        public virtual IEnumerator InvokeEvent(bool setToEventGameMode = true)
        {
            GameMode prevGameMode = GameMode.None;
            if (setToEventGameMode && GameManager.Instance != null)
            {
                prevGameMode = GameManager.Instance.CurrentGameMode;
                GameManager.Instance.CurrentGameMode = GameMode.Event;
            }

            foreach (SequenceEvent sequenceEvent in eventSequence)
                yield return StartCoroutine(sequenceEvent.InvokeSequenceEvent());

            if (setToEventGameMode && GameManager.Instance != null)
                GameManager.Instance.CurrentGameMode = prevGameMode;

            OnCompleteEventInvocation?.Invoke();
        }

        public delegate void OnCompleteEventInvocationDelegate();
        public event OnCompleteEventInvocationDelegate OnCompleteEventInvocation;
    }
}