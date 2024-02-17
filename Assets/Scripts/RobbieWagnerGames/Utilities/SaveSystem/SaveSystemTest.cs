using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class SaveSystemTest : MonoBehaviour
{
    [SerializeField] private SerializableUnit unit;

    private void Awake()
    {
        #if UNITY_EDITOR
        StartCoroutine(TestSaveSystem());
        #endif
    }

    private void SaveUnit()
    {
        GameSession.Instance?.playerParty.Add(unit);
        GameSession.Instance?.SaveGameSessionData();
    }

    private IEnumerator TestSaveSystem()
    {
        Debug.Log("Save System Test starting in 5 seconds");
        yield return new WaitForSeconds(5);
        Debug.Log("Testing save system");
        SaveUnit();
        Debug.Log("Saving unit...");
        yield return new WaitForSeconds(1);
        Debug.Log("Loading saved unit. Are they identical?");
        GameSession.Instance?.LoadGameSessionData();
        Debug.Log("Loading...");
        yield return new WaitForSeconds(1);
        Debug.Log(string.Join("\n", GameSession.Instance?.playerParty.Select(p => p.ToString())));
    }
}