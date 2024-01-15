using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

namespace RobbieWagnerGames.UI
{
    public class SoundSettings : MenuTab
    {
        [Header("Sound")]
        [SerializeField] private AudioMixer audioMixer;

        [SerializeField] private VerticalLayoutGroup sliderGroupPrefab;
        [SerializeField] private VerticalLayoutGroup sliderParentPrefab;
        [SerializeField] private Slider sliderPrefab;
        [SerializeField] private TextMeshProUGUI sliderTextPrefab;

        private VerticalLayoutGroup mainVolumeSliderParent;
        private VerticalLayoutGroup musicVolumeSliderParent;
        private VerticalLayoutGroup combatVolumeSliderParent;
        private VerticalLayoutGroup dialogueVolumeSliderParent;
        private VerticalLayoutGroup playerVolumeSliderParent;
        private VerticalLayoutGroup uiVolumeSliderParent;

        private Slider mainVolumeSlider;
        private Slider musicVolumeSlider;
        private Slider combatVolumeSlider;
        private Slider dialogueVolumeSlider;
        private Slider playerVolumeSlider;
        private Slider uiVolumeSlider;

        private TextMeshProUGUI mainVolumeSliderText;
        private TextMeshProUGUI musicVolumeSliderText;
        private TextMeshProUGUI combatVolumeSliderText;
        private TextMeshProUGUI dialogueVolumeSliderText;
        private TextMeshProUGUI playerVolumeSliderText;
        private TextMeshProUGUI uiVolumeSliderText;

        float mainVolume;
        float musicVolume;
        float combatVolume;
        float dialogueVolume;
        float playerVolume;
        float uiVolume;

        void Start()
        {
            LoadSettings();
        }

