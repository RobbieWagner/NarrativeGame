using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RobbieWagnerGames.StrategyCombat
{
    [CreateAssetMenu(menuName = "Combat Action")]
    public class CombatAction : ScriptableObject
    {
        [SerializeField] public string actionName;
        [SerializeField] public Sprite actionIcon;

        [SerializeField] public bool targetsAllOpposition;
        [SerializeField] public bool targetsAllAllies;

        [SerializeField] public bool canTargetSelf;
        [SerializeField] public bool canTargetOpposition;
        [SerializeField] public bool canTargetAllies;

        [SerializeReference] public List<ActionEffect> effects;

        [ContextMenu(nameof(AddDamageEffect))] void AddDamageEffect(){effects.Add(new Damage());}
        [ContextMenu(nameof(AddHealEffect))] void AddHealEffect(){effects.Add(new Heal());}
        [ContextMenu(nameof(AddStatRaiseEffect))] void AddStatRaiseEffect(){effects.Add(new StatRaise());}
        [ContextMenu(nameof(AddStatLowerEffect))] void AddStatLowerEffect(){effects.Add(new StatLower());}
        [ContextMenu(nameof(AddPassTurnEffect))] void AddPassTurnEffect(){effects.Add(new Pass());}
        [ContextMenu(nameof(Clear))] void Clear(){effects.Clear();}
    }
}