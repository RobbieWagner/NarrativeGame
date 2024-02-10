using System;
using System.Collections;
using System.Collections.Generic;
using RobbieWagnerGames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ILevel : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private ICombatManager combatManagerPrefab;
    [SerializeField] private Transform combatZone;
    [SerializeField] private string combatSceneName;

    private Scene currentCombatScene;
    private ICombat currentCombat;
    public ICombat CurrentCombat
    {
        get => currentCombat;
        set
        {
            if(currentCombat != null && currentCombat.Equals(value)) return;
            currentCombat = value;
            StartLoadingCombatScene();
        }
    }

    public static ILevel Instance { get; private set; }

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

    public void StartLoadingCombatScene()
    {
        StartCoroutine(StartLoadingCombatSceneCo());
    }

    public IEnumerator StartLoadingCombatSceneCo()
    {
        string combatScenePath = $"{StaticGameStats.sceneFilePath}{combatSceneName}.unity";
        if (GameManager.Instance.CurrentGameMode != GameMode.Combat && ICombatManager.Instance == null && SceneUtility.GetBuildIndexByScenePath(combatScenePath) != -1)
        {
            GameManager.Instance.CurrentGameMode = GameMode.Combat;
            yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenIn());
            //Instantiate(combatManagerPrefab, combatZone);
            SceneManager.LoadSceneAsync(combatSceneName, LoadSceneMode.Additive);
            SceneManager.sceneLoaded += FinishLoadingCombatScene;
        }
    }

    private void FinishLoadingCombatScene(Scene scene, LoadSceneMode sceneLoadMode)
    {
        if (scene.name.Trim().Equals(combatSceneName.Trim(), StringComparison.CurrentCultureIgnoreCase))
        {
            SceneManager.sceneLoaded -= FinishLoadingCombatScene;
            StartCoroutine(FinishLoadingCombatSceneCo());
        }
    }

    public IEnumerator FinishLoadingCombatSceneCo()
    {
        ICombatManager.Instance.OnCombatResolved += EndCurrentCombat;
        //ICombatManager.Instance.transform.localPosition = Vector3.zero;
        CameraManager.Instance.TrySwitchGameCamera(CombatCamera.Instance);

        yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenOut());
        ICombatManager.Instance.StartNewCombat(currentCombat);
    }

    private IEnumerator EndCurrentCombat()
    {
        yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenIn());
        SceneManager.UnloadSceneAsync(combatSceneName);
        GameManager.Instance.CurrentGameMode = GameMode.Exploration;
        currentCombat = null;
        CameraManager.Instance.TrySwitchGameCamera(ExplorationCamera.Instance);
        yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenOut());
    }
}
