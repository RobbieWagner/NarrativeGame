using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PsychOutDestined
{
    [CreateAssetMenu(menuName = "Mentality")]
    public class Mentality : ScriptableObject
    {
        [SerializeReference] public List<MentalityEffect> effects;

        [ContextMenu("PSYCH OUT")] void AddPsychOutEffect() { effects.Add(new PsychOut()); }
        [ContextMenu("StatMultipliers")] void AddStatMultiplierEffect() { effects.Add(new StatModifier()); }
        [ContextMenu("StatModifier")] void AddStatModifierEffect() { effects.Add(new StatMultiplier()); }
        [ContextMenu("CLEAR")] void Clear() { effects.Clear(); }

        public bool RemoveMentalityEffects(Unit unit)
        {
            if(effects != null && effects.Any() && effects.First().RemoveMentalityEffect(unit))
            {
                int failed = 0;
                for(int i = 1; i < effects.Count; i++)
                {
                    if(effects[i].RemoveMentalityEffect(unit))
                        failed++;
                }
                if (failed > 0)
                    Debug.Log($"Failed to remove {failed} effects from mentality");
            }
            Debug.LogWarning($"Failed to Remove old Mentality Effects from unit {unit.UnitName}: no valid effects detected");
            return false;
        }

        public bool ApplyMentalityEffects(Unit unit)
        {
            if (effects != null && effects.Any() && effects.First().ApplyMentalityEffect(unit))
            {
                int failed = 0;
                for (int i = 1; i < effects.Count; i++)
                {
                    if (!effects[i].ApplyMentalityEffect(unit))
                        failed++;
                }
                if (failed > 0)
                    Debug.Log($"Failed to apply {failed} effects from mentality");
                return true;
            }
            Debug.LogWarning($"Failed to Apply new Mentality Effects to unit {unit.UnitName}: no valid effects detected");
            return false;
        }
    }
}