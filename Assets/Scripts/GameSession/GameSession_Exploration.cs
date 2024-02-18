using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RobbieWagnerGames;
using UnityEngine;

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
    private const string EXPLORATION_SAVE_DATA_FILE_NAME = "exploration_data";
    private const string EXPLORATION_SAVE_KEY = "ExplorationData";
    [HideInInspector] public string currentSceneName = "";
    [HideInInspector] public Vector3 currentPlayerPosition = Vector3.zero;

    private void LoadExplorationData()
    {

    }

    private void SaveExplorationData()
    {
        if(PlayerMovement.Instance != null)
            currentPlayerPosition = PlayerMovement.Instance.transform.position;
        else
            Debug.LogWarning("Could not save player overworld position: player not found");

        if(Level.Instance != null)
            currentSceneName = Level.Instance.explorationSceneName;
        if(string.IsNullOrWhiteSpace(currentSceneName))
            Debug.LogWarning("Scene name found empty, will not be saved.");

        SaveDataManager.SaveObject(EXPLORATION_SAVE_KEY, new ExplorationData(), EXPLORATION_SAVE_DATA_FILE_NAME);
    }
}