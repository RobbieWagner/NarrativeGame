using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebuggingManager : MonoBehaviour
{
    public static DebuggingManager Instance {get; private set;}
    private DebugControls controls;
    private bool holdingDebugButtonDown = false;

    #if UNITY_EDITOR

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

        controls.Enable();  
        SubscribeControls();
    }

    private void SubscribeControls()
    {
        controls.Debug.DebugHold.performed += StartDebugHold;
        controls.Debug.DebugHold.canceled += EndDebugHold; 
    }

    private void StartDebugHold(InputAction.CallbackContext context) => holdingDebugButtonDown = true;

    private void EndDebugHold(InputAction.CallbackContext context) => holdingDebugButtonDown = false;

    private void EndCombat(InputAction.CallbackContext context)
    {
        if(holdingDebugButtonDown)
        {
            ICombatManager.Instance?.EndCombat();
        }
    }
#endif
}