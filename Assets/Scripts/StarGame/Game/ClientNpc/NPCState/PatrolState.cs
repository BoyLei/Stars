///--------------------------------------------------------------------
/// 文件名   :   PatrolState.cs
/// 内  容   :  巡逻状态 
/// 说  明   :  
/// 创建日期 :   2023/05/04 16:09:58
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject.Game.Map;
using StarProject.Service.FindPath;
using StarProject.Service.LocalData;
using System.Collections.Generic;
using UnityEngine;
namespace ClientNpc
{
    public class PatrolState : BaseState
    {
        public List<Vector3> Paths = null;
        private float Delay = 1;
        private float WalkRange;
        private Vector3 Center;
        private float MinRange;
        private float EndFlag = 1;

        private NPCJsonData _npcConfig;
        private NPCJsonData NpcConfig
        {
            get
            {
                if (_npcConfig==null)
                {
                    _npcConfig = GameMap.sceneJsonData.GetNPCJsonData(mClientNpc.ConfigID);
                }
                return _npcConfig;
            }
        }

        private Vector3 Target;

        public override StateEnum GetStateEnum()
        {
            return StateEnum.Patrol;
        }

        private bool IsMoving = false;
        public PatrolState(ClientNpc npc) : base(npc)
        {
            Paths = new List<Vector3>();
        }

        public override void OnEnter()
        {
            float speed = 5.0f;
            var config = LocalDataManager.Instance.GetNPCDataCell(mClientNpc.ConfigID);
            if (config != null)
            {
                speed = config.GetWalkSpeed()/100.0f;
            }
            mClientNpc.SetMoveSpeed(speed);
            WalkRange = NpcConfig.WalkRange * 0.01f;
            MinRange = WalkRange * 0.1f;
            Center = NpcConfig.Position.Convert();
            Paths.Clear();
            IsMoving = false;
        }

        private bool InCircel(Vector3 position)
        {
            return Vector3.Distance(position, Center) < WalkRange;
        }

        private Vector3 GetTargetPosition()
        {
            float random = UnityEngine.Random.Range(MinRange, WalkRange);
            //随机角度
            float angel = UnityEngine.Random.Range(0, 360);
            //换算成弧度
            float rad = Mathf.Deg2Rad * angel;
            //计算朝向
            Vector3 dir = new(Mathf.Cos(rad), 0, Mathf.Sin(rad));
            //计算终点
            Vector3 position = mClientNpc.CtrlGroup.M_Curr.Position() + (dir * random);
            if (InCircel(position) && FindPathManager.Instance.GetWalkable(position, out Vector3 result))
            {
                return result;
            }
            else
            {
                return GetTargetPosition();
            }
        }

        private bool FindPath()
        {
            Paths.Clear();
            if (NpcConfig != null)
            {
                /*     
                 *     WalkInage = 1,                      //自由徘徊，徘徊范围
                 *     WalkByPath = 2,                     //沿路径行走，路径ID
                */
                //Paths.Add(mClientNpc.CtrlGroup.M_Curr.Position());
                if (NpcConfig.WalkType == 1)
                {
                    //目标点
                    Vector3 position = GetTargetPosition();
                    //计算路点
                    if (FindPathManager.Instance.FindPath(mClientNpc.CtrlGroup.M_Curr.Position(), position, 1, out UnityEngine.Vector3[] potions) && potions != null && potions.Length > 0)
                    {
                        foreach (var item in potions)
                        {
                            Paths.Add(item);
                        }
                    }
                    else
                    {
                        Paths.Add(position);
                    }
                    return true;
                }

                if (NpcConfig.WalkType == 2)
                {
                    var paths = GameMap.sceneJsonData.Paths[NpcConfig.PathID];
                    if (paths != null)
                    {
                        List<Vector3> Paths1 = new();
                        foreach (var item in paths.WayPoints)
                        {
                            Paths1.Add(item.Convert());
                        }

                        if (paths.IsLoop)
                        {
                            for (int i = paths.WayPoints.Count - 1; i > -1; i--)
                            {
                                Paths1.Add(paths.WayPoints[i].Convert());
                            }
                        }

                        int newIndex = (pathIndex + 1) % Paths1.Count;
                        //SGF.Debuger.LogWarning($"寻路看看 pathIndex={pathIndex},Count={Paths1.Count},newIndex={newIndex}");
                        if (newIndex > 0 && newIndex < Paths1.Count)
                        {
                            for (int i = newIndex; i < Paths1.Count; i++)
                            {
                                Paths.Add(Paths1[i]);
                            }

                            //for (int i = 0; i < newIndex; i++)
                            //{
                            //    Paths.Add(Paths1[i]);
                            //}
                        }
                        else
                        {
                            Paths = Paths1;
                        }
                    }
                    return true;
                }
            }
            return false;
        }



