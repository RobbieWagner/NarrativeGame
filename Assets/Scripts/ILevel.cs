using System.Collections;
using System.Collections.Generic;
using RobbieWagnerGames;
using UnityEngine;

public class ILevel : MonoBehaviour
{
    [SerializeField] private ICombatManager combatManagerPrefab;
    [SerializeField] private Transform combatZone;

    public static ILevel Instance {get; private set;}

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
        } 
        else 
        { 
            Instance = this; 
        } 

        if(DialogueManager.Instance == null) Debug.LogWarning("Missing Dialogue Manager from scene, please create a Dialogue Manager and add to this scene.");
        if(ExplorationManager.Instance == null) Debug.LogWarning("Missing Exploration Manager from scene, please create an Exploration Manager and add to this scene.");
    }

    public IEnumerator CreateNewCombat(ICombat combat)
    {
        if(GameManager.Instance.currentGameMode != GameMode.Combat && ICombatManager.Instance == null)
        {
            GameManager.Instance.currentGameMode = GameMode.Combat;
            yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenIn());

            if(combatManagerPrefab != null)
            {
                Instantiate(combatManagerPrefab, combatZone);
                yield return null;
                ICombatManager.Instance.OnCombatResolved += EndCurrentCombat;
                ICombatManager.Instance.transform.localPosition = Vector3.zero;
                CameraManager.Instance.TrySwitchGameCamera(CombatCamera.Instance);
            }

            yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenOut());
            ICombatManager.Instance.StartNewCombat(combat);
        }
    }

    private IEnumerator EndCurrentCombat()
    {
        yield return null;
        if(ICombatManager.Instance != null) Destroy(ICombatManager.Instance.gameObject);
        GameManager.Instance.currentGameMode = GameMode.Exploration;
    }
}
