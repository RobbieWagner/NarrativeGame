using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using System.Text.RegularExpressions;

namespace RobbieWagnerGames
{
    public partial class DialogueManager : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private bool canvasEnabledOnStart;

        private Story currentStory;
        public IEnumerator dialogueCoroutine {get; private set;}
        private DialogueInputActions controls;

        private string currentSentenceText = "";
        private bool sentenceTyping = false;
        private bool skipSentenceTyping = false;
        private bool currentSpeakerIsOnLeft = true;

        [SerializeField] private AudioSource dialogueSound;

        [Header("Choices")]
        [SerializeField] private DialogueChoice choicePrefab;
        [SerializeField] private LayoutGroup choiceParent;
        private List<DialogueChoice> choices;
        private int currentChoice;
        public int CurrentChoice
        {
            get { return currentChoice; }
            set 
            {    
                if(choices.Count == 0) return;
                if(currentChoice >= 0 && currentChoice < choices.Count)choices[currentChoice].SetInactive();
                currentChoice = value;
                if(currentChoice < 0) currentChoice = choices.Count - 1;
                else if(currentChoice >= choices.Count) currentChoice = 0;
                choices[currentChoice].SetActive();
            }
        }

        private bool canContinue;
        public bool CanContinue
        {
            get { return canContinue; }
            set
            {
                if(value == canContinue) return;
                canContinue = value;

                if(canContinue) DisplayDialogueChoices();
            }
        }

        public static DialogueManager Instance {get; private set;}

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

            DisableSpeakerVisuals();
            dialogueCanvas.enabled = canvasEnabledOnStart;
            CanContinue = false;
            continueIcon.enabled = false;
            controls = new DialogueInputActions();
            controls.UIInput.Navigate.performed += OnNavigateDialogueMenu;
            controls.UIInput.Select.performed += OnNextDialogueLine;
        }

        public void EnterDialogueMode(Story story)
        {
            StartCoroutine(EnterDialogueModeCo(story));
        }

        public IEnumerator EnterDialogueModeCo(Story story)
        {
            if(dialogueCoroutine == null)
            {
                ConfigureDialogueControls();
                dialogueCoroutine = RunDialogue(story);
                yield return StartCoroutine(dialogueCoroutine);
            }

            StopCoroutine(EnterDialogueModeCo(story));
        }

        private void ConfigureDialogueControls()
        {
            controls.Enable();
        }

        #region core mechanics
        public IEnumerator RunDialogue(Story story)
        {
            yield return null;

            currentStory = story;
            dialogueCanvas.enabled = true;
            currentSpeakerIsOnLeft = true;

            ToggleSpeaker(leftSpeakerNamePlate, leftSpeakerName, true);
            ToggleSpeaker(rightSpeakerNamePlate, rightSpeakerName, true);

            yield return StartCoroutine(ReadNextSentence());

            while(dialogueCoroutine != null)
            {
                yield return null;
            }

            StopCoroutine(RunDialogue(story));
        }

        public IEnumerator EndDialogue()
        {
            yield return null;
            currentText.text = "";
            dialogueCanvas.enabled = false;
            dialogueCoroutine = null;
            currentStory = null;
            StopCoroutine(EndDialogue());
        }
        #endregion

        #region choices

        private bool DialogueHasChoices()
        {
            if(currentStory == null) return false;
            return currentStory.currentChoices.Count > 0;
        }

        private void DisplayDialogueChoices()
        {
            if(DialogueHasChoices())
            {
                List<Choice> choiceOptions = currentStory.currentChoices;

                for(int i = 0; i < choiceOptions.Count; i++)
                {
                    Choice choice = choiceOptions[i];
                    DialogueChoice choiceObject = Instantiate(choicePrefab, choiceParent.transform).GetComponent<DialogueChoice>();
                    choiceObject.choice = choice;
                    choiceObject.Initialize(choice);
                    choices.Add(choiceObject);
                }

                CurrentChoice = 0;
            }
            else
            {
                continueIcon.enabled = true;
            }
        }

        private void OnNavigateDialogueMenu(InputAction.CallbackContext context)
        {
            if(DialogueHasChoices())
            {
                float value = context.ReadValue<float>();
                if(value > 0f) 
                {
                    CurrentChoice++;
                }
                else if(value < 0f)
                {
                    CurrentChoice--;
                }
            }
        }

        private void RemoveChoiceGameObjects()
        {
            if(choices != null)
            {
                foreach(DialogueChoice choice in choices)
                {
                    Destroy(choice.gameObject);
                }

                choices.Clear();
            }

            choices = null;
        }
        #endregion
    }
}