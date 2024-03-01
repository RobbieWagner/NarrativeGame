using Ink.Parsed;
using UnityEngine;

namespace PsychOutDestined
{
    public enum GameMode
    {
        None = -1,
        Exploration = 0,
        Event = 1,
        Combat = 2,
        Other = 3
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private GameMode currentGameMode;
        public GameMode CurrentGameMode
        {
            get => currentGameMode;

            set
            {
                if (currentGameMode == value) return;
                currentGameMode = value;
                OnGameModeChanged?.Invoke(currentGameMode);
            }
        }
        public delegate void OnGameModeChangedDelegate(GameMode gameMode);
        public event OnGameModeChangedDelegate OnGameModeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            CurrentGameMode = GameMode.None;
        }

        public bool PauseGame()
        {
            Time.timeScale = 0;
            return true;
            // Get enabled control schemes and disable them
            // Only pause the game if certain kinds of control schemes are not active.
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
            // Get previously enabled control schemes and reenable them
            OnResumeGame?.Invoke();
        }
        public delegate void OnResumeGameDelegate();
        public event OnResumeGameDelegate OnResumeGame;
    }
}