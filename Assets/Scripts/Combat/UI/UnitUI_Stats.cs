using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PsychOutDestined
{
    public partial class UnitUI : MonoBehaviour
    {
        public Transform statbarParent;
        public Statbar statbarPrefab;
        [HideInInspector] public Statbar HPBar;
        public Color hpColor;
        public Sprite hpIcon;
        [HideInInspector] public Statbar StressBar;
        public Color stressColor;
        public Sprite stressIcon;

        [Space(10)]
        public Image statTextParent;
        public StatText statTextPrefab;
        [HideInInspector] public StatText brawnStatUI;
        public Sprite brawnIcon;
        [HideInInspector] public StatText agilityStatUI;
        public Sprite agilityIcon;
        [HideInInspector] public StatText defenseStatUI;
        public Sprite defenseIcon;
        [HideInInspector] public StatText psychStatUI;
        public Sprite psychIcon;
        [HideInInspector] public StatText focusStatUI;
        public Sprite focusIcon;
        [HideInInspector] public StatText heartStatUI;
        public Sprite heartIcon;

        public void SetupStatDisplay()
        {
            HPBar = Instantiate(statbarPrefab, statbarParent);
            SetupNewStatbar(HPBar, hpColor, hpIcon, UnitStat.HP);
            StressBar = Instantiate(statbarPrefab, statbarParent);
            SetupNewStatbar(StressBar, stressColor, stressIcon, UnitStat.Stress);

            brawnStatUI = Instantiate(statTextPrefab, statTextParent.transform);
            SetupNewStatText(brawnStatUI, brawnIcon, UnitStat.Brawn);
            agilityStatUI = Instantiate(statTextPrefab, statTextParent.transform);
            SetupNewStatText(agilityStatUI, agilityIcon, UnitStat.Agility);
            defenseStatUI = Instantiate(statTextPrefab, statTextParent.transform);
            SetupNewStatText(defenseStatUI, defenseIcon, UnitStat.Defense);
            psychStatUI = Instantiate(statTextPrefab, statTextParent.transform);
            SetupNewStatText(psychStatUI, psychIcon, UnitStat.Psych);
            focusStatUI = Instantiate(statTextPrefab, statTextParent.transform);
            SetupNewStatText(focusStatUI, focusIcon, UnitStat.Focus);
            heartStatUI = Instantiate(statTextPrefab, statTextParent.transform);
            SetupNewStatText(heartStatUI, heartIcon, UnitStat.Heart);
        }

        public void SetupNewStatbar(Statbar statbar, Color sliderFillColor, Sprite icon, UnitStat stat)
        {
            statbar.sliderFill.color = sliderFillColor;
            statbar.statIcon.sprite = icon;
            statbar.Initialize(Unit, Unit.GetMaxStatValue(stat), Unit.GetStatValue(stat), stat);
        }

        public void SetupNewStatText(StatText statText, Sprite icon, UnitStat stat)
        {
            statText.statIcon.sprite = icon;
            statText.Initialize(Unit, Unit.GetMaxStatValue(stat), Unit.GetStatValue(stat), stat);
        }

        public void EnableStatUI()
        {
            statTextParent.enabled = true;
            brawnStatUI.EnableUI();
            agilityStatUI.EnableUI();
            defenseStatUI.EnableUI();
            psychStatUI.EnableUI();
            focusStatUI.EnableUI();
            heartStatUI.EnableUI();
        }

        public void DisableStatUI()
        {
            statTextParent.enabled = false;
            brawnStatUI.DisableUI();
            agilityStatUI.DisableUI();
            defenseStatUI.DisableUI();
            psychStatUI.DisableUI();
            focusStatUI.DisableUI();
            heartStatUI.DisableUI();
        }
    }
}