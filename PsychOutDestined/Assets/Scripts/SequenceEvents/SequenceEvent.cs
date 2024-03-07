using System.Collections;
using UnityEngine;

namespace PsychOutDestined
{
    public class SequenceEvent : MonoBehaviour
    {
        public virtual IEnumerator InvokeSequenceEvent()
        {
            yield return null;
        }
    }
}