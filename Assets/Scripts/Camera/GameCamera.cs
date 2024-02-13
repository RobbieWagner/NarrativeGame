using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Camera cam;
    public bool switchToOnEnable = false;
    public Vector3 defaultPosition = Vector3.zero;

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

    public virtual IEnumerator MoveCamera(Vector3 position, float time = 1f)
    {
        yield return transform.DOMove(position, time).WaitForCompletion();
    }
    
    public virtual IEnumerator MoveCameraSpeed(Vector3 position, int speed = 1)
    {
        if(speed < 1) StopCoroutine(MoveCamera(position, speed)); 
        yield return transform.DOMove(position, Vector3.Distance(transform.position, position)/speed).WaitForCompletion();
    }

    public virtual IEnumerator ResetCameraPosition(float time = 1f)
    {
        yield return transform.DOMove(defaultPosition, time).WaitForCompletion();
    }

    public virtual IEnumerator ResetCameraPositionSpeed(int speed = 1)
    {
        if(speed < 1) StopCoroutine(ResetCameraPosition(speed)); 
        yield return transform.DOMove(defaultPosition, Vector3.Distance(transform.position, defaultPosition)/speed).WaitForCompletion();
    }
}
