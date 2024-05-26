using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ink.Runtime;
using RobbieWagnerGames;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PsychOutDestined
{
    public partial class GameSession : MonoBehaviour
    {
        private GameSessionControls controls;
        public static GameSession Instance { get; private set; }

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

            SceneTransitionController.Instance.TurnOnScreenCover();

            new JsonDataService(); //Initialize for singleton
            StaticGameStats.persistentDataPath = Application.persistentDataPath;
            LoadSaveFiles();

            controls = new GameSessionControls();
            controls.Enable();
            controls.Pause.Pause.performed += TogglePause;
            OnLoadComplete += CompleteGameSetup;
        }

        public void LoadSaveFiles() => LoadSaveFilesAsync();

        private async void LoadSaveFilesAsync()
        {
            await Task.Run(() =>
            {
                LoadPlayersParty();
                LoadExplorationData();
            });

            //TODO: Get this scene load back on the main thread if worried about memory usage
            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(explorationData.CurrentSceneName, LoadSceneMode.Additive); 
            while(!sceneLoad.isDone) await Task.Yield();

            InitializePlayerPosition();

            OnLoadComplete?.Invoke();
        }
        public delegate void OnLoadCompleteDelegate();
        public event OnLoadCompleteDelegate OnLoadComplete;

        private void InitializePlayerPosition() => PlayerMovement.Instance.SetPosition(explorationData.PlayerPosition());

        private void CompleteGameSetup()
        {
            GameManager.Instance.canPause = true;
        }

        public void SaveGameSessionData() => StartCoroutine(SaveGameSessionDataAsync());

        public IEnumerator SaveGameSessionDataAsync()
        {
            yield return new WaitForSecondsRealtime(.1f);
            bool taskComplete = false;
            Task saveTask = Task.Run(() =>
            {
                SavePlayersParty();
                SaveExplorationData();
                taskComplete = true;
            });

            while(!taskComplete) 
                yield return new WaitForSecondsRealtime(.1f);
            
            OnSaveComplete?.Invoke();
        }
        public delegate void OnSaveCompleteDelegate();
        public event OnSaveCompleteDelegate OnSaveComplete;

        private void TogglePause(InputAction.CallbackContext context)
        {
            Debug.Log("pause toggled");
            if(GameManager.Instance != null)
            {
                if(!GameManager.Instance.paused)
                    GameManager.Instance.PauseGame();
                else
                    GameManager.Instance.ResumeGame();
            }
        }
    }
}