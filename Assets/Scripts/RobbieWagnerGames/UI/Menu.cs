using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RobbieWagnerGames.UI
{
    public class Menu : MonoBehaviour
    {

        [SerializeField] public Canvas thisCanvas;

        [SerializeField] protected Button backButton;
        [HideInInspector] public Canvas lastCanvas;

        protected virtual void Awake()
        {
            
        }
        
        protected virtual void OnEnable()
        {
            ToggleButtonInteractibility(true);

            if(backButton != null) backButton.onClick.AddListener(BackToLastMenu);
        }

        protected virtual void OnDisable()
        {
        ToggleButtonInteractibility(false);  

        if(backButton != null) backButton.onClick.RemoveListener(BackToLastMenu);
        }

        protected virtual void ToggleButtonInteractibility(bool toggleOn)
        {
            if(backButton != null) backButton.interactable = toggleOn;
        }

        protected virtual void BackToLastMenu()
        {
            if(lastCanvas != null)
            {
                StartCoroutine(SwapCanvases(thisCanvas, lastCanvas));
            }
        }

        protected virtual IEnumerator SwapCanvases(Canvas active, Canvas next)
        {
            yield return new WaitForSecondsRealtime(.1f);

            Menu activeMenu = active.gameObject.GetComponent<Menu>();
            Menu nextMenu = next.gameObject.GetComponent<Menu>();

            active.enabled = false;
            next.enabled = true;

            nextMenu.enabled = true;
            nextMenu.ToggleButtonInteractibility(true);
            nextMenu.lastCanvas = activeMenu.thisCanvas;
            activeMenu.enabled = false;
        
            StopCoroutine(SwapCanvases(active, next));
        }
    }
}
