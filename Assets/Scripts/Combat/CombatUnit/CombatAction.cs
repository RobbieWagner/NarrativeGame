using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CombatAction")]
public class CombatAction : ScriptableObject
{
    public string actionName;
    public Sprite actionSprite;
    
    public bool targetsAllOpposition;
    public bool targetsAllAllies;

    public bool canTargetSelf;
    public bool canTargetAllies;
    public bool canTargetEnemies;

    [SerializeReference] public List<ActionEffect> effects;

    [ContextMenu("Heal")] void AddHealActionEffect(){effects.Add(new HealTargetsActionEffect());}
    [ContextMenu("Attack")] void AddAttackActionEffect(){effects.Add(new AttackActionEffect());}
    [ContextMenu("Auto Hit Attack")] void AddAutoHitAttackActionEffect(){effects.Add(new AutoHitAttackActionEffect());}
    [ContextMenu("Auto Hit Stat Change")] void AddStatChangeActionEffect(){effects.Add(new StatChangeActionEffect());}
    [ContextMenu("Stat Change")] void AddStatChangeChanceActionEffect(){effects.Add(new StatChangeChanceActionEffect());}
    [ContextMenu("Pass")] void AddPassTurnEffect(){effects.Add(new PassEffect());}
    [ContextMenu("CLEAR ACTION")] void Clear(){effects.Clear();}

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