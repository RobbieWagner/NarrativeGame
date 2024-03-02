using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PsychOutDestined
{
    public partial class CombatManager : CombatManagerBase
    {
        [SerializeField] private Battlefield battleField;

        #region Combat Phases
        protected override IEnumerator SetupCombat()
        {
            yield return StartCoroutine(battleField?.SetupBattlefield());

            currentUI = Instantiate(currentCombat.combatUIPrefab, transform);
            yield return StartCoroutine(currentUI.InitializeUI());

            allies = new List<Unit>();
            enemies = new List<Unit>();

            List<SerializableUnit> playerParty = GameSession.Instance != null ? GameSession.Instance.playerParty : null;
            if(playerParty != null && usePartyUnits)
            {
                for(int i = 0; i < unitLimit; i++)
                {
                    if(playerParty.Count == i) break;
                    if(TryAddAllyToCombat(GameSession.Instance.partyUnitPrefab))
                    {
                        PartyUnit ally = allies[i] as PartyUnit;
                        ally.InitializeUnit(GameSession.Instance.GetPartyMember(i));
                    }
                }
            }
            else
            {
                foreach (Unit ally in currentCombat.allyPrefabs)
                    TryAddAllyToCombat(ally);
            }
            foreach (Unit enemy in currentCombat.enemyPrefabs)
                TryAddEnemyToCombat(enemy);

            battleField?.PlaceUnits(allies, true);
            battleField?.PlaceUnits(enemies, false);

            yield return StartCoroutine(base.SetupCombat());
        }

        protected override IEnumerator StartTurn()
        {
            yield return StartCoroutine(base.StartTurn());
        }

        protected override IEnumerator HandleActionSelectionPhase()
        {
            yield return StartCoroutine(base.HandleActionSelectionPhase());
        }

        protected override IEnumerator ExecuteUnitAction()
        {

            yield return StartCoroutine(base.ExecuteUnitAction());
        }

        protected override IEnumerator EndTurn()
        {

            yield return StartCoroutine(base.EndTurn());
        }

        protected override IEnumerator ResolveCombat()
        {

            yield return StartCoroutine(base.ResolveCombat());
        }
        #endregion

        protected override bool TryAddAllyToCombat(Unit ally)
        {
            return base.TryAddAllyToCombat(ally);
        }

        protected override bool TryAddEnemyToCombat(Unit enemy)
        {
            return base.TryAddEnemyToCombat(enemy);
        }
    }
}