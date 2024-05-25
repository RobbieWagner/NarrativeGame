using System;
using System.Collections;
using UnityEngine;

namespace PsychOutDestined
{
    public class SaveGameButton : MenuButton
    {
        public override IEnumerator SelectButton(Menu menu)
        {
            yield return StartCoroutine(GameSession.Instance.SaveGameSessionDataAsync());
            yield return new WaitForSecondsRealtime(.1f);
        }
    }
}