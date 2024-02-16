using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTriggerExplorationEvent : ExplorationEvent
{
    private bool hasBeenTriggered = false;

    private void Awake()
    {
        //TODO: Add handling to see if event has been triggered
        hasBeenTriggered = false;

        OnCompleteEventInvocation += MarkTriggered;
    }

    private void MarkTriggered() => hasBeenTriggered = true;
}
