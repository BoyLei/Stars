using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Smooth
{
    /// <summary>
    /// 平滑移动的 类
    /// </summary>
    public class SmoothMove
    {

        /// <summary>
        /// 初始化 的 移动缓存需要记录的数量
        /// </summary>
        public int initCacheCount = 10;
        /// <summary>
        /// 初始化的 剩余移动补帧数量
        /// </summary>
        public int initLeastMoveCount = 3;

        /// <summary>
        /// 移动向量的 链表(因为要不停的删除第一个元素，所以没用List)
        /// </summary>
        public LinkedList<float> moveLinkList = new();

        /// <summary>
        /// 移动的平均速度
        /// </summary>
        public float moveAverageSpeed = 0;
        /// <summary>
        /// 移动偏移量允许的范围的偏移量
        /// </summary>
        public float moveRadito = 1.15f;

        /// <summary>
        /// 当前的移动变量
        /// </summary>
        public float curMoveData = 0;
        /// <summary>
        /// 剩余的移动的偏移量
        /// </summary>
        public float leastMoveData = 0;
        /// <summary>
        /// 剩余的移动的偏移量扣除的次数
        /// </summary>
        public float leastMoveDataCount = 0;
        /// <summary>
        /// 临时的移动补偿量
        /// </summary>
        public float tempMove = 0;

        /// <summary>
        /// 大于0 方向为 1， 小于0 方向 -1
        /// </summary>
        private int tempMoveDir = 1;

        private bool isSameMoveDir = false;

        /// <summary>
        /// 上一帧的移动速度
        /// </summary>
        private float tempLastMoveSpeed = 0;
        /// <summary>
        /// 计算的当前帧 速度, 目前当前帧速度 = (之前的平均速度 + 上一帧的速度 )/2
        /// </summary>
        private float tempCurMoveSpeed = 0;
        /// <summary>
        /// 是否受用了 预测移动, 由于网络延迟， 客户端在移动间隔会受到 很小的位移量或者 反方向的位移量。
        /// 此时 客户端根据 惯性 测下下一步 会继续踏出 一小步距离.
        /// 同时 打上这个标记
        /// </summary>
        private bool usePreMove = false;
        private bool UsePreMove
        {
            set
            {
                usePreMove = value;
                preMoveCount = 0;
            }
            get
            {
                return UsePreMove;
            }
        }

        private int preMoveCount = 0;

        public void UpdateMoveData(float move)
        {
            if (move == 0 && moveAverageSpeed == 0)
            {
                curMoveData = 0;
                return;
            }
            if (moveLinkList.Count < initCacheCount)
            {

                curMoveData = move;
                UsePreMove = false;
            }
            else
            {
                // 先计算出 剩余还需要移动的 距离
                tempMove = move + leastMoveData;


                // 根据剩余的 移动偏移量 ，计算出 当前帧 需要移动的方向
                if (tempMove == 0)
                {
                    // 如果 剩余移动的偏移量 是 0, 那就按上一帧的 移动速度朝向
                    tempMoveDir = curMoveData >= 0 ? 1 : -1;
                }
                else
                {
                    tempMoveDir = tempMove >= 0 ? 1 : -1;
                }

                // 计算 出 当前帧的移动方向是否 与前一帧发生变化 , 如果 > =0 ,说明朝向一直
                isSameMoveDir = tempMove * curMoveData >= 0;

                bool isCurMoveSame2LastMove = curMoveData * move >= 0;

                // 上一帧的移动偏移量
                tempLastMoveSpeed = math.abs(curMoveData);

                tempCurMoveSpeed = (moveAverageSpeed + tempLastMoveSpeed) * 0.5f;

                // 如果上一帧的移动速度恰好为0 , 那就按平均速度
                // 如果平均速度也为 0 , 那就直接按当前帧的速度
                if (tempLastMoveSpeed == 0)
                {
                    if (moveAverageSpeed > 0)
                    {
                        tempLastMoveSpeed = moveAverageSpeed;
                    }
                    else if (move > 0)
                    {
                        tempLastMoveSpeed = move;
                    }
                    else
                    {
                        tempLastMoveSpeed = 0.167f;
                    }
                }



                // 首先要比较一下 当前移动的偏移量是否 大于 移动允许的偏移量
                if (math.abs(tempMove) > moveAverageSpeed * moveRadito)
                {

                    // 如果 当前移动与上一帧的移动 方向相同, 那就按最大速率 计算
                    if (isSameMoveDir)
                    {
                        // 设置当前的移动偏移量为 当前的平均速度 * 偏移允许的速率
                        curMoveData = tempCurMoveSpeed * moveRadito * tempMoveDir;
                    }
                    else
                    {
                        // 如果朝向放生了改变,那就按上一帧 速度的一半 做起始步
                        curMoveData = tempLastMoveSpeed * tempMoveDir * 0.5f;
                    }


                    // 设置 剩余的 移动补偿数据
                    leastMoveData = tempMove - curMoveData;

                    // 设置移动 补偿的 剩余次数为 初始速度
                    leastMoveDataCount = initLeastMoveCount;

                    // 如果 剩余移动的补偿量 为0, 那剩余帧就不需要补偿了
                    if (leastMoveData == 0)
                    {
                        leastMoveDataCount = 0;
                    }

                    UsePreMove = false;

                }
                else
                {
                    // 如果方向发生了转变, 并且认为 转变后的 方向位移量 + 上一帧的位移量仍然在合理区间内
                    // 那就 按当前方向 减速 挪动一步
                    if (!isSameMoveDir)
                    {
                        // 如果当前帧 受到的移动指令跟 上一帧的移动方向一致, 就保持当前速度移动
                        if (isCurMoveSame2LastMove)
                        {
                            curMoveData = tempLastMoveSpeed * tempMoveDir * -0.85f;
                            // 相应的 增加
                            leastMoveData = tempMove - curMoveData;
                        }
                        else
                        {
                            // 如果当前帧发的方向与上一帧不一样, 那就是说到服务器往回跑
                            // 继续往上一次的移动方向 位移一小步
                            // 方向发生了变化, 此时移动的 先缩小
                            curMoveData = tempLastMoveSpeed * tempMoveDir * 0.4f;
                            // 相应的 增加
                            leastMoveData = leastMoveData + move - curMoveData;

                        }

                        if (false)
                        {
                            // 如果剩余步长 小于1步，并且方向发生了变化
                            if (math.abs(tempMove) <= tempLastMoveSpeed && usePreMove)
                            {
                                // 如果 当前方向相反 并且是 之前 预测的多走一步，并且 当前 move为0
                                // 说明连续收到了 2帧的 空帧. 那么此时 在走 之前的半步, 等到下一次 在回啦；
                                // 相当于 如果第二次的move 也为 0 的话， 客户端 多走 2步

                                if (math.abs(move) <= tempLastMoveSpeed * 0.5f && preMoveCount == 0)
                                {
                                    preMoveCount++;
                                    // 继续往上一次的移动方向 位移一小步
                                    curMoveData = tempLastMoveSpeed * tempMoveDir * -0.85f;
                                    // 相应的 增加
                                    leastMoveData = tempMove - curMoveData;
                                }
                                else
                                {
                                    // 首先要确定上一帧是否 按惯性 往前踏一步, 如果踏了一步, 那此处就是一步走到位置.
                                    curMoveData = tempMove;

                                    leastMoveData = 0;
                                    UsePreMove = false;
                                }



                            }
                            else
                            {

                                // 预测多走 3帧
                                if (preMoveCount == 0)
                                {
                                    UsePreMove = true;
                                }
                                else if (preMoveCount < 5)
                                {
                                    preMoveCount++;
                                }
                                else
                                {
                                    UsePreMove = false;
                                }

                                if (leastMoveDataCount == 0)
                                {
                                    leastMoveDataCount = math.ceil(leastMoveData * 2 / tempLastMoveSpeed);
                                }
                            }

                        }

                    }
                    else
                    {

                        // 如果当前帧受到的移动方向 与上一帧一样
                        if (isCurMoveSame2LastMove)
                        {
                            // 如果服务器同步的移动偏移量 小于上一帧的移动距离,那就速度缩小
                            if (math.abs(move) <= tempLastMoveSpeed)
                            {
                                curMoveData = tempLastMoveSpeed * tempMoveDir * 0.85f;
                            }
                            else
                            {
                                // 如果服务器同步的移动偏移量 大于上一帧的移动距离,因为移动补偿导致需要移动的距离减少，所以就先速度不变
                                curMoveData = tempLastMoveSpeed * tempMoveDir;
                            }
                            leastMoveData = tempMove - curMoveData;
                        }
                        else
                        {
                            // 如果 发生的朝向的转变, 那就按上一帧的速度依旧 踏出半步, 后面继续做位移偏移
                            curMoveData = tempMove;
                            leastMoveData = 0;
                            leastMoveDataCount = 0;

                            UsePreMove = false;

                        }
                        if (false)
                        {

                            // 如果方向相同, 且剩余的 需要移动的偏移量 小于 上一帧的移动速度
                            if (math.abs(tempMove) < tempLastMoveSpeed)
                            {
                                // 如果当前帧 移动变量小于速度的一半, 并且上一帧移动变量 不为0,并且上一帧 也没有使用 惯性提前多走一步,
                                // 那么此处就 往前踏 出一小步
                                if (math.abs(tempMove) <= tempLastMoveSpeed * 0.85 && curMoveData != 0 && !usePreMove)
                                {
                                    // 如果收到了 0 ，那要么是 服务器 卡帧，当前没发过来, 要么是 站着不动，传过来0
                                    curMoveData = tempLastMoveSpeed * tempMoveDir * 0.85f;
                                    leastMoveData = tempMove - curMoveData;

                                    // 预测多走 3帧
                                    if (preMoveCount == 0)
                                    {
                                        UsePreMove = true;
                                    }
                                    else if (preMoveCount < 5)
                                    {
                                        preMoveCount++;
                                    }
                                    else
                                    {
                                        UsePreMove = false;
                                    }

                                }
                                else
                                {

                                }
                            }
                        }
                        else
                        {
                            // 此时与上一次的方向相同:
                            // 剩余 需要移动的 补偿量 = 当前帧剩余的移动偏移量 -  上一帧的偏移量
                            leastMoveData = tempMove - curMoveData;

                            if (leastMoveDataCount == 0 && math.abs(leastMoveData) > 0)
                            {
                                leastMoveDataCount = math.ceil(math.abs(leastMoveData * 5 / tempCurMoveSpeed));
                            }

                            if (leastMoveDataCount == 0)
                            {
                                leastMoveDataCount = 1;
                            }


                            // 计算出 当前帧 移动 需要的补偿量 =  剩余移动的偏移量/ 剩余补偿次数
                            float curMoveOffset = leastMoveData / leastMoveDataCount;

                            // 移动的 剩余量
                            leastMoveData = leastMoveData - curMoveOffset;

                            // 当前帧 移动的偏移量 =  上一帧移动的偏移量 + 当前帧移动的补偿量
                            curMoveData = curMoveData + curMoveOffset;

                            // 消耗一次 补偿次数
                            leastMoveDataCount--;
                            if (leastMoveDataCount < 0)
                            {
                                leastMoveDataCount = 0;
                            }
                            UsePreMove = false;

                        }
                    }
                }
                moveLinkList.RemoveFirst();

            }
            moveLinkList.AddLast(move);



            moveAverageSpeed = 0;
            foreach (float item in moveLinkList)
            {
                moveAverageSpeed += math.abs(item);
            }

            moveAverageSpeed = moveAverageSpeed / moveLinkList.Count;
        }
    }


    public class SmoothMoveV3
    {
        public SmoothMove smoothMoveX;
        public SmoothMove smoothMoveY;
        public SmoothMove smoothMoveZ;

        public Vector3 CurMoveData = Vector3.zero;
        public SmoothMoveV3()
        {
            smoothMoveX = new SmoothMove();
            smoothMoveY = new();
            smoothMoveZ = new();
        }

        public SmoothMoveV3 UpdateMoveData(Vector3 moveV3)
        {
            smoothMoveX.UpdateMoveData(moveV3.x);
            smoothMoveY.UpdateMoveData(moveV3.y);
            smoothMoveZ.UpdateMoveData(moveV3.z);
            CurMoveData.Set(smoothMoveX.curMoveData, smoothMoveY.curMoveData, smoothMoveZ.curMoveData);
            return this;
        }
    }
}

