using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;


//绑定animation，有播放功能，和逻辑回调
[LuaCallCSharp]
[RequireComponent(typeof(Animation))]
public class StarAnimEvent : MonoBehaviour
{
    //播放完毕回调
    [SerializeField]
    private Action OnAnimPlayEnd;//注册给回调

    //动画组件
    private Animation _Animation;
    public Animation m_Animation
    {
        get
        {
            if (_Animation == null)
            {
                _Animation = GetComponent<Animation>();
            }
            return _Animation;
        }
    }
    //时间延迟调用，确保动画会丢掉帧,ProtectMutiCall
    public bool IsInvoked = false;


    public void Stop()
    {
        //【不用记录初始值】单独Stop，animationactive == false都是不行的//removeClip--start记录Clip；SampleAnimation都可以
        // [需要美术抠首帧，因为有池，一个ui默认是Scale==0他首帧不能是0.8，第二帧可以是0.8都没问题]
        if (m_Animation != null)
        {
            if (m_Animation.clip != null)
            {
                m_Animation.clip.SampleAnimation(gameObject, 0);
            }
            m_Animation.Stop();
        }

    }


    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="animationname">动画名称</param>
    /// <param name="action">播放完毕回调</param>
    public void Play(string animationname, System.Action action = null)
    {

        IsInvoked = false;
        float animTime = 0;//时间可能缩放，动画可能暂停,不稳，暂时没问题
        AnimationState animationState = m_Animation[animationname];
        if (animationState != null)
        {
            AnimationClip animationClip = animationState.clip;
            if (animationClip != null)
            {
                animTime = m_Animation.GetClip(animationname).length;
                bool isFind = false;
                var events = animationClip.events;
                if (events != null && events.Length > 0)
                {
                    foreach (var item in events)
                    {
                        if (item.functionName == "OnAnimPlayEndEvent")
                        {
                            isFind = true;
                        }
                    }
                }

                if (!isFind)
                {
                    //m_Animation.clip.events()//遍历是否是0，并且有没有一个“OnAnimPlayEndEvent”的事件
                    //如果是尾帧自动加回调，可能设计就不需要事件，需要他就要加没加是美术问题
                    m_Animation.GetClip(animationname).AddEvent(new AnimationEvent() { functionName = "OnAnimPlayEndEvent", time = animTime });

                }
            }
            //把animlists数据存储到，当前播放的anim上，已让Stop的时候可以找到动画从而进行的动画rewind【多状态可能有问题不适用】目前基于一个动画没问题
            m_Animation.clip = animationClip;
        }

        OnAnimPlayEnd = action;//只有一个
        m_Animation.Play(animationname);
        Invoke("OnAnimPlayEndEvent", animTime);
    }


    /// <summary>
    /// 留给AniamtionWindow 选择事件
    /// </summary>
    public void OnAnimPlayEndEvent()
    {
        if (IsInvoked == false)
        {
            Stop();

            if (OnAnimPlayEnd != null)
            {
                OnAnimPlayEnd.Invoke();

                //回调结束置为null
                OnAnimPlayEnd = null;
            }
            IsInvoked = true;
        }

    }
}
