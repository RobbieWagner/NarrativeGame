using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public class ActionSelectionMenu : Menu
    {
        [SerializeField] private TurnMenu turnMenu;
        [SerializeField] private UseActionButton actionButtonPrefab;
        [SerializeField] private Transform buttonParent;

        protected override void Awake()
        {
            base.Awake();
            ReturnToPreviousMenu += ReturnToTurnMenu;
            menuControls.UIInput.Cancel.performed += GoToPreviousMenu;
        }

        public override void SetupMenu()
        {
            Debug.Log("opening action menu");
            if(menuButtons != null)
            {
                foreach(MenuButton button in menuButtons)
                    Destroy(button.gameObject);
                menuButtons.Clear();
            }

            transform.position = turnMenu.unit.transform.position + turnMenu.WORLDSPACE_UNIT_OFFSET;
            menuButtons = new List<MenuButton>();

            foreach(CombatAction action in turnMenu.unit.availableActions)
            {
                UseActionButton newActionButton = Instantiate(actionButtonPrefab, buttonParent);
                newActionButton.SetNameText(action.actionName);
                newActionButton.buttonAction = action;
                menuButtons.Add(newActionButton);
            }

            if(menuButtons.Count == 0)
            {
                Debug.LogWarning("No Actions found, passing units turn");
                CombatManagerBase.Instance.SelectActionForCurrentUnit(CombatManagerBase.Instance.passTurn);
                DisableMenu(false);
            } 
            else
            {
                curButton = turnMenu.unit.lastSelectedActionMenuOptionIndex;
                if(curButton < 0 || curButton >= menuButtons.Count)
                    curButton = 0;
                base.SetupMenu();
            }            
        }

        private void ReturnToTurnMenu()
        {
            ReturnToPreviousMenu -= ReturnToTurnMenu;
            turnMenu.SetupMenu();
        }

        protected override void SelectMenuItem(InputAction.CallbackContext context)
        {
            Debug.Log($"{gameObject.name} selected action {curButton + 1}");
            turnMenu.unit.lastSelectedActionMenuOptionIndex = CurButton;
            StartCoroutine(menuButtons[CurButton].SelectButton(this));
            DisableMenu(false);
        }
    }
}

