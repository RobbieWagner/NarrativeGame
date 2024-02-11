using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatEventHandler : MonoBehaviour
{
    private Dictionary<CombatEvent, int> combatEvents;

    private void Awake()
    {
        combatEvents = new Dictionary<CombatEvent, int>();
    } 

    public void Subscribe(CombatEvent combatEvent, int priority = -1)
    {
        bool succeeded = combatEvents.TryAdd(combatEvent, priority);
        if(!succeeded) Debug.LogWarning($"Could not add event: {combatEvent.gameObject.name}");
    }

    public void Unsubscribe(CombatEvent combatEvent)
    {
        combatEvents.Remove(combatEvent);
    }

    public IEnumerator Invoke()
    {
        //Debug.Log($"Invoking combat event: {combatEvents.Count}");
        List<CombatEvent> events = combatEvents.OrderByDescending(x => x.Value).Select(e => e.Key).ToList();

        if(events != null)
        {
            foreach(CombatEvent combatEvent in events)
            {
                yield return StartCoroutine(combatEvent.InvokeEvent());
            }
        }
        else
            Debug.Log("no events found");
    
    }
}