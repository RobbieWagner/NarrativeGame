using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RobbieWagnerGames
{

    //Manages data from different events in game. 
    public static class SaveDataManager 
    {
        public static string persistentPath = "";
        private static string SAVE_DATA_LOCAL_FILE_PATH = "SaveData"; 
        private static string DATA_FILE_PATH
        {
            get
            {
                if(string.IsNullOrWhiteSpace(persistentPath)) 
                {
                    Debug.LogError("An attempt was made to access the persistent data path before it was defined!");
                    return null;
                }
                return Path.Combine(persistentPath, SAVE_DATA_LOCAL_FILE_PATH);
            }
        }

        //Saves any object as a json file
        public static void SaveObject<T>(string key, T obj, string[] filePathStrings = null)
        {
            if (obj == null)
            {
                SaveData<T> saveData = new SaveData<T>(key, obj);
                string filePath = null;
                if(filePathStrings == null)
                    filePath = DATA_FILE_PATH;
                else
                    filePath = Path.Combine(DATA_FILE_PATH, Path.Combine(filePathStrings));

                if(!string.IsNullOrWhiteSpace(filePath))
                {
                    if(!filePath.EndsWith(".json")) filePath += ".json";
                    Debug.Log(filePath);
                    StreamWriter streamWriter = new StreamWriter(filePath);
                    streamWriter.Write(saveData.GetAsJson());
                }
                else
                    Debug.LogWarning($"could not save object at {filePath}: file path is null");
            }
        }

        public static T LoadObject<T>(string key, string[] filePathStrings, T defaultReturn = default)
        {
            string filePath = null;
            if(filePathStrings == null)
                filePath = DATA_FILE_PATH;
            else
                filePath = Path.Combine(DATA_FILE_PATH, Path.Combine(filePathStrings));

            if(!string.IsNullOrWhiteSpace(filePath))
            {
                if(!filePath.ToLower().EndsWith(".json")) filePath += ".json";
                Debug.Log(filePath);
                string jsonData = File.ReadAllText(filePath);
                List<SaveData<string>> fileSaveData = JsonUtility.FromJson<List<SaveData<string>>>(jsonData);
                foreach(SaveData<string> saveData in fileSaveData)
                {
                    if(saveData.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                        return JsonUtility.FromJson<T>(saveData.Value);
                }
                Debug.LogWarning("Could not load object: object key does not exist in file");
                return default;
            }
            else
            {
                Debug.LogWarning("Could not load object: file path not found");
                return default;
            }
        }
    }
}