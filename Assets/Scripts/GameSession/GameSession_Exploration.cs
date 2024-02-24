using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RobbieWagnerGames;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsychOutDestined
{
    [System.Serializable]
    public class ExplorationData
    {
        [SerializeField] public string CurrentSceneName;
        [SerializeField] public Vector3 CurrentPlayerPosition;

        public ExplorationData()
        {
            CurrentSceneName = GameSession.Instance.currentSceneName;
            CurrentPlayerPosition = GameSession.Instance.currentPlayerPosition;
        }
    }

    public partial class GameSession : MonoBehaviour
    {
        private const string EXPLORATION_SAVED_SCENE_PATH = "/Exploration/currentScene";
        private const string PLAYER_SAVED_POSITION_PATH = "/Exploration/playerPosition";
        private const string DEFAULT_EXPLORATION_SCENE = "ExplorationSceneTemplate";
        [HideInInspector] public string currentSceneName = "";
        [HideInInspector] public Vector3 currentPlayerPosition = Vector3.zero;

        private void LoadExplorationData()
        {
            currentPlayerPosition =  JsonDataService.Instance.LoadData(PLAYER_SAVED_POSITION_PATH, Vector3.zero, false);
            currentSceneName = JsonDataService.Instance.LoadData(EXPLORATION_SAVED_SCENE_PATH, DEFAULT_EXPLORATION_SCENE, false);
            if(string.IsNullOrWhiteSpace(currentSceneName))
                currentSceneName = DEFAULT_EXPLORATION_SCENE;
            Debug.Log(currentPlayerPosition);
        }

        private void SaveExplorationData()
        {
            if (PlayerMovement.Instance != null)
                currentPlayerPosition = PlayerMovement.Instance.transform.position;
            else
                Debug.LogWarning("Could not save player overworld position: player not found");

            if (Level.Instance != null)
                currentSceneName = Level.Instance.explorationSceneName;
            if (string.IsNullOrWhiteSpace(currentSceneName))
                Debug.LogWarning("Scene name found empty, will not be saved.");

            JsonDataService.Instance.SaveData(PLAYER_SAVED_POSITION_PATH, currentPlayerPosition, false);
            JsonDataService.Instance.SaveData(EXPLORATION_SAVED_SCENE_PATH, currentSceneName, false);
        }
    }
}