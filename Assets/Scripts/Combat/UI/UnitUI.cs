using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public TextMeshProUGUI unitNameText;
    public Transform statbarParent;
    public Statbar statbarPrefab;
    public Statbar HPBar;
    public Statbar ManaBar;

    [Space(10)]
    public Transform statTextParent; 
    public StatText brawnStatUI;
    public StatText agilityStatUI;
    public StatText defenseStatUI;
    public StatText psychStatUI;
    public StatText focusStatUI;
    public StatText heartStatUI;
    [Space(10)]
    public ActionSelectionUI selectionUIPrefab;
    [HideInInspector] public ActionSelectionUI selectionUIInstace;

    public Image backgroundImage;

    private Unit unit;
    [HideInInspector] public ICombatUI combatUI;

    public Unit Unit
    {
        get { return unit; }
        set
        {
            if (unit != null && unit.Equals(value)) return;
            if (unit != null) Unsubscribe();
            unit = value;
        }
    }

    private void Unsubscribe()
    {
        combatUI.OnUpdateActionUI -= UpdateActionsUI;
    }

    private void Subscribe()
    {
        combatUI.OnUpdateActionUI += UpdateActionsUI;

        if (ICombatManager.Instance != null)
        {
            //ICombatManager.Instance.OnConsiderAction += UpdateActionsUI;
            //ICombatManager.Instance.OnBeginTargetSelection += DisableActionUI; 
        }
        else Debug.LogWarning("Could not subscribe to combat manager OnConsiderAction, as combat manager does not exist");
    }

    public void InitializeUnitUI()
    {
        if(Unit != null)
        {
            Subscribe();
            HPBar = Instantiate(statbarPrefab, statbarParent);
            HPBar.Initialize(Unit, Unit.GetMaxStatValue(UnitStat.HP), Unit.HP, UnitStat.HP);
            ManaBar.Initialize(Unit, Unit.GetMaxStatValue(UnitStat.Mana), Unit.Mana, UnitStat.Mana);
        }
    }

    public void UpdateActionsUI(Unit user, CombatAction action, ICombatUI combatUI)
    {
        if (user.Equals(Unit))
        {
            if (selectionUIInstace == null)
            {
                selectionUIInstace = Instantiate(selectionUIPrefab, combatUI.WorldSpaceCanvas.transform);
                selectionUIInstace.transform.position = unit.transform.position + new Vector3(0, .01f, -.5f);
            }
            int actionIndex = user.availableActions.IndexOf(action);
            if (actionIndex >= 0)
            {
                CombatAction curAction = user.availableActions[actionIndex];

                CombatAction prevAction;
                CombatAction nextAction;

                if (actionIndex == 0)
                    prevAction = user.availableActions[user.availableActions.Count - 1];
                else
                    prevAction = user.availableActions[actionIndex - 1];

                if (actionIndex == user.availableActions.Count - 1)
                    nextAction = user.availableActions[0];
                else
                    nextAction = user.availableActions[actionIndex + 1];

                selectionUIInstace.prevActionImage.sprite = prevAction.actionSprite;
                selectionUIInstace.curActionImage.sprite = curAction.actionSprite;
                selectionUIInstace.nextActionImage.sprite = nextAction.actionSprite;

                selectionUIInstace.EnableActionUI();
            }
        }
        else if (selectionUIInstace != null)
        {
            Destroy(selectionUIInstace.gameObject); // TODO: CONSIDER SHOWING THE SELECTION UI AFTER SELECTION SO PLAYER CAN SEE WHAT THEY SELECTION
            selectionUIInstace = null;
        }
    }
}