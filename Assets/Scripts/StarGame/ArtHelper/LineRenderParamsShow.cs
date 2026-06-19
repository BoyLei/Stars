using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Runtime.Serialization;
using SkillEditor;
using System;
using SGF.Time;

/// <summary>
///升级情况关心的什么变化
///1，颜色
///2，scale
///3，甚至换一个（材质）都可以配置
///它需要持有材质，以应付自动化的，美术修改特效自动跟着变化
/// </summary>
[System.Serializable]
public class LineRenderParams
{
    public int StartTime = 0;
    public int EndTime = 0;
    public int LerpTime = 0;
    public List<Vector3> LinePoints;

    public LineRenderParams Update(LineRendererConfig lineRendererConfig, List<Vector3> points, int startTime)
    {
        StartTime = startTime;
        EndTime = lineRendererConfig.EndTime;
        LerpTime = lineRendererConfig.LerpTime;

        // point 采用拷贝数据, 防止外面会变
        LinePoints = points.KToList();
        return this;
    }
}



[System.Serializable]
public class LineRenderParamsShow : MonoBehaviour
{
    /// <summary>
    /// lineRender 播放结束的 通知.
    /// </summary>
    private Action ActionOnFinish;

    public List<Vector3> LinePoints;

    public int LerpTime = 0;
    public float LerpedTime = 0;

    public Vector3 LerpStartPos = Vector3.zero;
    public Vector3 LerpEndPos = Vector3.zero;

    private List<Vector3> showPoints = new();

    public int EndTime = 0;
    public float RunningTime = 0;
    public List<LineRenderer> LineRenderers;


    public void PlayLineRender(LineRenderParams lineRenderParams, Action finishAction)
    {
        this.gameObject.SetActive(true);
        ActionOnFinish = finishAction;

        UpdateLineRender(lineRenderParams);
    }

    public void UpdateLineRender(LineRenderParams lineRenderParams)
    {
        int startTime = lineRenderParams.StartTime;

        // 如果startTime !=-1, 说明时间 发生了变化, 可能向前也可能向后, 所以此时需要同步 runningTime
        if (startTime != -1)
        {
            RunningTime = startTime;
        }

        EndTime = lineRenderParams.EndTime;
        LerpTime = lineRenderParams.LerpTime;
        LinePoints = lineRenderParams.LinePoints;



        // 按gl 的说法，如果是lerp 插值表现过去,  LinePoints 一定是2个点.目前 外部调用的 几种形式也都是 2个点
        // 如果 lerp 插值, 就用 插值时间 同步结束点
        if (LerpTime > 0)
        {

            LerpStartPos = LinePoints[0];
            LerpEndPos = LinePoints[1];
            RefreshLerpLineRenderPoint();
        }
        else
        {
            // 如果不是插值, 那就直接连接 首尾两个点
            RefreshLineRenderPoints(LinePoints);
        }
    }

    public void Stop()
    {
        OnEnd();
    }

    public void RefreshLineRenderPoints(List<Vector3> points)
    {
        //SGF.Debuger.LogError($"[LineRender]: -------------------------");

        LineRenderers.ForEach((lineRender) =>
        {
            for (int i = 0; i < points.Count; i++)
            {
                //SGF.Debuger.LogError($"[LineRender]: i {i}, point: {points[i]}");
                lineRender.SetPosition(i, points[i]);
            }
        });
    }

    public void RefreshLerpLineRenderPoint()
    {

        LerpStartPos = Vector3.Lerp(LerpStartPos, LerpEndPos, LerpedTime / LerpTime);
        showPoints.Clear();
        showPoints.Add(LinePoints[0]);
        showPoints.Add(LerpStartPos);

        RefreshLineRenderPoints(showPoints);
    }



    private void Reset()
    {
        ActionOnFinish = null;
        LinePoints.Clear();

        LerpTime = 0;
        LerpedTime = 0;

        LerpStartPos = Vector3.zero;
        LerpEndPos = Vector3.zero;
        showPoints.Clear();

        EndTime = 0;
        RunningTime = 0;

    }

    private void OnEnd()
    {
        Reset();
        this.gameObject.SetActive(false);
    }


    private void FixedUpdate()
    {
        if (RunningTime == EndTime)
        {
            return;
        }
        if (RunningTime < EndTime)
        {
            RunningTime += TimeUtils.FixedDeltaTime;
        }

        // 已经到了最大时间了, 那就 结束这个 lineRender
        if (RunningTime >= EndTime)
        {
            RunningTime = EndTime;

            // 运行结束回调
            ActionOnFinish?.Invoke();
            OnEnd();
            return;
        }

        if (LerpTime <= 0 || LerpedTime >= LerpTime)
        {
            return;
        }
        LerpedTime += TimeUtils.FixedDeltaTime;


        RefreshLerpLineRenderPoint();
    }

}



