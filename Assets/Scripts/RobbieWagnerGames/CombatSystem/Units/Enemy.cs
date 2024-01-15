using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobbieWagnerGames;

namespace RobbieWagnerGames.StrategyCombat.Units
{
    public class Enemy : Unit
    {

        protected override void Awake() 
        {
            unitAnimator.ChangeAnimationState(UnitAnimationState.CombatIdleLeft);
            base.Awake(); 
        }
    
    }
}