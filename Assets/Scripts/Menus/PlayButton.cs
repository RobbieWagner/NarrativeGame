using System.Collections;
using RobbieWagnerGames;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsychOutDestined
{
    public class PlayButton : MenuButton
    {
        [SerializeField] private bool deleteCurrentProgress = false;
        public override IEnumerator SelectButton(Menu menu)
        {
            yield return StartCoroutine(base.SelectButton(menu));
            if (deleteCurrentProgress)
            {
                new JsonDataService();
                JsonDataService.Instance.PurgeData();
                JsonDataService.Instance.ResetInstance();
            }

            StartGame();
        }

        public void StartGame()
        {
            SceneManager.LoadScene("Game");
        }
    }
}