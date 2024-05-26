using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

namespace PsychOutDestined
{
    public class SceneTransitionController : MonoBehaviour
    {
        [SerializeField] private Canvas transitionScreen;
        [SerializeField] private Image transitionImage;

        public static SceneTransitionController Instance { get; private set; }

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

            transitionScreen.enabled = false;
        }

        public IEnumerator FadeScreenIn()
        {
            transitionImage.color = Color.clear;
            transitionScreen.enabled = true;
            yield return transitionImage.DOColor(Color.white, 1f).WaitForCompletion();
        }

        public IEnumerator FadeScreenOut()
        {
            yield return transitionImage.DOColor(Color.clear, 1f).WaitForCompletion();
            transitionImage.color = Color.clear;
            transitionScreen.enabled = false;
        }

        public void TurnOnScreenCover()
        {
            transitionImage.color = Color.black;
            transitionScreen.enabled = true;
        }
    }
}