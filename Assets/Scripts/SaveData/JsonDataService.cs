using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace PsychOutDestined
{
    public class JsonDataService : IDataService
    {
        public bool SaveData<T>(string RelativePath, T Data, bool Encrypt = false)
        {
            Debug.Log("saving data");
            string path = Application.persistentDataPath + RelativePath;
            Debug.Log($"saving at path {path}");
            if(!path.EndsWith(".json")) path += ".json";

            Debug.Log($"saving at path {path}");
            bool result = SaveDataInternal(path, Data, Encrypt);
            return result;
        }

        private bool SaveDataInternal<T>(string FullPath, T Data, bool Encrypt)
        {
            Debug.Log("saving internally");
            try
            {
                if (File.Exists(FullPath))
                {
                    Debug.Log($"File exists at path {FullPath}. Overwriting");
                    File.Delete(FullPath);
                }
                Debug.Log($"Creating new file at path {FullPath}");
                FileStream stream = File.Create(FullPath);
                stream.Close();
                File.WriteAllText(FullPath, JsonConvert.SerializeObject(Data));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveDataAsync<T>(string FullPath, T Data, bool Encrypt = false)
        {
            if(!FullPath.EndsWith(".json")) FullPath += ".json";
            bool result = await Task.Run(() => SaveDataInternal(FullPath, Data, Encrypt));
            return result;
        }

        public T LoadData<T>(string RelativePath, T DefaultData, bool isEncrypted = false)
        {
            string path = Application.persistentDataPath + RelativePath;
            if(!path.EndsWith(".json")) path += ".json";

            return LoadDataInternal(path, DefaultData, isEncrypted);
        }

        public T LoadDataInternal<T>(string FullPath, T DefaultData, bool isEncrypted = false)
        {
            if(!File.Exists(FullPath))
            {
                Debug.LogWarning($"File at path {FullPath} not found, returning default data...");
                return DefaultData;
            }

            try
            {
                T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(FullPath));
                return data;
            }
            catch
            {
                Debug.LogWarning($"Data at file path {FullPath} was not of the correct type, returning default data...");
                return DefaultData;
            }
        }

        public async Task<T> LoadDataAsync<T>(string FullPath, T DefaultData, bool isEncrypted = false)
        {
            if(!FullPath.EndsWith(".json")) FullPath += ".json";
            T result = await Task.Run(() => LoadDataInternal(FullPath, DefaultData, isEncrypted));
            return result;
        }

        public bool PurgeData()
        {
            string path = Application.persistentDataPath;
            Debug.LogWarning("File purge begun. Deleting all save data...");
            try
            {
                DirectoryInfo pathInfo = new DirectoryInfo(path);
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

                return true;
            }
            catch
            {
                Debug.LogWarning("Data could not be purged, aborting purge process.");
                return false;
            }
        }
    }
}