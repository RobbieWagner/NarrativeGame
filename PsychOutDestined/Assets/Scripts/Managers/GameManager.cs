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

    public partial class GameManager  : MonoBehaviour
    {
        [Header("Pausing")]
        public bool canPause = false;
        public bool paused = false; 

        public static GameManager Instance { get; private set; }

        private GameMode currentGameMode = GameMode.None;
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
            if(canPause)
            {
                Time.timeScale = 0;
                AudioListener.pause = true;
                paused = true;
                IInputManager.Instance.DisableActions();
                OnPauseGame?.Invoke();
                return true;
            }
            return false;
        }
        public delegate void OnPauseGameDelegate();
        public event OnPauseGameDelegate OnPauseGame;

        public void ResumeGame()
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            IInputManager.Instance.ReenableActions();
            OnResumeGame?.Invoke();
            paused = false;
        }
        public delegate void OnResumeGameDelegate();
        public event OnResumeGameDelegate OnResumeGame;
    }
}