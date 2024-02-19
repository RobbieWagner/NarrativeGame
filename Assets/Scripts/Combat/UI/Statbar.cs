using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PsychOutDestined
{
    public class Statbar : MonoBehaviour
    {
        [SerializeField] private Slider statSlider;
        [SerializeField] private Slider bgSlider;
        [SerializeField] private TextMeshProUGUI statText;
        [SerializeField] private TextMeshProUGUI statNameText;
        public Image statIcon;
        public Image sliderFill;

        private Sequence currentValueChangeCo;

        public void Initialize(Unit unit, int maxValue, int curValue, UnitStat stat)
        {
            unit.SubscribeToStatChangeEvent(UpdateVisual, stat);
            statSlider.maxValue = maxValue;
            bgSlider.maxValue = maxValue;
            statSlider.value = curValue;
            bgSlider.value = curValue;
            statText.text = $"{statSlider.value}/{statSlider.maxValue}";
            statNameText.text = stat.ToString().ToUpper();
        }

        public void UpdateVisual(int newValue)
        {
            statSlider.value = newValue;
            statText.text = $"{statSlider.value}/{statSlider.maxValue}";

            if (currentValueChangeCo != null && currentValueChangeCo.IsPlaying())
                currentValueChangeCo.Kill();
            currentValueChangeCo = DOTween.Sequence();

            currentValueChangeCo.Append(bgSlider.DOValue(newValue, 1f));
        }
    }
}