using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobbieWagnerGames;
using UnityEngine.InputSystem;
using DG.Tweening;

public class ExplorationEvent : IInteractable
{
    [Header("Events")]
    [SerializeField] protected List<SequenceEvent> sequenceEvents;

    protected override void Awake()
    {
        canInteract = false;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
            OnInteract(new InputAction.CallbackContext());
    }

    protected override void OnInteract(InputAction.CallbackContext context)
    {
        if(PlayerMovement.Instance != null) PlayerMovement.Instance.DisablePlayerMovement();
        StartCoroutine(Interact());
    }

    protected override IEnumerator Interact()
    {
        foreach(SequenceEvent sequenceEvent in sequenceEvents)
            yield return StartCoroutine(sequenceEvent.InvokeSequenceEvent());
        yield return base.Interact();
    }
}
