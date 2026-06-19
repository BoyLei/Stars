using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[AddComponentMenu("UI/UTween/UTweenScale")]
[RequireComponent(typeof(RectTransform))]
public class UTweenScale : UTween
{
    public Vector3 from;
    public Vector3 to;
    Vector3 startR;
    Vector3 endR;

    public Vector3 value
    {
        get
        {
            return transform.localScale;
        }
        set
        {
            transform.localScale = value;
        }
    }

    void Awake()
    {
        startR = from;
        endR = to;
    }

    public override void Play()
    {
        Tweener tweemer = transform.DOScale(to, during);
        if (callback != null)
            tweemer.OnComplete(callback);
    }

    public override void ResetBegine()
    {
        base.ResetBegine();
        value = startR;
        from = startR;
        to = endR;
    }

    public override void ResetEnd()
    {
        base.ResetEnd();
        value = endR;
        from = endR;
        to = startR;
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
