using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public class TurnMenu : Menu
    {
        public bool canFlee = true;
        public bool canUseItems = true;
        [HideInInspector] public Unit unit;

        [SerializeField] private OpenMenuButton actionButton;
        [SerializeField] private OpenMenuButton itemsButton;
        [SerializeField] private FleeButton fleeButton;
        public Vector3 WORLDSPACE_UNIT_OFFSET;

        protected override void Awake()
        {
            base.Awake();

            if (ICombatManager.Instance != null)
            {
                ICombatManager.Instance.OnStartActionSelectionForUnit += SetupMenu;
                ICombatManager.Instance.OnReturnToUnitsActionSelectionMenu += ReturnToUnitsLastSelection;
            }
        }

        private void ReturnToUnitsLastSelection(Unit lastUnit)
        {
            unit = lastUnit;
            curButton = unit.lastSelectedTurnMenuOptionIndex;
            SelectMenuItem(new InputAction.CallbackContext());
        }

        public void SetupMenu(Unit currentUnit)
        {
            transform.position = currentUnit.transform.position + WORLDSPACE_UNIT_OFFSET;

            menuButtons = new List<MenuButton>();
            if(currentUnit.availableActions.Count > 0)
            {
                actionButton.gameObject.SetActive(true);
                menuButtons.Add(actionButton);
            }
            if (canUseItems)
            {
                itemsButton.gameObject.SetActive(true);
                menuButtons.Add(itemsButton);
            }
            if (canFlee)
            {
                fleeButton.gameObject.SetActive(true);
                menuButtons.Add(fleeButton);
            }

            unit = currentUnit;

            SetupMenu();
        }

        public override void SetupMenu()
        {
            Debug.Log("opening turn menu");
            base.SetupMenu();
            curButton = unit.lastSelectedTurnMenuOptionIndex;
            ConsiderMenuButton(curButton);
        }

        protected override void SelectMenuItem(InputAction.CallbackContext context)
        {
            unit.lastSelectedTurnMenuOptionIndex = CurButton;
            base.SelectMenuItem(context);
        }
    }
}