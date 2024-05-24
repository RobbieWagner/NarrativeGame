using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Experimental.AI;

namespace PsychOutDestined
{
    public class FlashScreenSequenceEvent : SequenceEvent
    {
        [SerializeField] private Canvas screenFlash;
        [SerializeField] private float timeToLive;
        [SerializeField] private Vector2 startPos;
        [SerializeField] private Vector2 endPos;
        [SerializeField] private Ease ease = Ease.InOutCirc;
        [SerializeField] private RectTransform screenUI;

        public override IEnumerator InvokeSequenceEvent()
        {
            screenUI.anchoredPosition = startPos;
            screenFlash.enabled = true;
            yield return screenUI.DOAnchorPos(endPos, timeToLive).SetEase(ease).WaitForCompletion();
            screenFlash.enabled = false;
        }
    }
}