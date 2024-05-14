using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PsychOutDestined
{
    public class InitiativeBar : MonoBehaviour
    {
        [SerializeField] private List<Image> unitIcons;
        [SerializeField] private List<Image> borders;
        private List<Unit> units;
        private List<Unit> currentInactiveUnits;

        private Dictionary<Unit, Image> unitImages;

        public void SetBarImages(List<Unit> initiativeOrder, List<Unit> inactiveUnits)
        {
            Color inactiveColor = new Color(.25f,.25f,.25f,.5f);

            units = initiativeOrder;
            currentInactiveUnits = inactiveUnits;

            for(int i = 0; i < unitIcons.Count; i++)
            {
                if(i < units.Count)
                {
                    unitIcons[i].sprite = units[i].headSprite;
                    unitIcons[i].color = Color.white;
                }
                else
                {
                    unitIcons[i].sprite = currentInactiveUnits[i-units.Count].headSprite;
                    unitIcons[i].color = inactiveColor;
                }
            }
        }
    }
}