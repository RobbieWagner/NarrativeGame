using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace PsychOutDestined
{
    public enum UnitAnimationState
    {
        //movement
        Idle = 0,
        IdleForward = 1,
        IdleLeft = 2,
        IdleRight = 3,

        WalkForward = 4,
        WalkBack = 5,
        WalkLeft = 6,
        WalkRight = 7,

        CombatIdleLeft = 8,
        CombatIdleRight = 9,
    }

    public class UnitAnimator : MonoBehaviour
    {

        [SerializeField] public Animator animator;

        [SerializeField] private List<UnitAnimationState> states;
        private UnitAnimationState currentState;

        [SerializeField] private SpriteRenderer unitSprite;

        protected virtual void Awake()
        {
            OnAnimationStateChange += StartAnimation;
            SetAnimationState(UnitAnimationState.Idle);
        }

        public void SetAnimationState(UnitAnimationState state)
        {
            if (state != currentState && states.Contains(state))
            {
                currentState = state;

                OnAnimationStateChange(state);
            }
            else if (state != currentState)
            {
                Debug.LogWarning($"Animation Clip Not Set Up For Unit {state}");
            }
        }

        public delegate void OnAnimationStateChangeDelegate(UnitAnimationState state);
        public event OnAnimationStateChangeDelegate OnAnimationStateChange;

        public UnitAnimationState GetAnimationState()
        {
            return currentState;
        }

        protected void StartAnimation(UnitAnimationState state)
        {
            animator.Play(state.ToString());
        }
    }
}