using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CombatAction")]
public class CombatAction : ScriptableObject
{
    public string actionName;
    
    public bool targetsAllOpposition;
    public bool targetsAllAllies;

    public bool canTargetSelf;
    public bool canTargetAllies;
    public bool canTargetEnemies;

    [SerializeReference] public List<ActionEffect> effects;

    [ContextMenu(nameof(AddContestDamageEffect))] void AddContestDamageEffect(){effects.Add(new ContestDamageEffect());}
    [ContextMenu(nameof(AddStatChangeEffect))] void AddStatChangeEffect(){effects.Add(new StatChangeEffect());}
    [ContextMenu(nameof(AddPassTurnEffect))] void AddPassTurnEffect(){effects.Add(new PassEffect());}
    [ContextMenu(nameof(Clear))] void Clear(){effects.Clear();}

    public IEnumerator ExecuteAction(Unit user, List<Unit> targets)
    {
        foreach(var effect in effects) yield return ICombatManager.Instance?.StartCoroutine(effect.ExecuteActionEffect(user, targets));
        yield return null;
    }

    internal List<Unit> GetTargetUnits(List<Unit> selectedUnits)
    {
        return selectedUnits;
    }
}