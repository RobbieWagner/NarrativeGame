using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace PsychOutDestined
{
    public class TutorialUI : MonoBehaviour
    {
        public static TutorialUI Instance { get; private set; }

        public Canvas canvas;
        [SerializeField] private Image visualAid;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI tutorialText;
        [SerializeField] private TextMeshProUGUI pageCounter;
        [SerializeField] private Image nextImage;
        [SerializeField] private Image prevImage;
        [SerializeField] private Image closeImage;
        [SerializeField] private float minTextHeight = 100;

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
        }

        public void DisplayTutorialPage(TutorialPage page, int pageNumber, int pageMax, string defaultTutorialTitle = "")
        {
            if (page.visualAidImage == null)
            {
                tutorialText.rectTransform.sizeDelta = new Vector2(tutorialText.rectTransform.sizeDelta.x, minTextHeight + visualAid.rectTransform.sizeDelta.y);
                visualAid.gameObject.SetActive(false);
            }
            else
            {
                tutorialText.rectTransform.sizeDelta = new Vector2(tutorialText.rectTransform.sizeDelta.x, minTextHeight);
                visualAid.sprite = page.visualAidImage;
                visualAid.gameObject.SetActive(true);
            }

            prevImage.enabled = pageNumber > 1;
            nextImage.enabled = pageNumber < pageMax;
            closeImage.enabled = pageNumber == pageMax;

            titleText.text = string.IsNullOrWhiteSpace(page.pageTutorialTitle) ? defaultTutorialTitle : page.pageTutorialTitle;
            pageCounter.text = $"{pageNumber}/{pageMax}";

            tutorialText.text = page.tutorialText;
            canvas.enabled = true;
        }
    }
}