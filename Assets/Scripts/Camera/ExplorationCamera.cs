using UnityEngine;

namespace PsychOutDestined
{
    public class ExplorationCamera : GameCamera
    {
        public static ExplorationCamera Instance { get; private set; }

        protected override void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            base.Awake();
        }
    }
}