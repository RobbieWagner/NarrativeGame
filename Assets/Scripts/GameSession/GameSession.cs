using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

        LoadGameSessionData();
        //foreach()
    }

    private void LoadGameSessionData()
    {
        Thread fileLoader = new Thread(LoadSaveFiles);
        fileLoader.Start();
    }

    private void LoadSaveFiles()
    {
        LoadPlayersParty();
    }

    private void SaveGameSessionData()
    {
        Thread fileSaver = new Thread(UpdateSaveFiles);
        fileSaver.Start();
    }

    private void UpdateSaveFiles()
    {
        SavePlayersParty();
    }
}