        public override void OnUpdate()
        {
            //站立
            if (NpcConfig.WalkType == 0)
            {
                return;
            }

            if (Delay > 0)
            {
                Delay -= Time.deltaTime;
                return;
            }

            if (mClientNpc!=null && mClientNpc.CtrlGroup != null && mClientNpc.CtrlGroup.M_Curr!=null)
            {
                if (IsMoving)
                {
                    //在移动，是否停止
                    if (Vector3.Distance(mClientNpc.CtrlGroup.M_Curr.Position(), Target) < EndFlag)
                    {
                        Delay = UnityEngine.Random.Range(NpcConfig.RandomMin, NpcConfig.RandomMax);
                        pathIndex = -1;
                        IsMoving = false;
                        //SGF.Debuger.LogError($"寻路看看 pathIndex={pathIndex} 结束了------------------------");
                    }
                    else if (!mClientNpc.CtrlGroup.M_Curr.Is_MainPlayer_FindingPath)
                    {
                        pathIndex = -1;
                        IsMoving = false;
                        //SGF.Debuger.LogError($"寻路看看 pathIndex={pathIndex} 结束了------------------------22222222");
                    }
                }
                else
                {
                    if (FindPath())
                    {
                        mClientNpc.CtrlGroup.M_Curr.Speed = mClientNpc.MoveSpeed;
                        Target = Paths[Paths.Count - 1];

                        mClientNpc.CtrlGroup.M_Curr.SetWayPointData(Paths.ToArray(), null, CachePath);
                        //SGF.Debuger.LogError($"寻路看看 pathIndex={pathIndex} 开始-------------------------");

                        //mClientNpc.CtrlGroup.M_Curr.AddServerV3WayPointsCache(Paths.ToArray());
                        //mClientNpc.CtrlGroup.M_Curr.ContinuousEnterWaypointDataByIndex();
                        //mClientNpc.CtrlGroup.M_Curr.ClientExecuteWayPoint(true);

                        IsMoving = true;
                    }
                }
            }
        }

        private int pathIndex = -1;

        private void CachePath()
        {
            pathIndex++;
            //SGF.Debuger.Log($"寻路看看 pathIndex={pathIndex}");
        }

        public override void OnExit()
        {
            if (mClientNpc.CtrlGroup != null)
            {
                mClientNpc.CtrlGroup.BreakFindPath();
                //mClientNpc.CtrlGroup.M_Curr.MainPlayer_Clear_FollowUp_Target();
                ////打断dotween
                //mClientNpc.CtrlGroup.M_Curr.ActionStopMoveDotween?.Invoke("mClientNpc.CtrlGroup.M_Curr.ActionStopMoveDotween");
            }
            Paths.Clear();
            Delay = 1;
            WalkRange = 0;
            Center = Vector3.zero;
            MinRange = 0;
            EndFlag = 0;
            _npcConfig = null;
            IsMoving = false;
            Target = Vector3.zero;
        }

    }
}