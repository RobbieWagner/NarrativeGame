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
        public string CurrentSceneName { get; set; }
        public float playerPosition_x { get; set; }
        public float playerPosition_y { get; set; }
        public float playerPosition_z { get; set; }

    public ExplorationData(string sceneName, Vector3 playerPos)
        {
            CurrentSceneName = sceneName;
            playerPosition_x = playerPos.x;
            playerPosition_y = playerPos.y;
            playerPosition_z = playerPos.z;
        }

        public Vector3 PlayerPosition() { return new Vector3(playerPosition_x, playerPosition_y, playerPosition_z); }
    }

    public partial class GameSession : MonoBehaviour
    {
        //private const string EXPLORATION_SAVED_SCENE_PATH = "/Exploration/currentScene";
        //private const string PLAYER_SAVED_POSITION_PATH = "/Exploration/playerPosition";
        private const string EXPLORATION_SAVE_DATA_PATH = "/Exploration/playerData";
        private const string DEFAULT_EXPLORATION_SCENE = "ExplorationSceneTemplate";
        //[HideInInspector] public string currentSceneName = "";
        //[HideInInspector] public Vector3 currentPlayerPosition = Vector3.zero;
        [HideInInspector] public ExplorationData explorationData;

        private void LoadExplorationData()
        {
            explorationData = new ExplorationData(DEFAULT_EXPLORATION_SCENE, Vector3.zero);
            //currentPlayerPosition =  JsonDataService.Instance.LoadData(PLAYER_SAVED_POSITION_PATH, Vector3.zero, true);
            //currentSceneName = JsonDataService.Instance.LoadData(EXPLORATION_SAVED_SCENE_PATH, DEFAULT_EXPLORATION_SCENE, true);
            explorationData = JsonDataService.Instance.LoadData(EXPLORATION_SAVE_DATA_PATH, explorationData, true, false);

            if(string.IsNullOrWhiteSpace(explorationData.CurrentSceneName))
                explorationData.CurrentSceneName = DEFAULT_EXPLORATION_SCENE;
            //Debug.Log(explorationData.playerPosition);
        }

        private void SaveExplorationData()
        {
            if (PlayerMovement.Instance == null)
            {
                Debug.LogWarning("Could not save player overworld position: player not found");
                return;
            }

            if (Level.Instance == null)
            {
                Debug.LogWarning("Scene name found empty, will not be saved.");
                return;
            }

            explorationData = new ExplorationData(Level.Instance.explorationSceneName, PlayerMovement.Instance.transform.position);

            //JsonDataService.Instance.SaveData(PLAYER_SAVED_POSITION_PATH, currentPlayerPosition, false);
            //JsonDataService.Instance.SaveData(EXPLORATION_SAVED_SCENE_PATH, currentSceneName, false);

            JsonDataService.Instance.SaveData(EXPLORATION_SAVE_DATA_PATH, explorationData);
        }
    }
}