using System.Collections;
using System.Collections.Generic;
using Ink.Parsed;
using UnityEngine;

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
        foreach(GameCamera cam in gameCameras) Debug.Log(cam.gameObject.name);

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
                cam.audioListener.enabled = false;
                cam.cam.enabled = false;
            }
            activeGameCamera = camera;
            camera.cam.enabled = true;
            camera.audioListener.enabled = true;
            return true;
        }
        foreach(GameCamera cam in gameCameras) Debug.Log(cam.gameObject.name);
        Debug.LogWarning("Could not switch game cameras (game camera was never added to the manager)");
        return false;
    }
}
