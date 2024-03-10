using UnityEngine;

namespace PsychOutDestined
{
    public class ExplorationCamera : GameCamera
    {
        public static ExplorationCamera Instance { get; private set; }
        public GameObject followObject;

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

        protected override void OnEnable()
        {
            if(switchToOnEnable)
                CameraManager.Instance?.TrySwitchGameCamera(this, followObject);
        }
    }
}