using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, IInputManager
{
    public static InputManager Instance {get; private set;}
    private HashSet<IInputActionCollection> activeActionCollections;
    private HashSet<InputActionMap> actionMaps;

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

        activeActionCollections = new HashSet<IInputActionCollection>();
        actionMaps = new HashSet<InputActionMap>();
    }

    public void RegisterActionCollection(IInputActionCollection actionCollection)
    {
        bool added = activeActionCollections.Add(actionCollection);
        if(added)
            actionCollection.Enable();
    }

    public void DeregisterActionCollection(IInputActionCollection actionCollection)
    {
        bool removed = activeActionCollections.Remove(actionCollection);
        Debug.Log("removed " + removed);
        if(removed)
            actionCollection.Disable();
    }

    public void RegisterActionMap(InputActionMap map)
    {
        bool added = actionMaps.Add(map);
        if(added)
            map.Enable();
    }

    public void DeregisterActionMap(InputActionMap map)
    {
        bool removed = actionMaps.Remove(map);
        if(removed)
            map.Enable();
    }

    public void ReenableActions()
    {
        foreach(IInputActionCollection actionCollection in activeActionCollections)
            actionCollection?.Enable();
    }

    public void DisableActions()
    {
        foreach(IInputActionCollection actionCollection in activeActionCollections)
            actionCollection?.Disable();
    }
}