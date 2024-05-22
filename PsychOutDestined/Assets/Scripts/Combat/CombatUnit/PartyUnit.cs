using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using System;

namespace PsychOutDestined
{
    public class PartyUnit : Unit
    {
        [HideInInspector] protected SerializableUnit unitSaveData;

        protected override void Awake()
        {
            base.Awake();
        } 

        public SerializableUnit GetUnitSaveData()
        {
            unitSaveData = new SerializableUnit(this);
            return unitSaveData;
        }

        public void InitializeUnit(SerializableUnit unitData)
        {
            if(unitData == null) return;
            unitSaveData = unitData;
            //Debug.Log(unitSaveData.ToString());
            InitializeUnit();
        }

        protected override void InitializeUnit()
        {
            if (unitSaveData != null)
            {
                UnitName = unitSaveData.UnitName;
                maxStatValues[UnitStat.Brawn] = unitSaveData.Brawn;
                maxStatValues[UnitStat.Agility] = unitSaveData.Agility;
                maxStatValues[UnitStat.Defense] = unitSaveData.Defense;
                maxStatValues[UnitStat.Psych] = unitSaveData.Psych;
                maxStatValues[UnitStat.Focus] = unitSaveData.Focus;
                maxStatValues[UnitStat.Heart] = unitSaveData.Heart;

                animatorControllerPath = unitSaveData.animatorFilePath;
                if(!string.IsNullOrWhiteSpace(animatorControllerPath))
                    animatorControllerPath = animatorControllerPath.Replace(StaticGameStats.combatAnimatorFilePath + "/", "");

                RuntimeAnimatorController animatorController = Resources.Load<RuntimeAnimatorController>(GetAnimatorResourcePath());
                if(animatorController == null) 
                    animatorController = Resources.Load<RuntimeAnimatorController>(StaticGameStats.defaultCombatAnimatorFilePath);
                if(!unitAnimator.SetAnimator(animatorController))
                    Debug.LogWarning($"Failed to set Animator Controller on unit: {UnitName}");

                availableActions = new List<CombatAction>();
                foreach(string combatActionPath in unitSaveData.actionFilePaths)
                {
                    CombatAction action = Resources.Load<CombatAction>($"{StaticGameStats.combatActionFilePath}/{combatActionPath}");
                    if(action != null) 
                        availableActions.Add(action);
                }

                headSprite = Resources.Load<Sprite>(string.IsNullOrWhiteSpace(unitSaveData.headSpriteRelativePath) ? GetHeadSpriteResourcePath() : $"{StaticGameStats.headSpriteFilePath}/{unitSaveData.headSpriteRelativePath}");

                InitializeStats();

                HP = unitSaveData.HP;
                Stress = unitSaveData.Stress;

                //Load animator/base sprite
                SetUnitAnimatorState(UnitAnimationState.CombatIdleRight);

                Enum.TryParse(unitSaveData.mentality, true, out currentMentalityType);
                if(currentMentalityType == MentalityType.NONE)
                    currentMentalityType = MentalityType.FINE;

                currentMentality = MentalityManager.Instance.GetMentality(currentMentalityType);
                if (currentMentality == null)
                    currentMentality = MentalityManager.Instance.GetMentality(MentalityManager.Instance.baseMentalityType);
                currentMentality.ApplyMentalityEffects(this);
            }
            else
            {
                Debug.LogWarning($"Could not find save data for unit {UnitName}");
                base.InitializeUnit();
            }

            HandleOnUnitInitialized();
        }
    }
}