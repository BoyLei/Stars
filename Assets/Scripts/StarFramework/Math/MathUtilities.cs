// -------------------------------------------------------
//  Created by Andrew Witte.
// -------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reign
{
    /// <summary>
    /// Helper methods and values.
    /// </summary>
    public static class MathUtilities
    {
        /// <summary>
        /// PI * 2
        /// </summary>
        public const float Pi2 = Mathf.PI * 2;

        /// <summary>
        /// Use to fit one object into another if the object is larger.
        /// </summary>
        /// <param name="objectWidth">Obj to fit width.</param>
        /// <param name="objectHeight">Obj to fit height.</param>
        /// <param name="viewWidth">View to fit Obj into width.</param>
        /// <param name="viewHeight">View to fit Obj into height.</param>
        /// <returns>Returns new size.</returns>
        public static Vector2 FitInViewIfLarger(float objectWidth, float objectHeight, float viewWidth, float viewHeight)
        {
            Vector2 objectSize, viewSize;
            objectSize.x = objectWidth;
            objectSize.y = objectHeight;
            viewSize.x = viewWidth;
            viewSize.y = viewHeight;
            return FitInViewIfLarger(objectSize, viewSize);
        }

        /// <summary>
        /// Use to fit one object into another if the object is larger.
        /// </summary>
        /// <param name="objectSize">Obj to fit size.</param>
        /// <param name="viewSize">View to fit Obj into size.</param>
        /// <returns>Returns new size.</returns>
        public static Vector2 FitInViewIfLarger(Vector2 objectSize, Vector2 viewSize)
        {
            if (objectSize.x <= viewSize.x && objectSize.y <= viewSize.y) return objectSize;
            return FitInView(objectSize, viewSize);
        }

        /// <summary>
        /// Use to fit one object into another if the object is smaller.
        /// </summary>
        /// <param name="objectWidth">Obj to fit width.</param>
        /// <param name="objectHeight">Obj to fit height.</param>
        /// <param name="viewWidth">View to fit Obj into width.</param>
        /// <param name="viewHeight">View to fit Obj into height.</param>
        /// <returns>Returns new size.</returns>
        public static Vector2 FitInViewIfSmaller(float objectWidth, float objectHeight, float viewWidth, float viewHeight)
        {
            Vector2 objectSize, viewSize;
            objectSize.x = objectWidth;
            objectSize.y = objectHeight;
            viewSize.x = viewWidth;
            viewSize.y = viewHeight;
            return FitInViewIfSmaller(objectSize, viewSize);
        }

        /// <summary>
        /// Use to fit one object into another if the object is smaller.
        /// </summary>
        /// <param name="objectSize">Obj to fit size.</param>
        /// <param name="viewSize">View to fit Obj into size.</param>
        /// <returns>Returns new size.</returns>
        public static Vector2 FitInViewIfSmaller(Vector2 objectSize, Vector2 viewSize)
        {
            if (objectSize.x >= viewSize.x || objectSize.y >= viewSize.y) return objectSize;
            return FitInView(objectSize, viewSize);
        }

        /// <summary>
        /// Use to fit one object into another.
        /// </summary>
        /// <param name="objectWidth">Obj to fit width.</param>
        /// <param name="objectHeight">Obj to fit height.</param>
        /// <param name="viewWidth">View to fit Obj into width.</param>
        /// <param name="viewHeight">View to fit Obj into height.</param>
        /// <returns>Returns new size.</returns>
        public static Vector2 FitInView(float objectWidth, float objectHeight, float viewWidth, float viewHeight)
        {
            Vector2 objectSize, viewSize;
            objectSize.x = objectWidth;
            objectSize.y = objectHeight;
            viewSize.x = viewWidth;
            viewSize.y = viewHeight;
            return FitInView(objectSize, viewSize);
        }

        /// <summary>
        /// Use to fit one object into another.
        /// </summary>
        /// <param name="objectSize">Obj to fit size.</param>
        /// <param name="viewSize">View to fit Obj into size.</param>
        /// <returns>Returns new size.</returns>
        public static Vector2 FitInView(Vector2 objectSize, Vector2 viewSize)
        {
#if DEBUG
            if (objectSize.x == 0 || objectSize.y == 0 || viewSize.x == 0 || viewSize.y == 0)
            {
                throw new Exception("Object and View sizes can't be 0.");
            }
#endif

            float objectSlope = objectSize.y / objectSize.x;
            float viewSlope = viewSize.y / viewSize.x;

            if (objectSlope >= viewSlope) return new Vector2(objectSize.x / objectSize.y, 1) * viewSize.y;
            else return new Vector2(1, objectSize.y / objectSize.x) * viewSize.x;
        }

        /// <summary>
        /// Get scale value needed to fit one object into another.
        /// </summary>
        /// <param name="objectWidth">Obj to fit width.</param>
        /// <param name="objectHeight">Obj to fit height.</param>
        /// <param name="viewWidth">View to fit Obj into width.</param>
        /// <param name="viewHeight">View to fit Obj into height.</param>
        /// <returns>Returns scale.</returns>
        public static Vector2 ScaleToFitInView(float objectWidth, float objectHeight, float viewWidth, float viewHeight)
        {
            Vector2 objectSize, viewSize;
            objectSize.x = objectWidth;
            objectSize.y = objectHeight;
            viewSize.x = viewWidth;
            viewSize.y = viewHeight;
            return ScaleToFitInView(objectSize, viewSize);
        }

        /// <summary>
        /// Get scale value needed to fit one object into another.
        /// </summary>
        /// <param name="objectSize">Obj to fit size.</param>
        /// <param name="viewSize">View to fit Obj into size.</param>
        /// <returns>Returns scale.</returns>
        public static Vector2 ScaleToFitInView(Vector2 objectSize, Vector2 viewSize)
        {
            var fitViewSize = FitInView(objectSize, viewSize);
            objectSize.x /= fitViewSize.x;
            objectSize.y /= fitViewSize.y;
            return objectSize;
        }

        /// <summary>
        /// Use to fill one object into another.
        /// </summary>
        /// <param name="objectWidth">Obj to fill width.</param>
        /// <param name="objectHeight">Obj to fill height.</param>
        /// <param name="viewWidth">View to fill Obj into width.</param>
        /// <param name="viewHeight">View to fill Obj into height.</param>
        /// <returns>Returns new size.</returns>
        public static Vector2 FillView(float objectWidth, float objectHeight, float viewWidth, float viewHeight)
        {
            Vector2 objectSize, viewSize;
            objectSize.x = objectWidth;
            objectSize.y = objectHeight;
            viewSize.x = viewWidth;
            viewSize.y = viewHeight;
            return FillView(objectSize, viewSize);
        }

        /// <summary>
        /// Use to fill one object into another.
        /// </summary>
        /// <param name="objectSize">Obj to fill size.</param>
        /// <param name="viewSize">View to fill Obj into size.</param>
        /// <returns>Returns new size.</returns>
        public static Vector2 FillView(Vector2 objectSize, Vector2 viewSize)
        {
#if DEBUG
            if (objectSize.x == 0 || objectSize.y == 0 || viewSize.x == 0 || viewSize.y == 0)
            {
                throw new Exception("Object and View sizes can't be 0.");
            }
#endif

            float objectSlope = objectSize.y / objectSize.x;
            float viewSlope = viewSize.y / viewSize.x;

            if (objectSlope <= viewSlope) return new Vector2(objectSize.x / objectSize.y, 1) * viewSize.y;
            else return new Vector2(1, objectSize.y / objectSize.x) * viewSize.x;
        }



        /// <summary>
        /// 圆形攻击
        /// </summary>
        /// <param name="attacked">被攻击方</param>
        /// <param name="skillPosition">技能释放位置</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        public static bool CircleAttack(Transform attacked, Transform skillPosition, float radius)
        {
            float distance = Vector3.Distance(attacked.position, skillPosition.position);
            if (distance < radius)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 圆形攻击
        /// </summary>
        /// <param name="attacked">被攻击方</param>
        /// <param name="skillPosition">技能释放位置</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        public static bool CircleAttack(Vector3 attackedPos, Vector3 skillPosition, float radius)
        {
            float distance = Vector3.Distance(attackedPos, skillPosition);
            if (distance < radius)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断俩个坐标的俩个范围半径的圆是否相交
        /// </summary>
        /// <param name="aPos">敌方坐标</param>
        /// <param name="aRadius">敌方半径</param>
        /// <param name="bPos">技能释放坐标</param>
        /// <param name="bRadius">技能半径</param>
        /// <returns></returns>
        public static bool CalIntersection(Vector3 aPos, float aRadius, Vector3 bPos, float bRadius)
        {
            if (bRadius <= 0)
            {
                return false;
            }
            float maxDic = aRadius + bRadius;
            float distance = Vector3.Distance(aPos, bPos);
            return maxDic > distance;
        }
        /// <summary>
        /// 扇形
        /// </summary>
        /// <param name="player">玩家位置</param>
        /// <param name="target">敌⼈位置</param>
        /// <param name="angle">攻击⾓度</param>
        /// <param name="radius">攻击半径</param>
        /// <returns></returns>
        public static bool UmbrellaAttact(Transform player, Transform target, float angle, float radius)
        {
            //玩家指向敌⼈的⽅向向量
            Vector3 attackDir = target.position - player.position;
            //⾸先通过两个单位向量间的点乘Dot得到两个向量之间⾓度的余弦值
            //在通过Acos得到弧度
            //接下来通过弧度转⾓度Rad2Deg 的到两个向量之间的夹⾓
            //真正的夹⾓度数
            float realAngle = Mathf.Acos(Vector3.Dot(attackDir.normalized, player.forward)) * Mathf.Rad2Deg;
            //判断⾓度是否⼩于技能⾓度的⼀半
            //在使⽤向量的长度的平⽅来判断与半径平⽅的⼤⼩使⽤平⽅运算⽐magnitude块很多因为magnitude要使⽤勾股定理进⾏开⽅
            if (realAngle < angle/**0.5f*/&& attackDir.sqrMagnitude < radius * radius)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 扇形
        /// </summary>
        /// <param name="mainPos">玩家位置</param>
        /// <param name="dir">玩家朝向</param>
        /// <param name="attackedPos">敌⼈位置</param>
        /// <param name="angle">攻击⾓度</param>
        /// <param name="radius">攻击半径</param>
        /// <returns></returns>
        public static bool UmbrellaAttact(Vector3 mainPos, Vector3 dir, Vector3 attackedPos, float angle, float radius)
        {
            //玩家指向敌⼈的⽅向向量
            Vector3 attackDir = attackedPos - mainPos;
            //⾸先通过两个单位向量间的点乘Dot得到两个向量之间⾓度的余弦值
            //在通过Acos得到弧度
            //接下来通过弧度转⾓度Rad2Deg 的到两个向量之间的夹⾓
            //真正的夹⾓度数
            float realAngle = Mathf.Acos(Vector3.Dot(attackDir.normalized, dir)) * Mathf.Rad2Deg;
            //判断⾓度是否⼩于技能⾓度的⼀半
            //在使⽤向量的长度的平⽅来判断与半径平⽅的⼤⼩使⽤平⽅运算⽐magnitude块很多因为magnitude要使⽤勾股定理进⾏开⽅
            if (realAngle < angle/**0.5f*/&& attackDir.sqrMagnitude < radius * radius)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 环扇形
        /// </summary>
        /// <param name="mainPos">玩家位置</param>
        /// <param name="dir">施法朝向</param>
        /// <param name="attackedPos">敌⼈位置</param>
        /// <param name="angle">范围角度</param>
        /// <param name="maxRadius">最大半径</param>
        /// <param name="minRadius">最小半径</param>
        /// <returns></returns>
        public static bool RingFanAttact(Vector3 mainPos, Vector3 dir, Vector3 attackedPos, float angle, float maxRadius, float minRadius)
        {
            //玩家指向敌⼈的⽅向向量
            Vector3 attackDir = attackedPos - mainPos;
            //⾸先通过两个单位向量间的点乘Dot得到两个向量之间⾓度的余弦值
            //在通过Acos得到弧度
            //接下来通过弧度转⾓度Rad2Deg 的到两个向量之间的夹⾓
            //真正的夹⾓度数
            float realAngle = Mathf.Acos(Vector3.Dot(attackDir.normalized, dir)) * Mathf.Rad2Deg;
            //判断⾓度是否⼩于技能⾓度的⼀半
            //在使⽤向量的长度的平⽅来判断与半径平⽅的⼤⼩使⽤平⽅运算⽐magnitude块很多因为magnitude要使⽤勾股定理进⾏开⽅
            if (realAngle < angle/**0.5f*/ && attackDir.sqrMagnitude < maxRadius * maxRadius && attackDir.sqrMagnitude < minRadius * minRadius)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 矩形
        /// </summary>
        /// <param name="Player">玩家位置</param>
        /// <param name="target">⽬标位置</param>
        /// <param name="rectangleWide">矩形的宽度</param>
        /// <param name="rectangleHigh">矩形的⾼度</param>
        /// <returns></returns>
        public static bool RectangleAttack(Transform Player, Transform target, float rectangleWide, float rectangleHigh)
        {
            //得到玩家位置指向被攻击物体的⽅向向量
            Vector3 dirVector = target.position - Player.position;
            //与玩家位置正前⽅做点乘得到投影
            float forwardDistance = Vector3.Dot(dirVector, Player.forward.normalized);
            if (forwardDistance > 0 && forwardDistance < rectangleHigh)
            {
                //此时在矩形的⾼度范围内
                //与玩家位置正右⽅标量继续做点乘得到投影
                float rightDistance = Vector3.Dot(dirVector, Player.right.normalized);
                //绝对值⼩于矩形的宽度的⼀半
                if (Mathf.Abs(rightDistance) <= rectangleWide * 0.5f)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        /// <summary>
        /// 矩形
        /// </summary>
        /// <param name="attackerPos">攻击方位置</param>
        /// <param name="dir">玩家朝向</param>
        /// <param name="rightDir">玩家右朝向</param>
        /// <param name="attackedPos">被攻击方位置</param>
        /// <param name="rectangleLength">矩形的长度</param>
        /// <param name="rectangleWidth">矩形的宽度</param>
        /// <returns></returns>
        public static bool RectangleAttack(Vector3 attackerPos, Vector3 dir, Vector3 rightDir, Vector3 attackedPos, float rectangleLength, float rectangleWidth)
        {
            //得到玩家位置指向被攻击物体的⽅向向量
            Vector3 dirVector = attackedPos - attackerPos;
            //与玩家位置正前⽅做点乘得到投影
            float forwardDistance = Vector3.Dot(dirVector, dir.normalized);
            if (forwardDistance > 0 && forwardDistance <= rectangleLength)
            {
                //此时在矩形的⾼度范围内
                //与玩家位置正右⽅标量继续做点乘得到投影
                float rightDistance = Vector3.Dot(rightDir.normalized, dirVector);
                //绝对值⼩于矩形的宽度的⼀半
                if (Mathf.Abs(rightDistance) <= rectangleWidth * 0.5f)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// 等腰三角形
        /// </summary>
        /// <param name="Player">玩家位置</param>
        /// <param name="target">⽬标位置</param>
        /// <param name="maxDistance">最大距离</param>
        /// <param name="maxAngle">最大角度</param>
        /// <returns></returns>
        public static bool TriangleRange(Transform Player, Transform target, float maxDistance, float maxAngle)
        {
            Vector3 playerDir = Player.forward;
            Vector3 enemydir = (target.position - Player.position).normalized;
            float angle = Vector3.Angle(playerDir, enemydir);
            if (angle > maxAngle/* * 0.5f*/)
            {
                return false;
            }
            float angleDistance = maxDistance * Mathf.Cos(maxAngle /** 0.5f*/ * Mathf.Deg2Rad) / Mathf.Cos(angle * Mathf.Deg2Rad);
            float distance = Vector3.Distance(target.position, Player.position);
            if (distance <= angleDistance)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 等腰三角形
        /// </summary>
        /// <param name="mainPos">玩家位置</param>
        /// <param name="dir">玩家朝向</param>
        /// <param name="target">⽬标位置</param>
        /// <param name="maxDistance">最大距离</param>
        /// <param name="maxAngle">最大角度</param>
        /// <returns></returns>
        public static bool TriangleRange(Vector3 mainPos, Vector3 dir, Transform target, float maxDistance, float maxAngle)
        {
            Vector3 playerDir = dir;
            Vector3 enemydir = (target.position - mainPos).normalized;
            float angle = Vector3.Angle(playerDir, enemydir);
            if (angle > maxAngle /** 0.5f*/)
            {
                return false;
            }
            float angleDistance = maxDistance * Mathf.Cos(maxAngle /** 0.5f*/ * Mathf.Deg2Rad) / Mathf.Cos(angle * Mathf.Deg2Rad);
            float distance = Vector3.Distance(target.position, mainPos);
            if (distance <= angleDistance)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 半圆
        /// </summary>
        /// <param name="Player">玩家位置</param>
        /// <param name="target">⽬标位置</param>
        /// <returns></returns>
        public static bool SemiCircleRange(Transform Player, Transform target)
        {
            // 计算玩家与敌人的距离
            float distance = Vector3.Distance(Player.position, target.position);
            // 玩家与敌人的方向向量
            Vector3 temVec = target.position - Player.position;
            // 与玩家正前方做点积
            float forwardDistance = Vector3.Dot(temVec, Player.forward.normalized);
            if (forwardDistance > 0 && forwardDistance <= 10)
            {
                // 绘制半圆区域（绘制时取消注释）
                if (distance <= 5)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 半圆
        /// </summary>
        /// <param name="mainPos">玩家位置</param>
        /// <param name="dir">玩家朝向</param>
        /// <param name="target">⽬标位置</param>
        /// <returns></returns>
        public static bool SemiCircleRange(Vector3 mainPos, Vector3 dir, Transform target)
        {
            // 计算玩家与敌人的距离
            float distance = Vector3.Distance(mainPos, target.position);
            // 玩家与敌人的方向向量
            Vector3 temVec = target.position - mainPos;
            // 与玩家正前方做点积
            float forwardDistance = Vector3.Dot(temVec, dir.normalized);
            if (forwardDistance > 0 && forwardDistance <= 10)
            {
                // 绘制半圆区域（绘制时取消注释）
                if (distance <= 5)
                {
                    return true;
                }
            }
            return false;
        }

        #region 【贝塞尔曲线】
        /// <summary>
        /// 
        /// 策划会给距离，水平速度，最高点；本质不享受重力，他不是重力加速度，他是弧线
        /// 方法你通过DoTween来移动，时间平分这几个顶点的移动呗，不要太多
        /// 我传递给你三个点，一个是起始点，一个是终点，还有一个是中间点(中间点你通过横纵随便生成百分比，高度策划配置)，
        /*  你帮我生成N个值，其实生成的N个点都是我给三个点之间的差值点，我还有一个参数就是我要结果一共多少个点
          整体意识就是我给你3个点，并且要插值数量，你连同我给的三个点和插值一同返回给我
          你返回给我的值集合连线起来趋近于一个曲线*/
        /*【注意这里没法走到中心点】限制：1没有约束一定会吧三个值都给你，第二他是无限趋近（中心点）概念，也不是到达这个点*/
        /// </summary>
        /// <param name="start"></param>
        /// <param name="middle">中间点你自己决定</param>
        /// <param name="end"></param>
        /// <param name="totalCount">数量不要太多，也一定大于3</param>
        /// <returns>结果是一堆点的集合，你要DG过去，所有DG的时间综合是策划配置时间平均分就好</returns>
        //中间点：
        public static Vector3[] SampleBezierCurve(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int extraPointCount)
        {
            if (extraPointCount % 2 == 1)
            {
                extraPointCount++;
            }

            int pointCount = extraPointCount + 3;
            Vector3[] points = new Vector3[pointCount];


            //points[0] = startPoint;

            for (int i = 0; i <= extraPointCount + 1; i++)
            {
                float t = (float)i / (extraPointCount + 1);
                points[i] = CalculateBezierPoint(startPoint, controlPoint, endPoint, t);
            }
            points[extraPointCount + 2] = endPoint;
            return points;
        }

        private static Vector3 CalculateBezierPoint(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 point = (uu * startPoint) + (2 * u * t * controlPoint) + (tt * endPoint);
            return point;
        }

        /// <summary>
        /// 计算 三次贝塞尔 曲线 对应的 控制点
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="controlPoint"></param>
        /// <param name="controlPoint2"></param>
        /// <param name="endPoint"></param>
        /// <param name="t"></param>
        public static Vector3 CalculateBezier3Point(Vector3 startPoint, Vector3 controlPoint, Vector3 controlPoint2, Vector3 endPoint, float t)
        {
            // P(t) = (1-t)^3 * P0 + 3t(1-t)^2 * P1 + 3t^2(1-t) * P2 + t^3 * P3

            float u = 1 - t;
            float uu = u * u;
            float uuu = uu * u;
            float tt = t * t;
            float ttt = t * tt;
            Vector3 point = (uuu * startPoint) + (3 * t * uu * controlPoint) + 3 * tt * u * controlPoint2 + ttt * endPoint;

            return point;
        }

        /// <summary>
        /// 计算 贝塞尔曲线 在某个点 上的 切线向量
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="controlPoint"></param>
        /// <param name="controlPoint2"></param>
        /// <param name="endPoint"></param>
        /// <param name="t"></param>
        public static Vector3 CalculateBezier3CurvePoint(Vector3 startPoint, Vector3 controlPoint, Vector3 controlPoint2, Vector3 endPoint, float t)
        {
            // Curve(t) = 3(1-t)^2(P1-P0)+6(1-t)t(P2-P1)+3t^2(P3-P2);

            float u = 1 - t;
            float uu = u * u;
            float tt = t * t;
            Vector3 dir = 3 * uu * (controlPoint - startPoint) + 6 * u * t * (controlPoint2 - controlPoint) + 3 * tt * (endPoint - controlPoint2);

            return dir;
        }

        #endregion

        public static Vector3 CalculateDirection2EulerAngles(Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction.normalized);
            return rotation.eulerAngles;
        }

        #region【圆形】坏的！
        /// <summary>
        /// 
        /// unity 帮我写一个方法，四个参数分别是：我给你起始点，中心点，结束点，要几个额外的点坐标（int），返回值是一个数组里面包含，起始点，结束点，中心点，和其他要求数量的额外坐标；方法内部，你把，起始点，中心点，结束点都当作再一个弧形上的三个顶点，方法上你首先实现，通过以上三个点确定唯一圆心，通过圆心和任意点距离确定半径，通过圆心是否在三角形（三个顶点的连线）之内确定这个弧度值；通常如果圆心在三角形外边，那三个顶点所形成的弧度最大一定不超过180度，那在180之内平均在弧度上帮我取出（参数要求）的额外点数量，返回给我
        /// 【三个顶点确定一个弧度，并且180弧度范围内生成顶点还给你】
        /// 【平面确定，通过XZ确定X轴，和Y确定一个2D平面】
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="centerPoint">自己确认这个中心点在一个平面上，用开始到结尾中间给个高度</param>
        /// <param name="endPoint"></param>
        /// <param name="extraPointCount">原来三个点必然还给你，还有额外点，但是你必须是奇数</param>
        /// <returns></returns>
        public static Vector3[] CalculateArcVertices(Vector3 startPoint, Vector3 centerPoint, Vector3 endPoint, int extraPointCount)
        {
            if (extraPointCount % 2 == 1)
            {
                extraPointCount++;
            }
            Vector3 sphereCenter = CalculateSphereCenter(startPoint, centerPoint, endPoint);
            float sphereRadius = CalculateSphereRadius(startPoint, centerPoint, endPoint);
            float sphereAngle = CalculateSphereAngle(startPoint, centerPoint, endPoint);
            /* if (crossProduct.z < 0)
             {
                 angleRange = Mathf.Clamp(angleRange, -180f, 180f);
             }*/

            // 计算每个额外点的角度增量
            /*         float angleIncrement = sphereAngle / (extraPointCount + 1);*/


            // Calculate start angle and end angle
            Vector3 startDirection = (startPoint - sphereCenter).normalized;
            Vector3 endDirection = (endPoint - sphereCenter).normalized;
            float startAngle = Mathf.Atan2(startDirection.y, startDirection.x);
            float endAngle = Mathf.Atan2(endDirection.y, endDirection.x);

            // Calculate total angle
            float totalAngle = Mathf.Abs(endAngle - startAngle);

            // Calculate point count
            int pointCount = Mathf.CeilToInt(totalAngle / extraPointCount) + 1;

            // Calculate step angle
            float stepAngle = totalAngle / (pointCount - 1);

            // Create array to store points
            Vector3[] points = new Vector3[pointCount];

            // Calculate points on arc
            for (int i = 0; i < pointCount; i++)
            {
                float angle = Mathf.Lerp(startAngle, endAngle, i / (float)(pointCount - 1));
                float x = sphereCenter.x + (sphereRadius * Mathf.Cos(angle));
                float y = sphereCenter.y + (sphereRadius * Mathf.Sin(angle));
                float z = sphereCenter.z;

                points[i] = new Vector3(x, y, z);
            }

            return points;
        }
        public static Vector3[] GetPointsInArc(Vector3 sphereCenter, float sphereRadius, float startAngle, float endAngle, float stepSize)
        {
            int pointCount = Mathf.CeilToInt((endAngle - startAngle) / stepSize) + 1;
            Vector3[] points = new Vector3[pointCount];
            int index = 0;

            for (float angle = startAngle; angle <= endAngle; angle += stepSize)
            {
                float x = sphereCenter.x + (sphereRadius * Mathf.Sin(angle));
                float y = sphereCenter.y + (sphereRadius * Mathf.Cos(angle));
                float z = sphereCenter.z;

                points[index++] = new Vector3(x, y, z);
            }

            return points;
        }

        // 计算三个点确定的球体的球心
        public static Vector3 CalculateSphereCenter(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            // 根据点1和点2的中点计算球的中心线
            Vector3 centerLine = (point1 + point2) / 2f;

            // 计算从中心线到点3的向量
            Vector3 toPoint3 = point3 - centerLine;

            // 计算球心坐标
            Vector3 sphereCenter = centerLine + (toPoint3 / 2f);

            return sphereCenter;
        }

        // 计算三个点确定的球体的半径
        public static float CalculateSphereRadius(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            // 计算球心坐标
            Vector3 sphereCenter = CalculateSphereCenter(point1, point2, point3);

            // 计算球心到任意一点的距离作为半径
            float sphereRadius = Vector3.Distance(sphereCenter, point1);

            return sphereRadius;
        }

        // 计算三个点确定的球体的弧度
        //这里三个顶点确定的平面推荐和Y平行
        //返回1 和3 夹角
        public static float CalculateSphereAngle(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            // 计算球心坐标
            Vector3 sphereCenter = CalculateSphereCenter(point1, point2, point3);

            // 计算球心到给定三个点的向量
            Vector3 toPoint1 = point1 - sphereCenter;
            //Vector3 toPoint2 = point2 - sphereCenter;
            Vector3 toPoint3 = point3 - sphereCenter;

            // 计算两个向量的夹角弧度
            float angle = /*Vector3.Angle(toPoint1, toPoint2) + Vector3.Angle(toPoint2, toPoint3) */+Vector3.Angle(toPoint3, toPoint1);

            return angle;
        }


        #endregion

        #region 【重力】
        public static Vector3[] AvgFallGraph(Vector3 selfPosition, Vector3 targetPosition, float initialHorizontalVelocity, float requiredHeight/*,float process*/, int extraPointCount)
        {
            //额外点必须是奇数，不然不会给你中心点的
            if (extraPointCount % 2 == 1)
            {
                extraPointCount = extraPointCount + 1;
            }
            // 生成全部顶点的数组
            Vector3[] allVertices = new Vector3[extraPointCount + 3];
            float starProcess = 0;
            float Increment = 1f / ((float)extraPointCount - 1f); //1/4 0.25 5个
                                                                  //0要返回3个
            for (int i = 0; i <= extraPointCount + 2; i++)
            {
                allVertices[i] = FallGraph(selfPosition, targetPosition, initialHorizontalVelocity, requiredHeight/*,float process*/, starProcess + (Increment * i));

            }
            return allVertices;
        }

        public static Vector3[] CustomFallGraph(Vector3 selfPosition, Vector3 targetPosition, float initialHorizontalVelocity, float requiredHeight, float[] process)
        {
            /*if (process.Length % 2 == 1)//这个我不限定了因为他都没要中心点,甚至我都不会多给他三个
            {
                delete最后一个
            }*/
            // 生成全部顶点的数组
            Vector3[] allVertices = new Vector3[process.Length];
            for (int i = 0; i < process.Length; i++)
            {
                allVertices[i] = FallGraph(selfPosition, targetPosition, initialHorizontalVelocity, requiredHeight/*,float process*/, process[i]);

            }
            return allVertices;


        }
        //[Example]
        //【重力】--以后的要求，其实可以都写这种process这种，第一不约束你调用我的方式，但是你实现必须实现这个，这个可以画图，回溯，倒播，运行散列时间颗粒交给你 
        //模拟自由落体,但是重力是我们自己控制的downwardForce
        //process 我传递0到1，返回我这时应该在的坐标，比如我传0算出的结果就是selfposition，传1恰好就在targetPosition
        //requiredHeight就是相对起点的高度，如果其实和末尾是水平的其实中值+= height即可
        //优化到别人看不懂最好，电脑明白就行，算法根据采样会很复杂的
        //【G】用我的时候别开G，你就开没有任何重力状态，因为也不是实时运算了
        //【假设开始和结尾就是水平的】不那么真实，不然结尾你要探测Ray，忽略了开始结尾中间的重力势能转换，本身你加速度也是假的
        //【没到地点有障碍就到地点穿越障碍销毁】，到地点还没落地也销毁，就是水平的
        //【手的高度忽略，手抛到对面也是手的高度】，脚下抛，到对面也是脚下，忽略忽略本身G都是假的要什么真是
        //必然返回三个，其他你还要几个
        //你的数量确认是1米一个就可以（水平），你要填写偶数，我注定给你奇数3，因为我是平分点的奇数才会给你中心点（你要求的点[也应该是中心点]）
        //从高度开始抛-dl
        public static Vector3/*[]*/ FallGraph(Vector3 selfPosition, Vector3 targetPosition, float initialHorizontalVelocity, float requiredHeight, float process/*, int extraPointCount*/)
        {


            // 计算双方距离s
            float distance = Vector3.Distance(selfPosition, targetPosition);

            // 计算时间
            float time = distance / initialHorizontalVelocity;

            float halfTime = time / 2f;
            // 推算向下的拉力 v1 = at ;s = v2t + 0.5 * a * t * t ;t=0.5 *Time;v1 = -V2 ,重力有方向的是负数！
            float downwardForce = -2 * requiredHeight / halfTime / halfTime;
            // 推算向上的初始速度
            float upwardInitialVelocity = downwardForce * -1 * halfTime;//向上就是整的



            /*        allVertices[0] = selfPosition;
                    // 添加结束点和中心点
                    allVertices[extraPointCount + 2] = targetPosition;*/




            /*       // 添加额外点 起始和结尾，中间多方一个含中间
                   float starProcess = 0;
                   float Increment = 1f/((float)extraPointCount - 1f); //1/4 0.25 5个
                   float tempProcess = 0 ;*/
            //0要返回3个
            /*            for (int i = 0; i <= extraPointCount + 2; i++)
                        {
                            tempProcess = starProcess + Increment * i;*/
            Vector3 hor = (selfPosition + targetPosition) * process;//水平,预算

            if (process <= 0)
            {
                hor = selfPosition;//全更新
            }
            else if (process >= 1)
            {
                hor = targetPosition;
            }
            else if (process == 0.5)
            {
                //Y更新
                hor.y = selfPosition.y + requiredHeight;//hor.y + requiredHeight; 其实就是初始加高度，假设平的都无所谓，如果是其实和目标具备高地差策划目的是起始点加高度；真有高低差不管直接掉下去

            }
            else if (process < 0.5)
            {
                //映射右侧，少算个vt，垂直对称
                float processtime = time * (0.5f - process);
                float downDis = 0.5f * downwardForce * processtime * processtime;
                hor.y = selfPosition.y + requiredHeight + downDis;
            }
            else if (process > 0.5)
            {

                float processtime = time * (process - 0.5f);
                float downDis = 0.5f * downwardForce * processtime * processtime;
                hor.y = selfPosition.y + requiredHeight + downDis;
            }
            return hor;
            /* allVertices[i + 1] = hor;*/
            /*}*/

            //return allVertices;
        }
        #endregion

        #region dl
        /// <summary>
        /// 根据 3个平面上的点, 获取一个【圆形】 圆心 和 半径R 的平方
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <param name="centerpoint"></param>
        /// <param name="powR"></param>
        public static void Get2DCircle(Vector2 point1, Vector2 point2, Vector2 point3, ref Vector2 centerpoint, ref double powR)
        {
            double Ax = point1.x; double Ay = point1.y;
            double Bx = point2.x; double By = point2.y;
            double Cx = point3.x; double Cy = point3.y;

            double mat1, mat2, mat3;
            mat1 = (((Bx * Bx) + (By * By) - ((Ax * Ax) + (Ay * Ay))) * (2 * (Cy - Ay))) -
                    (((Cx * Cx) + (Cy * Cy) - ((Ax * Ax) + (Ay * Ay))) * (2 * (By - Ay)));

            mat2 = (2 * (Bx - Ax) * ((Cx * Cx) + (Cy * Cy) - ((Ax * Ax) + (Ay * Ay)))) -
                    (2 * (Cx - Ax) * ((Bx * Bx) + (By * By) - ((Ax * Ax) + (Ay * Ay))));

            mat3 = 4 * (((Bx - Ax) * (Cy - Ay)) - ((Cx - Ax) * (By - Ay)));

            centerpoint.x = (float)(mat1 / mat3);
            centerpoint.y = (float)(mat2 / mat3);

            powR = Math.Pow(Ax - centerpoint.x, 2) + Math.Pow(Ay - centerpoint.y, 2);

        }




        ///// <summary>
        ///     不如用概念，别封装了。
        ///// 单位时间百分比，对应3D坐标的函数，约束：三个点取得的平面是平行Y轴的
        ///// 【注意调用时候关闭重力影响】【实体都有影响】
        ///// </summary>
        ///// <param name="pointStart"></param>
        ///// <param name="pointMiddle"></param>
        ///// <param name="pointEnd"></param>
        ///// <param name="process"></param>
        ///// <returns></returns>
        //public static List<Vector3> Get2DParacurvePoints(Vector3 pointStart, Vector3 pointMiddle, Vector3 pointEnd, float[] process)
        //{
        //    return Get2DParacurvePoints(new Vector2(pointStart.x + pointStart.z, pointStart.y), new Vector2(pointMiddle.x + pointMiddle.z, pointMiddle.y), new Vector2(pointEnd.x + pointEnd.z, pointEnd.y), process);
        //}

        /// <summary>
        /// 根据 3个 点 确定一条抛物线. 途径 起始点 pointStart, 中间的点 pointMiddle, 和 终点 pointEnd。
		/// 【获取 2D抽象 抛物线 Y的高度】 [process - Y 对应图形，通过参数和process来获取]
        /// 返回Process上面的点坐标，【不会额外返回，起始点，中心点，结束点，因为这个用户知道】
        /// 这个逻辑是3D，抽象的2D process理解成时间T 单位时间，【vector2理解成2D封装抛物线坐标】[他是一个抽象概念，【Y一定是垂直直接设置】，X可以是速度可以是时间但其实应该是S距离，但是你要*你的初始移动向量(水平)]
        /// 
        /// 得到的 是 [T----> Y] 的值
        /// </summary>
        /// <param name="pointStart">抛物线的起点</param>
        /// <param name="pointMiddle">抛物线上中间的点</param>
        /// <param name="pointEnd">抛物线的结束点</param>
        /// <param name="process">返回单位之为1的，0~1中的N个点，取得逻辑由外围决定</param>
        public static List<Vector2> Get2DParacurveYValue(Vector2 pointStart, Vector2 pointMiddle, Vector2 pointEnd, List<float> process)
        {
            double a = 0;
            double b = 0;
            double c = 0;

            Get2DParacurve(pointStart, pointMiddle, pointEnd, ref a, ref b, ref c);

            // 抛物线 x 轴 总的 长度
            float total = pointEnd.x - pointStart.x;

            // process 开始点
            float processStart = pointStart.x;

            List<Vector2> points = new();

            // 生成 需要点数的 抛物线上的点, 包含 起始点, 总共生成了 pointCount 个点
            for (int i = 0; i < process.Count; i++)
            {
                float x = processStart + (total * process[i]);
                float y = (float)Get2DParacurvePoint(a, b, c, x);
                points.Add(new Vector2(x, y));
            }

            return points;
        }

        /// <summary>
        /// 获取一条 抛物线上的 过程点. 
		///【获取 3D具体 抛物线 3维坐标的落点】 [process - 3Dpos 对应图形，通过参数和process来获取]
        /// 抛物线 基于时间 t 作为参数, 根据 tYStart，tYMiddle，tYEnd 构建一条 基于时间轴 t 对应 Y 的抛物线.
        /// 而水平面 根据速度 speedXZ , 确定 时间t 在 XZ 平面的 落点.
        /// 从而 构建一条 抛物线。
        /// </summary>
        /// <param name="tYStart">抛物线 时间t 对应 y轴的起点</param>
        /// <param name="tYMiddle"> 同上 的中点</param>
        /// <param name="tYEnd">同上 的结束点</param>
        /// <param name="speedXZ"> 在XZ平面的速度</param>
        /// <param name="process">过程点集</param>
        /// <returns></returns>
        public static List<Vector3> GetTimeParacurvePoints(Vector2 tYStart, Vector2 tYMiddle, Vector2 tYEnd, Vector3 speedXZ, List<float> process)
        {
            double a = 0;
            double b = 0;
            double c = 0;

            Get2DParacurve(tYStart, tYMiddle, tYEnd, ref a, ref b, ref c);

            // 抛物线 x 轴 总的 长度
            float total = tYEnd.x - tYStart.x;

            // process 开始点
            float processStart = tYStart.x;

            List<Vector3> points = new();

            // 生成 需要点数的 抛物线上的点, 包含 起始点, 总共生成了 pointCount 个点
            for (int i = 0; i < process.Count; i++)
            {
                float t = processStart + (total * process[i]);
                float x = speedXZ.x * t;
                float z = speedXZ.z * t;
                float y = (float)Get2DParacurvePoint(a, b, c, t);
                points.Add(new Vector3(x, y, z));
            }

            return points;
        }

        /// <summary>
        /// 【数学公式一定要跟具体，这种要放在commonExtend中，但不符合数学】
        /// 【尽量做到 我关心的直接是怎么用 process-pos3d】
        ///  通过 过程点 和 对应 坐标轴的 方程式, 得到对应的 坐标点
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static List<Vector3> GetPointsBaseProcess(Func<float, float> xBaseTimeFunc, Func<float, float> yBaseTimeFunc, Func<float, float> zBaseTimeFunc, List<float> process)
        {
            List<Vector3> points = new();
            // 生成 需要点数的 抛物线上的点, 包含 起始点, 总共生成了 pointCount 个点
            for (int i = 0; i < process.Count; i++)
            {
                float proce = process[i];
                float x = xBaseTimeFunc.Invoke(proce);
                float y = yBaseTimeFunc.Invoke(proce);
                float z = zBaseTimeFunc.Invoke(proce);

                points.Add(new Vector3(x, y, z));
            }

            return points;
        }


        /// <summary>
        /// 根据 3个 点 确定一条抛物线. 途径 起始点 pointStart, 中间的点 pointMiddle, 和 终点 pointEnd。 根据 process 返回在抛物线上对应的点
        /// </summary>
        /// <param name="pointStart">抛物线的起点</param>
        /// <param name="pointMiddle">抛物线上中间的点</param>
        /// <param name="pointEnd">抛物线的结束点</param>
        /// <param name="process">在抛物线上的进度 [0,1]</param>
        public static double Get2DParacurveProcess(Vector2 pointStart, Vector2 pointMiddle, Vector2 pointEnd, float process)
        {
            double a = 0;
            double b = 0;
            double c = 0;

            Get2DParacurve(pointStart, pointMiddle, pointEnd, ref a, ref b, ref c);

            double x = pointStart.x + ((pointEnd.x - pointStart.x) * process);

            return Get2DParacurvePoint(a, b, c, x);
        }

        /// <summary>
        /// 给定平面（一定是一个平面的，而且这个平面平行于Y轴，所以Middle是自己算的） 3个点， 确定一条【抛物线】 y= ax^2+bx+c 
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public static void Get2DParacurve(Vector2 point0, Vector2 point1, Vector2 point2, ref double a, ref double b, ref double c)
        {
            double x0 = point0.x;
            double y0 = point0.y;

            double x_10 = point1.x - point0.x;
            double y_10 = point1.y - point0.y;

            double x_10_2 = Math.Pow(point1.x, 2) - Math.Pow(point0.x, 2);

            double x_20 = point2.x - point0.x;
            double y_20 = point2.y - point0.y;

            double x_20_2 = Math.Pow(point2.x, 2) - Math.Pow(point0.x, 2);

            a = ((y_10 * x_20) - (y_20 * x_10)) / ((x_10_2 * x_20) - (x_20_2 * x_10));

            b = (y_10 / x_10) - (a * x_10_2 / x_10);

            c = y0 - (a * Math.Pow(x0, 2)) - (b * x0);

        }

        /// <summary>
        /// 给定要给 【抛物线】, 取抛物线 x 对应的值
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="x"></param>
        public static double Get2DParacurvePoint(double a, double b, double c, double x)
        {
            return (a * x * x) + (b * x) + c;
        }




        /// <summary>
        ///  计算 一个 二维平面 圆心 为centerPoint ， 半径平方为 powR ，水平轴 x 对应的 y点坐标
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="powR"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Get2DCircleY(Vector2 centerPoint, double powR, float x)
        {
            // 圆的方程式 为 (x-a)^2+(y-b)^2 = R^2;
            // y =sqrt(R^2-(x-a)^2)+b 

            double yMax = centerPoint.y + Math.Sqrt(powR - Math.Pow(x - centerPoint.x, 2));

            return yMax;
        }
        #endregion
    }


}