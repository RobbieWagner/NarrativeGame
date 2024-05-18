using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;

namespace PsychOutDestined
{
    public class MoveFlash : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshProUGUI moveText;
        [SerializeField] private Image moveIcon;
        [SerializeField] private TextMeshProUGUI userText;
        [SerializeField] private Image userIcon;

        private void Awake()
        {
            //CombatManagerBase.Instance.OnStartActionExecution += FlashUserMoveHandler;
        }

        public IEnumerator SlideInActionUI(Unit unit, CombatAction combatAction)
        {
            moveText.text = combatAction.actionName;
            moveIcon.sprite = combatAction.actionSprite;
            userText.text = unit.UnitName;
            userIcon.sprite = unit.headSprite;

            yield return rectTransform.DOAnchorPos(Vector2.zero, 0.5f).WaitForCompletion();
        }

        public IEnumerator SlideOutActionUI()
        {
            yield return rectTransform.DOAnchorPos(new Vector2(0, rectTransform.sizeDelta.y), 0.75f).WaitForCompletion();
        }
    }
}