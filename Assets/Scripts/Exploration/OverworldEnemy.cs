using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldEnemy : MonoBehaviour
{
    [SerializeField] ICombat combat;

    protected virtual void Awake()
    {
        if(Vector3.Distance(PlayerMovement.Instance.transform.position, transform.position) < 5)
            Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(GameManager.Instance.CurrentGameMode == GameMode.Exploration)
            {
                if(Level.Instance != null)
                    StartCombat();
                else
                    Debug.LogWarning("Found no level instance, could not start combat.");
            }
            Destroy(this.gameObject);
        }
    }

    protected virtual void StartCombat()
    {
        Level.Instance.CurrentCombat = combat;
    }
}
