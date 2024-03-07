using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
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
                    Debug.LogWarning("An attempt was made to access the persistent data path before it was defined! Data cannot be loaded until file path is defined.");
                    return null;
                }
                return Path.Combine(persistentPath, SAVE_DATA_LOCAL_FILE_PATH);
            }
        }

        //Saves any object as a json file
        public static void SaveObject<T>(string key, T obj, string fileName = "SaveData", string[] extraFileStrings = null)
        {
            if(obj != null)
            {
                SaveData<T> saveData = new SaveData<T>(key, obj);
                string filePath = null;
                if(extraFileStrings == null)
                    filePath = DATA_FILE_PATH;
                else
                    filePath = Path.Combine(DATA_FILE_PATH, Path.Combine(extraFileStrings));

                if(!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                filePath = Path.Combine(filePath, fileName);

                if(!filePath.EndsWith(".json")) filePath += ".json";
                Debug.Log(filePath);

                var currentData = new SaveDataList();
                if(File.Exists(filePath)) // if the file and key already exist, data to save is current data plus new data
                {
                    currentData = GetAllDataFromFile(filePath);
                    foreach(SaveData<string> data in currentData.SaveData)
                    {
                        if(data.Key.Equals(saveData.Key, StringComparison.InvariantCultureIgnoreCase)) 
                            data.Value = JsonUtility.ToJson(saveData.Value);
                    }

                    if(!currentData.SaveData.Where(d => d.Key.Equals(saveData.Key, StringComparison.InvariantCultureIgnoreCase)).Any()) // if key does not exist, preserve current data and add new
                    {
                        Debug.Log("appending file");
                        currentData.SaveData.Add(new SaveData<string>(saveData.Key, JsonUtility.ToJson(saveData.Value)));
                    }
                        
                }
                else 
                    currentData.SaveData.Add(new SaveData<string>(saveData.Key, JsonUtility.ToJson(saveData.Value))); // else data to save is just the object we currently have
                
                StreamWriter streamWriter = new StreamWriter(filePath);
                streamWriter.WriteLine(JsonUtility.ToJson(currentData)); // TODO: Consider making async
                streamWriter.Close();
            }
            else
                Debug.LogWarning("Cannot save an empty object");
        }

        public static T LoadObject<T>(string key, string fileName, T defaultValue, string[] extraFileStrings = null)
        {
            T returnObject;
            return LoadObject(key, fileName, out returnObject, extraFileStrings) ? returnObject : defaultValue ;
        }

        private static bool LoadObject<T>(string key, string fileName, out T loadedData, string[] extraFileStrings = null)
        {
            string filePath = null;
            if(string.IsNullOrWhiteSpace(DATA_FILE_PATH))
            {
                Debug.LogWarning("Data file path is not defined, so save data could not be loaded");
                loadedData = default;
                return false;
            }

            if(extraFileStrings == null)
                filePath = Path.Combine(DATA_FILE_PATH, fileName);
            else
                filePath = Path.Combine(Path.Combine(DATA_FILE_PATH, Path.Combine(extraFileStrings)), fileName);

            if(!filePath.ToLower().EndsWith(".json")) filePath += ".json";
            Debug.Log(filePath);
            if(File.Exists(filePath))
            {
                SaveDataList fileSaveData = GetAllDataFromFile(filePath);
                foreach(SaveData<string> saveData in fileSaveData.SaveData)
                {
                    if(saveData.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        loadedData = JsonUtility.FromJson<T>(saveData.Value);
                        return true;
                    }
                }
                Debug.LogWarning($"Could not load object: object key {key} does not exist in file");
                loadedData = default;
                return false;
            }
            else
            {
                Debug.LogWarning("Could not load object: file path not found");
                loadedData = default;
                return false;
            }
        }

        private static SaveDataList GetAllDataFromFile(string filePath)
        {
            string fileText = File.ReadAllText(filePath);
            Debug.Log(fileText);
            if(!string.IsNullOrWhiteSpace(fileText))
                return JsonUtility.FromJson<SaveDataList>(fileText);
            return null;
        } 

        public static void PurgeAllSaveData()
        {
            PurgeAllSaveDataAsync();
        }

        private static async void PurgeAllSaveDataAsync() //TODO: make process async once deletion becomes expensive
        {
            Debug.LogWarning("Deleting all saved data! ");
            if(!string.IsNullOrWhiteSpace(persistentPath) && Directory.Exists(persistentPath))
            {
                DirectoryInfo pathInfo = new DirectoryInfo(persistentPath);
                if(pathInfo != null)
                {
                    foreach (FileInfo file in pathInfo.EnumerateFiles())
                    {
                        file.Delete(); 
                    }
                    foreach (DirectoryInfo dir in pathInfo.EnumerateDirectories())
                    {
                        dir.Delete(true); 
                    }
                }

                return;
            }
            Debug.LogWarning($"Could not delete save data at path \"{persistentPath}\"");
        }
    }
}