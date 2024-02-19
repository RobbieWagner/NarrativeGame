using System.Collections;
using UnityEngine;

namespace PsychOutDestined
{
    public class QuitToDesktopButton : MenuButton
    {
        public override IEnumerator SelectButton(Menu menu)
        {
            yield return StartCoroutine(base.SelectButton(menu));

            Application.Quit();
        }
    }
}