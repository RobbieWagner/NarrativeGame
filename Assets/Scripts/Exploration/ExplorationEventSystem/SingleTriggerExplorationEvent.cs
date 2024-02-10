using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTriggerExplorationEvent : ExplorationEvent
{
    private bool hasBeenTriggered = false;

    protected override void Awake()
    {
        //TODO: Add handling to see if event has been triggered
        hasBeenTriggered = false;

        base.Awake();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if(!hasBeenTriggered)
            base.OnTriggerEnter(other);
    }

    protected override void OnUninteract()
    {
        hasBeenTriggered = true;
        //TODO: save triggerstate
        base.OnUninteract();
    }
}
