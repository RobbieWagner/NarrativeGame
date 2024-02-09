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
    public GameMode currentGameMode;

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

        currentGameMode = GameMode.None;
    }
}