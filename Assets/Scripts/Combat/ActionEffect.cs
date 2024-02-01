using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionEffect
{
    public virtual IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
    {
        yield return null;
    }
}

[Serializable]
public class ContestStatEffect : ActionEffect
{
    [Header("Contest")]
    public UnitStat userContestStat;
    public UnitStat targetContestStat;
    protected Dictionary<Unit, bool> hitTargets;

    public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
    {
        Debug.Log("getting hit list");
        hitTargets = new Dictionary<Unit, bool>();
        foreach(Unit target in targets) hitTargets.Add(target, IsUserWinner(user, target));
        yield return null;
    }

    protected virtual bool IsUserWinner(Unit user, Unit target)
    {
        return true;
    }
}

[Serializable]
public class ContestDamageEffect : ContestStatEffect
{

    [Header("Damage Contest")]
    [SerializeField] private int power;

    public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
    {
        yield return ICombatManager.Instance?.StartCoroutine(base.ExecuteActionEffect(user, targets));
        Debug.Log($"{user.name} is attacking");
        foreach(KeyValuePair<Unit, bool> hitTarget in hitTargets)
        {
            if(hitTarget.Value)
            {
                hitTarget.Key.HP -= power;

                Debug.Log($"{user.name} hit {hitTarget.Key.name} for {power} damage!");
            }
        }
    }
}

[Serializable]
public class StatChangeEffect : ActionEffect
{
    [Header("Stat Change")]
    [SerializeField] private UnitStat stat;

    public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets)
    {
        throw new System.NotImplementedException();
    }
}

[Serializable]
public class PassEffect : ActionEffect
{
    [Header("Pass Turn")]
    [SerializeField] private bool revitalizes = true;

    public override IEnumerator ExecuteActionEffect(Unit user, List<Unit> targets = null)
    {
        Debug.Log($"{user.name} is resting");
        yield return null;
    }
}