using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using RobbieWagnerGames;
using System.Linq;

namespace PsychOutDestined
{
    public class DialogueScene : MonoBehaviour
    {
        public static DialogueScene Instance { get; private set; }
        [SerializeField] private List<SceneEvent> sceneEvents;
        [SerializeField] private Transform dialogueEventParent;

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
            if (sceneEvents != null && sceneEvents.Count > 0)
            {
                foreach (SceneEvent sceneEvent in sceneEvents)
                    yield return StartCoroutine(sceneEvent.RunSceneEvent());
            }
            else if (dialogueEventParent.childCount > 0)
            {
                sceneEvents = dialogueEventParent.GetComponentsInChildren<SceneEvent>().ToList();

                foreach (SceneEvent sceneEvent in sceneEvents)
                    yield return StartCoroutine(sceneEvent.RunSceneEvent());
            }

            StopCoroutine(PlayDialogueScene());
        }
    }
}