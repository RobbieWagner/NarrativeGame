using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PsychOutDestined;
using UnityEngine;
using UnityEngine.UI;

namespace PsychOutDestined
{
    [System.Serializable]
    public class InitiativeImages
    {
        public Image unitIcon;
        public Image border;
    }

    public class InitiativeBar : MonoBehaviour
    {
        [SerializeField] private List<InitiativeImages> initiativeIcons;
        private List<Unit> units;
        private List<Unit> currentInactiveUnits;

        private Dictionary<Unit, InitiativeImages> unitImages;

        private Color inactiveColor;

        private void Awake()
        {
            inactiveColor = new Color(.25f,.25f,.25f,.5f);
        }

        public void InitializeInitiativeBar()
        {
            CombatManagerBase.Instance.OnStartActionSelectionForUnit += SetActiveBorder;
            CombatManagerBase.Instance.OnEnemyActionSelectionBegin += SetActiveBorder;
        }

        public void SetActiveBorder(Unit unit)
        {
            foreach(Image image in initiativeIcons.Select(i => i.border))
                image.enabled = false;
            
            InitiativeImages images;
            if(unitImages.TryGetValue(unit, out images))
                images.border.enabled = true;
        }

        public void SetBarImages(List<Unit> initiativeOrder, List<Unit> inactiveUnits)
        {
            if(units != null && units.Any())
            {
                foreach(Unit unit in initiativeOrder)
                    unit.OnDeactivateUnit -= DeactivateUnitInitiativeSprite;
            }
            if(currentInactiveUnits != null && currentInactiveUnits.Any())
            {
                foreach(Unit unit in currentInactiveUnits)
                    unit.OnDeactivateUnit -= DeactivateUnitInitiativeSprite;
            }

            units = initiativeOrder;
            currentInactiveUnits = inactiveUnits;
            unitImages = new Dictionary<Unit, InitiativeImages>();

            for(int i = 0; i < initiativeIcons.Count; i++)
            {
                if(i < units.Count)
                    SetupInitiativeIcon(units[i], initiativeIcons[i], Color.white);
                else
                    SetupInitiativeIcon(inactiveUnits[i-units.Count], initiativeIcons[i], inactiveColor);
            }
        }

        public void DeactivateUnitInitiativeSprite(Unit unit)
        {
            InitiativeImages images;
            if(unitImages.TryGetValue(unit, out images))
                images.unitIcon.color = inactiveColor;
        }

        public void ActivateUnitInitiativeSprite(Unit unit)
        {
            InitiativeImages images;
            if(unitImages.TryGetValue(unit, out images))
                images.unitIcon.color = Color.white;
        }

        private void SetupInitiativeIcon(Unit unit, InitiativeImages image, Color color)
        {
            image.unitIcon.sprite = unit.headSprite;
            image.unitIcon.color = color;

            unit.OnDeactivateUnit += DeactivateUnitInitiativeSprite;

            unitImages.Add(unit, image);
        }
    }
}