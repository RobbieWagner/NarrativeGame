using UnityEngine;

namespace PsychOutDestined
{
    public class ItemsMenu : Menu
    {
       [SerializeField] private TurnMenu turnMenu;

        protected override void Awake()
        {
            base.Awake();
            OnEnablePreviousMenu += ReturnToTurnMenu;
        }

        private void ReturnToTurnMenu()
        {
            OnEnablePreviousMenu -= ReturnToTurnMenu;
            turnMenu.SetupMenu();
        } 
    }
}