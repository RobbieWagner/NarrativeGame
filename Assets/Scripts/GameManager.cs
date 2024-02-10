using Ink.Parsed;
using UnityEngine;

public enum GameMode
{
    None = -1,
    Exploration = 0,
    Dialogue = 1,
    Combat = 2,
    Other = 3
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}

    private GameMode currentGameMode;
    public GameMode CurrentGameMode
    {
        get => currentGameMode;

        set
        {
            if(currentGameMode == value) return;
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
}