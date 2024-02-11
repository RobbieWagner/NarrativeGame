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

    private void Awake()
    {
        controls = new TutorialControls();
        controls.Tutorial.Next.started += NextPage;
        controls.Tutorial.Previous.started += PreviousPage;
        controls.Tutorial.CloseTutorial.started += CloseTutorial;
    }

    public void OpenTutorial()
    {
        if(TutorialUI.Instance != null)
        {
            currentPage = 0;
            if(tutorialPages.Count > 0)
            {
                TutorialUI.Instance?.DisplayTutorialPage(tutorialPages[currentPage], currentPage + 1, tutorialPages.Count, tutorialTitle);
                controls.Enable();
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
        if(!isOnLastPage)
        {
            currentPage++;
            TutorialUI.Instance?.DisplayTutorialPage(tutorialPages[currentPage], currentPage + 1, tutorialPages.Count, tutorialTitle);
        }
    }

    public void PreviousPage(InputAction.CallbackContext context)
    {
        if(currentPage > 0)
        {
            currentPage--;
            TutorialUI.Instance?.DisplayTutorialPage(tutorialPages[currentPage], currentPage + 1, tutorialPages.Count, tutorialTitle);
        }
    }    

    public void CloseTutorial(InputAction.CallbackContext context)
    {
        if(isOnLastPage)
        {
            TutorialUI.Instance.canvas.enabled = false;
            OnCompleteTutorial?.Invoke();
            controls.Disable();
        }
    }

    public delegate void OnCompleteTutorialDelegate();
    public event OnCompleteTutorialDelegate OnCompleteTutorial;
}