using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using AYellowpaper.SerializedCollections;

namespace PsychOutDestined
{
    public enum MentalityType
    {
        NONE = -1,
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
        CONNECTED
    }

    public class MentalityManager: MonoBehaviour
    {
        public MentalityType baseMentalityType = MentalityType.FINE;
        [SerializedDictionary("Type","Mentality")][SerializeField] private SerializedDictionary<MentalityType, Mentality> mentalities;

        public static MentalityManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        public Mentality GetMentality(MentalityType mentalityType)
        {
            if(mentalities.TryGetValue(mentalityType, out var mentality))
                return mentality;
            Debug.LogWarning($"Error retrieving mentality: mentality type {mentalityType} is not defined in mentalities list!!");
            return mentalities[baseMentalityType];
        }

        public bool ApplyMentality(Unit unit, MentalityType mentalityType)
        {
            if(mentalities.TryGetValue(mentalityType, out Mentality mentality))
                return unit.SetMentality(mentalityType, mentality);
            return false;
        }

        public bool RemoveCurrentMentality(Unit unit)
        {
            return ApplyMentality(unit, baseMentalityType);
        }
    }


}
