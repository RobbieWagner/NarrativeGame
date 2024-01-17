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

        private void ConfigureTextField()
        {
            List<string> tags = currentStory.currentTags;

            string speaker = "";
            Sprite characterSprite = null;
            bool removeSpriteOnLeft = false;
            bool removeSpriteOnRight = false;
            bool? placeSpriteOnLeft = null;

            foreach(string t in tags)
            {
                string tag = Regex.Replace(t, @"\s+", "");
                tag = tag.Replace("~"," ");
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
                    tag = tag.Remove(tag.IndexOf("PLACESPRITEONLEFT"), 17).ToLower();
                    string filePath = StaticGameStats.characterSpriteFilePath + tag;
                    characterSprite = Resources.Load<Sprite>(filePath);
                    if(characterSprite == null) Debug.LogWarning($"sprite name provided \"{tag}\" is not a valid sprite name, please reconfigure this tag");
                    placeSpriteOnLeft = true;
                }
                else if(tag.Contains("PLACESPRITEONRIGHT"))
                {
                    tag = tag.Remove(tag.IndexOf("PLACESPRITEONRIGHT"), 18).ToLower();
                    string filePath = StaticGameStats.characterSpriteFilePath + tag;
                    characterSprite = Resources.Load<Sprite>(filePath);
                    if(characterSprite == null) Debug.LogWarning($"sprite name provided \"{tag}\" is not a valid sprite name, please reconfigure this tag");
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
                else if(tag.Contains("LEFTSHAKESPRITE"))
                {
                    StartCoroutine(ShakeSprite(leftSpeakerSprite, 40f));
                }
                else if(tag.Contains("RIGHTSHAKESPRITE"))
                {
                    StartCoroutine(ShakeSprite(rightSpeakerSprite, 40f));
                }
                else if(tag.Contains("SHAKEBACKGROUND"))
                {
                    StartCoroutine(ShakeSprite(backgroundImage, 15f));
                }
                else if(tag.Contains("CHANGEBACKGROUND"))
                {
                    tag = tag.Remove(tag.IndexOf("CHANGEBACKGROUND"), 16).ToLower();
                    string filePath = StaticGameStats.backgroundSpriteFilePath + tag;
                    Sprite sprite = Resources.Load<Sprite>(filePath);
                    if(sprite == null) Debug.LogWarning($"sprite name provided \"{tag}\" is not a valid sprite name, please reconfigure this tag");
                    StartCoroutine(ChangeBackground(sprite));
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
                ToggleSprite(leftSpeakerSprite, false, characterSprite);
            if(removeSpriteOnRight) 
            {
                ToggleSprite(rightSpeakerSprite, false, characterSprite);
            }

            if(placeSpriteOnLeft == true)
                ToggleSprite(leftSpeakerSprite, true, characterSprite);
            else if(placeSpriteOnLeft == false)
                ToggleSprite(rightSpeakerSprite, true, characterSprite);
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
    }
}