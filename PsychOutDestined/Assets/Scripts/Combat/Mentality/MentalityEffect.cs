using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PsychOutDestined
{
    [Serializable]
    public class MentalityEffect
    {
        public virtual bool ApplyMentalityEffect(Unit unit)
        {
            return false;
        }

        public virtual bool RemoveMentalityEffect(Unit unit)
        {
            return false;
        }
    }

    [Serializable]
    public class EmptyMentalityEffect : MentalityEffect 
    {
        public override bool ApplyMentalityEffect(Unit unit)
        {
            return true;
        }

        public override bool RemoveMentalityEffect(Unit unit)
        {
            return true;
        }
    }

    [Serializable]
    // Adds multipliers to the units stat multipliers
    public class StatMultiplier: MentalityEffect
    {
        public UnitStat stat;
        public float multiplier = 1;

        public override bool ApplyMentalityEffect(Unit unit)
        {
            if (multiplier != 0 && !(stat == UnitStat.HP || stat == UnitStat.Stress))
            {
                //Makes sure there is at least some difference in the multiplier
                unit.ModifyStatValue(stat, multiplier);
                if (multiplier > 1)
                    unit.ModifyStatValue(stat, 1);
                else if (multiplier < 1)
                    unit.ModifyStatValue(stat, -1);

                return true;
            }

            return false;
        }

        public override bool RemoveMentalityEffect(Unit unit)
        {
            if (multiplier != 0 && !(stat == UnitStat.HP || stat == UnitStat.Stress))
            {
                //Makes sure to remove the offset change made from multiplier application
                if (multiplier > 1)
                    unit.ModifyStatValue(stat, -1);
                else if(multiplier < 1)
                    unit.ModifyStatValue(stat, 1);

                unit.ModifyStatValue(stat, 1/multiplier);
                return true;
            }

            return false;
        }
    }

    [Serializable]
    // Adds modifiers to the units stat modifiers
    public class StatModifier: MentalityEffect
    {
        public UnitStat stat;
        public int modifier = 0;

        public override bool ApplyMentalityEffect(Unit unit)
        {
            if (modifier != 0 && !(stat == UnitStat.HP || stat == UnitStat.Stress))
            {
                unit.ModifyStatValue(stat, modifier);
                return true;
            }

            return false;
        }

        public override bool RemoveMentalityEffect(Unit unit)
        {
            if (!(stat == UnitStat.HP || stat == UnitStat.Stress))
            {
                unit.ModifyStatValue(stat, -modifier);
                return true;
            }

            return false;
        }
    }

    [Serializable]
    // Applies the Psyched Out condition
    public class PsychOut : MentalityEffect
    {
        public string PSYCH_OUT = "PSYCH OUT";
        public override bool ApplyMentalityEffect(Unit unit)
        {
            return true;
        }

        public override bool RemoveMentalityEffect(Unit unit)
        {
            return true;
        }
    }
}