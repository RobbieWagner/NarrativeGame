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
            thisMenu.ReturnToPreviousMenu += ReturnToPreviousMenu;
        }

        public override IEnumerator SelectButton(Menu menu)
        {
            Debug.Log("selected a menu button");
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