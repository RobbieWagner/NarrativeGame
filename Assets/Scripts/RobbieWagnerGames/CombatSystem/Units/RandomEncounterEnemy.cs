using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobbieWagnerGames;

namespace RobbieWagnerGames.StrategyCombat.Units
{
    public class RandomEncounterEnemy : Enemy
    {

        [SerializeField] private int difficultyClass;

        protected override void Awake()
        {
            baseHP = difficultyClass * 10 + UnityEngine.Random.Range(difficultyClass + 1, (difficultyClass + 1) * 10);
            SetupUnit();
            HP = maxHP;

            base.Awake();
        }
    }
}
