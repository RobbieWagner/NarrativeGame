using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;

namespace RobbieWagnerGames.UI
{
    public class DialogueChoice : MonoBehaviour
    {
        [SerializeField] private Color inactiveColor;
        [SerializeField] private Color activeColor;

        public TextMeshProUGUI choiceText;
        public Choice choice;

        private void Awake()
        {
            choiceText.color = inactiveColor;
        }

        public void SetActive()
        {
            choiceText.color = activeColor;
        }

        public void SetInactive()
        {
            choiceText.color = inactiveColor;
        }

        public void Initialize(Choice newChoice)
        {
            choiceText.text = newChoice.text;
            SetInactive();
        }
    }
}