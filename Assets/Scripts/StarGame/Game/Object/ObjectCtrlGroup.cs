///--------------------------------------------------------------------
/// 文件名   :   ObjectCtrlGroup
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   #CREATETIME#
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using SGF;
using SGF.Module.Framework;
using SGF.Unity;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.View.VitalSign;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Map;
using StarProject.Game.Player;
using StarProject.Game.Player.Component;
using StarProject.Module;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProject.Service.ServerService;
using StarProject.Service.SystemOpen;
using StarProjectDef;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.Game
{
    public class ObjectCtrlGroup : EntityCtrlBase
    {
        protected new string LOG_TAG = "ObjectCtrlGroup";

        private ObstacleBase m_CurrentCtrledVitBase;
        public override AOIEntityObject M_Curr => m_CurrentCtrledVitBase;


        private NoneVitalSignData m_NoneVitaldata;

        public NoneVitalSignData NoneVitalData
        {
            get { return m_NoneVitaldata; }
        }

        /// <summary>
        /// 有服务时显示
        /// </summary>
        private GameObject mTaskEffect;

        private GameObject mNormalEffect;
        private string mTriggerKey;

        public int ConfigID { get; private set; }

        //外发光以及特效相关
        public int SpecialEffect { get; private set; }
        public string EffectAddress { get; private set; }
        public float Range { get; private set; }
        public string ObjectName { get; private set; }

        private int m_AssetIndex;
        private string EndinterIdle;

        private InterActionModule _InterActionModule;

        private InterActionModule m_InterActionModule
        {
            get
            {
                //if (_InterActionModule == null)
                //{
                //    _InterActionModule = ModuleManager.Instance.GetModule(ModuleDef.Name.InterActionModule) as InterActionModule;
                //}

                //return _InterActionModule;
                return ModuleManager.Instance.GetModule(ModuleDef.Name.InterActionModule) as InterActionModule;
            }
        }

        public int Index
        {
            get
            {
                if (NoneVitalData != null && NoneVitalData.Attrs != null)
                {
                    return NoneVitalData.Attrs.GetAoiValue<int>(EnumAOIType.Int, AOIAttrDefine.SpaceIndex);
                }

                return 0;
            }
        }

        //是否已经被隐藏
        public bool IsShow = true;

        // private EntityTempVisibility TempVisibility;

        /// <summary>
        /// 矿物是否枯竭
        /// </summary>
        public bool MineIsDrain { get; set; }

        /// <summary>
        /// 是不是采集物
        /// </summary>
        public int MineID { get; private set; }

        public InteractDataCell M_InteractDataCell;

        private bool m_ViewCreateStart = false; // 显示层创建开始
        private bool m_ViewCreateFinifh = false; // 显示层创建完成
        private bool m_VisibilityChange = false; // 缓存已经执行过

        //public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        //{
        //    LOG_TAG = "[ObjectCtrlGroup_" + data.M_EntityID + "]";
        //    MineIsDrain = false;
        //    m_NoneVitaldata = (NoneVitalSignData)data;
        //    m_NoneVitaldata.myOwnerNttGroup = this;
        //    m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<ObstacleBase>();
        //    ConfigID = (int)NoneVitalData.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Index);
        //    base.Create(data, pos);
        //    m_CurrentCtrledVitBase.Create(0, m_NoneVitaldata, m_container.transform);
        //    m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction;

        //    //【我，其他人】放置在出生坐标
        //    if (pos.x != -999) //AOI的Default，不能进
        //    {
        //        SyncBornPos(pos); //客户端本地创建的，默认负数（000有用），则不进
        //    }

        //    if (LocalDataManager.Instance.M_InteractData.StaticInteractDatas.TryGetValue(ConfigID, out var interactData) && interactData != null)
        //    {
        //        SpecialEffect = interactData.GetSpecialEffect();
        //        EffectAddress = interactData.EffectAddress;
        //        Range = interactData.GetTriggerRange() * 0.01f;
        //        m_AssetIndex = interactData.GetAssetIndex();
        //        ObjectName = interactData.ModelName;
        //        EndinterIdle = interactData.EndinterIdle;
        //        MineID = (int)interactData.GetMineID();
        //        if (!string.IsNullOrEmpty(interactData.DefaultIdle))
        //        {
        //            m_CurrentCtrledVitBase.SetIdleAnimation?.Invoke(interactData.DefaultIdle);
        //        }
        //    }

        //    ObjectUnitPendant unitPendant = new(this);
        //    m_listCompoent.Add(unitPendant);

        //    m_CurrentCtrledVitBase.OnBornComplete += OnBornComplete;
        //    TempVisibility = new EntityTempVisibility();
        //    TempVisibility.Initialize(m_CurrentCtrledVitBase.EntityType, (int)m_CurrentCtrledVitBase.ConfigIndex, OnVisibilityChange);
        //    BusinessManager.Instance.SetObjectIsVisiable(this);
        //}

        public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        {
            LOG_TAG = "[ObjectCtrlGroup_" + data.M_EntityID + "]";
            MineIsDrain = false;

            m_NoneVitaldata = (NoneVitalSignData)data;
            m_NoneVitaldata.myOwnerNttGroup = this;
            var treasureData = BusinessManager.Instance.GetTreasureData();
            if (treasureData != null)
            {
                if (treasureData.TreasureEID != 0 && treasureData.TreasureMapID == GameManager.Instance.GetCurMapId())
                {
                    if (treasureData.TreasureInterID == Index)
                    {
                        pos = new Vector3(treasureData.TreasurePos.X, treasureData.TreasurePos.Y, treasureData.TreasurePos.Z);
                    }
                }
            }
            base.Create(data, pos);

            ConfigID = (int)NoneVitalData.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Index);
            M_InteractDataCell = LocalDataManager.Instance.GetInteractDataCell(ConfigID);
            if (M_InteractDataCell != null)
            {
                SpecialEffect = M_InteractDataCell.GetSpecialEffect();
                EffectAddress = M_InteractDataCell.EffectAddress;
                Range = M_InteractDataCell.GetTriggerRange() * 0.01f;
                m_AssetIndex = M_InteractDataCell.GetAssetIndex();
                ObjectName = M_InteractDataCell.ModelName;
                EndinterIdle = M_InteractDataCell.EndinterIdle;
                MineID = (int)M_InteractDataCell.GetMineID();
            }

            m_CurrentCtrledVitBase = EntityFactory.InstanceEntity<ObstacleBase>();
            m_CurrentCtrledVitBase.ActionOnViewCreateFinish += OnActionOnViewCreateFinifh;
            m_CurrentCtrledVitBase.ActionOnSyncBorthPos += SyncBornPos;

            m_CurrentCtrledVitBase.CompoentShowAction = OnCompoentShowAction; // 显隐组件

            m_ViewCreateFinifh = false;
            m_ViewCreateStart = m_NoneVitaldata.IsServerAOI;

            if (m_NoneVitaldata.IsServerAOI)
            {
                m_CurrentCtrledVitBase.Create(0, m_NoneVitaldata);
                m_CurrentCtrledVitBase.CreateModel(m_container.transform, OnTemplateCreateFinifh);
            }
            else
            {
                //SGF.Debuger.LogWarning($"本地创建实体 交互物件实体 初始化 不创建 ID={m_NoneVitaldata.M_EntityID}");
                m_CurrentCtrledVitBase.Create(0, m_NoneVitaldata);
                m_CurrentCtrledVitBase.SetCurrentPos(pos);
            }
        }

        /// <summary> 壳子加载完成 </summary>
        protected override void OnTemplateCreateFinifh(GameObject gob)
        {
            base.OnTemplateCreateFinifh(gob);

            ObjectUnitPendant unitPendant = new(this);
            m_listCompoent.Add(unitPendant);

            CreateTrigger(); // 创建触发器

            // TempVisibility = new EntityTempVisibility();
            //TempVisibility.Initialize(m_CurrentCtrledVitBase.EntityType, ConfigID, OnVisibilityChange);
        }

        /// <summary> 加载模型完成 </summary>
        protected override void OnActionOnViewCreateFinifh()
        {
            m_ViewCreateFinifh = true;

            base.OnActionOnViewCreateFinifh();

            SetSceneJsonRot();

            if (M_InteractDataCell != null)
            {
                if (!string.IsNullOrEmpty(M_InteractDataCell.DefaultIdle))
                {
                    m_CurrentCtrledVitBase.SetIdleAnimation?.Invoke(M_InteractDataCell.DefaultIdle);
                }
                //SetVisiable(M_InteractDataCell.GetIsShow());
            }
            //临时显隐优先级高于 服务条件显隐
            /*if (TempVisibility != null)
            {
                if (!TempVisibility.Visibility)
                {
                    OnVisibilityChange(IsShow);
                }
            }*/
            BusinessManager.Instance.SetObjectIsVisiable(this);

            GlobalEvent.OnFreshServerEntityVisiable.AddListener(OnFreshServerEntityVisiable);

            CreateTaskEffect();
            CreateNormalEffect();
        }

        private void OnFreshServerEntityVisiable(int arg0)
        {
            BusinessManager.Instance.SetObjectIsVisiable(this);
        }

        private void SetSceneJsonRot()
        {
            if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.Mines != null)
            {
                if (GameMap.sceneJsonData.Mines.TryGetValue(Index, out var data) && data != null)
                {
                    if (data.ClientRot != null)
                    {
                        Animator animator = m_container.GetComponentInChildren<Animator>();
                        if (animator != null)
                        {
                            if (data.ClientRot.x != 0 || data.ClientRot.z != 0)
                            {
                                animator.transform.localRotation = Quaternion.Euler(new Vector3(data.ClientRot.x, -data.ClientRot.y, data.ClientRot.z));
                            }
                        }
                    }
                }
            }
        }

        //public bool NeedDraw = false;
        protected override void SyncBornPos(Vector3 pos)
        {
            //读取玩家属性
            var treasureData = BusinessManager.Instance.GetTreasureData();
            if (treasureData != null)
            {
                if (treasureData.TreasureEID != 0 && treasureData.TreasureMapID != 0)
                {
                    if (treasureData.TreasureInterID == Index)
                    {
                        //Debug.LogError($"藏宝图出生坐标 Index={Index}  Pos{pos} treasureData.TreasurePos={treasureData.TreasurePos} ");

                        pos.x = treasureData.TreasurePos.X;
                        pos.y = treasureData.TreasurePos.Y;
                        pos.z = treasureData.TreasurePos.Z;
                        // treasureData.TreasureInterID = GveTreasureInterID;
                        //NeedDraw = true;
                    }
                }
            }

            m_CurrentCtrledVitBase.MoveByServerNew(pos, true, true);
        }

        private void ActiveTaskEffect(bool active)
        {
            if (mTaskEffect != null)
            {
                if (active)
                {
                    var show = false;
                    var services = ServerServiceManager.Instance.GetServices(ServerServiceType.InterAction, (uint)ConfigID);
                    if (services != null && services.Count > 0)
                    {
                        foreach (var service in services)
                        {
                            if (service.IsTaskServer)
                            {
                                show = true;
                                break;
                            }
                        }
                    }

                    if (show)
                    {
                        mTaskEffect.SetActive(true);
                    }
                }
                else
                {
                    mTaskEffect.SetActive(false);
                }
            }
        }

        private void CreateNormalEffect()
        {
            if (M_InteractDataCell != null)
            {
                string path = M_InteractDataCell.EffectAddress;
                if (!string.IsNullOrEmpty(path))
                {
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(
                        path,
                        (GameObject go) =>
                        {
                            if (go == null)
                            {
                                return;
                            }

                            if (Container == null)
                            {
                                return;
                            }

                            mNormalEffect = GameObject.Instantiate<GameObject>(go);
                            if (mNormalEffect != null)
                            {
                                var parent = Container.transform;
                                var objView = Container.GetComponentInChildren<ViewVitalObjectAnim>();
                                if (objView != null)
                                {
                                    parent = objView.gameObject.transform;
                                }

                                mNormalEffect.transform.SetParent(parent);
                                mNormalEffect.transform.localPosition = Vector3.zero;
                                mNormalEffect.transform.localRotation = Quaternion.identity;
                                mNormalEffect.transform.localScale = Vector3.one;
                                mNormalEffect.SetActive(IsShow);
                            }
                        });
                }
            }
        }

        private void CreateTaskEffect()
        {
            if (M_InteractDataCell != null)
            {
                string path = M_InteractDataCell.TaskEffectAddress;
                if (!string.IsNullOrEmpty(path))
                {
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(
                        path,
                        (GameObject go) =>
                        {
                            if (go == null)
                            {
                                return;
                            }

                            if (Container == null)
                            {
                                return;
                            }

                            mTaskEffect = GameObject.Instantiate<GameObject>(go);
                            if (mTaskEffect != null)
                            {
                                var parent = Container.transform;
                                var objView = Container.GetComponentInChildren<ViewVitalObjectAnim>();
                                if (objView != null)
                                {
                                    parent = objView.gameObject.transform;
                                }

                                mTaskEffect.transform.SetParent(parent);
                                mTaskEffect.transform.localPosition = Vector3.zero;
                                mTaskEffect.transform.localRotation = Quaternion.identity;
                                mTaskEffect.transform.localScale = Vector3.one;
                                mTaskEffect.SetActive(HasService);
                            }
                        });
                }
            }
        }

        protected override void CreateMContainer(EntityBaseData data)
        {
            m_container = new GameObject("ObjectCtrlGroup_" + ConfigID + "_" + data.M_EntityID);
            m_container.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
        }

        public override void EnterFrame(int frameIndex)
        {
            if (!m_NoneVitaldata.IsServerAOI && !m_ViewCreateStart)
            {
                bool isExceedMapGridSize = GameManager.Instance.IsExceedMapGridSize(m_createPos);
                if (!isExceedMapGridSize)
                {
                    //SGF.Debuger.Log($"本地创建实体 交互物件实体 创建 开始 ID={m_NoneVitaldata.M_EntityID}");
                    m_ViewCreateStart = true;
                    m_CurrentCtrledVitBase.CreateModel(m_container.transform, OnTemplateCreateFinifh);
                }
            }

            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].EnterFrame(frameIndex);
            }
        }

        public override void Release()
        {
            m_ViewCreateStart = false;
            m_ViewCreateFinifh = false;
            m_VisibilityChange = false;
            /*if (NeedDraw)
            {
                GizmosHelper.Instance.RemoveDrawCricel(Index);
            }

            NeedDraw = false;*/

            /*if (TempVisibility != null)
            {
                TempVisibility.Release();
            }
            TempVisibility = null;*/
            GlobalEvent.OnFreshServerEntityVisiable.RemoveListener(OnFreshServerEntityVisiable);
            DestroyTrigger();
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnUpdateService");
            //Release Component!
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].Release();
            }

            m_listCompoent.Clear();

            ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "OnLeaveRange", NoneVitalData.M_EntityID);

            ObstacleBase node = m_CurrentCtrledVitBase;
            if (node != null)
            {
                EntityFactory.ReleaseEntity(node);
            }

            m_CurrentCtrledVitBase = null;

            if (mTaskEffect != null)
            {
                GameObject.Destroy(mTaskEffect);
                mTaskEffect = null;
            }

            if (mNormalEffect != null)
            {
                GameObject.Destroy(mNormalEffect);
                mNormalEffect = null;
            }

            if (m_container != null)
            {
                GameObject.Destroy(m_container);
                m_container = null;
            }

            base.Release();
        }

        #region 触发器相关逻辑

        private void CreateTrigger()
        {
            mTriggerKey = "ObjectCtrlGroup" + NoneVitalData.M_EntityID;
            Module.TriggerData trigger = new()
            {
                Key = mTriggerKey,
                Position = PhysicsUtils.GetGroundPoint(m_createPos),
                Radius = Range,
                TriggerEvent = OnTriggerHandler
            };
            //Debug.LogError($"创建触发器 Index={Index}   Position={trigger.Position}");
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "Register", trigger);
            /*if (NeedDraw)
            {
                GizmosHelper.Instance.AddDrawCricel(Index, trigger.Position, Range,
                    $"{Index}--> {NoneVitalData.M_EntityID}--> {trigger.Position} ");
            }*/

            //BusinessManager.Instance.SetObjectIsVisiable(this); // 这里重复了
        }

        private void DestroyTrigger()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "UnRegister", mTriggerKey);
        }
        private bool IsAotuInter(int type)
        {
            return type == 1;
        }
        private void OnTriggerHandler(bool isEnter, Vector3 position, float radius)
        {
            if (!IsShow)
            {
                return;
            }

            //DisplayProcessDispenser.Instance.AddSpecialMessage(isEnter ? "进入范围" : "离开范围");
            if (isEnter)
            {
                if (m_InterActionModule != null)
                {
                    //采集物，采集物需要判断系统是否开发
                    if (MineID > 0)
                    {
                        if (!SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.LifeSkill))
                        {
                            return;
                        }

                        var minecfg = LocalDataManager.Instance.GetLifeSkillMineDataCell(MineID);
                        if (minecfg != null)
                        {
                            var serJobInfo = BusinessManager.Instance.GetLifeJobSkill(minecfg.GetSkillID());
                            if (serJobInfo != null)
                            {
                                if (serJobInfo.SkillLevel < minecfg.GetLevel())
                                {
                                    return;
                                }
                            }
                        }
                    }

                    bool active = m_InterActionModule.InterActionCondition(ConfigID, GameManager.Instance.mainPlayerId,
                        Index, NoneVitalData.M_EntityID);
                    if (active)
                    {
                        if (MineID > 0)
                        {
                            GlobalEvent.OnGratherEnergy.Invoke(1, (long)NoneVitalData.M_EntityID, true);
                        }

                        if (M_InteractDataCell != null)
                        {
                            if (IsAotuInter(M_InteractDataCell.GetIsAutoInter()))
                            {
                                ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "QueryInter", NoneVitalData.M_EntityID);
                            }
                            else
                            {
                                ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "OnEnterRange", NoneVitalData.M_EntityID);
                                // GlobalEvent.OnTriggerEvent?.Invoke(ConfigID, m_NoneVitaldata.M_EntityID);
                            }
                        }
                    }
                }
            }
            else
            {
                ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "OnLeaveRange", NoneVitalData.M_EntityID);
                if (MineID > 0)
                {
                    GlobalEvent.OnGratherEnergy.Invoke(1, (long)NoneVitalData.M_EntityID, false);
                }

                GlobalEvent.OnExitEvent?.Invoke(ConfigID, NoneVitalData.M_EntityID);
            }
        }

        #endregion

        public Vector3 GetPosition()
        {
            return m_CurrentCtrledVitBase.m_currentPos;
        }

        private void OnVisibilityChange(bool visiblity)
        {
            IsShow = visiblity;

            if (m_ViewCreateFinifh)
            {
                if (m_CurrentCtrledVitBase != null)
                {
                    m_CurrentCtrledVitBase.ActionModelInteractiveEff?.Invoke(visiblity);
                    m_CurrentCtrledVitBase.ActionModelVisiable?.Invoke(visiblity, true);
                }

                m_VisibilityChange = false;
            }
            else
            {
                m_VisibilityChange = true;
            }

        }

        private bool HasService = false;

        public void OnServiceChange(bool b)
        {
            HasService = b;
            ActiveTaskEffect(HasService);
            if (!b && MineID == 0)
            {

                ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "OnLeaveRange", NoneVitalData.M_EntityID);
            }
        }

        public void SetVisiable(bool b)
        {
            if (!m_ViewCreateStart)
            {
                return;
            }
            //临时显隐优先级高于 服务条件显隐
            /*if (TempVisibility != null)
            {
                if (TempVisibility.Visibility)
                {
                    return;
                }
            }*/
            var visibility = b;

            //如果不可见则查询 服务器记录的
            if (!visibility && m_CurrentCtrledVitBase != null)
            {
                visibility = BusinessManager.Instance.GetServerMineVisiable(GameManager.Instance.GetCurMapId(), ConfigID);
            }

            //if (m_CurrentCtrledVitBase != null)
            //{
            //    bool isFind = false;
            //    bool temp = BusinessManager.Instance.GetServerMineVisiable(GameManager.Instance.GetCurMapId(), ConfigID, out isFind);
            //    if (isFind)
            //    {
            //        visibility = temp;
            //    }
            //}

            if (IsShow == visibility)
            {
                return;
            }

            IsShow = visibility;

            if (mNormalEffect != null)
            {
                mNormalEffect.SetActive(IsShow);
            }

            if (IsShow)
            {
                TriggerModule m_TriggerModule = ModuleManager.Instance.GetModule(ModuleDef.Name.TriggerModule) as TriggerModule;
                if (m_TriggerModule != null)
                {
                    bool result = false;
                    bool exist = m_TriggerModule.QueryTrrigerState(mTriggerKey, out result);
                    if (result && exist)
                    {
                        OnTriggerHandler(true, Vector3.zero, 0);
                    }
                }
            }
            else
            {
                if (NoneVitalData != null)
                {
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "OnLeaveRange", NoneVitalData.M_EntityID);
                }
                else
                {
                    SGF.Debuger.Log("NoneVitalData is null");
                }
            }

            if (m_CurrentCtrledVitBase != null)
            {
                m_CurrentCtrledVitBase.ActionModelInteractiveEff?.Invoke(IsShow);
                m_CurrentCtrledVitBase.ActionModelVisiable?.Invoke(IsShow, true);
            }

            OnCompoentShowAction(!IsShow);
        }

        private AnimParam GetAnimParam(string AnimationName, float speed = 1)
        {
            AnimParam animParam = new();
            animParam.SetBaseAnimName(AnimationName);
            animParam.SetBaseAnimSpeed(speed);
            return animParam;
        }

        public void OnInterRet(ProtoMsg.curstate state)
        {
            switch (state)
            {
                case ProtoMsg.curstate.Waiting:

                    m_CurrentCtrledVitBase?.ChangeState(ObjectSubState.Idle, true);
                    //动作
                    m_CurrentCtrledVitBase?.PlayAnimation(GetAnimParam("Idle"), ObjectSubState.Idle.ToString());
                    break;
                case ProtoMsg.curstate.Inting:
                    // -------- [交互中]
                    m_CurrentCtrledVitBase?.ChangeState(ObjectSubState.Intering, true);

                    //动作
                    var VitalState = m_CurrentCtrledVitBase?.PlayAnimation(GetAnimParam("InterStart_" + m_AssetIndex));
                    if (VitalState != null)
                    {
                        VitalState.OnVitalStateChanged += OnVitalStateComplte;
                    }

                    //特效
                    FxParam fxParam = new();
                    fxParam.InitWithLogic("InterStart_" + m_AssetIndex);
                    m_CurrentCtrledVitBase.PlaySpecialEffect(fxParam);

                    //播放音效
                    //m_CurrentCtrledVitBase?.PlaySound(GetAudioClip("InterStart_" + m_AssetIndex), false);
                    PlaySoundByName("InterStart");
                    break;
                case ProtoMsg.curstate.Break:
                    // -------- [中止]
                    m_CurrentCtrledVitBase?.ChangeState(ObjectSubState.Idle, true);

                    //动作
                    m_CurrentCtrledVitBase?.PlayAnimation(GetAnimParam("Idle"), ObjectSubState.Idle.ToString());

                    break;
                case ProtoMsg.curstate.End:
                    // -------- [结束]

                    if (!string.IsNullOrEmpty(EndinterIdle))
                    {
                        m_CurrentCtrledVitBase.SetIdleAnimation?.Invoke(EndinterIdle);
                    }

                    m_CurrentCtrledVitBase?.ChangeState(ObjectSubState.InterFinish, true);
                    //动作
                    var VitalStateEnd = m_CurrentCtrledVitBase?.PlayAnimation(GetAnimParam("InterEnd_" + m_AssetIndex));

                    if (VitalStateEnd == null || VitalStateEnd.Clip == null)
                    {
                        m_CurrentCtrledVitBase?.ChangeState(ObjectSubState.Idle, true);
                        m_CurrentCtrledVitBase?.PlayAnimation(
                            GetAnimParam(string.IsNullOrEmpty(EndinterIdle)
                                ? ObjectSubState.Idle.ToString()
                                : EndinterIdle), ObjectSubState.Idle.ToString());
                    }

                    //特效
                    FxParam fxParamEnd = new();
                    fxParamEnd.InitWithLogic("InterEnd_" + m_AssetIndex);
                    m_CurrentCtrledVitBase.PlaySpecialEffect(fxParamEnd);

                    //播放音效
                    //m_CurrentCtrledVitBase?.PlaySound(GetAudioClip("InterEnd_" + m_AssetIndex), false);
                    PlaySoundByName("InterEnd");

                    if (MineID < 1)
                    {
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "OnLeaveRange", NoneVitalData.M_EntityID);

                        if (M_InteractDataCell != null)
                        {
                            float delatTime = M_InteractDataCell.GetDelayTime();
                            if (delatTime != -1)
                            {
                                if (delatTime == 0)
                                {
                                    OnDelayDelete(null);
                                }
                                else
                                {
                                    DelayInvoker.DelayInvoke(delatTime / 1000, OnDelayDelete);
                                }
                            }
                        }
                    }

                    break;
            }
        }

        private void OnDelayDelete(object[] args)
        {
            if (m_CurrentCtrledVitBase == null)
            {
                return;
            }

            GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, NoneVitalData.M_EntityID, null);
            gameCommand.isServerAOI = false;
            GameManager.Instance.EntityDataCommand(gameCommand);
        }

        private void OnVitalStateComplte(VitalState vitalState, VitalStateEnum state)
        {
            vitalState.OnVitalStateChanged -= OnVitalStateComplte;

            var newVitalState = m_CurrentCtrledVitBase?.PlayAnimation(GetAnimParam("InterLoop_" + m_AssetIndex));

            FxParam fxParam = new();
            fxParam.InitWithLogic("InterLoop_" + m_AssetIndex);
            m_CurrentCtrledVitBase.PlaySpecialEffect(fxParam);

            //播放音效
            //m_CurrentCtrledVitBase?.PlaySound(GetAudioClip("InterLoop_" + m_AssetIndex), true);
            PlaySoundByName("InterLoop");
        }

        private void PlaySoundByName(string name)
        {
            m_CurrentCtrledVitBase?.ActionOnPlayWwiseAudio?.Invoke($"{name}_{m_AssetIndex}",
                E_SoundNTFtype.MyListener_SystemSound);
        }
    }
}