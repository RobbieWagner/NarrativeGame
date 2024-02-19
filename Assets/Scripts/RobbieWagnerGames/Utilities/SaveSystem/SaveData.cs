using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    #region SaveData

    [System.Serializable]
    public class SaveDataList
    {
        public SaveDataList()
        {
            SaveData = new List<SaveData<string>>();
        }

        [SerializeField] public List<SaveData<string>> SaveData;
    }

    [System.Serializable]
    public class SaveData<SerializableObject>
    {
        [SerializeField] public string Key;
        [SerializeField] public SerializableObject Value;

        public SaveData(string key, SerializableObject value)
        {
            Key = key;
            Value = value;
        }

        public string GetAsJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
   
    public class SaveString
    {
        public string Key;
        public string Value;

        public SaveString(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    public class SaveFloat
    {
        public string Key;
        public float Value;

        public SaveFloat(string Key, float value)
        {
            this.Key = Key;
            this.Value = value;
        }
    }

    public class SaveInt
    {
        public string Key;
        public int Value;

        public SaveInt(string Key, int value)
        {
            this.Key = Key;
            this.Value = value;
        }
    }

    public class SaveBool
    {
        public string Key;
        public bool Value;

        public SaveBool(string Key, bool value)
        {
            this.Key = Key;
            this.Value = value;
        }
    }
    #endregion

    //Allows for the creation of savedata without immediately saving the data
    //Useful for systems where the player should have control over saving
    //[System.Serializable]
    public class SessionSaveData 
    {

        public List<SaveString> saveStrings;
        public List<SaveFloat> saveFloats;
        public List<SaveInt> saveInts;
        public List<SaveBool> saveBools;

        public SessionSaveData()
        {
            saveStrings = new List<SaveString>();
            saveFloats = new List<SaveFloat>();
            saveInts = new List<SaveInt>();
            saveBools = new List<SaveBool>();
        }

        #region AddToSaveList
        public void AddToSaveList(SaveString Value) 
        {
            saveStrings.RemoveAll(x => x.Key.Equals(Value.Key));
            saveStrings.Add(Value);
        }
        public void AddToSaveList(SaveFloat saveFloat) 
        {
            saveFloats.RemoveAll(x => x.Key.Equals(saveFloat.Key));
            saveFloats.Add(saveFloat);
        }
        public void AddToSaveList(SaveInt saveInt) 
        {
            saveInts.RemoveAll(x => x.Key.Equals(saveInt.Key));
            saveInts.Add(saveInt);
        }
        public void AddToSaveList(SaveBool saveBool) 
        {
            saveBools.RemoveAll(x => x.Key.Equals(saveBool.Key));
            saveBools.Add(saveBool);
        }

        public void SaveAllSaveLists()
        {
            // foreach(SaveString Value in saveStrings){SaveDataManager.SaveString(Value.Key, Value.Value);}
            // foreach(SaveFloat saveFloat in saveFloats){SaveDataManager.SaveFloat(saveFloat.Key, saveFloat.Value);}
            // foreach(SaveInt saveInt in saveInts){SaveDataManager.SaveInt(saveInt.Key, saveInt.Value);}
            // foreach(SaveBool saveBool in saveBools){SaveDataManager.SaveBool(saveBool.Key, saveBool.Value);}
        }
        #endregion
    }
}