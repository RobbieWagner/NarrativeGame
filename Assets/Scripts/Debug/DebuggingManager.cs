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

        controls = new DebugControls();
        controls.Enable();  
        SubscribeControls();
    }

    // private void Update()
    // {
    //     if(holdingDebugButtonDown) Debug.Log("awaiting debug input");
    // }

    private void SubscribeControls()
    {
        controls.Debug.DebugHold.performed += StartDebugHold;
        controls.Debug.DebugHold.canceled += EndDebugHold; 
        controls.Debug.TerminateCombat.performed += TerminateCombat;
    }

    private void StartDebugHold(InputAction.CallbackContext context) => holdingDebugButtonDown = true;

    private void EndDebugHold(InputAction.CallbackContext context) => holdingDebugButtonDown = false;

    private void TerminateCombat(InputAction.CallbackContext context)
    {
        if(holdingDebugButtonDown)
        {
            ICombatManager.Instance?.TerminateCombat();
        }
    }
#endif
}