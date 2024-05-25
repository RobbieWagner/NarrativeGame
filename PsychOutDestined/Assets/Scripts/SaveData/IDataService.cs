using UnityEngine;

namespace PsychOutDestined
{
    public interface IDataService
    {
        bool SaveData<T>(string RelativePath, T Data, bool Encrypt = false);
        T LoadData<T>(string RelativePath, T DefaultData, bool saveDefaultIfMissing, bool isEncrypted);
        bool PurgeData();
    }
}