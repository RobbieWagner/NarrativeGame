using UnityEngine;

namespace PsychOutDestined
{
    public class ItemsMenu : Menu
    {
       [SerializeField] private TurnMenu turnMenu;

        protected override void Awake()
        {
            base.Awake();
            ReturnToPreviousMenu += ReturnToTurnMenu;
            menuControls.UIInput.Cancel.performed += GoToPreviousMenu;
        }

        private void ReturnToTurnMenu()
        {
            ReturnToPreviousMenu -= ReturnToTurnMenu;
            turnMenu.SetupMenu();
        } 
    }
}