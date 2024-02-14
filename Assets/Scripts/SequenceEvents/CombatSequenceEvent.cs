using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatSequenceEvent : SequenceEvent
{
    [SerializeField] private ICombat combat;
    [SerializeField] private string combatSceneName;
    private bool combatIsPlaying = false;
    private GameMode? previousGameMode = null;

    public override IEnumerator InvokeSequenceEvent()
    {
        yield return StartCoroutine(base.InvokeSequenceEvent());
        combatIsPlaying = true;
        if(CombatLoadController.Instance != null) 
        {
            previousGameMode = GameManager.Instance?.CurrentGameMode;
            yield return StartCoroutine(CombatLoadController.Instance.StartLoadingCombatSceneCo(combat, combatSceneName));
            CombatLoadController.Instance.OnCombatEnded += OnCombatEnded;
        }
        else
        {
            Debug.LogWarning("Could not start combat event: Could not find CombatLoadController");
            combatIsPlaying = false;
        }

        while(combatIsPlaying) yield return null;
    }

    private void OnCombatEnded()
    {
        combatIsPlaying = false;
        GameManager.Instance.CurrentGameMode = previousGameMode != null ? previousGameMode.GetValueOrDefault() : GameMode.Exploration;
    }
}
