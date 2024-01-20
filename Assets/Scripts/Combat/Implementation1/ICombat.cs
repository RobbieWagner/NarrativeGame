using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames.CombatSystem
{
    public class ICombat : MonoBehaviour
    {

        public List<Unit> allies;
        public List<Unit> enemies;
        public Coroutine currentPhaseCoroutine = null;

        protected virtual void Awake()
        {
            CombatManager.Instance?.StartCombat(this);
        }
        
        //DO NOT CALL FROM WITHIN COMBAT. Instead, call CombatManager.Instance.CombatPhase = 
        public void ChangeCombatPhase(CombatPhase newPhase)
        {
            if(currentPhaseCoroutine != null) StopCoroutine(currentPhaseCoroutine);

            switch(newPhase)
            {
                case CombatPhase.NONE:
                EndCombat();
                break;
                case CombatPhase.SETUP:
                currentPhaseCoroutine = StartCoroutine(SetupCombat());
                break;   
                case CombatPhase.TURN_START:
                currentPhaseCoroutine = StartCoroutine(StartNewTurn());
                break; 
                case CombatPhase.ACTION_SELECTION:
                currentPhaseCoroutine = StartCoroutine(RunActionSelection());
                break; 
                case CombatPhase.ACTION_EXECUTION:
                currentPhaseCoroutine = StartCoroutine(ExecuteCombatActions());
                break; 
                case CombatPhase.TURN_END:
                currentPhaseCoroutine = StartCoroutine(EndTurn());
                break;
                case CombatPhase.RESOLVE:
                currentPhaseCoroutine = StartCoroutine(ResolveCombat());
                break; 
                default:
                break;
            }
        }

        protected virtual void EndCombatPhase(CombatPhase combatPhase)
        {
            StopCoroutine(currentPhaseCoroutine);
        }

        // 1: Define the units of the combat (Are they defined outright, randomization, etc?)
        // 2: Tell the Battle Field to place those units
        // 3: Spawn/Setup UI
        // 4: Play "FIGHT" Screen
        public virtual IEnumerator SetupCombat()
        {
            yield return StartCoroutine(CheckForCombatInterruptions());
        }

        // 1: Apply any conditions triggered on turn start
        // 2: Determine any changes to the battlefield
        // 3: Check for win condition
        public virtual IEnumerator StartNewTurn()
        {
            yield return StartCoroutine(CheckForCombatInterruptions());
        }

        // 1: Foreach ally
        //  if the ally is still able to act, allow them to select an action
        // 2: Allow the user to cancel a selection, and also to pass turn (make pass turn an action?)
        // 3: Store selected actions
        // 4: Decide on AI enemy actions
        public virtual IEnumerator RunActionSelection()
        {
            yield return StartCoroutine(CheckForCombatInterruptions());
        }

        // 1: Foreach selected actions
        //    run yield return action.execute
        // 2: Continuously check for combat completion
        // 3: Display changes to UI dynamically
        public virtual IEnumerator ExecuteCombatActions()
        {
            yield return StartCoroutine(CheckForCombatInterruptions());
        }

        // See turn start
        public virtual IEnumerator EndTurn()
        {
            yield return StartCoroutine(CheckForCombatInterruptions());
        }

        // 1: Determine Victory or Defeat
        // 2: Display a combat end screen
        // 3: if lose, see if it results in game over
        // 4: run end combat, which will tear down the combat instance
        public virtual IEnumerator ResolveCombat()
        {
            yield return StartCoroutine(CheckForCombatInterruptions());
        }

        public virtual void EndCombat()
        {
            CombatManager.Instance.EndCombat(this);
        }

        public virtual IEnumerator CheckForCombatInterruptions()
        {
            if(false && CombatManager.Instance != null)
            {
                CombatManager.Instance.canChangePhase = false;
                while(!CombatManager.Instance.canChangePhase)
                {
                    yield return null;
                }
            }

            StopCoroutine(CheckForCombatInterruptions());
        }

        protected virtual bool CheckForEndOfCombat()
        {
            return false;
        }
    }
}