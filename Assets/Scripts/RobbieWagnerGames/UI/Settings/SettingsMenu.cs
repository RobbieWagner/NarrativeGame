using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RobbieWagnerGames.UI
{
    public class SettingsMenu : Menu
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            backButton.onClick.AddListener(SaveSettings);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            backButton.onClick.RemoveListener(SaveSettings);
        }

        private void SaveSettings()
        {
            
        }
    }
}