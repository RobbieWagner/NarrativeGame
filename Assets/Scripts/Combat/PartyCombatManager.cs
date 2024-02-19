using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace PsychOutDestined
{
    public class PartyCombatManager : ICombatManager
    {
        [SerializeField] private Battlefield battleField;
        [SerializeField] private bool saveDataAfterwards = false;

        protected override void Awake()
        {
            base.Awake();
            if (saveDataAfterwards)
                OnCombatTerminated += UpdateGameSessionDataPostFight;
        }

        protected override IEnumerator SetupCombat()
        {
            yield return StartCoroutine(battleField?.SetupBattlefield());

            currentUI = Instantiate(currentCombat.combatUIPrefab, transform);
            yield return StartCoroutine(currentUI.InitializeUI());

            allies = new List<Unit>();
            enemies = new List<Unit>();

            for (int i = 0; i < 3; i++)
                TryAddAllyToCombat(null);
            foreach (Unit enemy in currentCombat.enemyPrefabs)
                TryAddEnemyToCombat(enemy);

            battleField?.PlaceUnits(allies, true);
            battleField?.PlaceUnits(enemies, false);

            yield return StartCoroutine(base.SetupCombat());
        }

        protected override bool TryAddAllyToCombat(Unit ally)
        {
            PartyUnit instantiatedUnit = null;
            if (allies.Count < 3)
            {
                if (GameSession.Instance != null)
                {
                    instantiatedUnit = Instantiate(GameSession.Instance.partyUnitPrefab);
                    instantiatedUnit.serializableUnit = GameSession.Instance.GetPartyMember(allies.Count);
                }
                if (instantiatedUnit == null)
                {
                    Debug.LogWarning("Could not instantiate party unit: GameSession instance is not active in heirrarchy");
                    return false;
                }
                if (instantiatedUnit.serializableUnit == null)
                {
                    Debug.LogWarning("Could not instantiate party unit: Unit Serializable found null");
                    Destroy(instantiatedUnit.gameObject);
                    return false;
                }


                allies.Add(instantiatedUnit);
                InvokeOnAddNewAlly(instantiatedUnit);
                return true;
            }
            else return false;
        }

        private void UpdateGameSessionDataPostFight()
        {
            Dictionary<int, SerializableUnit> activeParty = new Dictionary<int, SerializableUnit>();

            for (int i = 0; i < allies.Count; i++)
            {
                activeParty.Add(0, new SerializableUnit(allies[i]));
            }

            UpdateGameSessionData(activeParty);
        }

        public void SwitchActiveUnits(int unitToSwitchOut, int unitToSwitchIn)
        {
            //TODO: Implement. Do we want the switch to be saved in game session data, or left alone? (probably saved)
            throw new NotImplementedException();
        }

        private void UpdateGameSessionData(Dictionary<int, SerializableUnit> units)
        {
            if (GameSession.Instance != null)
                GameSession.Instance.UpdatePartyData(units);
            else
                Debug.LogWarning($"Could not append data to Game Session: Game Session not found");
        }
    }
}