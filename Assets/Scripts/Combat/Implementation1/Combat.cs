using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames.CombatSystem
{
    public class Combat : ICombat
    {

        protected override void EndCombatPhase(CombatPhase combatPhase)
        {
            base.EndCombatPhase(combatPhase);

            switch (combatPhase)
            {
                case CombatPhase.SETUP:
                    CombatManager.Instance.CurrentPhase = CombatPhase.TURN_START;
                    break;
                case CombatPhase.TURN_START:
                    if (CheckForEndOfCombat()) CombatManager.Instance.CurrentPhase = CombatPhase.RESOLVE;
                    else CombatManager.Instance.CurrentPhase = CombatPhase.ACTION_SELECTION;
                    break;
                case CombatPhase.ACTION_SELECTION:
                    CombatManager.Instance.CurrentPhase = CombatPhase.ACTION_EXECUTION;
                    break;
                case CombatPhase.ACTION_EXECUTION:
                    if (CheckForEndOfCombat()) CombatManager.Instance.CurrentPhase = CombatPhase.RESOLVE;
                    CombatManager.Instance.CurrentPhase = CombatPhase.TURN_END;
                    break;
                case CombatPhase.TURN_END:
                    if (CheckForEndOfCombat()) CombatManager.Instance.CurrentPhase = CombatPhase.RESOLVE;
                    CombatManager.Instance.CurrentPhase = CombatPhase.TURN_START;
                    break;
                case CombatPhase.RESOLVE:
                    CombatManager.Instance.CurrentPhase = CombatPhase.NONE;
                    break;
                default:
                    break;
            }
        }

        public override IEnumerator SetupCombat()
        {
            Debug.Log("setup start");

            yield return StartCoroutine(base.SetupCombat());
            Debug.Log("setup complete");
            EndCombatPhase(CombatPhase.SETUP);
        }

        public override IEnumerator StartNewTurn()
        {
            Debug.Log("Starting new turn");
            yield return new WaitForSeconds(.5f);

            yield return StartCoroutine(base.StartNewTurn());
            Debug.Log("New turn started");
            EndCombatPhase(CombatPhase.TURN_START);
        }

        public override IEnumerator RunActionSelection()
        {
            Debug.Log("Selecting Actions");
            yield return new WaitForSeconds(.5f);

            yield return StartCoroutine(base.RunActionSelection());
            Debug.Log("Actions Selected");
            EndCombatPhase(CombatPhase.ACTION_SELECTION);
        }

        public override IEnumerator ExecuteCombatActions()
        {
            Debug.Log("Executing Actions");
            yield return new WaitForSeconds(.5f);

            yield return StartCoroutine(base.ExecuteCombatActions());
            Debug.Log("Actions Executed");
            EndCombatPhase(CombatPhase.ACTION_EXECUTION);
        }

        public override IEnumerator EndTurn()
        {
            Debug.Log("Ending Turn");
            yield return new WaitForSeconds(.5f);

            yield return StartCoroutine(base.EndTurn());
            Debug.Log("Turn Ended");
            EndCombatPhase(CombatPhase.TURN_END);
        }

        public override IEnumerator ResolveCombat()
        {
            Debug.Log("Resolving combat");
            yield return new WaitForSeconds(.5f);

            yield return StartCoroutine(base.ResolveCombat());
            Debug.Log("Combat Complete");
            EndCombatPhase(CombatPhase.RESOLVE);
        }

        public override IEnumerator CheckForCombatInterruptions()
        {
            yield return null;
            StopCoroutine(CheckForCombatInterruptions());
        }
    }
}