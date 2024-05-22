using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace PsychOutDestined
{
    public class PartyCombatManager : CombatManagerBase
    {
        [SerializeField] private Battlefield battleField;
        [SerializeField] private bool saveDataAfterwards = false;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override IEnumerator SetupCombat()
        {
            yield return StartCoroutine(battleField?.SetupBattlefield());

            currentUI = Instantiate(currentCombat.combatUIPrefab, transform);
            yield return StartCoroutine(currentUI.InitializeUI());

            allies = new List<Unit>();
            enemies = new List<Unit>();

            for (int i = 0; i < unitLimit && i < GameSession.Instance.playerParty.Count; i++)
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
                    instantiatedUnit = Instantiate(GameSession.Instance.partyUnitPrefab, transform);
                    instantiatedUnit.InitializeUnit(GameSession.Instance.GetPartyMember(allies.Count));
                }
                if (instantiatedUnit == null)
                {
                    Debug.LogWarning("Could not instantiate party unit: GameSession instance is not active in heirrarchy");
                    return false;
                }


                allies.Add(instantiatedUnit);
                InvokeOnAddNewAlly(instantiatedUnit);
                return true;
            }
            else return false;
        }

        public void SwitchActiveUnits(int unitToSwitchOut, int unitToSwitchIn)
        {
            //TODO: Implement. Do we want the switch to be saved in game session data, or left alone? (probably saved)
            throw new NotImplementedException();
        }

        protected override IEnumerator ResolveCombat(bool endingEarly = false)
        {
            yield return StartCoroutine(base.ResolveCombat());
            if(saveDataAfterwards)
                GameSession.Instance.playerParty = allies.Select(a => new SerializableUnit(a)).ToList();
        }
    }
}