using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public class IInteractable : MonoBehaviour
    {

        [HideInInspector] public bool canInteract;
        [HideInInspector] protected ExplorationControls explorationControls;

        [Header("Visual Cue")]
        [SerializeField] protected SpriteRenderer visualCuePrefab;
        [SerializeField] protected Vector3 VISUAL_CUE_OFFSET; 
        protected SpriteRenderer currentVisualCue;

        protected virtual void Awake()
        {
            canInteract = false;
            explorationControls = new ExplorationControls();
            //IInputManager.Instance.RegisterActionCollection(explorationControls);
            explorationControls.Exploration.Interact.performed += OnInteract;
            GameManager.Instance.OnGameModeChanged += CheckGameMode;
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

        protected virtual void OnInteract(InputAction.CallbackContext context)
        {
            if(canInteract)
            {
                canInteract = false;
                if(PlayerMovement.Instance != null) PlayerMovement.Instance.DisablePlayerMovement();
                StartCoroutine(Interact());
            }
        }

        protected virtual void OnUninteract()
        {
            //ExplorationManager.Instance.currentInteractable = null;
            canInteract = true;
            PlayerMovement.Instance?.EnablePlayerMovement();
        }

        protected virtual IEnumerator Interact()
        {
            yield return null;
            OnUninteract();
            StopCoroutine(Interact());
        }

        private void CheckGameMode(GameMode gameMode)
        {
            if (gameMode == GameMode.Exploration)
                IInputManager.Instance.RegisterActionCollection(explorationControls);
            else
                IInputManager.Instance.DeregisterActionCollection(explorationControls);
        }
    }
}