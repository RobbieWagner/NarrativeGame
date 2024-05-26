using System;
using System.Collections;
using System.Collections.Generic;
using RobbieWagnerGames;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsychOutDestined
{
    public class Level : MonoBehaviour
    {
        [Header("Combat")]
        public string combatSceneName;
        [HideInInspector] public string explorationSceneName;

        [SerializeField] private ExplorationEvent preLoadEventSequence;
        [SerializeField] private EventSequence fadeOutScreenCover;
        [SerializeField] private ExplorationEvent postLoadEventSequence;

        private CombatBase currentCombat;
        public CombatBase CurrentCombat
        {
            get => currentCombat;
            set
            {
                if (currentCombat != null && currentCombat.Equals(value)) return;
                Debug.Log("new combat");
                currentCombat = value;
                if (CombatLoadController.Instance != null)
                {
                    Debug.Log("new combat");
                    CombatLoadController.Instance?.StartLoadingCombatScene(currentCombat, combatSceneName);
                    CombatLoadController.Instance.OnCombatEnded += ResetAfterCombat;
                }
            }
        }

        public static Level Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (Instance == this)
            {
                if (DialogueManager.Instance == null)
                    Debug.LogWarning("Missing Dialogue Manager from scene, please create a Dialogue Manager and add to this scene.");
                if (ExplorationManager.Instance == null)
                    Debug.LogWarning("Missing Exploration Manager from scene, please create an Exploration Manager and add to this scene.");

                explorationSceneName = gameObject.scene.name;

                StartCoroutine(StartExplorationScene());                
            }
        }

        private IEnumerator StartExplorationScene()
        {
            if (preLoadEventSequence != null)
                yield return StartCoroutine(preLoadEventSequence.InvokeEvent());
            if (fadeOutScreenCover != null)
                yield return StartCoroutine(fadeOutScreenCover.InvokeEvent());
            if (postLoadEventSequence != null)
                yield return StartCoroutine(postLoadEventSequence.InvokeEvent());

            GameManager.Instance.CurrentGameMode = GameMode.Exploration;
        }

        private void ResetAfterCombat()
        {
            GameManager.Instance.CurrentGameMode = GameMode.Exploration;
            CombatLoadController.Instance.OnCombatEnded -= ResetAfterCombat;
        }
    }
}