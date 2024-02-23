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
            Debug.Log($"action selected {buttonAction.actionName}");
            CombatManagerBase.Instance?.SelectActionForCurrentUnit(buttonAction);
            yield return StartCoroutine(base.SelectButton(menu));
        }

        public void SetNameText(string text) => nameText.text = text;
    }
}