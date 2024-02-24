using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PsychOutDestined
{
    [System.Serializable]
    public class SerializableUnit
    {
        public string UnitName;
        public int HP;
        public int Mana;


        public int MaxHP;
        public int MaxMana;
        public int Brawn;
        public int Agility;
        public int Defense;
        public int Psych;
        public int Focus;
        public int Heart;

        public List<string> actionFilePaths;

        //TODO: add refs for unitanimator.

        public SerializableUnit(Unit unit)
        {
            UnitName = unit.UnitName;
            HP = unit.HP;
            Mana = unit.Mana;

            MaxHP = unit.GetMaxStatValue(UnitStat.HP);
            MaxMana = unit.GetMaxStatValue(UnitStat.Mana);
            Brawn = unit.GetMaxStatValue(UnitStat.Brawn);
            Agility = unit.GetMaxStatValue(UnitStat.Agility);
            Defense = unit.GetMaxStatValue(UnitStat.Defense);
            Psych = unit.GetMaxStatValue(UnitStat.Psych);
            Focus = unit.GetMaxStatValue(UnitStat.Focus);
            Heart = unit.GetMaxStatValue(UnitStat.Heart);

            actionFilePaths = unit.availableActions.Select(a => StaticGameStats.GetCombatActionResourcePath(a)).ToList();
        }

        public SerializableUnit()
        {
            
        }

        public override string ToString()
        {
            return $"-----\n{UnitName}:\nHP:{HP}\nMana:{Mana}\nBrawn:{Brawn}\nAgility:{Agility}\nDefense:{Defense}\nPsych:{Psych}\nFocus:{Focus}\nHeart:{Heart}\n-----";
        }
    }
}