using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldEnemy : MonoBehaviour
{
    [SerializeField] ICombat combat;

    protected virtual void Awake()
    {
        
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && GameManager.Instance.CurrentGameMode == GameMode.Exploration)
        {
            if(ILevel.Instance != null)
            {
                StartCombat();
            }
            else
            {
                Debug.LogWarning("Found no level instance, could not start combat.");
            }
        }
    }

    protected virtual void StartCombat()
    {
        ILevel.Instance.CurrentCombat = combat;
        Destroy(this.gameObject);
    }
}
