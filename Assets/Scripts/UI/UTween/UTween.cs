using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class UTween : MonoBehaviour
{
    public float during = 1f;
    public TweenCallback forwardCallback;
    public TweenCallback reverseCallback;
    protected TweenCallback callback;

    public abstract void Play();

    public virtual void ResetBegine()
    {
        callback = forwardCallback;
    }

    public virtual void ResetEnd()
    {
        callback = reverseCallback;
    }

    public virtual void PlayForward()
    {
        ResetBegine();
        Play();
    }

    public virtual void PlayReverse()
    {
        ResetEnd();
        Play();
    }

    /// <summary>
    /// Set the 'from' value to the current one.
    /// </summary>

    public virtual void SetStartToCurrentValue() { }

    /// <summary>
    /// Set the 'to' value to the current one.
    /// </summary>

    public virtual void SetEndToCurrentValue() { }

}
