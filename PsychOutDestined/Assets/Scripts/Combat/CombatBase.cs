using UnityEngine;
using System;
using System.Collections.Generic;

namespace PsychOutDestined
{
    public class CombatBase : MonoBehaviour
    {
        public List<Unit> allyPrefabs;
        public List<Unit> enemyPrefabs;
        public CombatUIBase combatUIPrefab;
    }
}