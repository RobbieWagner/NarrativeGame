using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ActionSelectionUI : MonoBehaviour
{
    public Image prevActionImage;
    public Image curActionImage;
    public Image nextActionImage;

    private Sequence currentSpriteCo;
    private Vector3 originPosition = Vector3.positiveInfinity;

    public bool isInitialized = false;

    public void Initialize(Sprite sprite, Vector3 position)
    {
        curActionImage.sprite = sprite;
        curActionImage.transform.position = position;
        isInitialized = true;
    }

    public void EnableActionUI()
    {
        if(prevActionImage != null) prevActionImage.enabled = true;
        curActionImage.enabled = true;
        if(prevActionImage != null) nextActionImage.enabled = true;
    }

    public void SetCurrentSelectionSprite(Sprite actionSprite, bool actionIndexIncreased)
    {
        if(currentSpriteCo != null && currentSpriteCo.IsPlaying()) currentSpriteCo.Kill(true);
        if(!originPosition.Equals(Vector3.positiveInfinity)) curActionImage.transform.position = originPosition;
        originPosition = curActionImage.transform.position;

        Vector3 leaveToPos = actionIndexIncreased ? originPosition + Vector3.down/2 : originPosition + Vector3.up/2;
        Vector3 returnFromPos = actionIndexIncreased ? originPosition + Vector3.up/2 : originPosition + Vector3.down/2;

        currentSpriteCo = DOTween.Sequence();
        currentSpriteCo.Append(curActionImage.DOColor(Color.clear, .3f).OnComplete(() => curActionImage.sprite = actionSprite).SetEase(Ease.InQuint));
        currentSpriteCo.Join(curActionImage.transform.DOMove(leaveToPos, .3f).OnComplete(() => HideCurrentActionSelectionSprite(returnFromPos)));
        currentSpriteCo.AppendInterval(.1f);
        currentSpriteCo.Append(curActionImage.DOColor(Color.white, .15f).SetEase(Ease.OutQuint));
        currentSpriteCo.Join(curActionImage.transform.DOMove(originPosition, .15f));
    }

    private void HideCurrentActionSelectionSprite(Vector3 hiddenPosition)
    {
        curActionImage.color = Color.clear;
        curActionImage.rectTransform.position = hiddenPosition;
    }
}