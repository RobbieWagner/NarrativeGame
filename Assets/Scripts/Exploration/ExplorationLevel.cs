using System;
using System.Collections;
using System.Collections.Generic;
using RobbieWagnerGames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExplorationLevel : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private ICombatManager combatManagerPrefab;
    [SerializeField] private Transform combatZone;
    public string combatSceneName;

    private Scene currentCombatScene;
    private ICombat currentCombat;
    public ICombat CurrentCombat
    {
        get => currentCombat;
        set
        {
            if(currentCombat != null && currentCombat.Equals(value)) return;
            currentCombat = value;
            if(CombatLoadController.Instance != null) 
            {
                CombatLoadController.Instance?.StartLoadingCombatScene(currentCombat, combatSceneName);
                CombatLoadController.Instance.OnCombatEnded += ResetAfterCombat;
            }
        }
    }

    public static ExplorationLevel Instance { get; private set; }

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

        if (DialogueManager.Instance == null) Debug.LogWarning("Missing Dialogue Manager from scene, please create a Dialogue Manager and add to this scene.");
        if (ExplorationManager.Instance == null) Debug.LogWarning("Missing Exploration Manager from scene, please create an Exploration Manager and add to this scene.");
    }

    private void ResetAfterCombat()
    {
        GameManager.Instance.CurrentGameMode = GameMode.Exploration;
        CombatLoadController.Instance.OnCombatEnded -= ResetAfterCombat;
    }
}
