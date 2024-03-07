using System.Collections;
using System.Collections.Generic;
using Ink.Parsed;
using UnityEngine;
namespace PsychOutDestined
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance {get; private set;}
        
        private HashSet<GameCamera> gameCameras;
        private GameCamera activeGameCamera;
        public GameCamera ActiveGameCamera => activeGameCamera;
        public Camera ActiveCamera => activeGameCamera.cam;

        private void Awake()
        {
            if (Instance != null && Instance != this) 
                Destroy(gameObject); 
            else 
                Instance = this; 

            gameCameras = new HashSet<GameCamera>();
        }

        public void AddCamera(GameCamera camera, bool switchToNewCamera = false)
        {
            gameCameras.Add(camera);

            if(switchToNewCamera)
                TrySwitchGameCamera(camera);
        }

        public void RemoveCamera(GameCamera camera)
        {
            gameCameras.Remove(camera);
        }

        public bool TrySwitchGameCamera(GameCamera camera)
        {
            if(gameCameras.Contains(camera))
            {
                foreach(GameCamera cam in gameCameras)
                {
                    cam.cam.enabled = false;
                }
                activeGameCamera = camera;
                camera.cam.enabled = true;
                AudioListenerInstance.Instance.transform.position = camera.transform.position;
                return true;
            }
            Debug.LogWarning("Could not switch game cameras (game camera was never added to the manager)");
            return false;
        }
    }
}