using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using RobbieWagnerGames;

public class DialogueScene : MonoBehaviour
{
    [SerializeField] private TextAsset storyTextAsset;

    public static DialogueScene Instance {get; private set;}

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
        } 
        else 
        { 
            Instance = this; 
        } 

        StartCoroutine(PlayDialogueScene());
    }

    private IEnumerator PlayDialogueScene()
    {
        Story story = new Story(storyTextAsset.text);
        if(SceneTransition.Instance != null) yield return StartCoroutine(SceneTransition.Instance.FadeOutScreen());
        yield return StartCoroutine(DialogueManager.Instance.EnterDialogueModeCo(story));
        if(SceneTransition.Instance != null) yield return StartCoroutine(SceneTransition.Instance.FadeInScreen());

        StopCoroutine(PlayDialogueScene()); 
    }
}
