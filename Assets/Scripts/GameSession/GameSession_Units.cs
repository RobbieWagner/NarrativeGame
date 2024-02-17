using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RobbieWagnerGames;
using UnityEngine;

public partial class GameSession : MonoBehaviour
{
    public List<Unit> playerParty;
    public const int MAX_PARTY_SIZE = 20;
    public const string UNIT_SAVE_DATA_FILE_NAME = "party_member_";

    private void LoadPlayersParty()
    {
        List<SerializableUnit> units = new List<SerializableUnit>();
        for(int i = 1; i <= MAX_PARTY_SIZE; i++)
        {
            string unitJson = SaveDataManager.LoadString(UNIT_SAVE_DATA_FILE_NAME + i.ToString(), null);
            if(!string.IsNullOrWhiteSpace(unitJson))
            {
                units.Add(JsonUtility.FromJson<SerializableUnit>(unitJson));
            }
        }

        foreach(SerializableUnit unit in units) Debug.Log(unit.ToString());
    }

    private void SavePlayersParty()
    {
        for(int i = 0; i < playerParty.Count; i++)
        {
            string unitSaveName = UNIT_SAVE_DATA_FILE_NAME + (i+1).ToString();
            SaveDataManager.SaveObject(unitSaveName, new SerializableUnit(playerParty[i]));
        }
    }
}