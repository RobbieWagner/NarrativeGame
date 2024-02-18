using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RobbieWagnerGames;
using UnityEngine;

public partial class GameSession : MonoBehaviour
{
    public static GameSession Instance {get; private set;}

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

    public void LoadSaveFiles()
    {
        StartCoroutine(LoadSaveFilesAsync());
    }

    private IEnumerator LoadSaveFilesAsync()
    {
        yield return new WaitForEndOfFrame();
        Task loadTask = Task.Run(() =>
        {
            // Background thread work
            LoadPlayersParty();
            LoadExplorationData();
            //LoadSavedGameSessionData();
        });

        yield return new WaitUntil(() => loadTask.IsCompleted);
        OnLoadComplete?.Invoke();
    }
    public delegate void OnLoadCompleteDelegate();
    public event OnLoadCompleteDelegate OnLoadComplete; 

    // private void LoadSavedGameSessionData()
    // {
       
    // }

    public void SaveGameSessionData()
    {
        StartCoroutine(SaveGameSessionDataAsync());
    }

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
