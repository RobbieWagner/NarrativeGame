using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace RobbieWagnerGames.UI
{
    public class ControlsSubmenu : MenuTab
    {
        //[SerializeField] private TextMeshProUGUI sectionName;
        [SerializeField] public VerticalLayoutGroup actions;
        [SerializeField] public VerticalLayoutGroup keyBinds;

        [SerializeField] private PlayerInput playerInput;

        [SerializeField] private ControlsLibrary controlsLibrary;

        [SerializeField] private TextMeshProUGUI keyBindTextPrefab;

        public override void BuildTab()
        {
            base.BuildTab();

            //sectionName.text = tabName;

            Dictionary<InputType, ActionMapIconData> dict = controlsLibrary.dict;

            foreach(KeyValuePair<InputType, ActionMapIconData> keyValuePair in dict)
            {
                TextMeshProUGUI actionText = Instantiate(contentTextPrefab.gameObject, actions.transform).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI keyBind = Instantiate(keyBindTextPrefab.gameObject, keyBinds.transform).GetComponent<TextMeshProUGUI>();

                actionText.text = AddSpacesToString(keyValuePair.Key.ToString(), false);
                keyBind.text = keyValuePair.Value.mkbInputString;
            }
        }

    }
}