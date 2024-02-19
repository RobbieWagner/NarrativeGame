using UnityEngine;
using System;
using System.Collections.Generic;

namespace PsychOutDestined
{
    public class ICombat : MonoBehaviour
    {
        public List<Unit> allyPrefabs;
        public List<Unit> enemyPrefabs;
        public ICombatUI combatUIPrefab;
    }
}