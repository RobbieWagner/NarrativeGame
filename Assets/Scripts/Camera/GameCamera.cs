using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Camera cam;
    public bool switchToOnEnable = false;

    public void ActivateCamera()
    {
        CameraManager.Instance?.TrySwitchGameCamera(this);
    }

    protected void OnDestroy() => CameraManager.Instance?.RemoveCamera(this);

    protected virtual void Awake()
    {
        CameraManager.Instance?.AddCamera(this);

        if(switchToOnEnable)
            CameraManager.Instance?.TrySwitchGameCamera(this);
    }

    protected void OnEnable()
    {
        if(switchToOnEnable)
            CameraManager.Instance?.TrySwitchGameCamera(this);
    }
}
