using RobbieWagnerGames;
using UnityEngine;
using Ink.Runtime;
using System.Collections;

public class DialogueSequenceEvent : SequenceEvent
{
    [SerializeField] private Story dialogue;

    public override IEnumerator InvokeSequenceEvent()
    {
        yield return StartCoroutine(DialogueManager.Instance.EnterDialogueModeCo(dialogue));
    }
}