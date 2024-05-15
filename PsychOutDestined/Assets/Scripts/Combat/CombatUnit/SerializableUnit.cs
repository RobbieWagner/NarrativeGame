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
        public int Stress;


        public int MaxHP;
        public int MaxStress;
        public int Brawn;
        public int Agility;
        public int Defense;
        public int Psych;
        public int Focus;
        public int Heart;

        public List<string> actionFilePaths;
        public string animatorFilePath;
        public string headSpriteRelativePath;

        //TODO: add refs for unitanimator.

        public SerializableUnit(Unit unit)
        {
            UnitName = unit.UnitName;
            HP = unit.HP;
            Stress = unit.Stress;

            MaxHP = unit.GetMaxStatValue(UnitStat.HP);
            MaxStress = unit.GetMaxStatValue(UnitStat.Stress);
            Brawn = unit.GetMaxStatValue(UnitStat.Brawn);
            Agility = unit.GetMaxStatValue(UnitStat.Agility);
            Defense = unit.GetMaxStatValue(UnitStat.Defense);
            Psych = unit.GetMaxStatValue(UnitStat.Psych);
            Focus = unit.GetMaxStatValue(UnitStat.Focus);
            Heart = unit.GetMaxStatValue(UnitStat.Heart);

            actionFilePaths = unit.availableActions.Select(a => StaticGameStats.GetCombatActionResourcePath(a)).ToList();
            animatorFilePath = unit.GetAnimatorResourcePath();
        }

        public SerializableUnit() {}
        public override string ToString()
        {
            return $"-----\n{UnitName}:\nHP:{HP}\nMana:{Stress}\nBrawn:{Brawn}\nAgility:{Agility}\nDefense:{Defense}\nPsych:{Psych}\nFocus:{Focus}\nHeart:{Heart}\nActions:{string.Join(",", actionFilePaths)}\nAnimator:{animatorFilePath}\n-----";
        }
    }
}