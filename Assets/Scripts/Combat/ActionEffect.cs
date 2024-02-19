using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace PsychOutDestined
{
    [Serializable]
    public class ActionEffect
    {
        public virtual IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            yield return null;
        }
    }

    //Parent Action Effect class defining actions that may or may not hit their targets
    [Serializable]
    public class ChanceActionEffect : ActionEffect
    {
        [Header("Chance")]
        protected Dictionary<Unit, bool> hitTargets;
        [SerializeField] private int attackAccuracy = 100;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            Debug.Log("getting hit list");
            hitTargets = new Dictionary<Unit, bool>();
            foreach (Unit target in targets) hitTargets.Add(target, UserHitsTarget(user, target));
            yield return null;
        }

        protected virtual bool UserHitsTarget(Unit user, Unit target)
        {
            int hitChance = attackAccuracy + user.Focus - target.Agility;
            if (hitChance > 100) return true;
            return UnityEngine.Random.Range(0, 100) < hitChance;
        }
    }

    //TODO: Add action effect that is applied to user

    [Serializable]
    public class HealTargetsActionEffect : ActionEffect
    {
        [Header("Healing")]
        [SerializeField] private int power;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            yield return ICombatManager.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            Debug.Log($"{user.name} is healing a unit");
            //TODO: Add crit chance
            foreach (Unit target in targets)
            {
                int healthDelta = power + user.Heart;
                target.HP += healthDelta;
                Debug.Log($"{user.name} hit {target.name} for {healthDelta} damage!");
            }
        }
    }

    [Serializable]
    public class AutoHitAttackActionEffect : ActionEffect
    {
        [Header("Attack Action")]
        [SerializeField] private int power;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            yield return ICombatManager.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            Debug.Log($"{user.name} is attacking");
            //TODO: Add crit chance
            foreach (Unit target in targets)
            {
                int healthDelta = Math.Clamp(power + user.Brawn - target.Defense, 1, int.MaxValue);
                target.HP -= healthDelta;
                Debug.Log($"{user.name} hit {target.name} for {healthDelta} damage!");
            }
        }
    }

    [Serializable]
    public class AttackActionEffect : ChanceActionEffect
    {
        [Header("Attack Action")]
        [SerializeField] private int power;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            yield return ICombatManager.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            Debug.Log($"{user.name} is attacking");
            foreach (KeyValuePair<Unit, bool> hitTarget in hitTargets)
            {
                if (hitTarget.Value)
                {
                    //TODO: Add crit chance
                    int healthDelta = Math.Clamp(power + user.Brawn - hitTarget.Key.Defense, 1, int.MaxValue);
                    hitTarget.Key.HP -= healthDelta;
                    Debug.Log($"{user.name} hit {hitTarget.Key.name} for {healthDelta} damage!");
                }
            }
        }
    }

    [Serializable]
    public class StatChangeActionEffect : ActionEffect
    {
        [Header("Stat Change")]
        [SerializeField] private UnitStat stat;
        [SerializeField] private int power;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            int statDelta = power;
            if (power > 0) statDelta += user.Heart / 2;
            yield return ICombatManager.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            Debug.Log($"{user.name} is changing stats");
            foreach (Unit target in targets)
            {
                target.EffectStatValue(stat, statDelta);
                Debug.Log($"{user.name} {(statDelta > 0 ? "raised" : "lowered")} {target}'s {stat} by {Math.Abs(statDelta)}");
            }
        }
    }

    [Serializable]
    public class StatChangeChanceActionEffect : ChanceActionEffect
    {
        [Header("Stat Change (Chance)")]
        [SerializeField] private UnitStat stat;
        [SerializeField] private int power;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            int statDelta = power;
            if (power >= 0) statDelta += user.Heart / 2;

            yield return ICombatManager.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            Debug.Log($"{user.name} is changing stats");
            foreach (KeyValuePair<Unit, bool> hitTarget in hitTargets)
            {
                if (hitTarget.Value)
                {
                    hitTarget.Key.EffectStatValue(stat, statDelta);
                    Debug.Log($"{user.name} {(statDelta > 0 ? "raised" : "lowered")} {hitTarget.Value}'s {stat} by {Math.Abs(statDelta)}");
                }
            }
        }
    }

    [Serializable]
    public class PassEffect : ActionEffect
    {
        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets = null)
        {
            Debug.Log($"{user.name} is passing their turn");
            yield return null;
        }
    }

    [Serializable]
    public class Replenish : ActionEffect
    {
        [Header("Replenishment")]
        [SerializeField] private float manaRegenPercent = 0;
        [SerializeField] private float hpRegenPercent = 0;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets = null)
        {
            Debug.Log($"{user.name} is resting");
            yield return null;

            int manaRegained = ((int)(manaRegenPercent / 100 * user.GetMaxStatValue(UnitStat.Mana))) + user.Psych;
            int hpRegained = ((int)(hpRegenPercent / 100 * user.GetMaxStatValue(UnitStat.HP))) + (user.Defense / 3 * 2);

            user.Mana += manaRegained;
            user.HP += hpRegained;
        }
    }
}