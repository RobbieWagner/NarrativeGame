using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobbieWagnerGames.CombatSystem
{
    public enum CombatPhase
    {
        //None: Not in combat, or combat interrupted
        NONE = 0,
        //Setup: Setup the combat (display "fight screen", initialize units, etc.)
        SETUP = 1,
        //Turn Start: Apply any turn based conditions, check for game over status
        TURN_START = 2,
        //Action Selection: Each unit selects what action they will take that turn. Can also look up battle info
        ACTION_SELECTION = 3,
        //Action Execution: Execute each selected action one at a time: Continuously checking for a game over status
        ACTION_EXECUTION = 4,
        //Turn End: Same as Turn Start, but at the end of the turn
        TURN_END = 5,
        //Resolve: Resolve the current combat. Check which side won, and run the corresponding code.
        RESOLVE = 6
    }

    public class CombatManager : MonoBehaviour
    {
        private ICombat currentCombat;
        public bool canChangePhase = true;
        private CombatPhase currentPhase =  CombatPhase.NONE;
        public CombatPhase CurrentPhase
        {
            get { return currentPhase; }
            set 
            {
                if(currentPhase == value || currentCombat == null || !canChangePhase) return;
                currentPhase = value;
                OnChangeCombatPhase?.Invoke(currentPhase);
                currentCombat.ChangeCombatPhase(currentPhase);
                Debug.Log(currentPhase.ToString());
            }
        }
        
        public delegate void OnChangeCombatPhaseDelegate(CombatPhase combatPhase);
        public event OnChangeCombatPhaseDelegate OnChangeCombatPhase;

        public static CombatManager Instance {get; private set;}

        private void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
            } 
            else 
            { 
                Instance = this; 
            } 
        }

        public bool StartCombat(ICombat combat)
        {
            Debug.Log("Attempting to Start combat");
            if(CurrentPhase != CombatPhase.NONE || currentCombat != null) return false;

            Debug.Log("Combat started");
            currentCombat = combat;
            CurrentPhase = CombatPhase.SETUP;
            return true;
        }

        public bool EndCombat(ICombat combat)
        {
            if(currentCombat.Equals(combat) && CurrentPhase == CombatPhase.NONE)
            {
                //tear things down
                return true;
            }

            return false;
        }
    }
}