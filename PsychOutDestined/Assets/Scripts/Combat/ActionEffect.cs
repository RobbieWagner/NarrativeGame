using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMODUnity;
using System.Linq;
using RobbieWagnerGames.Common;
using UnityEngine.Serialization;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

namespace PsychOutDestined
{
    [Serializable]
    public class ActionEffect
    {
        [SerializeField] protected ImpactSoundType impactSound;

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
        [SerializeField] protected int stressForMissing = 1;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            //Debug.Log("getting hit list");
            hitTargets = new Dictionary<Unit, bool>();
            foreach (Unit target in targets) hitTargets.Add(target, UserHitsTarget(user, target));
            yield return null;
        }

        protected virtual bool UserHitsTarget(Unit user, Unit target)
        {
            int hitChance = attackAccuracy + user.GetStatValue(UnitStat.Focus) - target.GetStatValue(UnitStat.Agility);
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
            yield return CombatManagerBase.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            Debug.Log($"{user.name} is healing a unit");
            //TODO: Add crit chance
            foreach (Unit target in targets)
            {
                int healthDelta = power + user.GetStatValue(UnitStat.Heart);
                target.HP += healthDelta;
                Debug.Log($"{user.name} hit {target.name} for {healthDelta} damage!");
            }
            AudioManager.PlayOneShot(AudioEventsLibrary.Instance.FindActionImpactSound(impactSound), user.transform.position);
        }
    }

    [Serializable]
    public class AutoHitAttackActionEffect : ActionEffect
    {
        [Header("Attack Action")]
        [SerializeField] private int power;
        [SerializeField] private int stressAmount = 1;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            yield return CombatManagerBase.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            //Debug.Log($"{user.name} is attacking");
            //TODO: Add crit chance
            foreach (Unit target in targets)
            {
                int healthDelta = Math.Clamp(power + user.GetStatValue(UnitStat.Brawn) - target.GetStatValue(UnitStat.Defense), 1, int.MaxValue);
                target.HP -= healthDelta;
                target.Stress += stressAmount;
                Debug.Log($"{user.name} hit {target.name} for {healthDelta} damage!");
            }
            if(targets.Any()) 
                AudioManager.PlayOneShot(AudioEventsLibrary.Instance.FindActionImpactSound(impactSound), targets[0].transform.position);
            else
                AudioManager.PlayOneShot(AudioEventsLibrary.Instance.FindActionImpactSound(ImpactSoundType.Miss), user.transform.position);
        }
    }

    [Serializable]
    public class AttackActionEffect : ChanceActionEffect
    {
        [Header("Attack Action")]
        [SerializeField] private int power;
        [SerializeField] private int stressAmount = 1;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            yield return CombatManagerBase.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            Debug.Log($"{user.name} is attacking");
            foreach (KeyValuePair<Unit, bool> hitTarget in hitTargets)
            {
                if (hitTarget.Value)
                {
                    //TODO: Add crit chance
                    int healthDelta = Math.Clamp(power + user.GetStatValue(UnitStat.Brawn) - hitTarget.Key.GetStatValue(UnitStat.Defense), 1, int.MaxValue);
                    hitTarget.Key.HP -= healthDelta;
                    hitTarget.Key.Stress += stressAmount;
                    Debug.Log($"{user.name} hit {hitTarget.Key.name} for {healthDelta} damage!");
                }
                else
                {
                    Debug.Log($"{user.name} missed attack against {hitTarget.Key.name}");
                }
            }
            if(hitTargets.Where(t => t.Value).Any()) 
                AudioManager.PlayOneShot(AudioEventsLibrary.Instance.FindActionImpactSound(impactSound), hitTargets.First().Key.transform.position);
            else
                AudioManager.PlayOneShot(AudioEventsLibrary.Instance.FindActionImpactSound(ImpactSoundType.Miss), user.transform.position);

            if(hitTargets.Where(t => !t.Value).Any())
                user.Stress += stressForMissing;
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
            if (power > 0) statDelta += user.GetStatValue(UnitStat.Heart) / 4;
            yield return CombatManagerBase.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            //Debug.Log($"{user.name} is changing stats");
            foreach (Unit target in targets)
            {
                target.ModifyStatValue(stat, statDelta);
                Debug.Log($"{user.name} {(statDelta > 0 ? "raised" : "lowered")} {target}'s {stat} by {Math.Abs(statDelta)}");
            }
            if(targets.Any()) 
                AudioManager.PlayOneShot(AudioEventsLibrary.Instance.FindActionImpactSound(impactSound), targets[0].transform.position);
            else
                AudioManager.PlayOneShot(AudioEventsLibrary.Instance.FindActionImpactSound(ImpactSoundType.Miss), user.transform.position);
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
            if (power >= 0) statDelta += user.GetStatValue(UnitStat.Heart) / 4;

            yield return CombatManagerBase.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
            Debug.Log($"{user.name} is changing stats");
            foreach (KeyValuePair<Unit, bool> hitTarget in hitTargets)
            {
                if (hitTarget.Value)
                {
                    hitTarget.Key.ModifyStatValue(stat, statDelta);
                    Debug.Log($"{user.name} {(statDelta > 0 ? "raised" : "lowered")} {hitTarget.Value}'s {stat} by {Math.Abs(statDelta)}");
                }
            }
            if(hitTargets.Any()) 
                AudioManager.PlayOneShot(AudioEventsLibrary.Instance.FindActionImpactSound(impactSound), hitTargets.First().Key.transform.position);
            else
                AudioManager.PlayOneShot(AudioEventsLibrary.Instance.FindActionImpactSound(ImpactSoundType.Miss), user.transform.position);

            if (hitTargets.Where(t => !t.Value).Any())
                user.Stress += stressForMissing;
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
        [FormerlySerializedAs("stressRegenPercent")]
        [SerializeField] private float stressRelief = 0;
        [SerializeField] private float hpRegenPercent = 0;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets = null)
        {
            Debug.Log($"{user.name} is resting");
            yield return null;

            int stressRelieved = user.Stress >= 100 ? 0 : (int)(stressRelief + user.GetStatValue(UnitStat.Psych));
            int hpRegained = ((int)(hpRegenPercent / 100 * user.GetMaxStatValue(UnitStat.HP))) + (user.GetStatValue(UnitStat.Defense) / 3 * 2);

            user.Stress -= stressRelieved;
            user.HP += hpRegained;
        }
    }

    [Serializable]
    public class MentalityApplication : ActionEffect
    {
        [SerializeField] private MentalityType mentalityType;

        public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
        {
            yield return null;
            
            if(targets != null && targets.Any() && MentalityManager.Instance.ApplyMentality(targets.First(), mentalityType))
            {
                int failed = 0;
                for (int i = 1; i < targets.Count; i++)
                {
                    if (!MentalityManager.Instance.ApplyMentality(targets[i], mentalityType))
                        failed++;
                }
                if (failed > 0)
                    Debug.Log($"Failed to apply mentality {mentalityType} to {failed} units");
            }
            else
                Debug.LogWarning("Could not execute action MENTALITY_APPLICATION: no valid targets found");
        }
    }
}