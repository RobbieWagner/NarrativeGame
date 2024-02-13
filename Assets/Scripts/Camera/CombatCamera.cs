using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CombatCamera : GameCamera
{
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
}
