using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ink.Runtime;
using RobbieWagnerGames;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsychOutDestined
{
    public partial class GameSession : MonoBehaviour
    {
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

            new JsonDataService(); //Initialize for singleton
            StaticGameStats.persistentDataPath = Application.persistentDataPath;
            LoadSaveFiles();
        }

        public void LoadSaveFiles() => LoadSaveFilesAsync();

        private async void LoadSaveFilesAsync()
        {
            await Task.Run(() =>
            {
                LoadPlayersParty();
                LoadExplorationData();
            });

            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(currentSceneName, LoadSceneMode.Additive); //TODO: Get this back on the main thread
            while(!sceneLoad.isDone) await Task.Yield();

            InitializePlayerPosition();

            OnLoadComplete?.Invoke();
        }
        public delegate void OnLoadCompleteDelegate();
        public event OnLoadCompleteDelegate OnLoadComplete;

        private void InitializePlayerPosition() => PlayerMovement.Instance.SetPosition(currentPlayerPosition);

        public void SaveGameSessionData() => StartCoroutine(SaveGameSessionDataAsync());

        private IEnumerator SaveGameSessionDataAsync()
        {
            yield return new WaitForEndOfFrame();
            bool taskComplete = false;
            Task saveTask = Task.Run(() =>
            {
                SavePlayersParty();
                SaveExplorationData();
                taskComplete = true;
            });

            while(!taskComplete) 
                yield return new WaitForEndOfFrame();
            
            OnSaveComplete?.Invoke();
        }
        public delegate void OnSaveCompleteDelegate();
        public event OnSaveCompleteDelegate OnSaveComplete;
    }
}