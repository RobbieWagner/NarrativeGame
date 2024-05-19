using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public enum ModificationType
    {
        RAISE_REDUCE,
        MULTIPIER
    }

    [Serializable]
    public class MentalityEffectModel
    {
        ModificationType modificationType;
    }

    public class MentalityEffect
    {
        public List<MentalityEffectModel> effects;
        public virtual void ApplyMentalityEffect()
        {

        }

        public virtual void RemoveMentalityEffect()
        {

        }
    }
}