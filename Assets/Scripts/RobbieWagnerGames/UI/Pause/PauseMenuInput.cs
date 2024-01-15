using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames.UI
{
    public class PauseMenuInput : MonoBehaviour
    {
        [SerializeField] private PauseMenu pauseMenu;

        private void OnPause()
        {
            if(pauseMenu != null)
            {
                if(pauseMenu.enabled && pauseMenu.paused)
                {
                    pauseMenu.ResumeGame();
                }
                else if(pauseMenu.canPause && !pauseMenu.paused)
                {
                    pauseMenu.enabled = true;
                }
            }
        }
    }
}