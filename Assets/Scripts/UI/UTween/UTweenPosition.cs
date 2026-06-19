using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[AddComponentMenu("UI/UTween/TweenPosition")]
[RequireComponent(typeof(RectTransform))]
public class UTweenPosition : UTween
{
    public Vector3 from;
    public Vector3 to;
    Vector3 startP;
    Vector3 endP;
    RectTransform rect;
    public Vector3 value
    {
        get
        {
           return GetComponent<RectTransform>().anchoredPosition;
        }
        set
        {
            GetComponent<RectTransform>().anchoredPosition = value;
        }
    }

    private void Awake()
    {
        startP = from;
        endP = to;
        rect = GetComponent<RectTransform>();
        rect.anchoredPosition = startP;
    }

    public override void ResetBegine()
    {
        base.ResetBegine();
       // rect.anchoredPosition = startP;
        from = startP;
        to = endP;
    }

    public override void ResetEnd()
    {
        base.ResetEnd();
        //rect.anchoredPosition = endP;
        from = endP;
        to = startP;
    }

    [ContextMenu("Play")]
    public override void Play()
    {
        Vector3 vect = rect.anchoredPosition;
        Tweener tweener = DOTween.To(() => 
        { return vect; }, 
        x => { rect.anchoredPosition = x; }
        ,to, during);
        if(callback != null)
            tweener.OnComplete(callback); 
    }

    [ContextMenu("Set 'From' to current value")]
    public override void SetStartToCurrentValue() { from = value; }

    [ContextMenu("Set 'To' to current value")]
    public override void SetEndToCurrentValue() { to = value; }

    [ContextMenu("Assume value of 'From'")]
    void SetCurrentValueToStart() { value = from; }

    [ContextMenu("Assume value of 'To'")]
    void SetCurrentValueToEnd() { value = to; }
}
