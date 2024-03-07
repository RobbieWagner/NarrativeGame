using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public class SingleTriggerExplorationEvent : ExplorationEvent
    {
        private bool hasBeenTriggered = false;

        private void Awake()
        {
            //TODO: Add handling to see if event has been triggered (save system)
            hasBeenTriggered = false;

            OnCompleteEventInvocation += MarkTriggered;
        }

        private void MarkTriggered() => hasBeenTriggered = true;
    }
}