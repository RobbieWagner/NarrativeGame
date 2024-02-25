using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] protected Canvas canvas;

        protected MenuControls menuControls;
        [SerializeField] protected bool OnByDefault;
        [SerializeField] protected List<MenuButton> menuButtons;
        protected int curButton = 0;
        protected int CurButton
        {
            get => curButton;
            set
            {
                if (value == curButton) 
                    return;

                curButton = value;

                if (curButton >= menuButtons.Count) 
                    curButton = 0;
                if (curButton < 0) 
                    curButton = menuButtons.Count - 1;
            }
        }

        protected virtual void Awake()
        {
            canvas.enabled = false;
            menuControls = new MenuControls();
            menuControls.UIInput.Navigate.started += NavigateMenu;
            menuControls.UIInput.Select.performed += SelectMenuItem;

            if (OnByDefault) 
                SetupMenu();
        }

        public virtual void SetupMenu()
        {
            canvas.enabled = true;
            menuControls.Enable();
            ConsiderMenuButton(CurButton);
            foreach (MenuButton button in menuButtons) 
                button.parentMenu = this;
        }

        public void DisableMenu(bool returnToPreviousMenu = true)
        {
            StartCoroutine(DisableMenuCo(returnToPreviousMenu));
        }

        public IEnumerator DisableMenuCo(bool returnToPreviousMenu = true)
        {
            yield return null;
            canvas.enabled = false;
            menuControls.Disable();
            if (returnToPreviousMenu)
                ReturnToPreviousMenu?.Invoke();
        }
        public delegate void OnEnablePreviousMenuDelegate();
        public event OnEnablePreviousMenuDelegate ReturnToPreviousMenu;

        protected void ConsiderMenuButton(int activeButtonIndex)
        {
            foreach (MenuButton button in menuButtons)
                button.NavigateAway();
            menuButtons[activeButtonIndex].NavigateTo();
        }

        private void NavigateMenu(InputAction.CallbackContext context)
        {
            float direction = context.ReadValue<float>();

            if (direction > 0)
                CurButton++;
            else
                CurButton--;

            ConsiderMenuButton(CurButton);
        }

        protected virtual void SelectMenuItem(InputAction.CallbackContext context)
        {
            DisableMenu();
            StartCoroutine(menuButtons[CurButton].SelectButton(this));
        }

        protected virtual void GoToPreviousMenu(InputAction.CallbackContext context)
        {
            if(ReturnToPreviousMenu.GetInvocationList().Count() > 0) 
                DisableMenu(true);
        }
    }
}