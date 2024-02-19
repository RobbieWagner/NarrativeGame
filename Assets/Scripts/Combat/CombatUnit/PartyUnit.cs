using UnityEditor.Rendering;
using UnityEngine;

namespace PsychOutDestined
{
    public class PartyUnit : Unit
    {
        public SerializableUnit serializableUnit;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void InitializeUnit()
        {
            if (serializableUnit != null)
            {
                UnitName = serializableUnit.UnitName;
                maxStatValues[UnitStat.Brawn] = serializableUnit.Brawn;
                maxStatValues[UnitStat.Agility] = serializableUnit.Agility;
                maxStatValues[UnitStat.Defense] = serializableUnit.Defense;
                maxStatValues[UnitStat.Psych] = serializableUnit.Psych;
                maxStatValues[UnitStat.Focus] = serializableUnit.Focus;
                maxStatValues[UnitStat.Heart] = serializableUnit.Heart;

                InitializeStats();

                HP = serializableUnit.HP;
                Mana = serializableUnit.Mana;

                //Load animator/base sprite
                SetUnitAnimatorState(UnitAnimationState.CombatIdleRight);
            }
            else
            {
                Debug.LogWarning($"Could not find save data for unit {UnitName}");
                base.InitializeUnit();
            }
        }
    }
}