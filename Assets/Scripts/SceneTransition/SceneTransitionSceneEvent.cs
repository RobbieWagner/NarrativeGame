using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using RobbieWagnerGames;

namespace PsychOutDestined
{
    public class SceneTransitionSceneEvent : SceneEvent
    {
        [SerializeField] private bool fadeIn;

        public override IEnumerator RunSceneEvent()
        {
            if (fadeIn) yield return SceneTransition.Instance?.FadeInScreen();
            else yield return SceneTransition.Instance?.FadeOutScreen();

            yield return base.RunSceneEvent();
            StopCoroutine(RunSceneEvent());
        }
    }
}