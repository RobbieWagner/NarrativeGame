using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace PsychOutDestined
{
    public class SaveSystemTest : MonoBehaviour
    {
        [SerializeField] private SerializableUnit unit;
        [SerializeField] private PartyUnit testUnit;

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
            Debug.Log("Save System Test starting in 3 seconds");
            yield return new WaitForSeconds(3);
            Debug.Log("Testing save system");
            SaveUnit();
            Debug.Log("Saving unit...");
            yield return new WaitForSeconds(.5f);
            GameSession.Instance?.LoadSaveFiles();
            Debug.Log("Loading...");
            yield return new WaitForSeconds(1);
        }
    }
}