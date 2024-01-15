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
    public class DialogueManager : MonoBehaviour
    {

        [SerializeField] private bool canvasEnabledOnStart;

        [Header("General Dialogue Info")]
        [SerializeField] private Canvas dialogueCanvas;
        [SerializeField] private TextMeshProUGUI currentText;

        private Story currentStory;
        public IEnumerator dialogueCoroutine {get; private set;}
        private DialogueInputActions controls;

        private string currentSentenceText = "";
        private bool sentenceTyping = false;
        private bool skipSentenceTyping = false;
        private bool currentSpeakerIsOnLeft = true;

        [Header("Left Speaker")]
        [SerializeField] private Image leftSpeakerSprite;
        [SerializeField] private Image leftSpeakerNamePlate;
        [SerializeField] private TextMeshProUGUI leftSpeakerName;

        [Header("Right Speaker")]
        [SerializeField] private Image rightSpeakerSprite;
        [SerializeField] private Image rightSpeakerNamePlate;
        [SerializeField] private TextMeshProUGUI rightSpeakerName;

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

        [SerializeField] private Image continueIcon;
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

        private void OnNextDialogueLine(InputAction.CallbackContext context)
        {
            if(CanContinue)
            {
                if(DialogueHasChoices())
                {
                    currentStory.ChooseChoiceIndex(CurrentChoice);
                }

                continueIcon.enabled = false;

                StartCoroutine(ReadNextSentence());
            }
            else if(sentenceTyping)
            {
                skipSentenceTyping = true;
            }
        }

        public IEnumerator ReadNextSentence()
        {
            CanContinue = false;
            RemoveChoiceGameObjects();
            choices = new List<DialogueChoice>();

            yield return null;
            if(currentStory.canContinue)
            {
                currentText.text = "";
                sentenceTyping = true;

                currentSentenceText = ConfigureSentence(currentStory.Continue());

                ConfigureTextField();
                char nextChar;
                for(int i = 0; i < currentSentenceText.Length; i++)
                {
                    if(skipSentenceTyping) break;
                    nextChar = currentSentenceText[i];
                    if(nextChar == '<')
                    {
                        while(nextChar != '>' && i < currentSentenceText.Length)
                        {
                            currentText.text += nextChar;
                            i++;
                            nextChar = currentSentenceText[i];
                        } 
                    }

                    currentText.text += nextChar;
                    yield return new WaitForSeconds(.036f);
                }

                currentText.text = currentSentenceText;
                sentenceTyping = false;
                skipSentenceTyping = false;
                CanContinue = true;
            }
            else
            {
                yield return StartCoroutine(EndDialogue());
            }

            StopCoroutine(ReadNextSentence());
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

        #region Text and Text Field Configuration

        private string ConfigureSentence(string input)
        {
            string configuredText = input;

            List<char> allowList = new List<char>() {' ', '-', '\'', ',', '.'};
            string name = SaveDataManager.LoadString("name", "Lux");

            bool nameAllowed = true;
            foreach(char c in name)
            {
                if(!Char.IsLetterOrDigit(c) && !allowList.Contains(c))
                {
                    nameAllowed = false;
                    break;
                }
            }

            if(!nameAllowed)
            {
                name = "Lux";
                SaveDataManager.SaveString("name", "Lux");
            } 

            configuredText = configuredText.Replace("^NAME^", name);

            return configuredText;
        }

        private void ConfigureTextField()
        {
            List<string> tags = currentStory.currentTags;

            string speaker = "";
            bool removeSpriteOnLeft = false;
            bool removeSpriteOnRight = false;
            bool? placeSpriteOnLeft = null;

            foreach(string t in tags)
            {
                string tag = Regex.Replace(t, @"\s+", "");
                tag = tag.ToUpper();
                
                if(tag.Contains("SPEAKERISONLEFT"))
                {
                    currentSpeakerIsOnLeft = true;
                }
                else if(tag.Contains("SPEAKERISONRIGHT"))
                {
                    currentSpeakerIsOnLeft = false;
                }
                else if(tag.Contains("PLACESPRITEONLEFT"))
                {
                    placeSpriteOnLeft = true;
                }
                else if(tag.Contains("PLACESPRITEONRIGHT"))
                {
                    placeSpriteOnLeft = false;
                }
                else if(tag.Contains("REMOVESPRITEONLEFT"))
                {
                    removeSpriteOnLeft = true;
                }
                else if(tag.Contains("REMOVESPRITEONRIGHT"))
                {
                    removeSpriteOnRight = true;
                }
                else if(tag.Contains("REMOVESPRITES"))
                {
                    removeSpriteOnLeft = true;
                    removeSpriteOnRight = true;
                }
                else if(tag.Contains("SPEAKER"))
                {
                    tag = tag.Remove(tag.IndexOf("SPEAKER"), 7);
                    speaker = tag.ToLower();
                }
                else
                    Debug.LogWarning($"\"{t}\" could not be read, please check its formatting/spelling it or have it removed. Refer to the docs for more information on proper tag writing");
            }

            if(!string.IsNullOrEmpty(speaker))
            {
                SetSpeaker(speaker, currentSpeakerIsOnLeft);
                ToggleSpeaker(leftSpeakerNamePlate, leftSpeakerName, currentSpeakerIsOnLeft);
                ToggleSpeaker(rightSpeakerNamePlate, rightSpeakerName, !currentSpeakerIsOnLeft);
            }
            else
            {
                ToggleSpeaker(leftSpeakerNamePlate, leftSpeakerName, false);
                ToggleSpeaker(rightSpeakerNamePlate, rightSpeakerName, false);
            }

            if(removeSpriteOnLeft) 
                ToggleSprite(leftSpeakerSprite, false);
            if(removeSpriteOnRight) 
            {
                ToggleSprite(rightSpeakerSprite, false);
            }

            if(placeSpriteOnLeft == true)
                ToggleSprite(leftSpeakerSprite, true);
            else if(placeSpriteOnLeft == false)
                ToggleSprite(rightSpeakerSprite, true);
        }

        //TODO: include sprite placement
        private void SetSpeaker(string speaker, bool currentSpeakerIsOnLeft)
        {
            if(currentSpeakerIsOnLeft)
            {
                ToggleSpeaker(leftSpeakerNamePlate, leftSpeakerName, false);
                ToggleSpeaker(rightSpeakerNamePlate, rightSpeakerName, true);
                speaker = char.ToUpper(speaker[0]) + speaker.Substring(1);
                leftSpeakerName.text = speaker;
            }
            else
            {
                ToggleSpeaker(rightSpeakerNamePlate, rightSpeakerName, false);
                ToggleSpeaker(leftSpeakerNamePlate, leftSpeakerName, true);
                speaker = char.ToUpper(speaker[0]) + speaker.Substring(1);
                rightSpeakerName.text = speaker;
            }
        }

        private void ToggleSprite(Image spriteDisplay, bool on, Sprite sprite = null)
        {
            if(sprite != null && spriteDisplay != null) spriteDisplay.sprite = sprite;
            spriteDisplay.enabled = on;
        }

        private void ToggleSpeaker(Image namePlate, TextMeshProUGUI nameText, bool on)
        {
            namePlate.gameObject.SetActive(on);
            nameText.gameObject.SetActive(on);
        }

        private void DisableSpeakerVisuals()
        {
            ToggleSpeaker(rightSpeakerNamePlate, rightSpeakerName, false);
            ToggleSprite(leftSpeakerSprite, false);
            ToggleSpeaker(rightSpeakerNamePlate, rightSpeakerName, false);
            ToggleSprite(rightSpeakerSprite, false);
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