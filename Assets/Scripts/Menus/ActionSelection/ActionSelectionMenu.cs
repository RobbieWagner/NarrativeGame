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
            OnEnablePreviousMenu += ReturnToTurnMenu;
        }

        public override void SetupMenu()
        {
            if(menuButtons != null)
            {
                foreach(MenuButton button in menuButtons)
                    Destroy(button.gameObject);
                menuButtons.Clear();
            }

            foreach(CombatAction action in turnMenu.unit.availableActions)
            {
                UseActionButton newActionButton = Instantiate(actionButtonPrefab, buttonParent);
                newActionButton.SetNameText(action.actionName);
            }
            curButton = turnMenu.unit.lastSelectedActionMenuOptionIndex;
            base.SetupMenu();
        }

        private void ReturnToTurnMenu()
        {
            OnEnablePreviousMenu -= ReturnToTurnMenu;
            turnMenu.SetupMenu();
        }

        protected override void SelectMenuItem(InputAction.CallbackContext context)
        {
            turnMenu.unit.lastSelectedActionMenuOptionIndex = CurButton;
            base.SelectMenuItem(context);
        }
    }
}

