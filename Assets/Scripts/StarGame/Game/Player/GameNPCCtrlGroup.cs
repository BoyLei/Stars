using SGF.Module.Framework;
using SGF.Network;
using SGF.Unity;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Map;
using StarProject.Game.Player.Component;
using StarProject.Service.Battle;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Player
{
    public class GameNPCCtrlGroup : EntityCtrlBase
    {
        protected new string LOG_TAG = "GameNPCCtrlGroup";

        //======================================================================

        private GameNPCEntityBase m_CurrentCtrledVitBase; //P，被控制的Enity：目前设计只有自己，skPlayer

        //目前改成主角了//Id目前是恰好对：如果三位一体，就是三个Nttid了
        public ulong currentCtrlNttId
        {
            get { return m_CurrentCtrledVitBase.EntityId; }
        } // { get { return m_data.m_EnityId; } }

        public override AOIEntityObject M_Curr
        {
            get { return m_CurrentCtrledVitBase; }
        } //逻辑层Enity

        //======================================================================

        private string mTriggerKey;

        public float Range { get; private set; }

        public int Index
        {
            get
            {
                if (Data != null && Data.Attrs != null)
                {
                    return Data.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.SpaceIndex);
                }

                return 0;
            }
        }

        private bool m_isLookAtMainPlayer = false;

        //是否已经被隐藏
        public bool IsShow = true;

        public NpcDataCell M_NpcDataCell = null;

        //NPC 临时显隐
        //private EntityTempVisibility TempVisibility;

        private bool m_ViewCreateStart = false; // 显示层创建开始
        private bool m_ViewCreateFinifh = false; // 显示层创建完成
        private bool m_VisibilityChange = false; // 缓存已经执行过


        //public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        //{
        //    LOG_TAG = $"[GameNPCCtrlGroup_{data.M_EntityID}]";
        //    m_data = (VitalSignData)data;
        //    base.Create(data, pos);
        //    m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<GameNPCEntityBase>();
        //    m_CurrentCtrledVitBase.Create(0, m_data, m_container.transform);
        //    m_data.myOwnerNtt = m_CurrentCtrledVitBase;
        //    m_data.myOwnerNttGroup = this;
        //    IsShow = true;
        //    //【我，其他人】放置在出生坐标
        //    SyncBornPos(pos);

        //    // 创建头顶信息面板
        //    UnitPendant unitPendant = new UnitPendant(this);
        //    m_listCompoent.Add(unitPendant);
        //    m_CurrentCtrledVitBase.ActionOnCheckTarget += unitPendant.OnActionOnCheckTarget;
        //    // 创建选中指示器组件
        //    CheckIndicator checkIndicator = new CheckIndicator(this);
        //    m_listCompoent.Add(checkIndicator);
        //    m_CurrentCtrledVitBase.ActionOnCheckTarget += checkIndicator.OnActionOnCheckTarget;

        //    m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction;


        //    M_NpcDataCell = LocalDataManager.Instance.GetNPCDataCell(m_CurrentCtrledVitBase.ConfigIndex);
        //    if (M_NpcDataCell != null)
        //    {
        //        Range = M_NpcDataCell.GetTriggerRange() * 0.01f;
        //        if (!string.IsNullOrEmpty(M_NpcDataCell.StartIdle))
        //        {
        //            m_CurrentCtrledVitBase.SetIdleAnimation?.Invoke(M_NpcDataCell.StartIdle);
        //        }
        //    }

        //    // 创建触发器
        //    CreateTrigger();

        //    m_data.ActionOnBattleStateDead += OnDeadDisPlay;

        //    TempVisibility = new EntityTempVisibility();
        //    TempVisibility.Initialize(m_CurrentCtrledVitBase.EntityType, (int)m_CurrentCtrledVitBase.ConfigIndex, OnVisibilityChange);
        //    BusinessManager.Instance.SetNpcIsVisiable(this);

        //    ClientNpc.ClientNpcManager.Instance.OnEnterAOI(this);
        //    // 通知小地图创建
        //    GlobalEvent.NPCVisiableRet?.Invoke((int)m_CurrentCtrledVitBase.ConfigIndex, pos, IsShow);
        //    OnModelCreateSuccess();
        //}

        public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        {
            LOG_TAG = $"[GameNPCCtrlGroup_{data.M_EntityID}]";
            m_data = (VitalSignData)data;
            m_data.ActionOnBattleStateDead += OnDeadDisPlay;
            m_data.myOwnerNttGroup = this;

            base.Create(data, pos);

            IsShow = true;
            m_ViewCreateFinifh = false;
            m_ViewCreateStart = m_data.IsServerAOI;

            m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<GameNPCEntityBase>();
            m_data.myOwnerNtt = m_CurrentCtrledVitBase;
            m_CurrentCtrledVitBase.ActionOnViewCreateFinish += OnActionOnViewCreateFinifh;
            m_CurrentCtrledVitBase.ActionOnSyncBorthPos += SyncBornPos;

            m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction; // 显隐组件

            if (m_data.IsServerAOI)
            {
                m_CurrentCtrledVitBase.Create(0, m_data);
                m_CurrentCtrledVitBase.CreateModel(m_container.transform, OnTemplateCreateFinifh);
            }
            else
            {
                //SGF.Debuger.LogWarning($"本地创建实体 NPC实体 初始化 不创建 ID={Data.M_EntityID}");
                m_CurrentCtrledVitBase.Create(0, m_data);
                m_CurrentCtrledVitBase.SetCurrentPos(pos);
            }
        }

        /// <summary> 壳子加载完成 </summary>
        protected override void OnTemplateCreateFinifh(GameObject gob)
        {
            base.OnTemplateCreateFinifh(gob);

            // 创建头顶信息面板
            UnitPendant unitPendant = new(this);
            m_listCompoent.Add(unitPendant);
            m_CurrentCtrledVitBase.ActionOnCheckTarget += unitPendant.OnActionOnCheckTarget;

            M_NpcDataCell = LocalDataManager.Instance.GetNPCDataCell(m_CurrentCtrledVitBase.ConfigIndex);
            if (M_NpcDataCell != null)
            {
                Range = M_NpcDataCell.GetTriggerRange() * 0.01f;
            }

            // 创建触发器
            CreateTrigger();

            ClientNpc.ClientNpcManager.Instance.OnEnterAOI(this);
        }

        private void OnFreshServerEntityVisiable(int arg0)
        {
            BusinessManager.Instance.SetNpcIsVisiable(this);
        }

        /// <summary> 加载模型完成 </summary>
        protected override void OnActionOnViewCreateFinifh()
        {
            m_ViewCreateFinifh = true;

            base.OnActionOnViewCreateFinifh();

            if (!M_Curr.M_IsAlive)
            {
                OnDeadDisPlay();
            }

            // 创建选中指示器组件
            CheckIndicator checkIndicator = new(this);
            m_listCompoent.Add(checkIndicator);
            m_CurrentCtrledVitBase.ActionOnCheckTarget += checkIndicator.OnActionOnCheckTarget;

            SetSceneJsonRot();

            if (M_NpcDataCell != null)
            {
                if (!string.IsNullOrEmpty(M_NpcDataCell.StartIdle))
                {
                    m_CurrentCtrledVitBase.SetIdleAnimation?.Invoke(M_NpcDataCell.StartIdle);
                }
            }

            GlobalEvent.OnFreshServerEntityVisiable.AddListener(OnFreshServerEntityVisiable);

            if (M_NpcDataCell != null)
            {
                OnShaderStateChange((CharStateController.CharacterState)M_NpcDataCell.GetStartShader(), true);
                //SetVisiable(M_NpcDataCell.GetIsShow());
            }

            BusinessManager.Instance.SetNpcIsVisiable(this);

            GlobalEvent.NPCVisiableRet?.Invoke((int)m_CurrentCtrledVitBase.ConfigIndex, m_createPos, IsShow);
            // SetInitCreateModelVisibilityChange();
        }

        private void SetSceneJsonRot()
        {
            if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.Npcs != null)
            {
                if (GameMap.sceneJsonData.Npcs.TryGetValue(Index, out var data) && data != null)
                {
                    if (data.ClientRot != null)
                    {
                        if (data.ClientRot.x != 0 || data.ClientRot.z != 0)
                        {
                            Animator animator = m_container.GetComponentInChildren<Animator>();
                            if (animator != null)
                            {
                                animator.transform.localRotation = Quaternion.Euler(data.ClientRot.Convert());
                            }
                        }
                    }
                }
            }
        }

        /*private void SetInitCreateModelVisibilityChange()
        {
            if (m_VisibilityChange)
            {
                OnVisibilityChange(TempVisibility.Visibility);
            }
        }*/

        protected override void CreateMContainer(EntityBaseData data)
        {
            //创建用来显示视图的容器
            m_container = new GameObject(LOG_TAG);
            m_container.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
        }

        public override void Release()
        {
            ClientNpc.ClientNpcManager.Instance.OnLeaveAOI((int)m_CurrentCtrledVitBase.ConfigIndex);
            GlobalEvent.OnFreshServerEntityVisiable.RemoveListener(OnFreshServerEntityVisiable);
            /*if (TempVisibility != null)
            {
                TempVisibility.Release();
            }

            TempVisibility = null;*/
            m_ViewCreateStart = false;
            m_ViewCreateFinifh = false;
            m_VisibilityChange = false;

            m_isLookAtMainPlayer = false;

            DestroyTrigger();
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnUpdateService");
            m_data.ActionOnBattleStateDead -= OnDeadDisPlay;
            //Release Component!
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].Release();
            }

            m_listCompoent.Clear();

            //释放所有伙伴
            GameNPCEntityBase node = m_CurrentCtrledVitBase;
            while (node != null)
            {
                GameNPCEntityBase next = node.NextFriend;
                EntityFactory.ReleaseEntity(node);
                node = next;
            }

            //m_tailFriend = null;
            m_CurrentCtrledVitBase = null;
            M_NpcDataCell = null;
            m_data = null;
            base.Release();
            if (m_container != null)
            {
                GameObject.Destroy(m_container);
                m_container = null;
            }
        }

        public override void EnterFrame(int frameIndex)
        {
            if (!m_data.IsServerAOI && !m_ViewCreateStart)
            {
                bool isExceedMapGridSize = GameManager.Instance.IsExceedMapGridSize(m_createPos);
                if (!isExceedMapGridSize)
                {
                    SGF.Debuger.Log($"本地创建实体 NPC实体 创建 开始 ID={Data.M_EntityID}");
                    m_ViewCreateStart = true;
                    m_CurrentCtrledVitBase.CreateModel(m_container.transform, OnTemplateCreateFinifh);
                }
            }

            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].EnterFrame(frameIndex);
            }

            //ClientDoMove();
            ClientGroupSync();
        }

        protected override void SyncBornPos(UnityEngine.Vector3 pos)
        {
            m_CurrentCtrledVitBase.MoveByServerNew(pos, true, true);
        }

        //----------------------------------------------------------------------

        /// <summary>
        /// 客户端自己控制
        /// </summary>
        /// <param name="pos"></param>
        private void ClientGroupSync()
        {
            //玩家的其他3位1体，本地ai驱动TODO
            m_CurrentCtrledVitBase.EnterFrame();
        }

        #region RPC协议处理

        public override void OnServerSetPosNTF(MessageHandleData data)
        {
            ProtoMsg.ServerSetPosNTF serverSetPosMsg = (ProtoMsg.ServerSetPosNTF)data.data;
            UnityEngine.Vector3 v3 = UnityEngine.Vector3.zero;
            v3.x = serverSetPosMsg.Pos.X;
            v3.y = serverSetPosMsg.Pos.Y;
            v3.z = serverSetPosMsg.Pos.Z;
            ForceSyncPos(v3);
        }

        public override void OnReadyDeadNtf(MessageHandleData data)
        {
            base.OnReadyDeadNtf(data);

            M_Curr.IsReadyDead = true;
        }

        #endregion

        #region 死亡复活

        /// <summary>
        /// 死亡逻辑
        /// </summary>
        private void OnDeadDisPlay()
        {
            if (BattleManager.Instance.CurAtkEntity != null && BattleManager.Instance.CurAtkEntity.EntityId == Data.M_EntityID)
            {
                // BattleManager.Instance.SetCurAtkEntity(null);
                // 索敌目标如果死亡,那就切换索敌目标
                BattleManager.Instance.ClearSerarchTargets();
            }

            if (BattleManager.Instance.CurCheckNPC != null && BattleManager.Instance.CurCheckNPC.EntityId == Data.M_EntityID)
            {
                BattleManager.Instance.CurCheckNPC = null;
            }
            //m_CurrentCtrledVitBase.skillDispatcher.Reset();
            //M_Curr.M_IsAlive = false;
            //I_AnimParam animParam = m_CurrentCtrledVitBase.GetAnimParamByState(E_ULayerSubState.Deading);
            //m_CurrentCtrledVitBase.SetSubState(E_ULayerSubState.Deading, animParam, OnDelayDelete);

            //m_CurrentCtrledVitBase.skillDispatcher.Reset();
            I_AnimParam animParam = m_CurrentCtrledVitBase.GetAnimParamByState(E_ULayerSubState.Deading);
            m_CurrentCtrledVitBase.SetSubState(E_ULayerSubState.Deading, animParam);
            M_Curr.M_IsAlive = false;
            DelayInvoker.DelayInvoke(GameConfig.System_Die_Count_Down, OnDelayDelete);
        }

        private void OnDelayDelete(object[] args)
        {
            if (m_CurrentCtrledVitBase == null)
            {
                return;
            }

            GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, currentCtrlNttId, null);
            gameCommand.isServerAOI = false;
            GameManager.Instance.EntityDataCommand(gameCommand);
        }

        private void OnDelayDelete()
        {
            if (m_CurrentCtrledVitBase == null)
            {
                return;
            }

            GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, currentCtrlNttId, null);
            gameCommand.isServerAOI = false;
            GameManager.Instance.EntityDataCommand(gameCommand);
        }

        #endregion

        #region 触发器相关逻辑

        private void CreateTrigger()
        {
            mTriggerKey = "GameNPCCtrlGroup" + Data.M_EntityID;
            Module.TriggerData trigger = new()
            {
                Key = mTriggerKey,
                Position = m_createPos,
                Radius = Range,
                EntityID = Data.M_EntityID,
                TriggerEvent = OnTriggerHandler
            };
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "Register", trigger);
        }

        private void DestroyTrigger()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "UnRegister", mTriggerKey);
        }

        private void OnTriggerHandler(bool isEnter, UnityEngine.Vector3 position, float radius)
        {
            if (m_CurrentCtrledVitBase == null)
            {
                return;
            }

            //DisplayProcessDispenser.Instance.AddSpecialMessage(isEnter ? "进入范围" : "离开范围");
            if (!isEnter)
            {
                if (BattleManager.Instance.CurCheckNPC != null && currentCtrlNttId == BattleManager.Instance.CurCheckNPC.EntityId)
                {
                    BattleManager.Instance.CurCheckNPC = null;
                }
                if (m_isLookAtMainPlayer)
                {
                    m_CurrentCtrledVitBase.RecoverCfgAngel();
                    ClientNpc.ClientNpcManager.Instance.ResumeLast(Index);
                    m_isLookAtMainPlayer = false;
                }
            }
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnUpdateService");
        }

        #endregion

        public void LookAtMainPlayer()
        {
            if (m_CurrentCtrledVitBase == null)
            {
                return;
            }
            if (M_NpcDataCell != null)
            {
                if (M_NpcDataCell.GetIsVeer() == 0)
                {
                    return;
                }
            }
            ClientNpc.ClientNpcManager.Instance.TranslateState(Index, ClientNpc.StateEnum.Idle);
            m_CurrentCtrledVitBase.LookAtMainPlayer();
            m_isLookAtMainPlayer = true;
        }

        /// <summary>
        /// 服务器条件下发的效果，临时控制npc显隐
        /// </summary>
        /// <param name="visiblity"></param>
        private void OnVisibilityChange(bool visiblity)
        {
            if (m_ViewCreateFinifh)
            {
                if (visiblity)
                {
                    M_Curr.ActionOnStopHidden?.Invoke(false);
                }
                else
                {
                    M_Curr.ActionOnStartHidden?.Invoke(false);
                }

                m_VisibilityChange = false;
            }
            else
            {
                m_VisibilityChange = true;
            }
        }

        public void SetVisiable(bool b)
        {
            if (!m_ViewCreateStart)
            {
                return;
            }
            var visibility = b;
            //SGF.Debuger.LogWarning($"NPC显隐 11111111 id={Data.M_EntityID},visibility={visibility},IsShow={IsShow}");

            //Debuger.Log( "服务效果 和 条件 SetVisiable " + b + "m_CurrentCtrledVitBase==null" + (m_CurrentCtrledVitBase != null));
            //如果不可见则查询 服务器记录的
            //if (!visibility && m_CurrentCtrledVitBase != null)
            //{
            //    visibility = BusinessManager.Instance.GetServerMineVisiable(GameManager.Instance.GetCurMapId(), ConfigID);
            //}
            //if (m_CurrentCtrledVitBase != null)
            //{
            //    bool isFind = false;
            //    bool temp = BusinessManager.Instance.GetServerNpcVisiable(GameManager.Instance.GetCurMapId(), (int)m_CurrentCtrledVitBase.ConfigIndex, out isFind);
            //    if (isFind)
            //    {
            //        visibility = temp;
            //    }
            //}
            if (!visibility && m_CurrentCtrledVitBase != null)
            {
                visibility = BusinessManager.Instance.GetServerNpcVisiable(GameManager.Instance.GetCurMapId(), (int)m_CurrentCtrledVitBase.ConfigIndex);
            }

            if (IsShow == visibility)
            {
                return;
            }

            IsShow = visibility;

            if (IsShow)
            {
                M_Curr.ActionOnStopHidden?.Invoke(false);
            }
            else
            {
                M_Curr.ActionOnStartHidden?.Invoke(false);
            }

            if (!IsShow)
            {
                NPCEntityBase npcE = M_Curr as NPCEntityBase;
                if (npcE == BattleManager.Instance.CurCheckNPC)
                {
                    BattleManager.Instance.CurCheckNPC = null;
                }
            }
            //SGF.Debuger.LogError($"NPC显隐 11111111 id={Data.M_EntityID},visibility={visibility},IsShow={IsShow}");
        }
    }
}