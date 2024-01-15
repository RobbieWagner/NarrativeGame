using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace RobbieWagnerGames
{
    #region SaveData
    public class SaveString
    {
        public string name;
        public string saveString;

        public SaveString(string name, string saveString)
        {
            this.name = name;
            this.saveString = saveString;
        }
    }

    public class SaveFloat
    {
        public string name;
        public float saveFloat;

        public SaveFloat(string name, float saveFloat)
        {
            this.name = name;
            this.saveFloat = saveFloat;
        }
    }

    public class SaveInt
    {
        public string name;
        public int saveInt;

        public SaveInt(string name, int saveInt)
        {
            this.name = name;
            this.saveInt = saveInt;
        }
    }

    public class SaveBool
    {
        public string name;
        public bool saveBool;

        public SaveBool(string name, bool saveBool)
        {
            this.name = name;
            this.saveBool = saveBool;
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
        public void AddToSaveList(SaveString saveString) 
        {
            saveStrings.RemoveAll(x => x.name.Equals(saveString.name));
            saveStrings.Add(saveString);
        }
        public void AddToSaveList(SaveFloat saveFloat) 
        {
            saveFloats.RemoveAll(x => x.name.Equals(saveFloat.name));
            saveFloats.Add(saveFloat);
        }
        public void AddToSaveList(SaveInt saveInt) 
        {
            saveInts.RemoveAll(x => x.name.Equals(saveInt.name));
            saveInts.Add(saveInt);
        }
        public void AddToSaveList(SaveBool saveBool) 
        {
            saveBools.RemoveAll(x => x.name.Equals(saveBool.name));
            saveBools.Add(saveBool);
        }

        public void SaveAllSaveLists()
        {
            foreach(SaveString saveString in saveStrings){SaveDataManager.SaveString(saveString.name, saveString.saveString);}
            foreach(SaveFloat saveFloat in saveFloats){SaveDataManager.SaveFloat(saveFloat.name, saveFloat.saveFloat);}
            foreach(SaveInt saveInt in saveInts){SaveDataManager.SaveInt(saveInt.name, saveInt.saveInt);}
            foreach(SaveBool saveBool in saveBools){SaveDataManager.SaveBool(saveBool.name, saveBool.saveBool);}
        }
        #endregion
    }
}