
using UnityEngine;


using DG.Tweening;
using StarProjectDef;
using static UnityEngine.ParticleSystem;


using Fire;
using Reign;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

using System.Collections.Generic;
using System;

public class TestSimulateMove : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        Test();
    }
    //重置
    private void Test()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StartSimulateThrowBullet();
        }

        if (Input.GetMouseButtonDown(0))
        {
        }

    }

    public TweenerCore<float, float, FloatOptions> simulatMoveTween;
    /// <summary>
    /// 模拟移动时 的 位移的增量
    /// </summary>
    public Vector3 SimulateOffset = Vector3.zero;
    public Vector3 SimulateOffset1 = Vector3.zero;



    /// 最高点
    public float hMax = 2f;

    // 初始点
    public Vector3 modelOffset = Vector3.zero;

    // 移动在 x/z平面的朝向 
    public Vector3 direction = Vector3.zero;

    // 移动的速度
    public float speed = 0f;

    // 总的运动的时间 
    public float totalTime = 2;
    // 开始运动的时间
    public float startTime = 0;
    public Transform bullet;
    public Transform bullet1;



    //float runTime = 0;


    Vector2 tStart = Vector2.zero;
    Vector2 tEnd = Vector2.zero;
    Vector2 tMid = Vector2.zero;


    double a;
    double b;
    double c;

    /// <summary>
    /// 模拟一个 抛物线 运动
    /// </summary>
    public void StartSimulateThrowBullet()
    {
       /* SimulateOffset = Vector3.zero;

        UpdatePos();

        // 在 t 轴上的 开始结束点
        // 只考虑 x/y 的情况
        tStart = new Vector2(0, modelOffset.y);
        tEnd = new Vector2(totalTime, 0);

        Vector2 tSub = tStart + tEnd;
        // 只考虑 x/y 的情况
        tMid = new Vector2(tSub.x / 2, hMax);


        // 先拿到一个 圆心:
        // MathUtilities.Get2DCircle(tStart, tMid, tEnd, ref centerPoint, ref powR);


        // 由上，就可以确定 一个在时间轴 t 上，关于 Y 的 唯一圆

        // Vector2 centerPointXY = new Vector2(centerPoint.x, centerPoint.y);



        // 子弹的抛物线模拟, 分为两个 方面:
        // 1.子弹 表现层 Y 轴 的抛物线
        // 2.子弹 在 XZ 平面 根据速度的水平移动




        runTime = startTime;

        bullet.localPosition = SimulateOffset;

        float leastTime = totalTime - runTime;


        List<float> processList = new List<float>();
        List<float> tprocessList = new List<float>();

        // 1.先要生成 一些列的 process , 先用 剩余移动的时间计算 对应的点. 比如 总运行时长 1s, 剩余 需要运行时间 0.5s.  
        //   那 需要的 点数 = 剩余运行时间 / 总运行时间 * 单位时间(1s)的点数
        int pointCounts = Mathf.CeilToInt(leastTime / totalTime * 30);

        // 3. 每一次 process 增加的 量
        float processSubValue = leastTime / pointCounts;

        // 4. 生成所有的 processList + 1 个点 (+1 是为了 包含 最后的一个 终点)   
        for (int i = 0; i < pointCounts + 1; i++)
        {
            var process = (startTime + i * processSubValue) / totalTime;
            processList.Add(process);

            var tprocess = startTime + i * processSubValue;
            tprocessList.Add(tprocess);

        }

        // 5. 获取 基于时间 t 和 Y 轴坐标 对应的 点
        List<Vector2> points = MathUtilities.Get2DParacurveYValue(tStart, tMid, tEnd, processList);

        List<Vector3> pathPoints = new List<Vector3>();

        // 6. 将 vector2 转换成 doPath 能够使用的 vector3
        points.ForEach((point) =>
        {
            float process = point.x;
            var v3 = direction * process * speed;

            Vector3 path = new Vector3(v3.x, point.y, v3.z);
            SGF.Debuger.LogError($"add path : {path}");
            pathPoints.Add(path);
        });

        // 先根据3个点 确定 一条 抛物线
        MathUtilities.Get2DParacurve(tStart, tMid, tEnd, ref a, ref b, ref c);



        var tpathPoints = MathUtilities.GetPointsBaseProcess(
                  (float process) =>
                  {
                      // x 的方程式
                      return (direction * process * speed).x;
                  },
                  (float process) =>
                  {
                      return (float)(a * Math.Pow(process, 2) + b * process + c);
                  },
                  (float process) =>
                  {
                      return (direction * process * speed).z;
                  },
                  processList
              );
        //【只是测试】
        var tpathPoints2 = MathUtilities.GetTimeParacurvePoints(tStart, tMid, tEnd, direction * speed, processList);

        // 7. 生成 一份 临时的 节点, 将节点 执行 DOPath , 然后 这个临时节点的 y 的值 就是 这个抛物线 真正 需要的 y 的值
        GameObject go = new GameObject();
        go.transform.position = tpathPoints2[0];
        go.transform.DOPath(tpathPoints2.ToArray(), leastTime, PathType.Linear, PathMode.Full3D).SetEase(Ease.Linear).OnUpdate(() =>
        {
            SimulateOffset = go.transform.position;
            SimulateOffset.y = go.transform.position.y - modelOffset.y;
            UpdatePos();
        });

        GameObject go1 = new GameObject();
        go1.transform.position = pathPoints[0];
        go1.transform.DOPath(pathPoints.ToArray(), leastTime, PathType.Linear, PathMode.Full3D).SetEase(Ease.Linear).OnUpdate(() =>
        {
            SimulateOffset1 = go1.transform.position;
            SimulateOffset1.y = go1.transform.position.y - modelOffset.y;
            UpdatePos1();
        });*/


        return;

        // // 7. 生成 一份 临时的 节点, 将节点 执行 DOPath , 然后 这个临时节点的 y 的值 就是 这个抛物线 真正 需要的 y 的值
        // GameObject go = new GameObject();
        // go.transform.DOPath(pathPoints.ToArray(), leastTime, PathType.Linear, PathMode.Full3D).SetEase(Ease.Linear);

        // simulatMoveTween = DOTween.To(() => runTime, (changeOffset) =>
        //       {
        //           // 时间 t 的刷新
        //           runTime = changeOffset;

        //       }, totalTime, leastTime).SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed).OnUpdate(() =>
        //       {
        //           if (runTime >= totalTime)
        //           {
        //               runTime = totalTime;
        //           }

        //           // xz 平面上的位移 :
        //           {
        //               SimulateOffset = direction * runTime * speed;
        //           }

        //           // y 轴上的 位移:
        //           {
        //               // SimulateOffset.y = (float)MathUtilities.Get2DCircleY(centerPointXY, powR, runTime);
        //               //   SimulateOffset.y = (float)MathUtilities.Get2DParacurvePoint(a, b, c, runTime) - modelOffset.y;

        //               float paracurveY = (float)MathUtilities.Get2DParacurvePoint(a, b, c, runTime);

        //               Debug.Log($" paracurve: x: {runTime}, y: {paracurveY} , pathMove: {go.transform.position} , yOffset: {paracurveY - go.transform.position.y} ");
        //               SimulateOffset.y = go.transform.position.y - modelOffset.y;
        //           }

        //           UpdatePos();
        //       }).OnComplete(() =>
        //       {
        //           Destroy(go);
        //       });
        // simulatMoveTween.Delay(0.1);
    }



    public void UpdatePos()
    {
        bullet.localPosition = modelOffset + SimulateOffset;

    }
    public void UpdatePos1()
    {
        bullet1.localPosition = modelOffset + SimulateOffset1;
    }
}


