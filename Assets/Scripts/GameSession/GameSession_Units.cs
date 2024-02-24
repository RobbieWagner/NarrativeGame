using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RobbieWagnerGames;
using UnityEngine;

namespace PsychOutDestined
{
    public partial class GameSession : MonoBehaviour
    {
        [Header("Player Party")]
        public PartyUnit partyUnitPrefab;
        [SerializeField] public List<SerializableUnit> playerParty;
        [SerializeField] public List<Unit> units;
        public const int MAX_PARTY_SIZE = 20;
        public const string PARTY_SAVE_DATA_FILE_PATH = "/Combat/party";

        private void LoadPlayersParty()
        {
            units = JsonDataService.Instance.LoadData(PARTY_SAVE_DATA_FILE_PATH + "_units", new List<Unit>(), false);
            foreach (Unit unit in units) 
                Debug.Log(unit?.ToString());
            if (playerParty.Count == 0) Debug.LogWarning("Player does not have any save data for current party!");
        }

        private void SavePlayersParty()
        {
            JsonDataService.Instance.SaveData(PARTY_SAVE_DATA_FILE_PATH, playerParty);
            JsonDataService.Instance.SaveData(PARTY_SAVE_DATA_FILE_PATH + "_units", units);
        }

        public SerializableUnit GetPartyMember(int unitIndex)
        {
            if (unitIndex >= 0 && unitIndex < playerParty.Count)
                return playerParty[unitIndex];
            Debug.LogWarning($"Could not retrieve party member {unitIndex}: Index was outside the bounds of the party list");
            return null;
        }

        public void UpdatePartyData(Dictionary<int, SerializableUnit> units)
        {
            foreach (KeyValuePair<int, SerializableUnit> unit in units)
            {
                if (unit.Key >= 0 && unit.Key < playerParty.Count)
                    playerParty[unit.Key] = unit.Value;
                else if (unit.Key >= playerParty.Count && playerParty.Count < MAX_PARTY_SIZE)
                    playerParty.Add(unit.Value);
                else
                    Debug.LogWarning($"Could not add unit {unit.Value.UnitName} to party: Either party is too full, or the index provided {unit.Key} was negative");
            }
        }
    }
}