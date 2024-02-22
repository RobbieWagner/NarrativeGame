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

        protected override void Awake()
        {
            if (ICombatManager.Instance != null)
                ICombatManager.Instance.OnStartActionSelectionForUnit += SetupMenu;
        }

        public void SetupMenu(Unit currentUnit)
        {
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