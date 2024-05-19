using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public enum MentalityType
    {
        PSYCHED_OUT,
        FINE,
        
        HAPPY,
        CONFIDENT,
        SAD,
        FATIGUED,
        SHOCKED,
        HOPELESS,
        EXHAUSTED,
        PROTECTIVE,
        DISTRESSED,
        TRAUMATIZED,
        DETERMINED,
        UPLIFTED,
        TRUSTING,
    }

    public class Mentality
    {
        public MentalityType type;
    }
}