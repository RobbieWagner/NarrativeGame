using UnityEngine;

namespace PsychOutDestined
{
    public class ExplorationManager : MonoBehaviour
    {
        public static ExplorationManager Instance { get; private set; }

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

            //TODO: NOT HARDCODE THIS CALL
            StartExploration();
        }

        public void StartExploration() => GameManager.Instance.CurrentGameMode = GameMode.Exploration;
    }
}