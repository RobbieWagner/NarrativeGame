using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AYellowpaper.SerializedCollections;
using RobbieWagnerGames;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsychOutDestined
{
    public partial class GameSession : MonoBehaviour
    {
        private const string EXPLORATION_SAVE_DATA_PATH = "/Exploration/playerData";
        private const string DEFAULT_EXPLORATION_SCENE = "C1S1_School"; //"ExplorationSceneTemplate";
        [HideInInspector] public ExplorationData explorationData;

        private void LoadExplorationData()
        {
            explorationData = JsonDataService.Instance.LoadData(EXPLORATION_SAVE_DATA_PATH, explorationData, true, false);
            explorationData = explorationData != null ? explorationData : new ExplorationData(DEFAULT_EXPLORATION_SCENE, Vector3.zero);

            if(string.IsNullOrWhiteSpace(explorationData.CurrentSceneName))
                explorationData.CurrentSceneName = DEFAULT_EXPLORATION_SCENE;
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

            //explorationData = new ExplorationData(Level.Instance.explorationSceneName, PlayerMovement.Instance.transform.position);

            if (explorationData != null)
                JsonDataService.Instance.SaveData(EXPLORATION_SAVE_DATA_PATH, explorationData);
            else
                Debug.LogWarning("Save failed: Exploration Data for current session not found!");
        }

        public void SetEventTriggered(string triggeredEvent, bool triggered = true)
        {
            ExplorationEventData eventData = new ExplorationEventData(triggeredEvent, triggered);
            explorationData.explorationEventSaveData.RemoveAll(e => e.Equals(eventData));
            explorationData.explorationEventSaveData.Add(eventData);
        }
    }

    [System.Serializable]
    public class ExplorationEventData
    {
        public string eventName { get; set; }
        public bool triggered { get; set; }

        public ExplorationEventData(string key, bool value)
        {
            eventName = key;
            triggered = value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            // Check if the object is null or not of the same type
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            ExplorationEventData other = obj as ExplorationEventData;

            return eventName.Equals(other.eventName);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(eventName, triggered);
        }
    }

    [System.Serializable]
    public class ExplorationData
    {
        public string CurrentSceneName { get; set; }
        public float playerPosition_x { get; set; }
        public float playerPosition_y { get; set; }
        public float playerPosition_z { get; set; }
        public List<ExplorationEventData> explorationEventSaveData { get; set; }

        public ExplorationData(string sceneName, Vector3 playerPos)
        {
            CurrentSceneName = sceneName;
            playerPosition_x = playerPos.x;
            playerPosition_y = playerPos.y;
            playerPosition_z = playerPos.z;
            explorationEventSaveData = new List<ExplorationEventData>();
        }

        public Vector3 PlayerPosition() { return new Vector3(playerPosition_x, playerPosition_y, playerPosition_z); }
        public bool GetSavedExplorationEventData(string eventKey, out ExplorationEventData? data) 
        {
            var explorationEventData = explorationEventSaveData.Where(e => e.eventName.Equals(eventKey));
            
            if(explorationEventData.Any())
            {
                data = explorationEventData.First();
                return true;
            }
            
            data = null;
            return false;
        }
    }
}