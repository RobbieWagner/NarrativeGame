using System.Collections;
using UnityEngine;

namespace PsychOutDestined
{
    public class FleeButton : MenuButton
    {
        public override IEnumerator SelectButton(Menu menu)
        {
            yield return StartCoroutine(base.SelectButton(menu));
            if(CombatManagerBase.Instance.AttemptToFlee())
                Debug.Log("flee successful");
            else    
                Debug.Log("flee unsuccessful");
        }
    }
}