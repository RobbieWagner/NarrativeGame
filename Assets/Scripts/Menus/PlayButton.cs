using System.Collections;
using RobbieWagnerGames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MenuButton
{
    [SerializeField] private bool deleteCurrentProgress = false;
    public override IEnumerator SelectButton(Menu menu)
    {
        yield return StartCoroutine(base.SelectButton(menu));
        if(deleteCurrentProgress)
        {
            SaveDataManager.persistentPath = Application.persistentDataPath;
            SaveDataManager.PurgeAllSaveData();
        }
        
        StartGame();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}