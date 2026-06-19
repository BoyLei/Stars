///--------------------------------------------------------------------
/// 文件名   :   ClientNpc.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/04 13:40:54
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject.Game;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Service.Business;
using System;
using System.Collections;
using System.Collections.Generic;
using StarProject;
using StarProject.Service.Function;
using Trigger;
using UnityEngine;
namespace ClientNpc
{
    public class ClientNpc : IClientNpc
    {
        /// <summary>
        /// 缓存队列
        /// </summary>
        public static Queue<ClientNpc> Pools = new Queue<ClientNpc>();

        /// <summary>
        /// 移动速度
        /// </summary>
        public float MoveSpeed { get; private set; }

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public ulong EntityID { get; private set; }

        /// <summary>
        /// 实体是否创建
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// 配置ID
        /// </summary>
        public long ConfigID { get; private set; }

        public int Index { get; private set; }
        private NPCJsonData _NpcJsonData;
        private NPCJsonData NpcConfig
        {
            get
            {
                if(_NpcJsonData==null)
                {
                    _NpcJsonData = GameMap.sceneJsonData.GetNPCJsonData(ConfigID);
                }
                return _NpcJsonData;
            }
        }

        /// <summary>
        /// 当前状态机
        /// </summary>
        public BaseState CurrentState { get; private set; }

        public Dictionary<StateEnum, BaseState> States = null;

        private List<NpcTrigger> mTriggers;

        public GameNPCCtrlGroup CtrlGroup;

        public StateEnum LastState { get; private set; }

        //从巡逻进入待机
        private bool EnterIdleFromPatrol = false;

        public ClientEntityMoveLogic MoveLogic;

        public static ClientNpc Create(int index,long configid, List<TriggerGroupJsonData> triggerGroups)
        {
            ClientNpc clientNpc = null;
            if (Pools.Count > 0)
            {
                clientNpc = Pools.Dequeue();
            }
            else
            {
                clientNpc = new ClientNpc();
            }
            clientNpc.ConfigID = configid;
            clientNpc.Index = index;
            if (clientNpc.mTriggers == null)
            {
                clientNpc.mTriggers = new List<NpcTrigger>();
            }
            clientNpc.mTriggers.Clear();
            if (triggerGroups != null && triggerGroups.Count > 0)
            {
                foreach (var item in triggerGroups)
                {
                    if (GameMap.sceneJsonData.Triggers.TryGetValue(item.TriggerID, out TriggerJsonData jsonData) && jsonData != null)
                    {
                        foreach (var effect in item.Effects)
                        {
                            var trigger = new NpcTrigger(index,configid, jsonData, effect);
                            clientNpc.mTriggers.Add(trigger);
                        }
                    }
                }
            }
            clientNpc.OnCreate();

            return clientNpc;
        }

        private void OnCreate()
        {
            if (States == null)
            {
                States = new Dictionary<StateEnum, BaseState>();
            }
            States.Clear();
            States.Add(StateEnum.Idle, new IdleState(this));
            States.Add(StateEnum.Inter, new InterState(this));
            States.Add(StateEnum.Patrol, new PatrolState(this));
            Active = false;
            EnterIdleFromPatrol = false;
            if(NpcConfig.WalkType==1 || NpcConfig.WalkType == 2)
            {
                TranslateState(StateEnum.Patrol);
            }
            else
            {
                TranslateState(StateEnum.Idle);
            }
        }

        public void OnEnterAOI(EntityCtrlBase ctrlBase)
        {
            Active = true;
            EnterIdleFromPatrol = false;
            CtrlGroup = ctrlBase as GameNPCCtrlGroup;
            Position = ctrlBase.M_Curr.Position();
            EntityID = ctrlBase.M_Curr.EntityId;
            MoveLogic = new ClientEntityMoveLogic(CtrlGroup);
            if (mTriggers != null)
            {
                foreach (var trigger in mTriggers)
                {
                    trigger.SetEntityID(EntityID);
                }
            }
        }

        public void ResumeLast()
        {
            TranslateState(LastState);
        }

        public void TranslateState(StateEnum state)
        {
            if (CurrentState == null)
            {
                CurrentState = States[state];
                CurrentState.OnEnter();
            }
            else
            {
                if (CurrentState.GetStateEnum() == state)
                {
                    return;
                }

                CurrentState.OnExit();
                LastState = CurrentState.GetStateEnum();
                CurrentState = States[state];
                CurrentState.OnEnter();
            }
        }

        public void MoveByPosition(int mapid,int spid,int effectid)
        {
            if (MoveLogic == null)
            {
                return;
            }
            
            if (mapid == GameManager.Instance.GetCurMapId())
            {
                if (GameMap.sceneJsonData.Spawners != null &&
                    GameMap.sceneJsonData.Spawners.TryGetValue(spid, out var t) && t!=null)
                {
                    MoveLogic.MoveByPosition(t.Position.Convert(), () =>
                    {
                        GlobalEvent.OnEffectEnd?.Invoke(effectid);
                    });
                }
            }
        }
        public void MoveByPath(int mapid,int pathid,int effectid)
        {
            if (MoveLogic == null)
            {
                return;
            }
            
            if (mapid == GameManager.Instance.GetCurMapId())
            {
                if (GameMap.sceneJsonData.Paths != null &&
                    GameMap.sceneJsonData.Paths.TryGetValue(pathid, out var t) && t!=null)
                {
                    var  paths = new Vector3 [t.WayPoints.Count];
                    for (int i = 0; i < paths.Length; i++)
                    {
                        paths[i] = t.WayPoints[i].Convert();
                    }
                    MoveLogic.MoveByPath(paths, () =>
                    {
                        GlobalEvent.OnEffectEnd?.Invoke(effectid);
                    });
                }
            }
        }
        
        public void MoveByPath()
        {
            
        }

        public void EnterFrame(int frameIndex)
        {
            if (!Active)
            {
                return;
            }
            if(CtrlGroup!=null && CtrlGroup.M_Curr!=null)
            {
                Position = CtrlGroup.M_Curr.Position();
            }
            if (mTriggers != null && mTriggers.Count > 0)
            {
                foreach (var item in mTriggers)
                {
                    item.EnterFrame(frameIndex);
                }
            }

            if (CurrentState != null)
            {
                CurrentState.OnUpdate();
            }

           // ChangeState();
        }

        private void ChangeState()
        {
            if(!EnterIdleFromPatrol)
            {
                if (CurrentState.GetStateEnum() == StateEnum.Patrol)
                {
                    if (BusinessManager.Instance.IsNpcCanService((uint)ConfigID))
                    {
                        if (Vector3.Distance(CtrlGroup.M_Curr.Position(), GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position()) < 2)
                        {
                            //玩家和有服务的NPC 靠近时
                            TranslateState(StateEnum.Inter);
                            EnterIdleFromPatrol = true;
                        }
                    }
                }
            }
            else
            {
                if (Vector3.Distance(CtrlGroup.M_Curr.Position(), GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position())>= 2)
                {
                    //玩家和有服务的NPC 靠近时
                    ResumeLast();
                    EnterIdleFromPatrol = false;
                }
            }

        }

        public void SetMoveSpeed(float speed)
        {
            MoveSpeed = speed;
        }
        public void OnLeaveAOI()
        {
            Active = false;
            CtrlGroup = null;
        }

        public void OnRelease()
        {
            CurrentState = null;
            MoveLogic = null;
            CtrlGroup = null;
            if (States != null) { States.Clear(); }
            ConfigID = 0;
            _NpcJsonData = null;
            mTriggers.Clear();
            Pools.Enqueue(this);
        }
    }
}