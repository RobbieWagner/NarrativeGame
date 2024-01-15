using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames
{
    //Manages data from different events in game. 
    public static class SaveDataManager {

        #region SaveOptions
        public static void SaveString(string name, string saveString) {PlayerPrefs.SetString(name, saveString);}
        public static void SaveFloat(string name, float saveFloat) {PlayerPrefs.SetFloat(name, saveFloat);}
        public static void SaveInt(string name, int saveInt) {PlayerPrefs.SetInt(name, saveInt);}

        //Saves a bool as its integer counterpart
        public static void SaveBool(string name, bool saveBool) 
        {
            if(saveBool)
            {
                SaveInt(name, 1);
            }
            else SaveInt(name, 0);
        }

        //Saves any object as a json file
        public static void SaveObject<T>(string name, T obj)
        {
            if (obj == null)
            {
                string saveData = JsonUtility.ToJson(obj);
                PlayerPrefs.SetString(name, saveData);
            }
        }
        #endregion

        #region LoadOptions
        public static string LoadString(string name, string defaultValue)
        {
            string returnValue = PlayerPrefs.GetString(name, null);

            if(string.IsNullOrEmpty(returnValue)) return defaultValue;
            return PlayerPrefs.GetString(name, null);
        }
        public static float LoadFloat(string name, float defaultValue = 0f){return PlayerPrefs.GetFloat(name, defaultValue);}
        public static int LoadInt(string name, int defaultValue = 0){return PlayerPrefs.GetInt(name, defaultValue);}
        public static bool LoadBool(string name, int defaultValue = 0){return PlayerPrefs.GetInt(name, defaultValue) == 1;}
        #endregion
    }
}