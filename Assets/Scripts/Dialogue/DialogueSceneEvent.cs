using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using RobbieWagnerGames;

public class DialogueSceneEvent : SceneEvent
{
    [SerializeField] private TextAsset storyTextAsset;

    public override IEnumerator RunSceneEvent()
    {
        Story story = new Story(storyTextAsset.text);
        yield return DialogueManager.Instance?.EnterDialogueModeCo(story);

        yield return StartCoroutine(base.RunSceneEvent());
        StopCoroutine(RunSceneEvent());
    }
}