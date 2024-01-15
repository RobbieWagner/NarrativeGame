using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobbieWagnerGames;

namespace RobbieWagnerGames.StrategyCombat
{
    [Serializable]
    public class UnitStatInfo
    {
        private Unit unit;

        private int baseValue;
        private int statValue;
        [HideInInspector] public int StatValue
        {
            get { return statValue; }
            set
            {
                if(value == statValue) return;

                OnStatRaisedOrLowered?.Invoke(value - statValue, unit, Color.yellow);
                statValue = value;
                OnStatSet?.Invoke(statValue);
            }
        }

        public delegate void OnStatSetDelegate(int newValue);
        public event OnStatSetDelegate OnStatSet;

        public delegate void OnStatRaisedOrLoweredDelegate(int difference, Unit affectedUnit, Color color);
        public event OnStatRaisedOrLoweredDelegate OnStatRaisedOrLowered;

        public void init(Unit newUnit, int baseStatValue = 10)
        {
            baseValue = baseStatValue;
            statValue = baseValue;

            unit = newUnit;
        }
    }
}