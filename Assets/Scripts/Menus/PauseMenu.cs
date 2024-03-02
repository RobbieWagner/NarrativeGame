using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public class PauseMenu : Menu
    {
        public static PauseMenu Instance { get; private set; }

        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            base.Awake();
            GameManager.Instance.OnPauseGame += SetupMenuHandler;
            GameManager.Instance.OnResumeGame += DisableMenu;
        }

        public void SetupMenuHandler()
        {
            SetupMenu(false);
        }

        public override void SetupMenu(bool registerActionCollection = false)
        {
            base.SetupMenu(registerActionCollection);
        }

        private void DisableMenu()
        {
            StartCoroutine(DisableMenuCo());
        }

        public override IEnumerator DisableMenuCo(bool returnToPreviousMenu = true)
        {
            //Resume game with resume menu option selected
            yield return StartCoroutine(base.DisableMenuCo(returnToPreviousMenu));
        }

        protected override void SelectMenuItem(InputAction.CallbackContext context)
        {
            StartCoroutine(menuButtons[CurButton].SelectButton(this));
        }
    }
}