using System.Collections;
using UnityEngine;

public class SequenceEvent : MonoBehaviour
{
    public virtual IEnumerator InvokeSequenceEvent()
    {
        Debug.Log("sequence event invoked");
        yield return null;
    }
}