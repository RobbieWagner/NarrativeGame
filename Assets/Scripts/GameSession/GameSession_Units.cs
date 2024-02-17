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
    [SerializeField] public List<SerializableUnit> playerParty;
    public const int MAX_PARTY_SIZE = 20;
    public const string UNIT_PARTY_SAVE_KEY = "CurrentParty";
    public const string UNIT_SAVE_DATA_FILE_NAME = "party_data";

    private void LoadPlayersParty()
    {
        playerParty = new List<SerializableUnit>();
        int i = 0;
        while(i < MAX_PARTY_SIZE)
        {
            SerializableUnit unit = SaveDataManager.LoadObject<SerializableUnit>($"{UNIT_PARTY_SAVE_KEY}_{i+1}", UNIT_SAVE_DATA_FILE_NAME, null, null);
            if(unit == null) break;
            playerParty.Add(unit);
            i++;
        }

        foreach(SerializableUnit unit in playerParty) Debug.Log(unit.ToString());

        if(playerParty.Count == 0) Debug.LogWarning("Player does not have any save data for current party!");
    }

    private void SavePlayersParty()
    {
        for(int i = 0; i < playerParty.Count; i++)
        {
            SaveDataManager.SaveObject($"{UNIT_PARTY_SAVE_KEY}_{i+1}", playerParty[i], UNIT_SAVE_DATA_FILE_NAME);
            Debug.Log($"{UNIT_PARTY_SAVE_KEY}_{i+1}");
        }
        //SaveDataManager.SaveObject(UNIT_PARTY_SAVE_KEY, playerParty, UNIT_SAVE_DATA_FILE_NAME);
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