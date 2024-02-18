using UnityEngine;

public class PauseMenu : Menu
{
    public static PauseMenu Instance {get; private set;}

    protected override void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
        } 
        else 
        { 
            Instance = this; 
        } 

        base.Awake();
    }
}