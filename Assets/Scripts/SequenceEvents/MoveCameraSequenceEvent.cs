using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCameraSequenceEvent : SequenceEvent
{
    [SerializeField] private bool resetCamera = false;
    [SerializeField] private Vector3 camPosition = Vector3.zero;
    [SerializeField] private int movement = 1;
    [SerializeField] private bool isMovementValueCameraSpeed = false;

    public override IEnumerator InvokeSequenceEvent()
    {
        if(CameraManager.Instance != null)
        {
            if(isMovementValueCameraSpeed)
            {
                if(resetCamera)
                    yield return StartCoroutine(CameraManager.Instance.ActiveGameCamera.ResetCameraPositionSpeed(movement));
                else
                    yield return StartCoroutine(CameraManager.Instance.ActiveGameCamera.MoveCameraSpeed(camPosition, movement));
            }
            else
            {
                if(resetCamera)
                    yield return StartCoroutine(CameraManager.Instance.ActiveGameCamera.ResetCameraPosition(movement));
                else
                    yield return StartCoroutine(CameraManager.Instance.ActiveGameCamera.MoveCamera(camPosition, movement));
            }
        }
        else
            Debug.LogWarning("Could not move camera: no camera manager was found!");
    }
}
