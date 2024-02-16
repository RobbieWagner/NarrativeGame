using System.Collections;
using UnityEngine;

public class SequenceEvent : MonoBehaviour
{
    public virtual IEnumerator InvokeSequenceEvent()
    {
        yield return null;
    }
}