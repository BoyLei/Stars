///--------------------------------------------------------------------
/// 文件名   :   FollowDynamicTarget.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/25 14:22:08
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Service.FindPath;
using StarProject.Service.Input;
using StarProjectDef;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Task
{
    public class FollowDynamicTarget
    {
        private ulong FollowEnityID;
        public float Range { get; private set; }
        public System.Action<bool, string> CallBack { get; private set; }
        private List<Vector3> Paths = new List<Vector3>();
        private Vector3 Target;
        private bool IsMoving = false;
        private float PreRange;
        public bool IsFollowing { get { return FollowEnityID != 0; } }

        private float LastFindTime = 0;
        public string FollowKey;

        private bool CanFindPath()
        {
            if (InputManager.Instance.IskeyDown)
            {
                return false;
            }
            if (InputManager.Instance.IsJoystickMove)
            {
                return false;

            }

            var Player = (GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup).M_Curr;

            if (Player.Data.Is___ForbidMove)
            {
                return false;
            }

            // 如果玩家朝向 摇杆也被禁止了, 那就也不允许 自动寻路
            if (Player.IsForbidJoyStickDir)
            {
                return false;
            }
            E_ULayerSubState M_eSubState = Player.M_eSubState;
            //SGF.Debuger.LogError($"[Follow], [state]: {M_eSubState}");

            //技能中 return false

            // 2023/7/31
            // DL
            // 技能中 也应该可以寻路, 是否能够寻路应该只通过原子锁判断
            //var M_eSubState = Player.M_eSubState;

            if (M_eSubState == E_ULayerSubState.InterAction1)
            {
                return false;
            }
            if (M_eSubState == E_ULayerSubState.InterAction2)
            {
                return false;
            }
            return true;
        }

        public bool OnStart(string key, ulong enityid, float range = 1, System.Action<bool, string> action = null, bool tips = true)
        {

            if (!CanFindPath())
            {
                return false;
            }

            if (FollowEnityID == enityid)
            {
                return false;
            }

            //玩家正在寻路中，则打断寻路
            PlayerCtrlGroup player = GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup;
            if (player != null)
            {
                player.BreakFindPath();
            }

            //玩家在跟随目标中则打断上次跟随
            if (IsFollowing)
            {
                OnExit(false);
            }

            FollowEnityID = enityid;
            Range = range;
            CallBack = action;
            FollowKey = key;
            //扩50%
            PreRange = Range * (1 + 0.2f);
            // Debug.LogError($"{FollowKey} OnStart");
            //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Is_MainPlayer_FindingPath = true;
            CalculateWayPoint(true);

            //if (tips)
            //{
            //    // TODO: 曲
            //    // 根据曲爷的提示 临时增加的逻辑. 
            //    // 后续 需要 曲 将此处 tips 的逻辑 提出路点寻路 的流程中. 此处是临时增加的 tips 逻辑
            //    string tipsStr = StarProject.Service.Battle.BattleManager.Instance.IsAutoBattling ? "自动战斗中" : "自动寻路中";
            //    GlobalEvent.OnHitStarEvent?.Invoke(tipsStr);
            //}

            return true;
        }


        public (bool, Vector3) GetFollowPosition(ulong entityID)
        {
            var entityCtr = GameManager.Instance.GetEntityCtr(entityID);
            if (entityCtr != null && entityCtr.M_Curr != null)
            {
                return (true, entityCtr.M_Curr.Position());
            }
            else
            {
                var dataBase = GameManager.Instance.m_mapEntityData[entityID];
                if (dataBase != null)
                {
                    return (true, dataBase.Pos);
                }
            }

            return (false, Vector3.zero);

        }
        private void CalculateWayPoint(bool force = false)
        {
            // Debug.LogError($"{FollowKey} CalculateWayPoint 1");

            if (!force && (Time.time - LastFindTime < 0.33f))
            {
                return;
            }
            //   Debug.LogError($"{FollowKey} CalculateWayPoint 2");

            Paths.Clear();
            var result = GetFollowPosition(FollowEnityID);

            //var entityBase = GameManager.Instance.m_mapEntityData[FollowEnityID];
            if (result.Item1)
            {
                //    Debug.LogError($"{FollowKey} CalculateWayPoint 3");

                Vector3 rolePos = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
                Vector3 position = result.Item2;
                if (Vector3.Distance(rolePos, position) <= Range)
                {
                    //   Debug.LogError($"{FollowKey} CalculateWayPoint 4");

                    //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.MainPlayer_Clear_FollowUp_Target();
                    ////打断dotween
                    //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionStopMoveDotween?.Invoke("FollowDynamicTarget：：CalculateWayPoint");
                    //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ClientExecuteWayPoint(true);

                    GameManager.Instance.M_MainPlayerCtrlBase.BreakFindPath();

                    OnExit(true);
                }
                else
                {
                    //Debug.LogError($"{FollowKey} CalculateWayPoint 5");

                    //计算路点
                    if (FindPathManager.Instance.FindPath(rolePos, position, Range, out UnityEngine.Vector3[] potions) && potions != null && potions.Length > 0)
                    {
                        foreach (var item in potions)
                        {
                            Paths.Add(item);
                        }
                        Target = Paths[Paths.Count - 1];

                        GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.SetWayPointData(Paths.ToArray(), OnExit);

                        //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.AddServerV3WayPointsCache(Paths.ToArray());
                        //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ContinuousEnterWaypointDataByIndex();
                        //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ClientExecuteWayPoint(true);
                        //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Is_MainPlayer_FindingPath = true;
                        IsMoving = true;
                        LastFindTime = Time.time;
                        //    Debug.LogError($"{FollowKey} CalculateWayPoint 6 ");
                    }
                    else
                    {
                        OnExit(false);
                        //   Debug.LogError($"{FollowKey} CalculateWayPoint 7 ");
                    }
                }
            }
            else
            {
                OnExit(false);
                // Debug.LogError($"{FollowKey} CalculateWayPoint 8");
            }
        }

        public void OnEnterFrame(int frameindex)
        {
            if (FollowEnityID == 0)
            {
                return;
            }

            if (!IsMoving)
            {
                return;
            }
            if (CheckEnd())
            {
                return;
            }

            CheckCalculateNewWayPoint();
        }

        private bool CheckEnd()
        {
            var entityBase = GameManager.Instance.GetEntityCtr(FollowEnityID);
            if (entityBase != null)
            {
                Vector3 rolePos = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
                Vector3 position = entityBase.M_Curr.Position();
                if (Vector3.Distance(rolePos, position) <= Range)
                {
                    //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.MainPlayer_Clear_FollowUp_Target();
                    ////打断dotween
                    //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionStopMoveDotween?.Invoke("FollowDynamicTarget::CheckEnd");
                    //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ClientExecuteWayPoint(true);

                    GameManager.Instance.M_MainPlayerCtrlBase.BreakFindPath();

                    OnExit(true);
                    return true;
                }
            }
            return false;
        }

        private void CheckCalculateNewWayPoint()
        {
            Vector3 rolePos = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();

            if (Vector3.Distance(rolePos, Target) < PreRange)
            {
                CalculateWayPoint();
            }
        }

        public void Break()
        {
            if (IsFollowing)
            {
                //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Is_MainPlayer_FindingPath = false;
                //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.MainPlayer_Clear_FollowUp_Target();
                //GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionStopMoveDotween?.Invoke("FollowDynamicTarget::Break");

                GameManager.Instance.M_MainPlayerCtrlBase.BreakFindPath();

                OnExit(false);
            }
        }

        void OnFindPathPointsClear(object data)
        {
            ulong entityID = (ulong)data;
            if (entityID == GameManager.Instance.mainPlayerId)
            {
                Break();
            }
        }

        private void OnExit(bool sucess)
        {
            CallBack?.Invoke(sucess, FollowKey);
            FollowEnityID = 0;
            CallBack = null;
            IsMoving = false;
            Target = Vector3.zero;
            Range = 0;
            LastFindTime = 0;
            FollowKey = string.Empty;
            GlobalEvent.OnHitEndEvent?.Invoke(null);
        }
    }
}
