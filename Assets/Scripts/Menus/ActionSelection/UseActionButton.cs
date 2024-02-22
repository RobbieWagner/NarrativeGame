using System.Collections;
using TMPro;
using UnityEngine;

namespace PsychOutDestined
{
    public class UseActionButton : MenuButton
    {
        [HideInInspector] public CombatAction buttonAction;
        
        public override IEnumerator SelectButton(Menu menu)
        {
            ICombatManager.Instance?.SelectActionForCurrentUnit(buttonAction);
            yield return null;
        }

        public void SetNameText(string text) => nameText.text = text;
    }
}