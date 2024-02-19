using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

            SaveDataManager.persistentPath = Application.persistentDataPath;
            LoadSaveFiles();
        }

        public void LoadSaveFiles() => StartCoroutine(LoadSaveFilesAsync());

        private IEnumerator LoadSaveFilesAsync()
        {
            yield return new WaitForEndOfFrame();
            Task loadTask = Task.Run(() =>
            {
                // Background thread work
                LoadPlayersParty();
                LoadExplorationData();
            });

            yield return new WaitUntil(() => loadTask.IsCompleted);

            //Debug.Log($"loading scene: {currentSceneName}");
            AsyncOperation asyncSceneLoad = SceneManager.LoadSceneAsync(currentSceneName, LoadSceneMode.Additive);

            while (!asyncSceneLoad.isDone)
            {
                yield return null;
            }
            yield return null;

            InitializePlayerPosition();

            OnLoadComplete?.Invoke();
        }
        public delegate void OnLoadCompleteDelegate();
        public event OnLoadCompleteDelegate OnLoadComplete;

        private void InitializePlayerPosition() {PlayerMovement.Instance.SetPosition(currentPlayerPosition);Debug.Log($"pos{currentPlayerPosition}");}

        public void SaveGameSessionData() => StartCoroutine(SaveGameSessionDataAsync());

        private IEnumerator SaveGameSessionDataAsync()
        {
            yield return new WaitForEndOfFrame();
            Task saveTask = Task.Run(() =>
            {
                // Background thread work
                SavePlayersParty();
                SaveExplorationData();
            });

            yield return new WaitUntil(() => saveTask.IsCompleted);
            OnSaveComplete?.Invoke();
        }
        public delegate void OnSaveCompleteDelegate();
        public event OnSaveCompleteDelegate OnSaveComplete;
    }
}