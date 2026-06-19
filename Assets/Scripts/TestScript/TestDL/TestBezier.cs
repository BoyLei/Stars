using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Reign;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
public class TestBezier : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public Transform controlPoint1;

    public Transform controlPoint2;
    public int count = 10;

    public List<Vector3> points = new List<Vector3>();

    /// <summary>
    /// 移动表现的 节点，做 贝塞尔曲线运动
    /// </summary>
    public Transform target;

    /// <summary>
    /// 逻辑层的 节点，做直线运动
    /// </summary>
    public Transform logicTarget;

    public float Speed = 5;

    public float progress = 0;
    /// <summary>
    /// 当前 实际运行的时间
    /// </summary>
    public float curRunningTime = 0;
    /// <summary>
    /// 剩余 的运行时间
    /// </summary>
    public float leastTime = 0;

    /// <summary>
    /// 移动的 路点
    /// </summary>
    private List<Vector3> movePoints = new List<Vector3>();

    /// <summary>
    /// 移动的 起始点. 每次曲线 发生变化的时候,  movestartPoint 和 moveEndPoint 都是重新生成的.
    /// </summary>
    public Vector3 movestartPoint;
    /// <summary>
    /// 移动的终点
    /// </summary>
    public Vector3 moveEndPoint;

    /// <summary>
    /// 创建 贝塞尔曲线的 几个点. 先做 三次贝塞尔曲线. 
    /// </summary>
    public List<Vector3> bezierPoint = new();

    private void OnDrawGizmos()
    {
        points.Clear();

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            points.Add(MathUtilities.CalculateBezier3Point(startPoint.position, controlPoint1.position, controlPoint2.position, endPoint.position, t));
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < count - 1; ++i)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPoint.position, endPoint.position);

        DrawMoveLine();
    }

    /// <summary>
    /// 绘画出一条 节点移动到目标点的路径
    /// </summary>
    private void DrawMoveLine()
    {
        /// 移动路点有以下几种情况:
        /// 1.目标点 未发生移动, 那就按原始路径 直接运行到终点;
        /// 2.目标点发生移动:
        ///     a.如果 此时 节点在 起始点和  第一个控制点之间， 那就采用 [当前点 + 控制点1 + 控制点2 + 目标点] 构建一条新的贝塞尔3次曲线;
        ///     b.如果 此时 节点在 第一个和  第二个控制点之间,  那就采用 [当前点 + 控制点2 + 目标点] 构成一个 二次贝塞尔曲线
        ///     c.如果 此时 节点在 终点和    第二个控制点之间,  那就采用 [当前点 + 生成新的控制点 + 目标点] 构成 一个新的 二次贝塞尔曲线
        /// 3.如果 距离 是拉远,  使用上面 思路没问题;
        ///       距离 拉近,检查 结束点 相对 控制点的方向是否发生改变。
        /// 后续在考虑 优化问题



        movePoints.Clear();
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            movePoints.Add(MathUtilities.CalculateBezier3Point(target.position, controlPoint1.position, controlPoint2.position, endPoint.position, t));
        }

        Gizmos.color = Color.white;
        for (int i = 0; i < count - 1; ++i)
        {
            Gizmos.DrawLine(movePoints[i], movePoints[i + 1]);
        }
    }

    private void FixedUpdate()
    {
        if (progress >= 1)
        {
            progress = 0;
            target.position = startPoint.position;
            logicTarget.position = startPoint.position;
            curRunningTime = 0;

            return;
        }

        Vector3 offect = endPoint.position - logicTarget.position;


        // 对于逻辑层 节点来说, 是每一帧朝着 终点移动
        var deltaMove = (endPoint.position - startPoint.position).normalized * Speed * Time.fixedDeltaTime;

        var magnitude = offect.sqrMagnitude;
        // 如果剩余的距离 > 一帧的移动距离, 那就正常移动
        if (magnitude >= deltaMove.sqrMagnitude && magnitude != 0)
        {
            logicTarget.Translate(deltaMove, Space.World);
        }
        else
        {
            logicTarget.position = endPoint.position;
        }


        // 实际的 运行时间, 终点变化的时候,  总的运行时间 会发生变化, 相对应的 progress 也会发生变化
        curRunningTime += Time.fixedDeltaTime;

        // 剩余的 时间
        leastTime = (endPoint.position - logicTarget.position).magnitude / Speed;




        // 对于移动目标来说, 如果速度不变情况下, 那移动点的进度 progress = curRunningTime/(curRunningTime+ leastTime 剩余还需要运动的时间);
        var curProgress = curRunningTime / (curRunningTime + leastTime);

        // Debug.LogError($"chang progress: {curProgress - progress}");
        progress = curProgress;
        if (progress >= 1)
        {
            progress = 1;
            target.position = endPoint.position;
            return;
        }

        target.position = MathUtilities.CalculateBezier3Point(startPoint.position, controlPoint1.position, controlPoint2.position, endPoint.position, progress);

    }
}

#endif

