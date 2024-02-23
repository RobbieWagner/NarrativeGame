using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PsychOutDestined
{
    public class OverworldEnemy : MonoBehaviour
    {
        [SerializeField] CombatBase combat;

        protected virtual void Awake()
        {
            if (Vector3.Distance(PlayerMovement.Instance.transform.position, transform.position) < 5)
                Destroy(gameObject);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (GameManager.Instance == null || GameManager.Instance.CurrentGameMode == GameMode.Exploration)
                {
                    if (GameManager.Instance == null)
                        Debug.LogWarning("No instance of game manager found. This may break functionality in cases where game manager is expected");
                    if (Level.Instance != null)
                        StartCombat();
                    else
                        Debug.LogWarning("Found no level instance, could not start combat.");
                }
                Destroy(this.gameObject);
            }
        }

        protected virtual void StartCombat()
        {
            Debug.Log("start combat");
            Level.Instance.CurrentCombat = combat;
        }
    }
}