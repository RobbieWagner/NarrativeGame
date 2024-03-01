using System;
using System.Collections;
using UnityEngine;

namespace PsychOutDestined
{
    public class SaveGameButton : MenuButton
    {
        public override IEnumerator SelectButton(Menu menu)
        {
            GameSession.Instance.SaveGameSessionData();
            GameSession.Instance.OnSaveComplete += CompleteSaveProcess;
            yield return new WaitForSecondsRealtime(.1f);
        }

        private void CompleteSaveProcess()
        {
            
        }
    }
}