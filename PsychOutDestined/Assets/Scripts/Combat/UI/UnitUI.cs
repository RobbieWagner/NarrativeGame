using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PsychOutDestined
{
    public partial class UnitUI : MonoBehaviour
    {
        public TextMeshProUGUI unitNameText;
        [Space(10)]
        public Image backgroundImage;
        private Unit unit;
        [HideInInspector] public CombatUIBase combatUI;

        [SerializeField] Vector3 actionSelectionUIOffset;

        public Unit Unit
        {
            get { return unit; }
            set
            {
                if (unit != null && unit.Equals(value)) return;
                unit = value;
            }
        }

        public void InitializeUnitUI()
        {
            if (Unit != null)
            {
                SetupStatDisplay();
                DisableStatUI();
                Unit.OnMentalityChanged += UpdateMentalityText;
            }
        }
    }
}