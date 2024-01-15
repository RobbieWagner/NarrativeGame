using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames.FirstPerson
{
    public class SimpleFirstPersonPlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        private bool canMove = true;
        private bool isMoving = false;
        [SerializeField] private float initialSpeed = 5f;
        private float currentSpeed;
        private Vector3 inputVector = Vector3.zero;
        private PlayerMovementActions inputActions;

        [Header("Physics Components")]
        [SerializeField] private CharacterController characterController;

        [Header("Grounding and Gravity")]
        private bool isGrounded = false;
        [SerializeField] private float groundCheckDistance = 3f;
        private float GRAVITY = -9.8f;
        private float TERMINAL_VELOCITY = -100f;
        private float currentYVelocity = 0f;

        // Start is called before the first frame update
        void Start()
        {
            inputActions = new PlayerMovementActions();
            currentSpeed = initialSpeed;
        }

        void LateUpdate()
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance);

            isGrounded = hit.collider != null ? hit.collider.gameObject.CompareTag("Ground") : false;

            Vector3 movementVector = transform.right * inputVector.x + transform.forward * inputVector.z;

            if(!isGrounded && currentYVelocity > TERMINAL_VELOCITY) 
            {
                movementVector = new Vector3(movementVector.x, currentYVelocity + GRAVITY * Time.deltaTime, movementVector.z);
            }
            else if(!isGrounded) 
            {
                movementVector = new Vector3(movementVector.x, TERMINAL_VELOCITY, movementVector.z);
            }
            else 
            {
                movementVector = new Vector3(movementVector.x, 0, movementVector.z);
            }

            if(characterController.enabled) characterController.Move(movementVector * currentSpeed * Time.deltaTime);
            currentYVelocity = movementVector.y;
        }

        private void OnMove(InputValue inputValue)
        {
            if(canMove)
            {
                Vector2 input = inputValue.Get<Vector2>();

                if(inputVector.x != input.x && input.x != 0f)
                {
                    isMoving = true;
                }
                else if(input.x == 0 && inputVector.z != input.y && input.y != 0f)
                {
                    isMoving = true;
                }
                else if(input.x == 0 && input.y == 0)
                {
                    isMoving = false;
                }
                
                inputVector.x = input.x;
                inputVector.z = input.y;
            }
        }
    }
}