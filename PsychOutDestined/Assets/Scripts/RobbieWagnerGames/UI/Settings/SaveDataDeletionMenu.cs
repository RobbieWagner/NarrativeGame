using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobbieWagnerGames.UI
{
    public class SaveDataDeletionMenu : MenuTab
    {
        [Header("Save Data Deletion")]
        [SerializeField] private TextMeshProUGUI deleteDataText;
        [SerializeField] private Button deleteSaveDataButtonPrefab;
        private Button deleteSaveDataButton;

        public override void BuildTab()
        {
            base.BuildTab();
            Instantiate(deleteDataText, tabContentParent.transform);
            deleteSaveDataButton = Instantiate(deleteSaveDataButtonPrefab, tabContentParent.transform).GetComponent<Button>();

            deleteSaveDataButton.onClick.AddListener(DeleteSaveData);
        }

        private void DeleteSaveData()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}