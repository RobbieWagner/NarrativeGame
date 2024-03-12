using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

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
            OnSelectMenuItem += PlayMenuSelectionSound;

            if (OnByDefault) 
                SetupMenu();
        }

        public virtual void SetupMenu(bool registerActionCollection = true)
        {
            canvas.enabled = true;
            ConsiderMenuButton(CurButton);
            foreach (MenuButton button in menuButtons) 
                button.parentMenu = this;

            if(registerActionCollection)
                IInputManager.Instance.RegisterActionCollection(menuControls);
            else
                menuControls.Enable();
        }

        public virtual void DisableMenu(bool returnToPreviousMenu = true)
        {
            StartCoroutine(DisableMenuCo(returnToPreviousMenu));
        }

        public virtual IEnumerator DisableMenuCo(bool returnToPreviousMenu = true)
        {
            yield return null;
            canvas.enabled = false;
            if (returnToPreviousMenu)
                ReturnToPreviousMenu?.Invoke();
            
            if(!IInputManager.Instance.DeregisterActionCollection(menuControls))
                menuControls.Disable();
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

            AudioManager.PlayOneShot(
                AudioEventsLibrary.Instance.MenuNavigation, 
                AudioListenerInstance.Instance != null? AudioListenerInstance.Instance.GetAttenuationObjectPosition() : transform.position);
            ConsiderMenuButton(CurButton);
        }

        protected virtual void SelectMenuItem(InputAction.CallbackContext context)
        {
            DisableMenu();
            StartCoroutine(menuButtons[CurButton].SelectButton(this));
            InvokeOnSelectMenuItem();
        }

        public delegate void OnSelectMenuItemDelegate();
        public event OnSelectMenuItemDelegate OnSelectMenuItem;
        protected void InvokeOnSelectMenuItem()
        {
            OnSelectMenuItem?.Invoke();
        }

        protected void PlayMenuSelectionSound()
        {
            AudioManager.PlayOneShot(
                AudioEventsLibrary.Instance.MenuSelection,
                AudioListenerInstance.Instance != null? AudioListenerInstance.Instance.GetAttenuationObjectPosition() : transform.position);
        }

        protected virtual void GoToPreviousMenu(InputAction.CallbackContext context)
        {
            if(ReturnToPreviousMenu.GetInvocationList().Count() > 0) 
                DisableMenu(true);
        }
    }
}