        public override void BuildTab()
        {
            base.BuildTab();

            VerticalLayoutGroup leftGroup = Instantiate(sliderGroupPrefab, tabContentParent.transform).GetComponent<VerticalLayoutGroup>();
            VerticalLayoutGroup rightGroup = Instantiate(sliderGroupPrefab, tabContentParent.transform).GetComponent<VerticalLayoutGroup>();

            mainVolumeSliderParent = Instantiate(sliderParentPrefab, leftGroup.transform).GetComponent<VerticalLayoutGroup>();
            musicVolumeSliderParent = Instantiate(sliderParentPrefab, leftGroup.transform).GetComponent<VerticalLayoutGroup>();
            uiVolumeSliderParent = Instantiate(sliderParentPrefab, leftGroup.transform).GetComponent<VerticalLayoutGroup>();
            playerVolumeSliderParent = Instantiate(sliderParentPrefab, rightGroup.transform).GetComponent<VerticalLayoutGroup>();
            combatVolumeSliderParent = Instantiate(sliderParentPrefab, rightGroup.transform).GetComponent<VerticalLayoutGroup>();
            dialogueVolumeSliderParent = Instantiate(sliderParentPrefab, rightGroup.transform).GetComponent<VerticalLayoutGroup>();

            mainVolumeSlider = Instantiate(sliderPrefab, mainVolumeSliderParent.transform).GetComponent<Slider>();
            musicVolumeSlider = Instantiate(sliderPrefab, musicVolumeSliderParent.transform).GetComponent<Slider>();
            uiVolumeSlider = Instantiate(sliderPrefab, uiVolumeSliderParent.transform).GetComponent<Slider>();
            playerVolumeSlider = Instantiate(sliderPrefab, playerVolumeSliderParent.transform).GetComponent<Slider>();
            combatVolumeSlider = Instantiate(sliderPrefab, combatVolumeSliderParent.transform).GetComponent<Slider>();
            dialogueVolumeSlider = Instantiate(sliderPrefab, dialogueVolumeSliderParent.transform).GetComponent<Slider>();

            mainVolumeSlider.onValueChanged.AddListener(SetMainVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            uiVolumeSlider.onValueChanged.AddListener(SetUIVolume);
            playerVolumeSlider.onValueChanged.AddListener(SetPlayerVolume);
            combatVolumeSlider.onValueChanged.AddListener(SetCombatVolume);
            dialogueVolumeSlider.onValueChanged.AddListener(SetDialogueVolume);

            mainVolumeSliderText = Instantiate(sliderTextPrefab, mainVolumeSliderParent.transform).GetComponent<TextMeshProUGUI>();
            musicVolumeSliderText = Instantiate(sliderTextPrefab, musicVolumeSliderParent.transform).GetComponent<TextMeshProUGUI>();
            uiVolumeSliderText = Instantiate(sliderTextPrefab, uiVolumeSliderParent.transform).GetComponent<TextMeshProUGUI>();
            playerVolumeSliderText = Instantiate(sliderTextPrefab, playerVolumeSliderParent.transform).GetComponent<TextMeshProUGUI>();
            combatVolumeSliderText = Instantiate(sliderTextPrefab, combatVolumeSliderParent.transform).GetComponent<TextMeshProUGUI>();
            dialogueVolumeSliderText = Instantiate(sliderTextPrefab, dialogueVolumeSliderParent.transform).GetComponent<TextMeshProUGUI>();

            mainVolumeSliderText.text = "Main Volume";
            musicVolumeSliderText.text = "Music Volume";
            uiVolumeSliderText.text = "UI Volume";
            playerVolumeSliderText.text = "Player Volume";
            combatVolumeSliderText.text = "Combat Volume";
            dialogueVolumeSliderText.text = "Dialogue Volume";

            LoadSettings();
            SetVolumeSliders();
        }

        public void SetMainVolume(float volume)
        {
            if(volume < -40) volume = -80;
            mainVolume = volume;
            audioMixer.SetFloat("volume", mainVolume);
            SaveDataManager.SaveFloat("main_volume", mainVolume);
        } 

        public void SetMusicVolume(float volume)
        {
            if(volume < -40) volume = -80;
            musicVolume = volume;
            audioMixer.SetFloat("music", musicVolume);
            SaveDataManager.SaveFloat("music_volume", musicVolume);
        } 

        public void SetCombatVolume(float volume)
        {
            if(volume < -40) volume = -80;
            combatVolume = volume;
            audioMixer.SetFloat("combat", combatVolume);
            SaveDataManager.SaveFloat("combat_volume", combatVolume);
        } 

        public void SetDialogueVolume(float volume)
        {
            if(volume < -40) volume = -80;
            dialogueVolume = volume;
            audioMixer.SetFloat("dialogue", dialogueVolume);
            SaveDataManager.SaveFloat("dialogue_volume", dialogueVolume);
        } 

        
        public void SetPlayerVolume(float volume)
        {
            if(volume < -40) volume = -80;
            playerVolume = volume;
            audioMixer.SetFloat("player", playerVolume);
            SaveDataManager.SaveFloat("player_volume", playerVolume);
        } 
        
        public void SetUIVolume(float volume)
        {
            if(volume < -40) volume = -80;
            uiVolume = volume;
            audioMixer.SetFloat("ui", uiVolume);
            SaveDataManager.SaveFloat("ui_volume", uiVolume);
        } 

        public void SetVolumeSliders()
        {
            mainVolumeSlider.value = mainVolume;
            musicVolumeSlider.value = musicVolume;
            combatVolumeSlider.value = combatVolume;
            dialogueVolumeSlider.value = dialogueVolume;
            playerVolumeSlider.value = playerVolume;
            uiVolumeSlider.value = uiVolume;
        }

        public void LoadSettings()
        {
            //Set all volumes
            mainVolume = PlayerPrefs.GetFloat("main_volume", 0f);
            if(mainVolume < -40) mainVolume = -80;
            audioMixer.SetFloat("volume", mainVolume);

            musicVolume = PlayerPrefs.GetFloat("music_volume", -5f);
            if(musicVolume < -40) musicVolume = -80;
            audioMixer.SetFloat("music", musicVolume);

            combatVolume = PlayerPrefs.GetFloat("dialogue_volume", -10f);
            if(combatVolume < -40) combatVolume = -80;
            audioMixer.SetFloat("dialogue", combatVolume);

            dialogueVolume = PlayerPrefs.GetFloat("combat_volume", -10f);
            if(dialogueVolume < -40) dialogueVolume = -80;
            audioMixer.SetFloat("combat", dialogueVolume);

            playerVolume = PlayerPrefs.GetFloat("player_volume", -5f);
            if(playerVolume < -40) playerVolume = -80;
            audioMixer.SetFloat("player", playerVolume);

            uiVolume = PlayerPrefs.GetFloat("ui_volume", -5f);
            if(uiVolume < -40) uiVolume = -80;
            audioMixer.SetFloat("ui", uiVolume);
        }

        public static void LoadSettings(AudioMixer mixer)
        {
            //Set all volumes
            float loaded_mainVolume = PlayerPrefs.GetFloat("main_volume", 0f);
            if(loaded_mainVolume < -40) loaded_mainVolume = -80;
            mixer.SetFloat("volume", loaded_mainVolume);

            float loaded_musicVolume = PlayerPrefs.GetFloat("music_volume", -5f);
            if(loaded_musicVolume < -40) loaded_musicVolume = -80;
            mixer.SetFloat("music", loaded_musicVolume);

            float loaded_combatVolume = PlayerPrefs.GetFloat("dialogue_volume", -10f);
            if(loaded_combatVolume < -40) loaded_combatVolume = -80;
            mixer.SetFloat("dialogue", loaded_combatVolume);

            float loaded_dialogueVolume = PlayerPrefs.GetFloat("combat_volume", -10f);
            if(loaded_dialogueVolume < -40) loaded_dialogueVolume = -80;
            mixer.SetFloat("combat", loaded_dialogueVolume);

            float loaded_playerVolume = PlayerPrefs.GetFloat("player_volume", -5f);
            if(loaded_playerVolume < -40) loaded_playerVolume = -80;
            mixer.SetFloat("player", loaded_playerVolume);

            float loaded_uiVolume = PlayerPrefs.GetFloat("ui_volume", -5f);
            if(loaded_uiVolume < -40) loaded_uiVolume = -80;
            mixer.SetFloat("ui", loaded_uiVolume);
        }

        public static void SaveSettings(AudioMixer mixer)
        {
            List<float> volumes = new List<float>();
            List<string> volumeTypes = new List<string>() {"volume", "music", "dialogue", "combat", "player", "ui"};

            foreach(string volumeType in volumeTypes)
            {
                float x;
                mixer.GetFloat(volumeType, out x);
                volumes.Add(x);
            }        

            SaveDataManager.SaveFloat("main_volume", volumes[0]);
            SaveDataManager.SaveFloat("music_volume", volumes[1]);
            SaveDataManager.SaveFloat("dialogue_volume", volumes[2]);
            SaveDataManager.SaveFloat("combat_volume", volumes[3]);
            SaveDataManager.SaveFloat("player_volume", volumes[4]);
            SaveDataManager.SaveFloat("ui_volume", volumes[5]);
        }
        
        public void ResetVolumes()
        {
            mainVolumeSlider.value = PlayerPrefs.GetFloat("main_volume", 0f);
            musicVolumeSlider.value = PlayerPrefs.GetFloat("music_volume", 0f);
            combatVolumeSlider.value = PlayerPrefs.GetFloat("dialogue_volume", 0f);
            dialogueVolumeSlider.value = PlayerPrefs.GetFloat("combat_volume", 0f);
            playerVolumeSlider.value = PlayerPrefs.GetFloat("player_volume", 0f);
            uiVolumeSlider.value = PlayerPrefs.GetFloat("ui_volume", 0f);
        }
    }
}