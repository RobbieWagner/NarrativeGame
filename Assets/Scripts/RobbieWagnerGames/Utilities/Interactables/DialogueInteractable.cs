using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ink.Runtime;

namespace RobbieWagnerGames
{
    public class DialogueInteractable : IInteractable
    {
        [SerializeField] private string npcName;
        private string saveDataName;
        [SerializeField] protected TextAsset dialogueText;
        private int interactions;

        protected override void Awake()
        {
            base.Awake();
            saveDataName = SceneManager.GetActiveScene().name + "_" + npcName;
            interactions = SaveDataManager.LoadInt(saveDataName, 0);
        }

        private Story ConfigureStory()
        {
            Story configuredStory = new Story(dialogueText.text);
            configuredStory.variablesState["interactions"] = interactions;

            return configuredStory;
        }

        protected override IEnumerator Interact()
        {
            Story story = ConfigureStory();
            yield return StartCoroutine(DialogueManager.Instance.EnterDialogueModeCo(story));

            yield return base.Interact();

            StopCoroutine(Interact());
        }

        protected override void OnUninteract()
        {
            base.OnUninteract();

            interactions++;

            SaveInteractionData();
        }

        protected void SaveInteractionData()
        {
            SaveInt saveInt = new SaveInt(saveDataName, interactions);
            //GameManager.Instance.sessionSaveData.AddToSaveList(saveInt);
        }
    }
}