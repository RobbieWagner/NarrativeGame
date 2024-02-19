using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace PsychOutDestined
{
    public class MenuButton : MonoBehaviour
    {
        [HideInInspector] public Menu parentMenu;
        [SerializeField] private string buttonName;
        [SerializeField] private TextMeshProUGUI nameText;
        public const float DISABLED_TEXT_ALPHA = .4f;

        protected virtual void Awake()
        {
            nameText.color = new Color(nameText.color.r, nameText.color.g, nameText.color.b, DISABLED_TEXT_ALPHA);
        }

        public virtual void NavigateTo() => nameText.color = new Color(nameText.color.r, nameText.color.g, nameText.color.b, 1);

        public virtual void NavigateAway() => nameText.color = new Color(nameText.color.r, nameText.color.g, nameText.color.b, DISABLED_TEXT_ALPHA);

        public virtual IEnumerator SelectButton(Menu menu)
        {
            yield return null;
        }
    }
}