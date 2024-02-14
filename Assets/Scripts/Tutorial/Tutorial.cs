using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class TutorialPage
{
    public string pageTutorialTitle;
    public Sprite visualAidImage;
    [TextArea(15,20)] public string tutorialText;
} 

public class Tutorial : MonoBehaviour
{
    [SerializeField] private string tutorialTitle;
    [SerializeField] private List<TutorialPage> tutorialPages;
    private int currentPage = -1;
    private bool isOnLastPage => currentPage == tutorialPages.Count - 1;
    private TutorialControls controls;
    private bool checkingInitialNextState = false;
    private bool checkingInitialPrevState = false;
    private float nextInputValue = 2;
    private float prevInputValue = 2;

    private void Awake()
    {
        controls = new TutorialControls();
    }

    private void SubscribeControls()
    {
        controls.Enable();
        controls.Tutorial.Next.performed += NextPage;
        controls.Tutorial.NextAxis.performed += NextPageAxis;
        controls.Tutorial.Previous.performed += PreviousPage;
        controls.Tutorial.CloseTutorial.started += CloseTutorial;
        checkingInitialNextState = true;
        checkingInitialPrevState = true;
        nextInputValue = 1;
        prevInputValue = 1;
    }

    private void UnsubscribeControls()
    {
        controls.Disable();
        controls.Tutorial.Next.performed -= NextPage;
        controls.Tutorial.NextAxis.performed -= NextPageAxis;
        controls.Tutorial.Previous.performed -= PreviousPage;
        controls.Tutorial.CloseTutorial.started -= CloseTutorial;
        checkingInitialNextState = false;
        checkingInitialPrevState = false;
    }

    public void OpenTutorial()
    {
        if(TutorialUI.Instance != null)
        {
            currentPage = 0;
            if(tutorialPages.Count > 0)
            {
                TutorialUI.Instance?.DisplayTutorialPage(tutorialPages[currentPage], currentPage + 1, tutorialPages.Count, tutorialTitle);
                SubscribeControls();
            }
            else
            {
                Debug.LogWarning("No pages found in tutorial. Please add pages before attempting to display");
                OnCompleteTutorial?.Invoke();
            }
        }
        else Debug.LogWarning("No UI was found to display tutorial. Please add an object with the TutorialUI component to this scene");
    } 

    public void NextPage(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        if(!isOnLastPage && value >= nextInputValue)
        {
            currentPage++;
            TutorialUI.Instance?.DisplayTutorialPage(tutorialPages[currentPage], currentPage + 1, tutorialPages.Count, tutorialTitle);
        }
        nextInputValue = value;
    }

    public void NextPageAxis(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
         if(!isOnLastPage && value != 0 && value != nextInputValue && value != 1)
        {
            currentPage++;
            TutorialUI.Instance?.DisplayTutorialPage(tutorialPages[currentPage], currentPage + 1, tutorialPages.Count, tutorialTitle);
        }
        nextInputValue = value;
    }

    public void PreviousPage(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        if(currentPage > 0 && value >= prevInputValue)
        {
            currentPage--;
            TutorialUI.Instance?.DisplayTutorialPage(tutorialPages[currentPage], currentPage + 1, tutorialPages.Count, tutorialTitle);
        }
        prevInputValue = value;
    }    

    public void CloseTutorial(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        if(isOnLastPage && value >= nextInputValue)
        {
            TutorialUI.Instance.canvas.enabled = false;
            OnCompleteTutorial?.Invoke();
            UnsubscribeControls();
        }
    }

    public delegate void OnCompleteTutorialDelegate();
    public event OnCompleteTutorialDelegate OnCompleteTutorial;
}