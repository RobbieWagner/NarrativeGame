using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public class ScreenCoverEvent : SequenceEvent
    {
        public override IEnumerator InvokeSequenceEvent()
        {
            yield return StartCoroutine(SceneTransitionController.Instance.FadeScreenOut());
        }
    }
}