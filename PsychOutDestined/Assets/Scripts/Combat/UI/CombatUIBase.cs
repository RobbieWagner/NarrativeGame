using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PsychOutDestined
{
    public class CombatUIBase : MonoBehaviour
    {
        [SerializeField] protected UnitUI unitUIPrefab;
        [SerializeField] protected WorldSpaceStatbar worldSpaceStatbarPrefab;
        [SerializeField] protected LayoutGroup allyUnits;
        [SerializeField] protected LayoutGroup enemyUnits;

        [SerializeField] protected List<UnitUI> alliesUI;
        [SerializeField] protected List<WorldSpaceStatbar> enemiesUI;

        [SerializeField] public Canvas ScreenSpaceCanvas;
        [SerializeField] public Canvas WorldSpaceCanvas;

        [SerializeField] private InitiativeBar initiativeBar;

        public virtual IEnumerator InitializeUI()
        {
            yield return null;

            try
            {
                if (CombatManagerBase.Instance != null)
                {
                    CombatManagerBase.Instance.OnAddNewAlly += AddAllyUI;
                    CombatManagerBase.Instance.OnAddNewEnemy += AddEnemyUI;

                    CombatManagerBase.Instance.OnStopConsideringTarget += StopConsideringTarget;
                    CombatManagerBase.Instance.OnConsiderTarget += DisplayConsideredTarget;

                    CombatManagerBase.Instance.OnEndActionSelection += DisableActionInfo;
                    CombatManagerBase.Instance.OnEndActionSelection += DisableTargetInfo;
                    CombatManagerBase.Instance.OnBeginTargetSelection += CheckToEnableTargetInfo;

                    CombatManagerBase.Instance.OnToggleActionSelectionInfo += ToggleActionSelectionInfo;
                    CombatManagerBase.Instance.OnToggleTargetSelectionInfo += ToggleTargetSelectionInfo;
                
                    CombatManagerBase.Instance.OnUpdateInitiativeOrder += UpdateInitiativeOrder;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogError("Combat Manager Found Null in Combat UI, Please define a Combat Manager before calling InitializeUI.");
            }
        }

        private void UpdateInitiativeOrder(List<Unit> initiativeOrder, List<Unit> inactiveUnits)
        {
            initiativeBar.SetBarImages(initiativeOrder, inactiveUnits);
        }

        protected virtual void AddAllyUI(Unit ally)
        {
            UnitUI newUnitUI = Instantiate(unitUIPrefab, allyUnits.transform);
            newUnitUI.Unit = ally;
            newUnitUI.combatUI = this;
            newUnitUI.InitializeUnitUI();
            alliesUI.Add(newUnitUI);
        }

        protected virtual void AddEnemyUI(Unit enemy)
        {
            WorldSpaceStatbar newStatbar = Instantiate(worldSpaceStatbarPrefab, WorldSpaceCanvas.transform);
            newStatbar.Initialize(enemy, enemy.GetMaxStatValue(UnitStat.HP), enemy.HP, UnitStat.HP);
            newStatbar.transform.position = enemy.transform.position;
            enemiesUI.Add(newStatbar);
        }

        protected virtual void StopConsideringTarget(Unit target)
        {
            target?.StopBlinking();
        }

        protected virtual void DisplayConsideredTarget(Unit user, Unit target, CombatAction action)
        {
            target.StartBlinking();
        }

        protected virtual void ToggleActionSelectionInfo()
        {
            bool enable = (CombatManagerBase.Instance.isSelectingAction || CombatManagerBase.Instance.isSelectingTargets) && alliesUI.Count > 0 && !alliesUI.FirstOrDefault().statTextParent.enabled;
            SetActionInfoActiveState(enable);
        }

        protected virtual void ToggleTargetSelectionInfo()
        {

        }

        public virtual void SetActionInfoActiveState(bool enabled)
        {
            foreach (UnitUI unitUI in alliesUI)
            {
                if (enabled) unitUI.EnableStatUI();
                else unitUI.DisableStatUI();
            }
        }
        public virtual void DisableActionInfo() => SetActionInfoActiveState(false);
        public virtual void EnableActionInfo() => SetActionInfoActiveState(true);

        public virtual void SetTargetInfoActiveState(bool enabled)
        {

        }
        public virtual void DisableTargetInfo() => SetTargetInfoActiveState(false);
        public virtual void EnableTargetInfo() => SetTargetInfoActiveState(true);

        private void CheckToEnableTargetInfo()
        {
            if (alliesUI.Count > 0 && alliesUI.FirstOrDefault().statTextParent.gameObject.activeSelf)
                EnableTargetInfo();
            else
                DisableTargetInfo();
        }
    }
}