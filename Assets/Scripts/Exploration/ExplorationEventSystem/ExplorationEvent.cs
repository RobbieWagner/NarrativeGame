using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobbieWagnerGames;
using UnityEngine.InputSystem;
using DG.Tweening;

public class ExplorationEvent : EventSequence
{
    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
            StartCoroutine(InvokeEvent());
    }

    protected override IEnumerator InvokeEvent(bool setToEventGameMode = true)
    {
        yield return StartCoroutine(base.InvokeEvent(setToEventGameMode));
    }
}
