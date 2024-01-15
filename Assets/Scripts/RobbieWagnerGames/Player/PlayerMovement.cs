using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames
{
    public class PlayerMovement : MonoBehaviour
    {

        [HideInInspector] public bool canMove;
        [HideInInspector] public bool moving = false;

        private Vector3 movementVector;
        [SerializeField] private float defaultWalkSpeed = 3f;
        private float currentWalkSpeed;
        [SerializeField] public UnitAnimator movementAnimator;

        private CharacterController body;
        private bool isGrounded;
        private float GRAVITY = -7.5f;
        private Vector3 lastFramePos;
        [SerializeField] private LayerMask groundMask;

        private Vector3 lastPosition;
        private bool movingForcibly = false;
        [SerializeField] private CharacterController characterController;

        [Header("Footstep Sounds")]
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioClip[] footstepSoundClips;
        private int currentGroundType;
        public int CurrentGroundType
        {
            get { return currentGroundType; }
            set
            {
                if(currentGroundType == value) return;
                currentGroundType = value;
                ChangeFootstepSounds(footstepSoundClips[currentGroundType]);
            }
        }

        public static PlayerMovement Instance {get; private set;}

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

            currentWalkSpeed = defaultWalkSpeed;
            canMove = true;
            movementVector = Vector3.zero;

            body = GetComponent<CharacterController>();
            isGrounded = false;
        }

        private void LateUpdate() 
        {
            RaycastHit hit;
            isGrounded = Physics.Raycast(transform.position - new Vector3(0,0,0), Vector3.down, out hit, .05f, groundMask);

            if (hit.collider != null)
            {
                GroundInfo groundInfo = hit.collider.gameObject.GetComponent<GroundInfo>();
                if(groundInfo != null)
                {
                    if((int) groundInfo.groundType < footstepSoundClips.Length)
                    {
                        CurrentGroundType = (int) groundInfo.groundType;
                    }
                }
                else 
                {
                    CurrentGroundType = 0;
                }
            }
            
            if(!isGrounded)
            {
                movementVector.y += GRAVITY * Time.deltaTime;
                StartCoroutine(FootStepStopTimer(.25f));
            }
            else
            {
                movementVector.y = 0f;
            }

            if(characterController.enabled) body.Move(movementVector * currentWalkSpeed * Time.deltaTime);

            lastFramePos = transform.position;

            if(movingForcibly)
            {
                Animate();
            }

            if(moving && !footstepAudioSource.isPlaying) 
            {
                PlayMovementSounds();
            }
        }

        private void OnMove(InputValue inputValue)
        {
            if(canMove)
            {
                Vector2 input = inputValue.Get<Vector2>();

                if(movementVector.x != input.x && input.x != 0f)
                {
                    if(input.x > 0) movementAnimator.ChangeAnimationState(UnitAnimationState.WalkRight);
                    else movementAnimator.ChangeAnimationState(UnitAnimationState.WalkLeft);
                    moving = true;
                }
                else if(input.x == 0 && movementVector.z != input.y && input.y != 0f)
                {
                    if(input.y > 0) movementAnimator.ChangeAnimationState(UnitAnimationState.WalkForward);
                    else movementAnimator.ChangeAnimationState(UnitAnimationState.WalkBack);
                    moving = true;
                }
                else if(input.x == 0 && input.y == 0)
                {
                    if(movementVector.x > 0)movementAnimator.ChangeAnimationState(UnitAnimationState.IdleRight);
                    else if(movementVector.x < 0)movementAnimator.ChangeAnimationState(UnitAnimationState.IdleLeft);
                    else if(movementVector.z > 0) movementAnimator.ChangeAnimationState(UnitAnimationState.IdleForward);
                    else movementAnimator.ChangeAnimationState(UnitAnimationState.Idle);
                    moving = false;
                    StopMovementSounds();
                }
                
                movementVector.x = input.x;
                movementVector.z = input.y;
            }
        }

        private void OnDisable()
        {
            StopPlayer();
        }

        public void StopPlayer()
        {
            if(movementVector.x > 0) movementAnimator.ChangeAnimationState(UnitAnimationState.IdleRight);
            else if(movementVector.x < 0) movementAnimator.ChangeAnimationState(UnitAnimationState.IdleLeft);
            else if(movementVector.z > 0) movementAnimator.ChangeAnimationState(UnitAnimationState.IdleForward);
            else if(movementVector != Vector3.zero) movementAnimator.ChangeAnimationState(UnitAnimationState.Idle);

            movementVector = Vector3.zero;
            moving = false;
            StopMovementSounds();
        }

        public void CeasePlayerMovement()
        {
            canMove = false;
            StopPlayer();
        }

        public IEnumerator MoveUnitToSpot(Vector3 position, float unitsPerSecond = -1)
        {
            CeasePlayerMovement();
            PlayMovementSounds();
            characterController.enabled = false;

            lastPosition = transform.position;
            if(unitsPerSecond < 0) unitsPerSecond = currentWalkSpeed;
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

            if(Math.Abs(positionDelta.x) > Math.Abs(positionDelta.z))
            {
                if(positionDelta.x > 0) movementAnimator.ChangeAnimationState(UnitAnimationState.WalkRight);
                else movementAnimator.ChangeAnimationState(UnitAnimationState.WalkLeft);
            }
            else
            {
                if(positionDelta.z > 0) movementAnimator.ChangeAnimationState(UnitAnimationState.WalkForward);
                else movementAnimator.ChangeAnimationState(UnitAnimationState.WalkBack);
            }

            lastPosition = transform.position;
        }

        private void ChangeFootstepSounds(AudioClip clip)
        {
            StopMovementSounds();
            footstepAudioSource.clip = clip;
        }
        
        public void PlayMovementSounds()
        {
            footstepAudioSource.Play();
        }

        public void StopMovementSounds()
        {
            footstepAudioSource.Stop();
        }

        private IEnumerator FootStepStopTimer(float timeToTurnOff)
        {
            float timerValue = 0f;
            while (timerValue < timeToTurnOff)
            {
                yield return null;
                if(isGrounded) break;
                timerValue = Time.deltaTime;
            }
            if(timerValue >= timeToTurnOff) StopMovementSounds();

            StopCoroutine(FootStepStopTimer(timeToTurnOff));
        }
    }
}