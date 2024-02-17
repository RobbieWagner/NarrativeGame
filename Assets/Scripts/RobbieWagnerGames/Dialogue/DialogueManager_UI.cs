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
using DG.Tweening;
using Unity.Mathematics;

namespace RobbieWagnerGames
{
    public partial class DialogueManager : MonoBehaviour
    {
        [Header("General Dialogue Info")]
        [SerializeField] private Canvas dialogueCanvas;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI currentText;
        [SerializeField] private Image continueIcon;

        [Header("Left Speaker")]
        [SerializeField] private Image leftSpeakerSprite;
        [SerializeField] private Image leftSpeakerNamePlate;
        [SerializeField] private TextMeshProUGUI leftSpeakerName;

        [Header("Right Speaker")]
        [SerializeField] private Image rightSpeakerSprite;
        [SerializeField] private Image rightSpeakerNamePlate;
        [SerializeField] private TextMeshProUGUI rightSpeakerName;

        #region Text and Text Field Configuration

        private string ConfigureSentence(string input)
        {
            string configuredText = input;

            List<char> allowList = new List<char>() {' ', '-', '\'', ',', '.'};
            string name = SaveDataManager.LoadObject("name", "UserData", new string[]{StaticGameStats.dialogueSavePath}, "Morgan");

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
                //SaveDataManager.SaveString("name", "Lux");
            } 

            configuredText = configuredText.Replace("^NAME^", name);

            return configuredText;
        }

        private void ToggleSpeaker(Image namePlate, TextMeshProUGUI nameText, bool on)
        {
            namePlate.gameObject.SetActive(on);
            nameText.gameObject.SetActive(on);
        }

        #endregion

        #region UI Actions
        private IEnumerator ShakeSprite(Image sprite, float strength)
        {
            yield return sprite.rectTransform.DOShakeAnchorPos(.5f, strength).WaitForCompletion();

            StopCoroutine(ShakeSprite(sprite, strength));
        }

        private IEnumerator ChangeBackground(Sprite sprite)
        {
            yield return backgroundImage.DOColor(Color.black, .5f).WaitForCompletion();
            backgroundImage.sprite = sprite;
            yield return backgroundImage.DOColor(Color.white, .5f).WaitForCompletion();
        }

        private IEnumerator ToggleSprite(Image spriteDisplay, bool on, Coroutine thisCoroutine = null, Sprite sprite = null, bool switchImmediately = false)
        {
            if(!switchImmediately)
            {
                if(spriteDisplay.enabled && on)
                {
                    yield return StartCoroutine(SwitchCharacterSprites(spriteDisplay, sprite));
                }
                else if(on)
                {
                    if(spriteDisplay != null) spriteDisplay.sprite = sprite;
                    yield return StartCoroutine(FadeInImage(spriteDisplay));
                }
                else
                {
                    yield return StartCoroutine(FadeOutImage(spriteDisplay));
                }
            }
            else
            {           
                if(spriteDisplay != null) spriteDisplay.sprite = sprite;
                spriteDisplay.enabled = on;
            }

            StopToggleCoroutine(thisCoroutine);
        }

        private void StopToggleCoroutine(Coroutine coroutine)
        {
            if(coroutine != null) StopCoroutine(coroutine);
            if(coroutine == leftSpriteSwapCoroutine) leftSpriteSwapCoroutine = null;
            if(coroutine == rightSpriteSwapCoroutine) rightSpriteSwapCoroutine = null;
        }

        private IEnumerator SwitchCharacterSprites(Image image, Sprite sprite = null, float timeInOut = .5f)
        {
            yield return FadeOutImage(image, timeInOut);
            image.sprite = sprite;
            yield return FadeInImage(image, timeInOut);
            StopCoroutine(SwitchCharacterSprites(image, sprite, timeInOut));
        }

        private IEnumerator FadeInImage(Image image, float time = 1f)
        {
            image.color = Color.clear;
            image.enabled = true;
            yield return image.DOColor(Color.white, time);
            StopCoroutine(FadeInImage(image, time));
        }

        private IEnumerator FadeOutImage(Image image, float time = 1f)
        {
            yield return image.DOColor(Color.clear, time);
            image.enabled = false;
            StopCoroutine(FadeOutImage(image, time));
        }

        private void DisableSpeakerVisuals()
        {
            ToggleSpeaker(rightSpeakerNamePlate, rightSpeakerName, false);
            StartCoroutine(ToggleSprite(leftSpeakerSprite, false));
            ToggleSpeaker(rightSpeakerNamePlate, rightSpeakerName, false);
            StartCoroutine(ToggleSprite(rightSpeakerSprite, false));
        }
        #endregion
    }
}