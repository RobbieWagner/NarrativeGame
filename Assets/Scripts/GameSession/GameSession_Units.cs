using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RobbieWagnerGames;
using UnityEngine;

public partial class GameSession : MonoBehaviour
{
    [Header("Player Party")]
    public PartyUnit partyUnitPrefab;
    public List<SerializableUnit> playerParty;
    public const int MAX_PARTY_SIZE = 20;
    public const string UNIT_PARTY_SAVE_KEY = "CurrentParty";
    public const string UNIT_SAVE_DATA_FILE_NAME = "party_data";

    private void LoadPlayersParty()
    {
        List<SerializableUnit> units = new List<SerializableUnit>();
        playerParty = SaveDataManager.LoadObject<List<SerializableUnit>>(UNIT_PARTY_SAVE_KEY, new string[]{UNIT_SAVE_DATA_FILE_NAME});

        foreach(SerializableUnit unit in units) Debug.Log(unit.ToString());
    }

    private void SavePlayersParty()
    {
        SaveDataManager.SaveObject(UNIT_PARTY_SAVE_KEY, playerParty, new string[]{UNIT_SAVE_DATA_FILE_NAME});
    }

    public SerializableUnit GetPartyMember(int unitIndex)
    {
        if(unitIndex >= 0 && unitIndex < playerParty.Count) 
            return playerParty[unitIndex];
        Debug.LogWarning($"Could not retrieve party member {unitIndex}: Index was outside the bounds of the party list");
        return null;
    }

    public void UpdatePartyData(Dictionary<int, SerializableUnit> units)
    {
        foreach(KeyValuePair<int, SerializableUnit> unit in units)
        {
            if(unit.Key >= 0 && unit.Key < playerParty.Count)
                playerParty[unit.Key] = unit.Value;
            else if(unit.Key >= playerParty.Count && playerParty.Count < MAX_PARTY_SIZE)
                playerParty.Add(unit.Value);
            else
                Debug.LogWarning($"Could not add unit {unit.Value.UnitName} to party: Either party is too full, or the index provided {unit.Key} was negative");
        }
    }
}