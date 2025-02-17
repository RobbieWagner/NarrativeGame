using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsychOutDestined
{
    public class CombatLoadController : MonoBehaviour
    {
        private string currentCombatSceneName = "";
        private CombatBase currentCombat = null;

        public static CombatLoadController Instance { get; private set; }

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
        }

        public void StartLoadingCombatScene(CombatBase combat, string combatSceneName) => StartCoroutine(StartLoadingCombatSceneCo(combat, combatSceneName));

        public IEnumerator StartLoadingCombatSceneCo(CombatBase combat, string combatSceneNameIn)
        {
            if (string.IsNullOrWhiteSpace(currentCombatSceneName))
            {
                string combatSceneName = string.IsNullOrWhiteSpace(combatSceneNameIn) ? Level.Instance.combatSceneName : combatSceneNameIn;
                currentCombat = combat;
                currentCombatSceneName = combatSceneName;
                if (GameManager.Instance.CurrentGameMode != GameMode.Combat && CombatManagerBase.Instance == null)
                {
                    GameManager.Instance.CurrentGameMode = GameMode.Combat;
                    yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenIn());
                    AsyncOperation op = SceneManager.LoadSceneAsync(combatSceneName, LoadSceneMode.Additive);
                    yield return StartCoroutine(WaitForSceneLoad(combatSceneName, op));
                }
            }
            else
                Debug.LogWarning("Combat load scene failed: attempted to load a new combat while in combat");
        }

        private IEnumerator WaitForSceneLoad(string combatSceneName, AsyncOperation op)
        {
            while (!op.isDone)
            {
                //TODO: display UI like a load screen or something.
                yield return null;
            }

            if (SceneManager.GetSceneByName(combatSceneName) != null)
                FinishLoadingCombatScene();
            else
                HandleFailedCombatSceneLoad();
        }

        private void FinishLoadingCombatScene()
        {
            StartCoroutine(FinishLoadingCombatSceneCo());
        }

        private IEnumerator FinishLoadingCombatSceneCo()
        {
            CombatManagerBase.Instance.OnCombatTerminated += EndCurrentCombat;
            //CombatManagerBase.Instance.transform.localPosition = Vector3.zero;
            CameraManager.Instance.TrySwitchGameCamera(CombatCamera.Instance);

            yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenOut());
            CombatManagerBase.Instance.StartNewCombat(currentCombat);
        }

        private void EndCurrentCombat()
        {
            StartCoroutine(EndCurrentCombatCo());
        }

        private IEnumerator EndCurrentCombatCo()
        {
            yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenIn());
            SceneManager.UnloadSceneAsync(currentCombatSceneName);
            currentCombat = null;
            CameraManager.Instance.TrySwitchGameCamera(ExplorationCamera.Instance);
            yield return StartCoroutine(SceneTransitionController.Instance?.FadeScreenOut());

            currentCombatSceneName = "";
            currentCombat = null;
            OnCombatEnded?.Invoke();
        }

        private void HandleFailedCombatSceneLoad()
        {
            StartCoroutine(SceneTransitionController.Instance?.FadeScreenOut());
            Debug.LogWarning("CombatLoadControllerFailed to load a combat scene");
            OnCombatEnded?.Invoke();
        }
        public delegate void CombatEndedDelegate();
        public event CombatEndedDelegate OnCombatEnded;
    }
}