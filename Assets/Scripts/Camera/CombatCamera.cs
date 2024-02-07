using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CombatCamera : GameCamera
{

    public Vector3 defaultPosition = Vector3.zero;

    public static CombatCamera Instance {get; private set;}

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

    public IEnumerator MoveCamera(Vector3 position, float time = 1f)
    {
        yield return transform.DOMove(position, time).WaitForCompletion();
    }
    public IEnumerator MoveCameraSpeed(Vector3 position, int speed = 1)
    {
        if(speed < 1) StopCoroutine(MoveCamera(position, speed)); 
        yield return transform.DOMove(position, Vector3.Distance(transform.position, position)/speed).WaitForCompletion();
    }

    public IEnumerator ResetCameraPosition(float time = 1f)
    {
        yield return transform.DOMove(defaultPosition, time).WaitForCompletion();
    }

    public IEnumerator ResetCameraPositionSpeed(int speed = 1)
    {
        if(speed < 1) StopCoroutine(ResetCameraPosition(speed)); 
        yield return transform.DOMove(defaultPosition, Vector3.Distance(transform.position, defaultPosition)/speed).WaitForCompletion();
    }
}
