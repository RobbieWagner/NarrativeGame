using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobbieWagnerGames.UI
{
    public class GraphicsSettings : MenuTab
    {

        [Header("Graphics")]
        private int fullscreen;
        [SerializeField] private Toggle fullscreenTogglePrefab;
        private Toggle fullscreenToggle;
        [SerializeField] private TextMeshProUGUI toggleTextPrefab;
        private TextMeshProUGUI toggleText;

        Resolution[] resolutions;

        public override void BuildTab()
        {
            base.BuildTab();

            fullscreenToggle = Instantiate(fullscreenTogglePrefab, tabContentParent.transform).GetComponent<Toggle>();
            toggleText = Instantiate(toggleTextPrefab, tabContentParent.transform).GetComponent<TextMeshProUGUI>();

            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            toggleText.text = "Fullscreen";
        } 

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            if(isFullscreen) fullscreen = 1;
            else fullscreen = 0;

            if(isFullscreen)
            {
                resolutions = Screen.resolutions;
                Resolution resolution = resolutions[resolutions.Length-1];
                Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
            }
            else
            {
                Screen.SetResolution(960, 540, isFullscreen);
            }
        }
    }
}
