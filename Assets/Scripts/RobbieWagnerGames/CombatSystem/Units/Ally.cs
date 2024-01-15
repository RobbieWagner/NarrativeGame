using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobbieWagnerGames;

namespace RobbieWagnerGames.StrategyCombat.Units
{
    public class Ally : Unit
    {
        
        [HideInInspector] public int currentActionIndex;

        protected override void Awake()
        {
            base.Awake();
            HP = maxHP;
            MP = maxMP;

        }

        public override IEnumerator DownUnit()
        {
            isUnitFighting = false;

            yield return null;
            if(spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                yield return spriteRenderer.DOColor(new Color(0, 0, 0, 1), .1f).SetEase(Ease.Linear).WaitForCompletion();
            }

            StopCoroutine(DownUnit());
        }
    }
}
