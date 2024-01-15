using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobbieWagnerGames.UI
{
    public class MenuTab : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] public string tabName;
        [SerializeField] public LayoutGroup tabContentParent;

        [SerializeField] public TextMeshProUGUI contentTextPrefab;

        public virtual void BuildTab()
        {

        }

        public virtual void ToggleTab(bool on)
        {
            foreach(Transform child in transform)
            {
                transform.gameObject.SetActive(on);
            }
        }

        protected string AddSpacesToString(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) && 
                        i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        {
                            newText.Append(' ');
                        }
                }
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}