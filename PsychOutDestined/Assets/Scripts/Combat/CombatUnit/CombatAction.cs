using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public enum ActionType
    {
        None = -1,
        Damage,
        Healing,
        StatChange,
        MentalityApplication
    }

    [CreateAssetMenu(menuName = "CombatAction")]
    public class CombatAction : ScriptableObject
    {
        public string actionName;
        public ActionType actionType;
        public Sprite actionSprite;

        public bool targetsAllOpposition;
        public bool targetsAllAllies;

        public bool canTargetSelf;
        public bool canTargetAllies;
        public bool canTargetEnemies;

        [SerializeReference] public List<ActionEffect> effects;

        [ContextMenu("Heal")] void AddHealActionEffect() { effects.Add(new HealTargetsActionEffect()); }
        [ContextMenu("Auto Hit Attack")] void AddAutoHitAttackActionEffect() { effects.Add(new AutoHitAttackActionEffect()); }
        [ContextMenu("Attack")] void AddAttackActionEffect() { effects.Add(new AttackActionEffect()); }
        [ContextMenu("Auto Hit Stat Change")] void AddStatChangeActionEffect() { effects.Add(new StatChangeActionEffect()); }
        [ContextMenu("Stat Change")] void AddStatChangeChanceActionEffect() { effects.Add(new StatChangeChanceActionEffect()); }
        [ContextMenu("Replenish")] void AddRestActionEffect() { effects.Add(new Replenish()); }
        [ContextMenu("Mentality Addition")] void AddMentalityApplicationEffect() { effects.Add(new MentalityApplication()); }
        [ContextMenu("Pass")] void AddPassTurnEffect() { effects.Add(new PassEffect()); }
        [ContextMenu("CLEAR ACTION")] void Clear() { effects.Clear(); }

        public IEnumerator ExecuteAction(Unit user, List<Unit> targets)
        {
            foreach (var effect in effects) yield return CombatManagerBase.Instance?.StartCoroutine(effect.ExecuteActionEffect(user, targets));
            yield return null;
        }

        public List<Unit> GetTargetUnits(List<Unit> selectedUnits) => selectedUnits;
    }
}