using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames
{
    public class IInteractable : MonoBehaviour
    {

        [HideInInspector] public bool canInteract;
        [HideInInspector] protected PlayerInputActions playerControls;

        [Header("Visual Cue")]
        [SerializeField] protected SpriteRenderer visualCuePrefab;
        [SerializeField] protected Vector3 VISUAL_CUE_OFFSET; 
        protected SpriteRenderer currentVisualCue;

        protected virtual void Awake()
        {
            canInteract = false;
            playerControls = new PlayerInputActions();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.CompareTag("Player")) 
            {
                canInteract = true;
                currentVisualCue = Instantiate(visualCuePrefab, this.transform).GetComponent<SpriteRenderer>();
                currentVisualCue.transform.position += VISUAL_CUE_OFFSET;
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if(other.gameObject.CompareTag("Player")) 
            {
                canInteract = false;
                if(currentVisualCue != null) 
                {
                    Destroy(currentVisualCue);
                    currentVisualCue = null;
                }
            }
        }

        protected virtual void OnInteract(InputValue inputValue)
        {
            if(canInteract) //&& ExplorationManager.Instance.currentInteractable == null)
            {
                //ExplorationManager.Instance.currentInteractable = this;
                if(PlayerMovement.Instance != null) PlayerMovement.Instance.CeasePlayerMovement();
                StartCoroutine(Interact());
            }
        }

        protected virtual void OnUninteract()
        {
            //ExplorationManager.Instance.currentInteractable = null;
            canInteract = true;
            if(PlayerMovement.Instance != null) PlayerMovement.Instance.canMove = true;
        }

        protected virtual IEnumerator Interact()
        {
            yield return null;

            OnUninteract();
            StopCoroutine(Interact());
        }
    }
}