using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatManager : ICombatManager
{
    [SerializeField] private Battlefield debugBattlefield;

    #region Combat Phases
    protected override IEnumerator SetupCombat()
    {
        
        currentUI = Instantiate(currentCombat.combatUIPrefab);
        yield return currentUI.InitializeUI();

        allies = new List<Unit>();
        enemies = new List<Unit>();

        foreach(Unit ally in currentCombat.allyPrefabs)
            TryAddAllyToCombat(ally);
        foreach(Unit enemy in currentCombat.enemyPrefabs)
            TryAddEnemyToCombat(enemy);

        debugBattlefield?.PlaceUnits(allies.Select(a => a.transform).ToList(), true);
        debugBattlefield?.PlaceUnits(enemies.Select(e => e.transform).ToList(), false);

        yield return StartCoroutine(base.SetupCombat());
    }

    protected override IEnumerator StartTurn()
    {
        yield return StartCoroutine(base.StartTurn());
    }

    protected override IEnumerator HandleActionSelection()
    {
        yield return StartCoroutine(base.HandleActionSelection());
    }

    protected override IEnumerator ExecuteActions()
    {

        yield return StartCoroutine(base.ExecuteActions());
    }

    protected override IEnumerator EndTurn()
    {

        yield return StartCoroutine(base.EndTurn());
    }

    protected override IEnumerator ResolveCombat()
    {

        yield return StartCoroutine(base.ResolveCombat());
    }
    #endregion

    protected override bool TryAddAllyToCombat(Unit ally)
    {
        return base.TryAddAllyToCombat(ally);
    }

    protected override bool TryAddEnemyToCombat(Unit enemy)
    {
        return base.TryAddEnemyToCombat(enemy);
    }
}