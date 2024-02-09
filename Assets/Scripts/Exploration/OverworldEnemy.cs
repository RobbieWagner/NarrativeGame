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
        Debug.Log("col");
        if(other.gameObject.CompareTag("Player") && GameManager.Instance.currentGameMode == GameMode.Exploration)
        {
            if(ILevel.Instance != null)
            {
                Debug.Log("start combat!");
                StartCoroutine(StartCombat());
            }
            else
            {
                Debug.LogWarning("Found no level instance, could not start combat.");
            }
        }
    }

    protected virtual IEnumerator StartCombat()
    {
        yield return StartCoroutine(ILevel.Instance.CreateNewCombat(combat));
        Destroy(this.gameObject);
    }
}
