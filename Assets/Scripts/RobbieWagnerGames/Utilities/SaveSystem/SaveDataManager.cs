using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public static void SaveObject<T>(string key, T obj, string fileName = "SaveData", string[] filePathStrings = null)
        {
            if(obj != null)
            {
                SaveData<T> saveData = new SaveData<T>(key, obj);
                string filePath = null;
                if(filePathStrings == null)
                    filePath = Path.Combine(DATA_FILE_PATH, fileName);
                else
                    filePath = Path.Combine(Path.Combine(DATA_FILE_PATH, Path.Combine(filePathStrings)), fileName);

                if(!string.IsNullOrWhiteSpace(filePath))
                {
                    if(!filePath.EndsWith(".json")) filePath += ".json";
                    Debug.Log(filePath);
                    Directory.CreateDirectory(filePath);
                    if(Directory.Exists(filePath))
                    {
                        var currentData = new List<SaveData<string>>();
                        if(File.Exists(filePath)) // if the file and key already exist, data to save is current data plus new data
                        {
                            currentData = GetAllDataFromFile(filePath);
                            currentData.ForEach(d =>
                            {
                                if(d.Key.Equals(saveData.Key, StringComparison.InvariantCultureIgnoreCase)) 
                                    d.Value = JsonUtility.ToJson(saveData.Value);
                            } 
                            );

                            if(!currentData.Select(d => d.Key.Equals(saveData.Key, StringComparison.InvariantCultureIgnoreCase)).Any()) // if key does not exist, preserve current data and add new
                                currentData.Add(new SaveData<string>(saveData.Key, JsonUtility.ToJson(saveData.Value)));
                        }
                        else 
                            currentData.Add(new SaveData<string>(saveData.Key, JsonUtility.ToJson(saveData.Value))); // else data to save is just the object we currently have
                        
                        StreamWriter streamWriter = new StreamWriter(filePath);
                        foreach(SaveData<string> data in currentData)
                            streamWriter.WriteLine(data.GetAsJson());
                    }
                    else
                    Debug.LogWarning($"Could not find directory for path {filePath}");
                }
                else
                    Debug.LogWarning($"could not save object at {filePath}: file path is null");
            }
        }

        public static T LoadObject<T>(string key, string fileName, string[] filePathStrings = null, T defaultReturn = default)
        {
            string filePath = null;
            if(filePathStrings == null)
                filePath = Path.Combine(DATA_FILE_PATH, fileName);
            else
                filePath = Path.Combine(Path.Combine(DATA_FILE_PATH, Path.Combine(filePathStrings)), fileName);

            Debug.Log(filePath);
            if(!filePath.ToLower().EndsWith(".json")) filePath += ".json";
            if(Directory.Exists(filePath))
            {
                List<SaveData<string>> fileSaveData = GetAllDataFromFile(filePath);
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

        private static List<SaveData<string>> GetAllDataFromFile(string filePath) => JsonUtility.FromJson<List<SaveData<string>>>(File.ReadAllText(filePath));

        public static void PurgeAllSaveData()
        {
            Thread fileLoader = new Thread(PurgeAllSaveDataAsync);
            fileLoader.Start();
        }

        private static void PurgeAllSaveDataAsync()
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