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
        #endregion
    }
}