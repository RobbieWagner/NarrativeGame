using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ICombatUI : MonoBehaviour
{
    [SerializeField] protected UnitUI unitUIPrefab;
    [SerializeField] protected LayoutGroup allyUnits;
    [SerializeField] protected LayoutGroup enemyUnits;

    [SerializeField] protected List<UnitUI> alliesUI;
    [SerializeField] protected List<UnitUI> enemiesUI;

    public virtual IEnumerator InitializeUI()
    {
        yield return null;

        if(ICombatManager.Instance != null)
        {
            ICombatManager.Instance.OnAddNewAlly += AddAllyUI;
            ICombatManager.Instance.OnAddNewEnemy += AddEnemyUI;
        }
    }

    protected virtual void AddAllyUI(Unit ally)
    {
        UnitUI newUnitUI = Instantiate(unitUIPrefab, allyUnits.transform);
        newUnitUI.Unit = ally;
        alliesUI.Add(newUnitUI);
    }

    protected virtual void AddEnemyUI(Unit enemy)
    {
        UnitUI newUnitUI = Instantiate(unitUIPrefab, enemyUnits.transform);
        newUnitUI.Unit = enemy;
        enemiesUI.Add(newUnitUI);
    }
}
