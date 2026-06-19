
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class DoTweenFactory
{

    public static Tweener DoScale(GameObject go, Vector3 form, Vector3 to, float dur = .2f, Ease ease = Ease.Linear, TweenCallback OnStart = null, TweenCallback OnComplete = null)
    {
        if (go == null)
            return null;
        Transform tf = go.transform;
        go.transform.localScale = form;
        Tweener tween = tf.DOScale(to, dur);
        tween.SetEase(ease);
        if (OnStart != null)
            tween.OnStart(OnStart);
        if (OnComplete != null)
            tween.OnComplete(OnComplete);
        tween.Restart();
        tween.PlayForward();
        return tween;

    }

    public static Tweener DoSizeDelta(RectTransform rect, Vector3 to, float dur = .2f, Ease ease = Ease.Linear, TweenCallback OnStart = null, TweenCallback OnComplete = null)
    {
        if (rect == null)
            return null;
        Tweener tween = rect.DOSizeDelta(to, dur);
        tween.SetEase(ease);
        if (OnStart != null)
            tween.OnStart(OnStart);
        if (OnComplete != null)
            tween.OnComplete(OnComplete);
        tween.Restart();
        tween.PlayForward();
        return tween;
    }

    public static Tweener DOMove(GameObject go, Vector3 to, float dur = .2f, Ease ease = Ease.Linear, bool isLocal = false, TweenCallback OnStart = null, TweenCallback OnComplete = null, System.Action<Tweener> CustomSetCall = null)
    {
        if (go == null)
            return null;
        Transform tf = go.transform;
        Tweener tween = null;
        if (isLocal)
            tween = tf.DOLocalMove(to, dur);
        else
            tween = tf.DOMove(to, dur);
        tween.SetEase(ease);
        if (CustomSetCall != null)
            CustomSetCall(tween);
        tween.Restart();
        tween.PlayForward();
        if (OnStart != null)
            tween.OnStart(OnStart);
        if (OnComplete != null)
            tween.OnComplete(OnComplete);
        return tween;
    }

    public static Tweener DOValue(Slider target, float endValue, float duration, bool snapping = false)
    {
        return DOTween.To(() => target.value, x => target.value = x, endValue, duration)
            .SetOptions(snapping).SetTarget(target);
    }

    /*public static Tweener DORectMove(GameObject go, Vector3 to, float dur = .2f, Ease ease = Ease.Linear, TweenCallback OnStart = null, TweenCallback OnComplete = null, System.Action<Tweener> CustomSetCall = null)
    {
        if (go == null)
            return null;
        RectTransform rect = go.GetComponent<RectTransform>();
        return DORectMove(rect, to, dur, ease, OnStart, OnComplete, CustomSetCall);
    }*/

    public static Tweener DORectMove(RectTransform rect, Vector3 to, float dur = .2f, Ease ease = Ease.Linear, TweenCallback OnStart = null, TweenCallback OnComplete = null, System.Action<Tweener> CustomSetCall = null)
    {
        if (rect == null)
            return null;
        Tweener tween = null;
        tween = rect.DOAnchorPos(to, dur);
        tween.SetEase(ease);
        if (CustomSetCall != null)
            CustomSetCall(tween);
        tween.Restart();
        tween.PlayForward();
        if (OnStart != null)
            tween.OnStart(OnStart);
        if (OnComplete != null)
            tween.OnComplete(OnComplete);
        return tween;
    }

    public static Tweener DORotate(GameObject go, Vector3 form, Vector3 to, float dur = .2f, Ease ease = Ease.Linear, TweenCallback OnStart = null, TweenCallback OnComplete = null, System.Action<Tweener> CustomSetCall = null)
    {
        if (go == null)
            return null;
        Transform tf = go.transform;
        if (form != default(Vector3))
            tf.localRotation = Quaternion.Euler(form);
        Tweener tween = tf.DOLocalRotate(to, dur,RotateMode.FastBeyond360);
        tween.SetEase(ease);
        if (CustomSetCall != null)
            CustomSetCall(tween);
        tween.Restart();
        tween.PlayForward();
        if (OnStart != null)
            tween.OnStart(OnStart);
        if (OnComplete != null)
            tween.OnComplete(OnComplete);
        return tween;

    }

    public static Tweener DOColor(GameObject go, Color form, Color to, float dur = .2f, Ease ease = Ease.Linear, TweenCallback OnStart = null, TweenCallback OnComplete = null)
    {
        if (go == null)
            return null;
        UnityEngine.UI.Graphic graphic = AddComponent<UnityEngine.UI.Graphic>(go);
        graphic.color = form;
        Tweener tween = graphic.DOColor(to, dur);
        tween.SetEase(ease);
        tween.Restart();
        tween.PlayForward();
        if (OnStart != null)
            tween.OnStart(OnStart);
        if (OnComplete != null)
            tween.OnComplete(OnComplete);
        return tween;

    }

    public static Tweener DOPath(GameObject go, Vector3[] to, float dur = .2f, Ease ease = Ease.Linear, TweenCallback OnStart = null, TweenCallback OnComplete = null, System.Action<Tweener> CustomSetCall = null)
    {
        if (go == null)
            return null;
        Transform tf = go.transform;
        Tweener tween = tf.DOPath(to, dur);
        tween.SetEase(ease);
        if (CustomSetCall != null)
            CustomSetCall(tween);
        tween.Restart();
        tween.PlayForward();
        if (OnStart != null)
            tween.OnStart(OnStart);
        if (OnComplete != null)
            tween.OnComplete(OnComplete);
        return tween;
    }
    
    public static void Clear(bool destroy)
    {
        DOTween.Clear(destroy);
    }

    public static void Kill(GameObject go)
    {
        DOTween.Kill(go, true);
    }

    private static T AddComponent<T>(GameObject go) where T : Component
    {
        if (go != null)
        {
            T t = go.GetComponent<T>();
            if (t == null)
                t = go.AddComponent<T>();
            return t;
        }
        return default(T);
    }
}


