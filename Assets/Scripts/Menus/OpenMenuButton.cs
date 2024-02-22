using System.Collections;
using UnityEngine;

namespace PsychOutDestined
{
    public class OpenMenuButton : MenuButton
    {
        [SerializeField] private Menu thisMenu;
        private Menu previousMenu;

        protected override void Awake()
        {
            base.Awake();
            thisMenu.OnEnablePreviousMenu += ReturnToPreviousMenu;
        }

        public override IEnumerator SelectButton(Menu menu)
        {
            previousMenu = menu;
            yield return StartCoroutine(base.SelectButton(menu));
            thisMenu.SetupMenu();
        }

        protected virtual void ReturnToPreviousMenu()
        {
            previousMenu.SetupMenu();
        }
    }
}