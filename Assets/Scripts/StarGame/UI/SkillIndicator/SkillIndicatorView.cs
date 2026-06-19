using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Game.Skill.Utils;
using StarProject.Service.Battle;
using StarProject.Service.Cam;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.UI.SkillIndicator
{
    /// <summary>
    /// 技能指示器显示层
    /// </summary>
    public class SkillIndicatorView : MonoBehaviour
    {
        private string LOG_TAG = "[SkillIndicatorView]";

        private EntityCtrlBase m_entityCtrl;
        /// <summary> 技能分发器 </summary>
        private SkillDispatcher m_SkillDispatcher;

        /// <summary> 外圆指示器 </summary>
        private SGAMESkillShapeIndicator m_Skill_Area;
        private SGAMESkillShapeIndicator Skill_Area
        {
            get
            {
                if (m_Skill_Area == null)
                {
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_Area",
                    (GameObject go) =>
                    {
                        if (go == null)
                        {
                            return;
                        }
                        if (m_entityCtrl != null && m_entityCtrl.Data != null && m_Skill_Area == null)
                        {
                            var gob = GameObject.Instantiate<GameObject>(go);
                            if (gob != null)
                            {
                                m_Skill_Area = gob.GetComponent<SGAMESkillShapeIndicator>();
                                m_Skill_Area.transform.SetParent(this.transform, false);
                                m_Skill_Area.SetShapeActive(false);
                            }
                        }
                    });
                }
                return m_Skill_Area;
            }
        }
        /// <summary> 矩形指示器 </summary>
        private SGAMESkillShapeIndicator m_Skill_Arrow;
        private SGAMESkillShapeIndicator Skill_Arrow
        {
            get
            {
                if (m_Skill_Arrow == null)
                {
                    //GameObject go = Service.Resource.ResourceManager.Instance.LoadGameObject("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_Arrow");
                    ////GameObject go = ResourceManager.LoadPrefab("Skill/SkillIndicator/SkillIndicatorView_Arrow");
                    //if (go != null)
                    //{
                    //    m_Skill_Arrow = go.GetComponent<SkillShapeIndicator>();
                    //    m_Skill_Arrow.transform.SetParent(this.transform, false);
                    //}

                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_Arrow",
                    (GameObject go) =>
                    {
                        if (go == null)
                        {
                            return;
                        }
                        if (m_entityCtrl != null && m_entityCtrl.Data != null && m_Skill_Arrow == null)
                        {
                            var gob = GameObject.Instantiate<GameObject>(go);
                            if (gob != null)
                            {
                                m_Skill_Arrow = gob.GetComponent<SGAMESkillShapeIndicator>();
                                m_Skill_Arrow.transform.SetParent(this.transform, false);
                                m_Skill_Arrow.SetShapeActive(false);
                            }
                        }
                    });
                }
                return m_Skill_Arrow;
            }
        }
        /// <summary> 矩形指示器 </summary>
        private SGAMESkillShapeIndicator m_Skill_Dir;
        private SGAMESkillShapeIndicator Skill_Dir
        {
            get
            {
                if (m_Skill_Dir == null)
                {
                    //GameObject go = Service.Resource.ResourceManager.Instance.LoadGameObject("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_Dir");
                    ////GameObject go = ResourceManager.LoadPrefab("Skill/SkillIndicator/SkillIndicatorView_Dir");
                    //if (go != null)
                    //{
                    //    m_Skill_Dir = go.GetComponent<SkillShapeIndicator>();
                    //    m_Skill_Dir.transform.SetParent(this.transform, false);
                    //}
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_Dir",
                    (GameObject go) =>
                    {
                        if (go == null)
                        {
                            return;
                        }
                        if (m_entityCtrl != null && m_entityCtrl.Data != null && m_Skill_Dir == null)
                        {
                            var gob = GameObject.Instantiate<GameObject>(go);
                            if (gob != null)
                            {
                                m_Skill_Dir = gob.GetComponent<SGAMESkillShapeIndicator>();
                                m_Skill_Dir.transform.SetParent(this.transform, false);
                                m_Skill_Dir.SetShapeActive(false);
                            }
                        }
                    });
                }
                return m_Skill_Dir;
            }
        }
        /// <summary> 扇形指示器 </summary>
        private SGAMESkillShapeIndicator m_Skill_Sector;
        private SGAMESkillShapeIndicator Skill_Sector
        {
            get
            {
                if (m_Skill_Sector == null)
                {
                    //GameObject go = Service.Resource.ResourceManager.Instance.LoadGameObject("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_Sector");
                    ////GameObject go = ResourceManager.LoadPrefab("Skill/SkillIndicator/SkillIndicatorView_Sector");
                    //if (go != null)
                    //{
                    //    m_Skill_Sector = go.GetComponent<SkillShapeIndicator>();
                    //    m_Skill_Sector.transform.SetParent(this.transform, false);
                    //}
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_Sector",
                    (GameObject go) =>
                    {
                        if (go == null)
                        {
                            return;
                        }
                        if (m_entityCtrl != null && m_entityCtrl.Data != null && m_Skill_Sector == null)
                        {
                            var gob = GameObject.Instantiate<GameObject>(go);
                            if (gob != null)
                            {
                                m_Skill_Sector = gob.GetComponent<SGAMESkillShapeIndicator>();
                                m_Skill_Sector.transform.SetParent(this.transform, false);
                                m_Skill_Sector.SetShapeActive(false);
                            }
                        }
                    });
                }
                return m_Skill_Sector;
            }
        }
        /// <summary> 环扇形指示器 </summary>
        private SkillShapeIndicator m_Skill_RingFan;
        private SkillShapeIndicator Skill_RingFan
        {
            get
            {
                if (m_Skill_RingFan == null)
                {
                    //GameObject go = Service.Resource.ResourceManager.Instance.LoadGameObject("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_RingFan");
                    ////GameObject go = ResourceManager.LoadPrefab("Skill/SkillIndicator/SkillIndicatorView_RingFan");
                    //if (go != null)
                    //{
                    //    m_Skill_RingFan = go.GetComponent<SkillShapeIndicator>();
                    //    m_Skill_RingFan.transform.SetParent(this.transform, false);
                    //}
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_RingFan",
                    (GameObject go) =>
                    {
                        if (go == null)
                        {
                            return;
                        }
                        if (m_entityCtrl != null && m_entityCtrl.Data != null && m_Skill_RingFan == null)
                        {
                            var gob = GameObject.Instantiate<GameObject>(go);
                            if (gob != null)
                            {
                                m_Skill_RingFan = gob.GetComponent<SkillShapeIndicator>();
                                m_Skill_RingFan.transform.SetParent(this.transform, false);
                                m_Skill_RingFan.SetShapeActive(false);
                            }
                        }
                    });
                }
                return m_Skill_RingFan;
            }
        }
        /// <summary> 内圆指示器 </summary>
        private SGAMESkillShapeIndicator m_Skill_InnerArea;
        private SGAMESkillShapeIndicator Skill_InnerArea
        {
            get
            {
                if (m_Skill_InnerArea == null)
                {
                    //GameObject go = Service.Resource.ResourceManager.Instance.LoadGameObject("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_InnerArea");
                    ////GameObject go = ResourceManager.LoadPrefab("Skill/SkillIndicator/SkillIndicatorView_InnerArea");
                    //if (go != null)
                    //{
                    //    m_Skill_InnerArea = go.GetComponent<SkillShapeIndicator>();
                    //    m_Skill_InnerArea.transform.SetParent(this.transform, false);
                    //}
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>("UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_InnerArea",
                    (GameObject go) =>
                    {
                        if (go == null)
                        {
                            return;
                        }
                        if (m_entityCtrl != null && m_entityCtrl.Data != null && m_Skill_InnerArea == null)
                        {
                            var gob = GameObject.Instantiate<GameObject>(go);
                            if (gob != null)
                            {
                                m_Skill_InnerArea = gob.GetComponent<SGAMESkillShapeIndicator>();
                                m_Skill_InnerArea.transform.SetParent(this.transform, false);
                                m_Skill_InnerArea.SetShapeActive(false);
                            }
                        }
                    });
                }
                return m_Skill_InnerArea;
            }
        }
        /// <summary> 目标指示器 </summary>
        private GameObject m_Skill_Target;
        private GameObject Skill_Target
        {
            get
            {
                if (m_Skill_Target == null)
                {
                    GameObject go = ResourceHelperMono.LoadPrefab("TestPoint/5");
                    if (go != null)
                    {
                        m_Skill_Target = go;
                        m_Skill_Target.transform.SetParent(this.transform, false);
                    }
                }
                return m_Skill_Target;
            }
        }

        #region 基础配置

        private int m_CurSkillPos = -1; // 当前技能指示器的槽位下标
        private SkillWheelInfo m_CurSkillCfg;
        private ShapeRingFan m_CurWheelPange;
        private WheelConfig m_CurWheelCfg;

        private ShapeRingFan m_CurEneryWheelPange;
        private WheelConfig m_CurEneryWheelCfg;
        /// <summary> 当前显示的指示器类型 </summary>
        private Shape m_CurlcontrolType;
        private SkillInputType m_SkillInputType;
        /// <summary> 技能条件敌人阵营 </summary>
        private SelectType TargetSelect;
        private Vector3 m_localScale = new();
        private Vector3 m_TempAngles;
        private Vector3 m_TempPos = Vector3.zero;
        private List<AOIEntityObject> currentTriggetNtts = new();
        private Vector3 m_SkillTargetLastPos;
        /// <summary> 大圆半径 </summary>
        private float m_OuterRadius = 3f;
        /// <summary> 内圆半径（矩形长度）</summary>
        private float m_InsideRadius = 3f;
        /// <summary> 空心圆半径 </summary>
        private float m_HollowCircleRadius = 1f;
        /// <summary> 矩形宽度 （矩形长度使用的内圆半径） </summary>
        private float m_CubeWidth = 2f;
        /// <summary> 扇形角度 </summary>
        private int m_Angle = 30;

        #endregion

        #region 蓄力效果配置

        private EffectParam effectParam;
        /// <summary> 技能实体 </summary>
        private SkillEntity m_skillEntity;
        /// <summary> 动态碰撞盒范围修改方式 </summary>
        private DynamicRangeType dynamicRangeType;
        /// <summary> 单层蓄力的时间 </summary>
        private int energyTime;
        /// <summary> 已经蓄力的层数,可能蓄力0层</summary>
        private int energyedCount = 0;
        /// <summary> 当前蓄力的时间 </summary>
        private double curEnergyTime = 0f;
        /// <summary> 最大蓄力层数 </summary>
        private int energyMaxNum = 0;
        /// <summary> 是否显示蓄力 </summary>
        private bool m_isShowEnergy = false;

        #endregion

        public void Init(EntityCtrlBase entityCtrl)
        {
            m_entityCtrl = entityCtrl;
            if (m_entityCtrl == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} Init m_entityCtrl=null");
                return;
            }
            Vector3 pos = Vector3.zero;
            pos.y = 0.1f;
            transform.localPosition = pos;

            m_SkillDispatcher = (m_entityCtrl.M_Curr as NPCEntityBase).skillDispatcher;

            m_CurlcontrolType = Shape.None;

            if (m_entityCtrl.Data.isMainPlayer)
            {
                m_SkillDispatcher.SkillController.ActionOnEnergyStart += OnActionOnEnergyStart;
                m_SkillDispatcher.SkillController.ActionOnEnergyEnd += OnActionOnEnergyEnd;
                m_SkillDispatcher.SkillController.ActionOnEndSkillStage += OnActionOnEndSkillStage;
            }
            // 初始化的时候读取一下
            if (Skill_Area || Skill_Arrow || Skill_Dir || Skill_Sector || Skill_RingFan || Skill_InnerArea)
            {

            }
            gameObject.SetActive(true);
        }
        internal void EnterFrame(int frameIndex)
        {
            if (m_isShowEnergy && m_skillEntity != null)
            {
                // 所有的地方都要拿取技能分发器里面的蓄力时间
                // 不自己计算了
                curEnergyTime = m_skillEntity.GetCurEnergyTime(true);
                energyedCount = m_skillEntity.GetCurEnergyCount(true);
                //SGF.Debuger.Log($"{LOG_TAG} EnterFrame curEnergyTime={curEnergyTime},energyedCount={energyedCount}");

                float ratio = GetControlRatio();

                if (m_CurEneryWheelPange != null && m_CurWheelPange != null)
                {
                    m_OuterRadius = (float)(((m_CurEneryWheelPange.MaxRadius - m_CurWheelPange.MaxRadius) * ratio) + m_CurWheelPange.MaxRadius) / 100;
                    //Skill_Area.SetCircleRange(m_OuterRadius);
                    Skill_Area.SetSectorRange(m_OuterRadius);

                    switch (m_CurlcontrolType)
                    {
                        case Shape.Round:
                            {
                                //m_InsideRadius = ((float)m_CurWheelCfg.Shape.Round.Radius / 100) * ratio;
                                m_InsideRadius = (float)(((m_CurEneryWheelCfg.Shape.Round.Radius - m_CurWheelCfg.Shape.Round.Radius) * ratio) + m_CurWheelCfg.Shape.Round.Radius) / 100;
                                //Skill_InnerArea.SetCircleRange(m_InsideRadius);
                                Skill_InnerArea.SetSectorRange(m_InsideRadius);
                            }
                            break;
                        case Shape.HollowCircle:
                            {
                                //m_InsideRadius = ((float)m_CurWheelCfg.Shape.HollowCircle.MinRadius / 100) * ratio;
                                m_InsideRadius = (float)(((m_CurEneryWheelCfg.Shape.HollowCircle.MinRadius - m_CurWheelCfg.Shape.HollowCircle.MinRadius) * ratio) + m_CurWheelCfg.Shape.HollowCircle.MinRadius) / 100;
                                //Skill_InnerArea.SetCircleRange(m_InsideRadius);
                                Skill_InnerArea.SetSectorRange(m_InsideRadius);
                            }
                            break;
                        case Shape.Sector:
                            {
                                //m_InsideRadius = ((float)m_CurWheelCfg.Shape.Sector.Radius / 100) * ratio;
                                m_InsideRadius = (float)(((m_CurEneryWheelCfg.Shape.Sector.Radius - m_CurWheelCfg.Shape.Sector.Radius) * ratio) + m_CurWheelCfg.Shape.Sector.Radius) / 100;
                                Skill_Sector.SetSectorRange(m_InsideRadius);
                            }
                            break;
                        case Shape.RingFan:
                            {
                                //m_InsideRadius = ((float)m_CurWheelCfg.Shape.RingFan.MaxRadius / 100) * ratio;
                                m_InsideRadius = (float)(((m_CurEneryWheelCfg.Shape.RingFan.MaxRadius - m_CurWheelCfg.Shape.RingFan.MaxRadius) * ratio) + m_CurWheelCfg.Shape.RingFan.MaxRadius) / 100;
                                Skill_RingFan.SetRingFanRange(m_InsideRadius);
                            }
                            break;
                        case Shape.Arrow:
                            {
                                //m_InsideRadius = ((float)m_CurWheelCfg.Shape.Rect.Length / 100) * ratio;
                                m_InsideRadius = (float)(((m_CurEneryWheelCfg.Shape.Arrow.Length - m_CurWheelCfg.Shape.Arrow.Length) * ratio) + m_CurWheelCfg.Shape.Arrow.Length) / 100;
                                Skill_Arrow.SetRectLength(m_InsideRadius);
                            }
                            break;
                        case Shape.Rect:
                            {
                                //m_InsideRadius = ((float)m_CurWheelCfg.Shape.Rect.Length / 100) * ratio;
                                m_InsideRadius = (float)(((m_CurEneryWheelCfg.Shape.Rect.Length - m_CurWheelCfg.Shape.Rect.Length) * ratio) + m_CurWheelCfg.Shape.Rect.Length) / 100;
                                Skill_Dir.SetRectWidthAndLength(m_OuterRadius, m_InsideRadius);
                            }
                            break;
                        case Shape.RotRoute:
                        case Shape.InputTarget:
                            {
                            }
                            break;
                        default:
                            break;
                    }
                }

                if (energyedCount >= energyMaxNum)
                {
                    // 已经>=了最大蓄力层数
                    m_isShowEnergy = false;
                }
            }
        }

        public void Release()
        {
            if (m_entityCtrl != null)
            {
                if (m_entityCtrl.Data.isMainPlayer)
                {
                    m_SkillDispatcher.SkillController.ActionOnEnergyStart -= OnActionOnEnergyStart;
                    m_SkillDispatcher.SkillController.ActionOnEnergyEnd -= OnActionOnEnergyEnd;
                    m_SkillDispatcher.SkillController.ActionOnEndSkillStage -= OnActionOnEndSkillStage;
                }
            }
            currentTriggetNtts.Clear();
            m_SkillTargetLastPos = Vector3.zero;
            ReleaseEnergyData();
            GameObject.Destroy(gameObject);
        }

        public void SetFlashHide(bool isHide)
        {
            gameObject.SetActive(isHide);
        }

        #region 技能委托
        /// <summary>
        /// 服务器通知--怪物、其他人的使用蓄力技能
        /// </summary>
        /// <param name="skillUnit"></param>
        private void OnActionOnEnergyStart(SkillEntity skillEntity)
        {
            if (skillEntity == null)
            {
                return;
            }
            m_skillEntity = skillEntity;

            // 能找到就用，，找不到就用默认的
            effectParam = m_skillEntity.GetUserInput();
            m_CurSkillCfg = (m_entityCtrl.M_Curr as NPCEntityBase).skillDispatcher.SkillController.GetSkillWheelInfo(m_skillEntity.skillId);
            CastMethodType castMethodType = m_CurSkillCfg.CastMethod;
            dynamicRangeType = m_CurSkillCfg.DynamicRangeType;
            m_CurWheelCfg = m_CurSkillCfg.WheelCfg;
            m_CurlcontrolType = m_CurWheelCfg.Shape.ShapeType;
            m_CurWheelPange = m_CurSkillCfg.WheelRange;
            if (effectParam != null && (effectParam.BaseEffect as EffectTypeChargeInput) != null)
            {
                var chargeInput = effectParam.BaseEffect as EffectTypeChargeInput;
                m_CurEneryWheelCfg = chargeInput.WheelCfg;
                m_CurEneryWheelPange = chargeInput.WheelRange;
            }
            else
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} OnActionOnEnergyStart : entityId={m_entityCtrl.Data.M_EntityID},skillId={m_skillEntity.skillId},effectParam=null err!!!");
                HideEnergySkilllcontrol(skillEntity);
                return;
            }

            if (castMethodType != CastMethodType.GatherWheelCast)
            {
                HideEnergySkilllcontrol(skillEntity);
                return;
            }

            HideSkilllcontrol();
            energyTime = m_skillEntity.EnergyItemTime;
            energyMaxNum = m_skillEntity.EnergyMaxCount;
            energyedCount = m_skillEntity.GetCurEnergyCount();
            curEnergyTime = m_skillEntity.GetCurEnergyTime();

            // 设置指示器规格
            switch (m_CurlcontrolType)
            {
                case Shape.Round:
                    {
                        //m_InsideRadius = 0;
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.Round.Radius / 100;
                    }
                    break;
                case Shape.HollowCircle:
                    {
                        //m_InsideRadius = 0;
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.HollowCircle.MaxRadius / 100;
                        m_HollowCircleRadius = (float)m_CurWheelCfg.Shape.HollowCircle.MinRadius / 100;
                    }
                    break;
                case Shape.Sector:
                    {
                        //m_InsideRadius = 0;
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.Sector.Radius / 100;
                        m_Angle = m_CurWheelCfg.Shape.Sector.Angle;
                    }
                    break;
                case Shape.RingFan:
                    {
                        //m_InsideRadius = 0;
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.RingFan.MaxRadius / 100;
                        m_HollowCircleRadius = (float)m_CurWheelCfg.Shape.RingFan.MinRadius / 100;
                        m_Angle = m_CurWheelCfg.Shape.RingFan.Angle;
                    }
                    break;
                case Shape.Rect:
                    {
                        //m_InsideRadius = 0;
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.Rect.Length / 100;
                        m_CubeWidth = (float)m_CurWheelCfg.Shape.Rect.Width / 100;
                    }
                    break;
                case Shape.Arrow:
                    {
                        //m_InsideRadius = 0;
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.Arrow.Length / 100;
                        m_CubeWidth = (float)m_CurWheelCfg.Shape.Arrow.Width / 100;
                    }
                    break;
                case Shape.RotRoute:
                case Shape.InputTarget:
                    {
                    }
                    break;
                default:
                    break;
            }

            m_OuterRadius = (float)m_CurWheelPange.MaxRadius / 100;
            m_isShowEnergy = true;
            SkillAreaController(m_CurlcontrolType, true);
        }
        /// <summary>
        /// 蓄力技能结束
        /// </summary>
        /// <param name="num">层数</param>
        private void OnActionOnEnergyEnd(SkillEntity skillEntity)
        {
            HideEnergySkilllcontrol(skillEntity);
        }
        /// <summary>
        /// 服务器通知-技能结束
        /// </summary>
        private void OnActionOnEndSkillStage(SkillEntity skillEntity, E_SkillExitType e_SkillExitType)
        {
            HideEnergySkilllcontrol(skillEntity);
        }
        private void ReleaseEnergyData()
        {
            m_isShowEnergy = false;
            m_skillEntity = null;
            effectParam = null;
            m_CurEneryWheelCfg = null;
            m_CurEneryWheelPange = null;
        }
        #endregion

        /// <summary>
        /// 按下--显示指示器
        /// </summary>
        /// <param name="skillUnit"></param>
        public void ShowlcontrolById(SkillContainer skillUnit)
        {
            if (skillUnit == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} ShowlcontrolById skillUnit=null");
                return;
            }
            SkillWheelInfo skillWheelInfo = (m_entityCtrl.M_Curr as NPCEntityBase).skillDispatcher.SkillController.GetSkillWheelInfo(skillUnit.CurSkillId);
            if (skillWheelInfo == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} ShowlcontrolById skillWheelInfo=null");
                return;
            }
            // 呼出轮盘的才会显示
            if (skillWheelInfo.CastMethod != CastMethodType.WheelCast && skillWheelInfo.CastMethod != CastMethodType.GatherWheelCast)
            {
                //SGF.Debuger.LogWarning($"{LOG_TAG} ShowlcontrolById skillUnit.CurSkillId={skillUnit.CurSkillId},CastMethod={skillUnit.CurSkillInfo.cfg.CastMethod}");
                return;
            }
            if (skillWheelInfo.WheelCfg.Shape.ShapeType == Shape.None)
            {
                //SGF.Debuger.LogWarning($"{LOG_TAG} ShowlcontrolById skillUnit.CurSkillId={skillUnit.CurSkillId},ShapeType={skillUnit.CurSkillInfo.cfg.WheelCfg.Shape.ShapeType}");
                return;
            }

            m_CurSkillPos = skillUnit.skillPos.PosID;
            m_CurSkillCfg = skillWheelInfo;
            m_CurWheelCfg = m_CurSkillCfg.WheelCfg;
            m_CurWheelPange = m_CurSkillCfg.WheelRange;
            m_CurlcontrolType = m_CurWheelCfg.Shape.ShapeType;
            m_SkillInputType = m_CurSkillCfg.SkillInputType;
            HideSkilllcontrol();
            // 设置指示器规格
            switch (m_CurlcontrolType)
            {
                case Shape.Round:
                    {
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.Round.Radius / 100;
                    }
                    break;
                case Shape.HollowCircle:
                    {
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.HollowCircle.MaxRadius / 100;
                        m_HollowCircleRadius = (float)m_CurWheelCfg.Shape.HollowCircle.MinRadius / 100;
                    }
                    break;
                case Shape.Sector:
                    {
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.Sector.Radius / 100;
                        m_Angle = m_CurWheelCfg.Shape.Sector.Angle;
                    }
                    break;
                case Shape.RingFan:
                    {
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.RingFan.MaxRadius / 100;
                        m_HollowCircleRadius = (float)m_CurWheelCfg.Shape.RingFan.MinRadius / 100;
                        m_Angle = m_CurWheelCfg.Shape.RingFan.Angle;
                    }
                    break;
                case Shape.Arrow:
                    {
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.Arrow.Length / 100;
                        m_CubeWidth = (float)m_CurWheelCfg.Shape.Arrow.Width / 100;
                    }
                    break;
                case Shape.Rect:
                    {
                        m_InsideRadius = (float)m_CurWheelCfg.Shape.Rect.Length / 100;
                        m_CubeWidth = (float)m_CurWheelCfg.Shape.Rect.Width / 100;
                    }
                    break;
                case Shape.RotRoute:
                case Shape.InputTarget:
                    {
                    }
                    break;
                default:
                    break;
            }
            m_OuterRadius = (float)m_CurWheelPange.MaxRadius / 100;
            m_isShowEnergy = false;
            TargetSelect = SelectType.Enemy;
            // 设置技能选取的目标阵容
            foreach (SkillCondition skillCondition in skillUnit.CurShowSkillInfo.cfg.Conditions)
            {
                ConditionTypeSerialize condition = skillCondition.ConditionParams;
                ConditionType conditionType = condition.ConditionType;
                if (conditionType == ConditionType.Cond_TargetFaction)
                {
                    TargetSelect = condition.Cond_TargetFaction.TargetSelect;
                    break;
                }
            }
            SkillAreaController(m_CurlcontrolType, true);
            // 指示器显示前优先从锁定的敌人位置出现
            DefaultAutoShowDir(skillUnit);
            SGF.Debuger.Log($"{LOG_TAG} ShowlcontrolById skillUnit.CurSkillId={skillUnit.CurSkillId},m_currentShowlcontrolType={m_CurlcontrolType}");
        }

        /// <summary> 计算指示器大小 </summary>
        private float GetControlRatio()
        {
            float ratio = 1;
            if (m_CurSkillCfg == null)
            {
                return ratio;
            }
            // 蓄力时间影响
            if (dynamicRangeType == DynamicRangeType.ChargeTime)
            {
                //2、蓄力时间影响：
                // 当前蓄力时间 /（蓄力阶段最大蓄力次数*单次蓄力生效周期时间）---  是取最大有效蓄力时间 & 不可超过100%                                                                         
                //碰撞盒范围 = 碰撞盒范围数值参数 * 当前蓄力时间 /（蓄力阶段最大蓄力次数*单次蓄力生效周期时间）
                ratio = Mathf.Min((float)curEnergyTime / (energyMaxNum * energyTime), 1f);
                ////float radiusRatio = Mathf.Max(outerRadius * ratio, ((float)curSkillCfg.GetDynamicInitRange() / 100));
                //float radiusRatio = m_OuterRadius * ratio;
                //m_InsideRadius = radiusRatio;
                ////SGF.Debuger.Log($"{LOG_TAG} 碰撞盒范围 GetControl outerRadius={outerRadius},beishu={ratio},m_outerRadius={m_outerRadius},curEnergyTime={curEnergyTime}");
            }
            // 蓄力层数影响
            else if (dynamicRangeType == DynamicRangeType.ChargeNum)
            {
                //3、蓄力层数影响：
                //碰撞盒范围=碰撞盒范围数值参数*当前蓄力层数/最大蓄力次数
                float curEnergyNumRatio = energyedCount / (float)energyMaxNum;
                ratio = Mathf.Min(curEnergyNumRatio, 1f);
                ////float radiusRatio = Mathf.Max(outerRadius * ratio, ((float)curSkillCfg.GetDynamicInitRange() / 100));
                //float radiusRatio = m_OuterRadius * ratio;
                //m_InsideRadius = radiusRatio;
                ////SGF.Debuger.Log($"{LOG_TAG} 碰撞盒范围 GetControl outerRadius={outerRadius},beishu={ratio},m_outerRadius={m_outerRadius},energyedCount={energyedCount}");
            }
            return ratio;
        }

        private void DefaultAutoShowDir(SkillContainer skillUnit)
        {
            if (skillUnit == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} DefaultAutoShowDir skillUnit=null");
                return;
            }
            if (m_CurSkillCfg != null && m_CurSkillPos != skillUnit.skillPos.PosID)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} DefaultAutoShowDir m_CurSkillPos={m_CurSkillPos}[!=]PosID={skillUnit.skillPos.PosID}");
                return;
            }
            if (m_CurlcontrolType == Shape.None)
            {
                //SGF.Debuger.LogError($"{LOG_TAG} DefaultAutoShowDir m_currentShowlcontrolType=null");
                return;
            }

            Vector3 mainPlayerCurPos = m_entityCtrl.M_Curr.Position();
            NPCEntityBase curAtkEntity = BattleManager.Instance.CurAtkEntity;
            float maxRadius = (float)m_CurSkillCfg.WheelRange.MaxRadius / 100;    // 最大锁敌半径
            bool isClosestTarget = true;    // 是否找目标
            if (curAtkEntity != null)
            {
                // 判断和敌方的距离
                Vector3 curAtkEntityIdPos = curAtkEntity.Position();
                Vector3 dirInterpolation = curAtkEntityIdPos - mainPlayerCurPos;
                float distance = dirInterpolation.magnitude;
                isClosestTarget = distance > maxRadius;
                // 判断阵营是否是敌对阵营
                bool isTargetNtt = EntityFactoryUtils.CheckIsTriggleAOIEntity(curAtkEntity, m_entityCtrl.M_Curr.EntityId, m_entityCtrl.M_Curr.Faction, TargetSelect);
                //说明实体类型不是目标类型
                if (!isTargetNtt)
                {
                    curAtkEntity = null;
                }
            }
            if (isClosestTarget)
            {
                curAtkEntity = null;
                // 找距离最近的敌人---追击目标
                if (m_CurSkillCfg != null && m_entityCtrl.M_Curr != null)
                {
                    // 先判断 是否有正在运行的 输入轴的效果,如果有,就用输入轴的 轮盘配置
                    //SkillWheelInfo skillWheelInfo = (m_entityCtrl.M_Curr as NPCEntityBase).skillDispatcher.SkillController.GetRunningSkillWheelInfo(m_CurSkillCfg.ID);

                    int curWheelMaxRadius = m_CurSkillCfg.WheelRange.MaxRadius;

                    //// 如果 输入轴的 轮盘配置存在，就采用 输入轴 中配置的 最大范围
                    //if (skillWheelInfo != null)
                    //{
                    //    curWheelMaxRadius = skillWheelInfo.WheelRange.MaxRadius;
                    //}
                    curAtkEntity = SkillUtils.GetClosestTargetIdByTargets(m_entityCtrl.M_Curr.EntityId, m_entityCtrl.M_Curr.Faction, mainPlayerCurPos, curWheelMaxRadius, TargetSelect);
                }
            }
            if (curAtkEntity == null)
            {
                return;
            }

            Vector3 dir = Vector3.zero;
            Vector3 targetPos = curAtkEntity.Position();
            dir = (targetPos - mainPlayerCurPos).normalized;
            if (dir.magnitude > 0)
            {
                Vector3 VirtualCameraForward = CameraManager.Instance.GetPlayerCameraAnglesY();
                dir = Quaternion.Euler(0, VirtualCameraForward.y, 0) * dir;
            }

            Movelcontrol(skillUnit, dir, targetPos);
        }

        /// <summary>
        /// 移动指示器
        /// </summary>
        /// <param name="dir"> 方向向量 </param>
        public void Movelcontrol(SkillContainer skillUnit, Vector3 dir, Vector3 targetPos)
        {
            if (skillUnit == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} Movelcontrol skillUnit=null");
                return;
            }
            if (m_CurSkillCfg != null && m_CurSkillPos != skillUnit.skillPos.PosID)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} Movelcontrol m_CurSkillPos={m_CurSkillPos}[!=]PosID={skillUnit.skillPos.PosID}");
                return;
            }
            if (m_CurlcontrolType == Shape.None)
            {
                //SGF.Debuger.LogError($"{LOG_TAG} Movelcontrol m_currentShowlcontrolType=null");
                return;
            }
            if (dir.magnitude > 0)
            {
                Vector3 VirtualCameraForward = CameraManager.Instance.GetPlayerCameraAnglesY();
                dir = Quaternion.Euler(0, VirtualCameraForward.y, 0) * dir;
            }
            // 外圆会一直跟着走
            // 下面设置其他指示器类型的位置
            switch (m_CurlcontrolType)
            {
                case Shape.Round:
                    MoveSkillInner(dir, targetPos);
                    break;
                case Shape.HollowCircle:
                    MoveSkillInner(dir, targetPos);
                    break;
                case Shape.Sector:
                    MoveSkillSector(dir, targetPos);
                    break;
                case Shape.RingFan:
                    MoveSkillRingFan(dir, targetPos);
                    break;
                case Shape.Arrow:
                    MoveSkillArrow(dir, targetPos);
                    break;
                case Shape.Rect:
                    MoveSkillDir(dir, targetPos);
                    break;
                case Shape.RotRoute:
                    // 路径不知道要做成啥样
                    break;
                case Shape.InputTarget:
                    MoveSkillTarget(dir, targetPos);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 隐藏技能指示器
        /// </summary>
        public void HideSkilllcontrol(SkillContainer skillUnit)
        {
            if (skillUnit == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} HideSkilllcontrol skillUnit=null");
                return;
            }
            if (m_CurSkillCfg != null && m_CurSkillPos != skillUnit.skillPos.PosID)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} HideSkilllcontrol m_CurSkillPos={m_CurSkillPos}[!=]PosID={skillUnit.skillPos.PosID}");
                return;
            }
            if (m_CurlcontrolType == Shape.None)
            {
                //SGF.Debuger.LogError($"{LOG_TAG} HideSkilllcontrol m_currentShowlcontrolType=null,,skillUnit.CurSkillId={skillUnit.CurSkillId}");
                return;
            }
            SkillAreaController(m_CurlcontrolType, false);
            HideSkilllcontrol();
            SGF.Debuger.Log($"{LOG_TAG} HideSkilllcontrol skillUnit.CurSkillId={skillUnit.CurSkillId},m_currentShowlcontrolType={m_CurlcontrolType}");
            m_CurlcontrolType = Shape.None;
            m_CurSkillPos = -1;
            m_CurSkillCfg = null;
            m_CurWheelPange = null;
            m_CurWheelCfg = null;
            TargetSelect = SelectType.Enemy;
        }
        /// <summary>
        /// 隐藏蓄力技能指示器
        /// </summary>
        /// <param name="skillEntity"></param>
        public void HideEnergySkilllcontrol(SkillEntity skillEntity)
        {
            if (m_skillEntity != null && skillEntity != null && skillEntity.RuntimeID == m_skillEntity.RuntimeID)
            {
                ReleaseEnergyData();
                if (m_CurlcontrolType == Shape.None)
                {
                    return;
                }
                SkillAreaController(m_CurlcontrolType, false);
                HideSkilllcontrol();
                m_CurlcontrolType = Shape.None;
                m_CurSkillPos = -1;
                m_CurSkillCfg = null;
                m_CurWheelCfg = null;
                m_CurWheelPange = null;
                TargetSelect = SelectType.Enemy;
            }
        }

        #region 移动指示器

        private void MoveSkillInner(Vector3 dir, Vector3 targetPos)
        {
            if (!Skill_InnerArea.transform.gameObject.activeInHierarchy)
            {
                SkillAreaController(Shape.HollowCircle, true);
            }
            else
            {
                if (targetPos == Vector3.zero)
                {
                    m_TempPos.x = dir.x * m_OuterRadius;
                    m_TempPos.z = dir.z * m_OuterRadius;
                    targetPos = m_entityCtrl.M_Curr.Position() + m_TempPos;
                }
                targetPos.y = 0f;
                Skill_InnerArea.transform.position = targetPos;
                Vector3 localPos = Skill_InnerArea.transform.localPosition;
                localPos.y = 0f;
                Skill_InnerArea.transform.localPosition = localPos;
            }
        }

        private void MoveSkillDir(Vector3 dir, Vector3 targetPos)
        {
            if (!Skill_Dir.transform.gameObject.activeInHierarchy)
            {
                SkillAreaController(Shape.Rect, true);
            }
            else
            {
                m_TempAngles.y = (float)(Math.Atan2(-dir.z, dir.x) * 180 / Math.PI) + 90;
                Skill_Dir.transform.eulerAngles = m_TempAngles;
                if (m_SkillInputType == SkillInputType.PosInput)
                {
                    if (targetPos == Vector3.zero)
                    {
                        m_TempPos.x = dir.x * m_OuterRadius;
                        m_TempPos.z = dir.z * m_OuterRadius;
                        targetPos = m_entityCtrl.M_Curr.Position() + m_TempPos;
                    }
                    targetPos.y = 0f;
                    Skill_Dir.transform.position = targetPos;
                    Vector3 localPos = Skill_Dir.transform.localPosition;
                    localPos.y = 0f;
                    Skill_Dir.transform.localPosition = localPos;
                }

                //SGF.Debuger.Log($"矩形右朝向  节点  前 = {Skill_Dir.transform.forward} 右 = {Skill_Dir.transform.right} ");
                //VectorAngle(transform.forward, Skill_Dir.transform.forward);
            }
        }

        private void MoveSkillArrow(Vector3 dir, Vector3 targetPos)
        {
            if (!Skill_Arrow.transform.gameObject.activeInHierarchy)
            {
                SkillAreaController(Shape.Arrow, true);
            }
            else
            {
                m_TempAngles.y = (float)(Math.Atan2(-dir.z, dir.x) * 180 / Math.PI) + 90;
                Skill_Arrow.transform.eulerAngles = m_TempAngles;
                if (m_SkillInputType == SkillInputType.PosInput)
                {
                    if (targetPos == Vector3.zero)
                    {
                        m_TempPos.x = dir.x * m_OuterRadius;
                        m_TempPos.z = dir.z * m_OuterRadius;
                        targetPos = m_entityCtrl.M_Curr.Position() + m_TempPos;
                    }
                    targetPos.y = 0f;
                    Skill_Arrow.transform.position = targetPos;
                    Vector3 localPos = Skill_Dir.transform.localPosition;
                    localPos.y = 0f;
                    Skill_Arrow.transform.localPosition = localPos;
                }

                //SGF.Debuger.Log($"矩形右朝向  节点  前 = {Skill_Dir.transform.forward} 右 = {Skill_Dir.transform.right} ");
                //VectorAngle(transform.forward, Skill_Dir.transform.forward);
            }
        }

        private void MoveSkillSector(Vector3 dir, Vector3 targetPos)
        {
            if (!Skill_Sector.transform.gameObject.activeInHierarchy)
            {
                SkillAreaController(Shape.Sector, true);
            }
            else
            {
                m_TempAngles.y = (float)(Math.Atan2(-dir.z, dir.x) * Mathf.Rad2Deg) + 90;
                Skill_Sector.transform.eulerAngles = m_TempAngles;
                if (m_SkillInputType == SkillInputType.PosInput)
                {
                    if (targetPos == Vector3.zero)
                    {
                        m_TempPos.x = dir.x * m_OuterRadius;
                        m_TempPos.z = dir.z * m_OuterRadius;
                        targetPos = m_entityCtrl.M_Curr.Position() + m_TempPos;
                    }
                    targetPos.y = 0f;
                    Skill_Sector.transform.position = targetPos;
                    Vector3 localPos = Skill_Dir.transform.localPosition;
                    localPos.y = 0f;
                    Skill_Sector.transform.localPosition = localPos;
                }
                //if (Arror == null)
                //{
                ////    Arror = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/3") as GameObject);
                //    Arror = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/3");
                //    Arror.name = "NextArror11111111111111";
                //}
                //Arror.transform.position = m_entityCtrl.M_Curr.Position();
                //Arror.transform.eulerAngles = Skill_Area_60.transform.eulerAngles;
                //VectorAngle(transform.forward, Skill_Area_60.transform.forward);
            }
        }

        private void MoveSkillRingFan(Vector3 dir, Vector3 targetPos)
        {
            if (!Skill_RingFan.transform.gameObject.activeInHierarchy)
            {
                SkillAreaController(Shape.Sector, true);
            }
            else
            {
                m_TempAngles.y = (float)(Math.Atan2(-dir.z, dir.x) * 180 / Math.PI) + 90;
                Skill_RingFan.transform.eulerAngles = m_TempAngles;
                if (m_SkillInputType == SkillInputType.PosInput)
                {
                    if (targetPos == Vector3.zero)
                    {
                        m_TempPos.x = dir.x * m_OuterRadius;
                        m_TempPos.z = dir.z * m_OuterRadius;
                        targetPos = m_entityCtrl.M_Curr.Position() + m_TempPos;
                    }
                    targetPos.y = 0f;
                    Skill_RingFan.transform.position = targetPos;
                    Vector3 localPos = Skill_Dir.transform.localPosition;
                    localPos.y = 0f;
                    Skill_RingFan.transform.localPosition = localPos;
                }
                //VectorAngle(transform.forward, Skill_Area_120.transform.forward);
            }
        }

        private void MoveSkillTarget(Vector3 dir, Vector3 targetPos)
        {
            if (!Skill_Target.activeInHierarchy)
            {
                SkillAreaController(Shape.InputTarget, true);
            }
            else
            {
                if (targetPos == Vector3.zero)
                {
                    m_TempPos.x = dir.x * m_OuterRadius;
                    m_TempPos.z = dir.z * m_OuterRadius;
                    targetPos = m_entityCtrl.M_Curr.Position() + m_TempPos;
                }
                Skill_Target.transform.position = targetPos;

                if ((m_SkillTargetLastPos - Skill_Target.transform.position).magnitude > 0.05f)
                {
                    m_SkillTargetLastPos = Skill_Target.transform.position;
                    currentTriggetNtts.Clear();
                    SkillUtils.GetTargetInCircle(ref currentTriggetNtts, m_entityCtrl.Data.M_EntityID, m_SkillTargetLastPos, 1f, m_entityCtrl.M_Curr.Faction, SelectType.Enemy);
                    Skill_Target.transform.GetChild(0).GetChild(0).gameObject.SetActive(currentTriggetNtts.Count > 0);
                }
            }
        }

        #endregion


        /// <summary>
        /// 技能区域控制器
        /// </summary>
        /// <param name="areaType">技能类型</param>
        /// <param name="skillIsActive">是否显示</param>
        private void SkillAreaController(Shape areaType, bool skillIsActive)
        {
            //Debug.Log($"指示器 SkillAreaController ----开始------areaType={areaType}--{skillIsActive} time={TimeUtils.TimeLogString()}");

            Skill_Area.SetShapeActive(skillIsActive);
            if (skillIsActive)
            {
                //Skill_Area.SetCircleRange(m_OuterRadius);
                Skill_Area.SetSectorRange(m_OuterRadius);
            }
            //if (skillIsActive)
            //{
            //    m_localScale.x = m_OuterRadius * 2;
            //    m_localScale.y = m_OuterRadius * 2;
            //    m_localScale.z = m_OuterRadius * 2;
            //}
            //else
            //{
            //    m_localScale.x = 1;
            //    m_localScale.y = 1;
            //    m_localScale.z = 1;
            //}
            //Skill_Area.transform.localScale = m_localScale;
            switch (areaType)
            {
                case Shape.Round:
                    {
                        Skill_InnerArea.SetShapeActive(skillIsActive);
                        if (skillIsActive)
                        {
                            //Skill_InnerArea.SetCircleRange(m_InsideRadius);
                            Skill_InnerArea.SetSectorRange(m_InsideRadius);
                        }
                        Skill_InnerArea.transform.localPosition = Vector3.zero;
                    }
                    break;
                case Shape.HollowCircle:
                    {
                        Skill_InnerArea.SetShapeActive(skillIsActive);
                        if (skillIsActive)
                        {
                            //Skill_InnerArea.SetCircleRange(m_InsideRadius);
                            Skill_InnerArea.SetSectorRange(m_InsideRadius);
                        }
                        Skill_InnerArea.transform.localPosition = Vector3.zero;
                    }
                    break;
                case Shape.Sector:
                    {
                        if (skillIsActive)
                        {
                            Skill_Sector.SetSectorRange(m_InsideRadius);
                            Skill_Sector.SetSectorAngle((float)m_Angle * 2);
                        }
                        Skill_Sector.transform.localEulerAngles = m_entityCtrl.M_Curr.EulerAngles;
                        Skill_Sector.transform.localPosition = Vector3.zero;
                        Skill_Sector.SetShapeActive(skillIsActive);
                    }
                    break;
                case Shape.RingFan:
                    {
                        if (skillIsActive)
                        {
                            float maxRadius = m_InsideRadius;
                            Skill_RingFan.SetRingFanRange(maxRadius);
                            Skill_RingFan.SetRingFanAngle(m_Angle);
                            float minRadius = m_HollowCircleRadius;
                            float middleRing = minRadius / maxRadius;
                            Skill_RingFan.SetRingFanMiddleRingSize(middleRing);
                        }
                        Skill_RingFan.transform.localEulerAngles = m_entityCtrl.M_Curr.EulerAngles;
                        Skill_RingFan.transform.localPosition = Vector3.zero;
                        Skill_RingFan.SetShapeActive(skillIsActive);
                    }
                    break;
                case Shape.Arrow:
                    {
                        if (skillIsActive)
                        {
                            Skill_Arrow.SetRectLength(m_InsideRadius);
                            Skill_Arrow.SetRectRange(m_CubeWidth);
                        }
                        Skill_Arrow.SetShapeActive(skillIsActive);
                        Skill_Arrow.transform.localPosition = Vector3.zero;
                        Skill_Arrow.transform.localEulerAngles = m_entityCtrl.M_Curr.EulerAngles;
                    }
                    break;
                case Shape.Rect:
                    {
                        if (skillIsActive)
                        {
                            // TA做的时候，参数给反了，上面的是正确的，下面的错误的（先用错误的）
                            //Skill_Dir.SetRectRange(m_InsideRadius);
                            //Skill_Dir.SetRectLength(m_CubeWidth);
                            Skill_Dir.SetRectWidthAndLength(m_CubeWidth, m_InsideRadius);
                        }
                        Skill_Dir.SetShapeActive(skillIsActive);
                        Skill_Dir.transform.localPosition = Vector3.zero;
                        Skill_Dir.transform.localEulerAngles = m_entityCtrl.M_Curr.EulerAngles;
                    }
                    break;
                case Shape.RotRoute:
                    // 路径不知道要做成啥样
                    break;
                case Shape.InputTarget:
                    {
                        // 目标不知道要做成啥样
                        if (Skill_Target)
                        {
                            m_localScale = Vector3.one;
                        }
                        Skill_Target.SetActive(skillIsActive);
                        Skill_Target.transform.localPosition = Vector3.zero;
                        Skill_Target.transform.localEulerAngles = m_entityCtrl.M_Curr.EulerAngles;
                        Skill_Target.transform.localScale = m_localScale;
                        Skill_Target.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    }
                    break;
                default:
                    break;
            }

            //Debug.Log($"指示器 SkillAreaController ----结束------areaType={areaType}--{skillIsActive} time={TimeUtils.TimeLogString()}");
        }

        /// <summary>
        /// 计算夹角 -180到180 顺时针为正数
        /// </summary> 
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void VectorAngle(UnityEngine.Vector3 from, UnityEngine.Vector3 to)
        {
            // 其实直接那指示器的Y轴角度就好
            // UnityEngine.Vector3 n = UnityEngine.Vector3.up;
            // m_angle = Mathf.Atan2(UnityEngine.Vector3.Dot(n, UnityEngine.Vector3.Cross(from, to)), UnityEngine.Vector3.Dot(from, to)) * Mathf.Rad2Deg;
        }

        public void HideSkilllcontrol()
        {
            if (m_Skill_Area != null)
            {
                Skill_Area.SetShapeActive(false);
            }
            if (m_Skill_Dir != null)
            {
                Skill_Dir.SetShapeActive(false);
            }
            if (m_Skill_Sector != null)
            {
                Skill_Sector.SetShapeActive(false);
            }
            if (m_Skill_RingFan != null)
            {
                Skill_RingFan.SetShapeActive(false);
            }
            if (m_Skill_InnerArea != null)
            {
                Skill_InnerArea.SetShapeActive(false);
            }
            if (m_Skill_Target != null)
            {
                Skill_Target.SetActive(false);
            }
        }
    }
}