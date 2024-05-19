using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public enum MentalityType
    {
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
        TRUSTING
    }

    [CreateAssetMenu(menuName = "Mentality")]
    public class Mentality : ScriptableObject
    {
        public MentalityType mentalityType;
        [SerializeReference] public List<MentalityEffect> effects;
    }


}