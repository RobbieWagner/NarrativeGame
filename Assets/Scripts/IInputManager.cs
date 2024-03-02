using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputManager
{
    void RegisterActionCollection(IInputActionCollection actionCollection);
    void DeregisterActionCollection(IInputActionCollection actionCollection);
    void RegisterActionMap(InputActionMap map);
    void DeregisterActionMap(InputActionMap map);
    void DisableActions();
    void ReenableActions();
}