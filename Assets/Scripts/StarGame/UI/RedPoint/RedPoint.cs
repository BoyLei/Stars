using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;
using System;
using SGF.Unity;

public class RedPoint : MonoBehaviour
{
    public RedPointType redPointType;

    /// <summary>
    /// 红点状态, 后续这个状态 要改为 红点的icon
    /// </summary>
    public bool redPointState = false;

    /// <summary>
    /// 红点状态 发生改变时候的通知
    /// </summary>
    public Action<bool> OnStateChange;

    /// <summary>
    /// 刷新红点数量的通知
    /// </summary>
    public Action OnRefreshRedCount;
    public int RedCount = 0;

    // public bool dirty = false;

    public virtual void Awake()
    {
        // SGF.Debuger.Log($"[RedPoint] Awake : {this.transform.name} , type: {redPointType}");
        RedPointManager.Instance.RegisterRedPoint(this);
    }

    public virtual void OnEnable()
    {
        // SGF.Debuger.Log($"[RedPoint] OnEnable : {this.transform.name} , type: {redPointType}");
        MarkDirty();
    }

    // private void Start()
    // {
    //     SGF.Debuger.Log($"[RedPoint] Start : {this.transform.name} , type: {redPointType}");
    // }

    public virtual void OnDestroy()
    {
        RedPointManager.Instance.UnRegisterRedPoint(this);

        DelayInvoker.CancelInvoke(this);
    }


    public void SetState(bool state)
    {
        if (state == redPointState)
        {
            return;
        }
        redPointState = state;
        OnStateChange?.Invoke(redPointState);
    }

    /// <summary>
    /// 标示下一帧刷新,在一帧之内,可能收到多次的 事件通知需要刷新红点.
    /// 所以 收到通知后，只做一个脏标记, 下一帧做一次 Refresh
    /// </summary>
    public virtual void MarkDirty()
    {
        // dirty = true;
        // 设置下一帧检查
        if (DelayInvoker.ContainInvoke(this))
        {
            return;
        }
        DelayInvoker.DelayInvoke(0, DirtyRefresh, null);
    }

    public void DirtyRefresh(object[] args)
    {
        Refresh();
    }

    /// <summary>
    /// 刷新红点自己的 状态. 根据这个红点的 类型做条件检查
    /// </summary>
    public virtual void Refresh()
    {
        SetState(CheckConditions());

        RefreshRedCount();
    }

    /// <summary>
    /// 刷新这个红点 的红点数量
    /// </summary>
    public virtual void RefreshRedCount()
    {
        if (!redPointState)
        {
            RedCount = 0;
            return;
        }

        // redPointType 类型存储的红点数量。
        int count = RedPointManager.Instance.GetRedPointTypeCount(redPointType);

        // 如果 这个红点被点亮. 但是 功能模块中并没有写入这个 redPointType类型的 红点数量,那就默认为 1
        if (count == 0 && redPointState)
        {
            count = 1;
        }

        RedCount = count;
        OnRefreshRedCount?.Invoke();
    }

    /// <summary>
    /// 检查这个类型配置的 所有条件组
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckConditions()
    {
        return RedPointManager.Instance.CheckRedPointTypeCfgConditions(redPointType);
    }



}
