using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public class PlayerMovement : MonoBehaviour
    {
        [HideInInspector] public bool moving = false;

        private Vector3 movementVector;
        [SerializeField] private float defaultWalkSpeed = 3f;
        private float currentWalkSpeed;
        [SerializeField] public UnitAnimator movementAnimator;

        private ExplorationControls explorationControls;

        private bool isGrounded;
        private float GRAVITY = -7.5f;
        private Vector3 lastFramePos;
        [SerializeField] private LayerMask groundMask;

        private Vector3 lastPosition;
        private bool movingForcibly = false;
        [SerializeField] private CharacterController characterController;

        private bool playingFootstepSounds = false;
        private GroundType currentGroundType = GroundType.None;
        public GroundType CurrentGroundType
        {
            get { return currentGroundType; }
            set
            {
                if (currentGroundType == value) 
                    return;

                currentGroundType = value;
                if (AudioEventsLibrary.Instance.groundTypeSounds.ContainsKey(currentGroundType)) 
                    StartCoroutine(ChangeFootstepSounds(currentGroundType));
                else 
                    StartCoroutine(ChangeFootstepSounds());
            }
        }
        public EventInstance footstepSounds;

        public static PlayerMovement Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            GameManager.Instance.OnGameModeChanged += CheckGameMode;

            currentWalkSpeed = defaultWalkSpeed;
            movementVector = Vector3.zero;

            explorationControls = new ExplorationControls();
            IInputManager.Instance.RegisterActionCollection(explorationControls);

            explorationControls.Movement.Move.performed += HandleMovementInput;
            explorationControls.Movement.Move.canceled += StopPlayer;

            isGrounded = false;
            UpdateGroundCheck();
        }

        private void CheckGameMode(GameMode gameMode)
        {
            if (gameMode == GameMode.Exploration)
                IInputManager.Instance.RegisterActionCollection(explorationControls);
            else
                IInputManager.Instance.DeregisterActionCollection(explorationControls);
        }

        private void LateUpdate()
        {
            UpdateGroundCheck();

            if (characterController.enabled) 
                characterController.Move(movementVector * currentWalkSpeed * Time.deltaTime);

            lastFramePos = transform.position;

            if (movingForcibly)
                Animate();

            if (moving)
                PlayMovementSounds();
        }

        private void UpdateGroundCheck()
        {
            RaycastHit hit;
            isGrounded = Physics.Raycast(transform.position + new Vector3(0, .01f, 0), Vector3.down, out hit, .05f, groundMask);

            if (hit.collider != null)
            {
                GroundInfo groundInfo = hit.collider.gameObject.GetComponent<GroundInfo>();
                if (groundInfo != null)
                    CurrentGroundType = groundInfo.groundType;  
                else
                    CurrentGroundType = GroundType.None;
            }

            if (!isGrounded)
                movementVector.y += GRAVITY * Time.deltaTime;
            else
                movementVector.y = 0f;
        }

        private void HandleMovementInput(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();

            if (movementVector.x != input.x && input.x != 0f)
            {
                if (input.x > 0) 
                    movementAnimator.SetAnimationState(UnitAnimationState.WalkRight);
                else 
                    movementAnimator.SetAnimationState(UnitAnimationState.WalkLeft);
                moving = true;
            }
            else if (input.x == 0 && movementVector.z != input.y && input.y != 0f)
            {
                if (input.y > 0) 
                    movementAnimator.SetAnimationState(UnitAnimationState.WalkForward);
                else 
                    movementAnimator.SetAnimationState(UnitAnimationState.WalkBack);
                moving = true;
            }
            else if (input.x == 0 && input.y == 0)
            {
                if (movementVector.x > 0) 
                    movementAnimator.SetAnimationState(UnitAnimationState.IdleRight);
                else if (movementVector.x < 0) 
                    movementAnimator.SetAnimationState(UnitAnimationState.IdleLeft);
                else if (movementVector.z > 0) 
                    movementAnimator.SetAnimationState(UnitAnimationState.IdleForward);
                else 
                    movementAnimator.SetAnimationState(UnitAnimationState.Idle);
                moving = false;
                StopMovementSounds();
            }

            movementVector.x = input.x;
            movementVector.z = input.y;
        }

        private void OnDisable()
        {
            StopPlayer();
        }

        private void StopPlayer(InputAction.CallbackContext context) => StopPlayer();

        public void StopPlayer()
        {
            if (movementVector.x > 0) 
                movementAnimator.SetAnimationState(UnitAnimationState.IdleRight);
            else if (movementVector.x < 0) 
                movementAnimator.SetAnimationState(UnitAnimationState.IdleLeft);
            else if (movementVector.z > 0) 
                movementAnimator.SetAnimationState(UnitAnimationState.IdleForward);
            else if (movementVector != Vector3.zero) 
                movementAnimator.SetAnimationState(UnitAnimationState.Idle);

            movementVector = Vector3.zero;
            moving = false;
            StopMovementSounds();
        }

        public void DisablePlayerMovement()
        {
            IInputManager.Instance.DeregisterActionCollection(explorationControls);
            StopPlayer();
        }

        public void EnablePlayerMovement()
        {
            IInputManager.Instance.RegisterActionCollection(explorationControls);
        }

        public IEnumerator MoveUnitToSpot(Vector3 position, float unitsPerSecond = -1)
        {
            DisablePlayerMovement();
            PlayMovementSounds();
            characterController.enabled = false;

            lastPosition = transform.position;
            if (unitsPerSecond < 0) 
                unitsPerSecond = currentWalkSpeed;
            movingForcibly = true;
            yield return transform.DOMove(position, Vector3.Distance(position, transform.position) / unitsPerSecond)
                                            .SetEase(Ease.Linear).WaitForCompletion();
            movingForcibly = false;

            characterController.enabled = true;
            StopCoroutine(MoveUnitToSpot(position));
        }

        private void Animate()
        {
            Vector3 positionDelta = transform.position - lastPosition;

            if (Math.Abs(positionDelta.x) > Math.Abs(positionDelta.z))
            {
                if (positionDelta.x > 0) 
                    movementAnimator.SetAnimationState(UnitAnimationState.WalkRight);
                else
                    movementAnimator.SetAnimationState(UnitAnimationState.WalkLeft);
            }
            else
            {
                if (positionDelta.z > 0) 
                    movementAnimator.SetAnimationState(UnitAnimationState.WalkForward);
                else 
                    movementAnimator.SetAnimationState(UnitAnimationState.WalkBack);
            }

            lastPosition = transform.position;
        }

        public void PlayMovementSounds()
        {
            if(!playingFootstepSounds)
            {
                footstepSounds.start();
                playingFootstepSounds = true;
            }
        }

        public void StopMovementSounds()
        {
            if(playingFootstepSounds)
            {
                footstepSounds.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                playingFootstepSounds = false;
            }
        }

        public IEnumerator ChangeFootstepSounds(GroundType groundType = GroundType.None)
        {
            if(playingFootstepSounds)
            {
                StopMovementSounds();
                yield return null;
                footstepSounds.release();
                footstepSounds = AudioManager.CreateSoundEventInstance(AudioEventsLibrary.Instance.groundTypeSounds[groundType]);
                PlayMovementSounds();
            }
            else
                footstepSounds = AudioManager.CreateSoundEventInstance(AudioEventsLibrary.Instance.groundTypeSounds[GroundType.None]);
        }

        public bool IsFootstepSoundPlaying()
        {
            PLAYBACK_STATE playbackState;
            footstepSounds.getPlaybackState(out playbackState);
            Debug.Log(playbackState);
            return playbackState.Equals(PLAYBACK_STATE.PLAYING) || playbackState.Equals(PLAYBACK_STATE.SUSTAINING) || playbackState.Equals(PLAYBACK_STATE.STARTING);
        }

        private IEnumerator FootStepStopTimer(float timeToTurnOff)
        {
            float timerValue = 0f;
            while (timerValue < timeToTurnOff)
            {
                yield return null;
                if (isGrounded) break;
                timerValue = Time.deltaTime;
            }
            if (timerValue >= timeToTurnOff) 
                StopMovementSounds();

            StopCoroutine(FootStepStopTimer(timeToTurnOff));
        }

        public void SetPosition(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }
    }
}