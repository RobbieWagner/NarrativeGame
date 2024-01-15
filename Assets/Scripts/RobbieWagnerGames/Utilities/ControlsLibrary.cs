using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames
{
    [Serializable]
    public enum InputType
    {
        None = -1,
        Pause = 0,
        NavigateMenus = 1,
        MakeSelection = 2,
        Movement = 3,
        Interact = 4,
    }

    [Serializable]
    public class ActionMapIconData
    {
        public InputType inputType;
        public string mkbInputString;
    }
        
    [CreateAssetMenu]
    public class ControlsLibrary : ScriptableObject
    {
        [SerializeField] private ActionMapIconData[] serializedData;

        private Dictionary<InputType, ActionMapIconData> _dict = null;

        public Dictionary<InputType, ActionMapIconData> dict
        {
            get
            {
                if (_dict == null)
                {
                    _dict = new Dictionary<InputType, ActionMapIconData>();
                    foreach (ActionMapIconData data in serializedData)
                    {
                        _dict.Add(data.inputType, data);
                    }
                }
                return _dict;
            }
        }
    }
}