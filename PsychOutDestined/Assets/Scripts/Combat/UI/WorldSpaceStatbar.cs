using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PsychOutDestined
{
    public class WorldSpaceStatbar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Slider bgSlider;
        [SerializeField] private Image sliderFill;
        private Sequence currentValueChangeCo;
        private Unit _unit;

        public void Initialize(Unit unit, int maxValue, int curValue, UnitStat stat)
        {
            unit.SubscribeToStatChangeEvent(UpdateVisual, stat);
            slider.maxValue = maxValue;
            slider.value = curValue;
            bgSlider.maxValue = maxValue;
            bgSlider.value = curValue;
            slider.gameObject.SetActive(false);
            unit.OnUnitMoved += UpdatePosition;
            _unit = unit;
        }

        public void UpdateVisual(int newValue)
        {
            slider.gameObject.SetActive(true);
            if (currentValueChangeCo != null && currentValueChangeCo.IsPlaying())
            {
                currentValueChangeCo.Kill(true);
                currentValueChangeCo = null;
            }
            slider.value = newValue;
            currentValueChangeCo = DOTween.Sequence();
            currentValueChangeCo.AppendInterval(.5f);
            currentValueChangeCo.Append(bgSlider.DOValue(newValue, 1f).OnComplete(CompleteValueChange));
            currentValueChangeCo.AppendInterval(.75f);
            currentValueChangeCo.Play();
        }

        public void CompleteValueChange()
        {
            bgSlider.value = slider.value;
            slider.gameObject.SetActive(false);
        }

        public void UpdatePosition()
        {
            transform.position = _unit.transform.position;
        }
    }
}