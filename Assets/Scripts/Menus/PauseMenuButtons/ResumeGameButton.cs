using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public class ResumeGameButton : MenuButton
    {
        public override IEnumerator SelectButton(Menu menu)
        {
            yield return new WaitForSecondsRealtime(.01f);
            GameManager.Instance.ResumeGame();
        }
    }
}

