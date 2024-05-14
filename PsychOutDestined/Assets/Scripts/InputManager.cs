using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        private HashSet<IInputActionCollection> activeActionCollections;
        private HashSet<InputActionMap> actionMaps;

        private void Awake()
        {
            if (IInputManager.Instance != null && IInputManager.Instance as InputManager != this)
            {
                Destroy(gameObject);
            }
            else
            {
                IInputManager.Instance = this;
            }

            activeActionCollections = new HashSet<IInputActionCollection>();
            actionMaps = new HashSet<InputActionMap>();
        }

        public bool RegisterActionCollection(IInputActionCollection actionCollection)
        {
            bool added = activeActionCollections.Add(actionCollection);
            if (added)
                actionCollection.Enable();
            return added;
        }

        public bool DeregisterActionCollection(IInputActionCollection actionCollection)
        {
            bool removed = activeActionCollections.Remove(actionCollection);
            //Debug.Log("removed " + removed);
            if (removed)
                actionCollection.Disable();
            return removed;
        }

        public bool RegisterActionMap(InputActionMap map)
        {
            bool added = actionMaps.Add(map);
            if (added)
                map.Enable();
            return added;
        }

        public bool DeregisterActionMap(InputActionMap map)
        {
            bool removed = actionMaps.Remove(map);
            if (removed)
                map.Enable();
            return removed;
        }

        public void ReenableActions()
        {
            foreach (IInputActionCollection actionCollection in activeActionCollections)
                actionCollection?.Enable();
            foreach(InputActionMap map in actionMaps)
                map.Disable();
        }

        public void DisableActions()
        {
            foreach (IInputActionCollection actionCollection in activeActionCollections)
                actionCollection?.Disable();
            foreach(InputActionMap map in actionMaps)
                map.Enable();
        }
    }
}