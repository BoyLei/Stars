using Google.Protobuf;
using SGF.Network;
using SGF.Time;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Skill;
using StarProject.Game.Skill.Utils;
using StarProject.Service.Battle;
using StarProject.Service.Cam;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.UI.SkillBtn;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Player.Component
{

    public class SkillComponent
    {
        private string LOG_TAG = "[SkillComponent]";
        private const string m_modelPath = "UI/StarWorld/Prefab/SkillBtn";

        private EntityCtrlBase m_entityCtrl;
        private GameContext m_context;
        private NPCEntityBase m_NPCEntityBase;
        private EntityCtrlBase m_hostEntityCtrl;
        private NPCEntityBase m_hostNPCEntityBase;


        public SkillComponent()
        {
            m_context = GameManager.Instance.Context;

            M_BattleSocket = NetworkManager.Instance.gameSocket;
            skillMoveMsg = new ProtoMsg.Vector3(); // 使用技能坐标
            rawMsg = new ProtoMsg.RawMsg();     // 使用技能坐标输入数据
            ProtpMoveMsg2 = new ProtoMsg.MoveMsg2();
            protoVector3 = new ProtoMsg.Vector3();
            dateTimeOffset = new DateTimeOffset(DateTime.UtcNow);
        }

        /// <summary>
        /// TODO : 曲
        /// 按钮的逻辑 跟 EntityCtrlBase  绑定太死了
        /// 应该 将 EntityCtrlBase  与 按钮解绑, 按钮只需要根据 skillList 做相应的显示即可.
        /// 至于 要获取的 EntityCtrlBase 数据, 在需要的时候,通过 gameManager 来获取
        /// 
        /// note: 伙伴的技能按钮组件
        ///     曲的按钮(SkillComponent) 跟 EntityCtrlGroup 耦合的太死.
        ///     而对于 伙伴的 技能按钮而言, 玩家在create的时候, 伙伴的AOI实体并不一定创建。
        ///     而伙伴的技能 按钮, 是在 playerCtrlGroup 中创建, 由多个伙伴共同刷新.
        ///     好的做法, 是将 技能按钮 跟 EntityCtrlGroup 分离 , 里面只需要关系 ui的显示 和 点击相关逻辑;
        ///     对于 伙伴技能 这种 1 对 N 的 形式, 提供 update 刷新接口即可
        /// </summary>
        /// <param name="entityCtrl"></param>
        public void Init(EntityCtrlBase entityCtrl, EntityCtrlBase hostEntity = null)
        {
            m_entityCtrl = entityCtrl;
            m_hostEntityCtrl = hostEntity;
            if (m_hostEntityCtrl == null)
            {
                m_hostEntityCtrl = entityCtrl;
            }
            if (m_entityCtrl != null && m_entityCtrl.Data != null)
            {
                LOG_TAG = $"[SkillComponent_{m_entityCtrl.Data.EntityType}_{m_entityCtrl.Data.M_EntityID}]";

                m_NPCEntityBase = (NPCEntityBase)m_entityCtrl.M_Curr;

                m_hostNPCEntityBase = (NPCEntityBase)m_hostEntityCtrl.M_Curr;

                Reset();

                if (GameInput.Instance == null)
                {
                    //SGF.Debuger.Log($"{LOG_TAG} 创建刷新技能按钮 gameinput不存在，先添加委托");
                    GameInput.GameInputFinishAction += OnActionOnRefreshAllSkill;
                }
                else
                {
                    OnActionOnRefreshAllSkill();
                }

                if (m_entityCtrl.Data.isMainPlayer)
                {
                    layerMask = GroundLayer;
                    UnRegAllTagChangeListeners();
                    RegChangeListener();
                }
            }
        }

        private int groundLayer = -1;
        public int GroundLayer //这里是根据unity实际转化int值
        {
            get
            {
                if (groundLayer == -1)
                {
                    groundLayer = LayerMask.NameToLayer(E_LayerType.Ground.ToString());
                }
                return groundLayer;
            }
        }

        #region 屏幕点击释放技能
        private void RegChangeListener()
        {
            if (m_entityCtrl == null)
            {
                return;
            }
            if (m_entityCtrl.Data == null)
            {
                return;
            }
            m_entityCtrl.Data.RegChangeListener(GameConfig.TOUCH_USE_SKILL_EVENT, OnRegChangeListener, this.GetHashCode().ToString());

            // 注册 原子状态 属性的 变化通知 
            m_entityCtrl.Data.RegisterAttribute(AOIAttrDefine.State, OnAOIStateChange);
        }

        private void UnRegAllTagChangeListeners()
        {
            if (m_entityCtrl == null)
            {
                return;
            }
            if (m_entityCtrl.Data == null)
            {
                return;
            }
            m_entityCtrl.Data.UnRegAllTagChangeListeners(this.GetHashCode().ToString());

            m_entityCtrl.Data.UnRegisterAttribute(AOIAttrDefine.State, OnAOIStateChange);

        }

        private void OnRegChangeListener(object v)
        {
            if (v == null)
            {
                return;
            }
            TriggerTypeEffectData data = (TriggerTypeEffectData)v;
            KeyValuePair<int, Vector3> specializedData = (KeyValuePair<int, Vector3>)data.Value;
            switch (data.Event)
            {
                case GameConfig.TOUCH_USE_SKILL_EVENT:
                    {
                        SendSkill(specializedData.Key, specializedData.Value);
                    }
                    break;
                default:
                    break;
            }
        }

        private Camera _BattleCamera;
        public Camera BattleCamera
        {
            get
            {
                if (_BattleCamera == null)
                {
                    _BattleCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).GetComponent<Camera>();
                }
                return _BattleCamera;
            }
        }
        private LayerMask layerMask;
        private void SendSkill(int skillId, Vector3 pos)
        {
            Vector3 scenePos = Vector3.zero;
            Ray ray = BattleCamera.ScreenPointToRay(pos);
            RaycastHit raycast;
            if (Physics.Raycast(ray, out raycast, 100, layerMask))
            {
                //Debug.Log($"通过修改Z轴获取到的世界坐标是 222222 {raycast.point}");
                scenePos = raycast.point;
            }

            // 获取技能的配置
            LocalDataManager.Instance.GetSkillJson(skillId, (SkillJson skillJson) =>
            {
                if (skillJson == null)
                {
                    return;
                }

                Vector3 skillPos = m_NPCEntityBase.Position();

                ProtoMsg.SkillUseReq skillUseReq = new();
                skillUseReq.BlackList.Clear();
                Google.Protobuf.Collections.RepeatedField<global::ProtoMsg.BlackBoardNode> blackList = new();
                global::ProtoMsg.BlackBoardNode blackBoardNodeNew = new();
                // 填充数据
                skillUseReq.SkillID = skillId;
                skillMoveMsg.X = skillPos.x;
                skillMoveMsg.Y = skillPos.y;
                skillMoveMsg.Z = skillPos.z;
                skillUseReq.Pos = skillMoveMsg;
                skillUseReq.Rot = m_NPCEntityBase.ServerAngles;

                SkillInputType skillInputType = skillJson.config.SkillInputType;
                ShapeRingFan WheelRange = skillJson.config.WheelRange;   // 轮盘可选范围

                // 技能朝向
                Vector3 curDir = m_NPCEntityBase.M_EntityAnglesDir.normalized;
                if (skillInputType == SkillInputType.DirInput)
                {
                    // 如果技能是朝向输入
                    curDir = (scenePos - m_NPCEntityBase.Position()).normalized;
                }
                int curAngle = (int)(Math.Atan2(curDir.z, curDir.x) * Mathf.Rad2Deg);
                /////////////------------ 技能转向 规则 --------- start
                //bool isAutoTurnToTarget = skillJson.config.IsAutoTurnToTarget;
                ////SGF.Debuger.Log($"{LOG_TAG} 技能 自动转向---111111111111 自动转向={isAutoTurnToTarget}，，目标id={EnityId},,角度={skillUseReq.Rot},,我的角度={EulerAngles.y}");
                //if (skillJson.config.IsAutoTurnToTarget && GetIsPointerDownSendSkill())
                //{
                //    isAutoTurnToTarget = true;
                //}
                // -----根据输入类型不同，传入不同的黑板数据
                switch (skillInputType)
                {
                    case SkillInputType.DirInput:
                        {
                            // 输入坐标 InputRota
                            global::ProtoMsg.BlackBoardNode blackBoardNode = GetBlackBoardNodeNew(GameConfig.SKILL_INPUTTYPE_DIRINPUT);
                            blackBoardNode.Int32Value = curAngle;   // 锁敌的角度
                            // SGF.Debuger.LogError($"[Rotate] 点击按钮 的 填充的 黑板右摇杆 角度: {curAngle} ");

                            blackList.Add(blackBoardNode);
                        }
                        break;
                    case SkillInputType.PosInput:
                        {
                            // 输入坐标 InputCoord
                            rawMsg.MsgValue = null;
                            rawMsg.MsgID = (uint)(int)MsgIDEnum.Vector3ID;
                            skillMoveMsg.X = scenePos.x;
                            skillMoveMsg.Y = scenePos.y;
                            skillMoveMsg.Z = scenePos.z;
                            rawMsg.MsgValue = skillMoveMsg.ToByteString();
                            global::ProtoMsg.BlackBoardNode blackBoardNode = GetBlackBoardNodeNew(GameConfig.SKILL_INPUTTYPE_POSINPUT);
                            blackBoardNode.RawValue = rawMsg;
                            blackList.Add(blackBoardNode);
                            ////////////////////--------------------- 坐标输入的技能朝向是输入的朝向
                            global::ProtoMsg.BlackBoardNode blackBoardNodeRota = GetBlackBoardNodeNew(GameConfig.SKILL_INPUTTYPE_DIRINPUT);
                            blackBoardNodeRota.Int32Value = curAngle;
                            blackList.Add(blackBoardNodeRota);
                        }
                        break;
                    case SkillInputType.ObjInput:
                        {
                            // 输入目标 InputTarget 
                            // 磊子说了，屏幕输入的技能不会有目标输入的技能配置方式
                            //-------不管
                        }
                        break;
                    default:
                        break;
                }

                try
                {
                    foreach (var item in blackList)
                    {
                        skillUseReq.BlackList.Add(item);
                    }

                    E_UseSkillResult e_UseSkillRes = m_NPCEntityBase.skillDispatcher.SkillController.ClientUseSkill(skillUseReq, skillJson.config.CastMethod, E_BtnInputType.PointerUp, out bool isNormalSkill);
                    if (e_UseSkillRes == E_UseSkillResult.Succeed)
                    {
                        //SGF.Debuger.Log($"{LOG_TAG} SendUserSkillReq [Succeed] PlayerEnityId={EnityId},skillID={skillUseReq.SkillID},Pos={CurrentPos},sendPos={CurrentPos},anglesY={m_angles.y},rot={curAngle},e_UseSkillRes={e_UseSkillRes}");
                    }
                    else
                    {
                        //SGF.Debuger.LogWarning($"{LOG_TAG} SendUserSkillReq [failure] PlayerEnityId={EnityId},skillID={skillUseReq.SkillID},Pos={CurrentPos},sendPos={CurrentPos},anglesY={m_angles.y},rot={curAngle},e_UseSkillRes={e_UseSkillRes}");
                    }
                    if (!isNormalSkill)
                    {
                        PlayUseSkillTips(e_UseSkillRes, skillUseReq);
                    }
                }
                catch (Exception t)
                {
                    SGF.Debuger.LogError($"{LOG_TAG} SendUserSkillReq blackBoardNode={blackBoardNodeNew},,,,,t={t},Stack={t.StackTrace}");
                }
            }, false);
        }

        #endregion

        public void RomoveSkill(EntityCtrlBase entityCtrl)
        {
            if (m_entityCtrl == null)
            {
                return;
            }

            if (entityCtrl.M_Curr.EntityId != m_entityCtrl.M_Curr.EntityId)
            {
                return;
            }

            RemoveSkill();

            Reset();
        }

        public void EnterFrame(int frameIndex)
        {
            if (m_hostEntityCtrl != null)
            {
                // 技能按下输入释放倒计时
                if (m_currentSkillBtn != null && m_MainPlayerDownSkillBtnTime > 0 && m_isIndicator)
                {
                    m_MainPlayerDownSkillBtnTime -= Time.fixedDeltaTime;

                    if (m_MainPlayerDownSkillBtnTime <= 0)
                    {
                        //SGF.Debuger.LogWarning($"{LOG_TAG} SkillIndicatorView 按下的 skillid={m_currentSkillBtn.skillUnit.CurSkillInfo.cfg.ID}");

                        m_hostEntityCtrl.SkillPointerDownActions?.Invoke(m_currentSkillBtn.skillUnit);
                    }
                }
            }
        }

        public void OnDeadDisPlay()
        {
            if (m_currentSkillBtn != null)
            {
                SetCurSkillInfo(null);
                m_currentSkillBtn.m_IsKeyDown = false;
                m_currentSkillBtn.OnPointerUp(null);
                Reset();
            }
        }

        /// <summary>
        /// 设置当前按钮 使用的 skillInfo. 
        /// note:
        ///     skillCfg 的数据信息太少, 所以 此处 需要保存的是 skillInfo
        /// </summary>
        /// <param name="skillInfo">当前按钮 使用的 skillInfo</param>
        public void SetCurSkillInfo(SkillInfo skillInfo)
        {
            curSkillCfg = null;
            curSkillInfo = skillInfo;
            TargetSelect = SelectType.Enemy;
            if (curSkillInfo != null)
            {
                curSkillCfg = m_NPCEntityBase.skillDispatcher.SkillController.GetSkillWheelInfo(curSkillInfo.skillId);
                // 设置技能选取的目标阵容
                foreach (SkillCondition skillCondition in curSkillInfo.cfg.Conditions)
                {
                    ConditionTypeSerialize condition = skillCondition.ConditionParams;
                    ConditionType conditionType = condition.ConditionType;
                    if (conditionType == ConditionType.Cond_TargetFaction)
                    {
                        TargetSelect = condition.Cond_TargetFaction.TargetSelect;
                        break;
                    }
                }
            }
        }

        public void Reset()
        {
            m_currentSkillBtn = null;
            isEnergyIndicator = false;
            m_isIndicator = false;
            m_MainPlayerDownSkillBtnTime = 0f;
            SetCurSkillInfo(null);
            M_UserSkillStartAngleDir = Vector3.zero;

            skillMoveMsg.X = 0;
            skillMoveMsg.Y = 0;
            skillMoveMsg.Z = 0;

            rawMsg.MsgID = 0;
            rawMsg.MsgValue = ByteString.Empty;

            ProtpMoveMsg2.IsStart = false;
            ProtpMoveMsg2.Rot = 0;
            ProtpMoveMsg2.TimeStamp = 0;

            protoVector3.X = 0;
            protoVector3.Y = 0;
            protoVector3.Z = 0;
        }

        public void Release()
        {
            Reset();
            if (m_entityCtrl.Data.isMainPlayer)
            {
                UnRegAllTagChangeListeners();
            }
            skillMoveMsg = null; // 使用技能坐标
            rawMsg = null;     // 使用技能坐标输入数据
            ProtpMoveMsg2 = null;
            protoVector3 = null;
            //dateTimeOffset = null;
            M_BattleSocket = null;

            m_NPCEntityBase = null;
            m_entityCtrl = null;
            m_hostEntityCtrl = null;
            m_hostNPCEntityBase = null;
        }


        #region 【绑定技能按钮】
        /// <summary> 当前技能按钮 </summary>
        private UniversalButton m_currentSkillBtn;
        /// <summary> 是否是蓄力指示器 </summary>
        private bool isEnergyIndicator = false;
        /// <summary> 是否指示器 </summary>
        private bool m_isIndicator = false;
        /// <summary> 技能按下释放判断【时间（毫秒）】 </summary>
        private float m_MainPlayerDownSkillBtnTime = 0f;
        /// <summary> 当前技能SkillInfo </summary>
        private SkillInfo curSkillInfo;
        /// <summary> 当前技能配置 </summary>
        private SkillWheelInfo curSkillCfg;
        /// <summary> 使用技能开始的朝向 </summary>
        private Vector3 M_UserSkillStartAngleDir;
        /// <summary> 技能条件敌人阵营 </summary>
        private SelectType TargetSelect;

        private void SetCurSkillBtn(UniversalButton skillBtn)
        {
            if (m_currentSkillBtn != null)
            {
                if (skillBtn.skillUnit.CurShowSkillInfo != null && m_currentSkillBtn.skillUnit.CurShowSkillInfo != null && skillBtn.skillUnit.CurShowSkillInfo.skillId != m_currentSkillBtn.skillUnit.CurShowSkillInfo.skillId)
                {
                    // 已有技能在按下，就把他抬起来
                    //SGF.Debuger.LogError($"{LOG_TAG} SkillIndicatorView 抬起原来的----1111 skillid={m_currentSkillBtn.skillUnit.CurSkillInfo.cfg.ID}");
                    m_currentSkillBtn.m_IsKeyDown = false;
                    m_currentSkillBtn.OnPointerUp(null);
                    //SGF.Debuger.LogError($"{LOG_TAG} SkillIndicatorView 抬起原来的----777777777777 skillid={m_currentSkillBtn.skillUnit.CurSkillInfo.cfg.ID}");
                }
                else
                {
                    //SGF.Debuger.LogWarning($"{LOG_TAG} SkillIndicatorView 抬起原来的----一样一样 skillid={m_currentSkillBtn.skillUnit.CurSkillInfo.cfg.ID}");
                }
            }
            m_currentSkillBtn = skillBtn;
        }

        /// <summary> 按下 </summary>
        private void OnPointerDown(UniversalButton skillBtn)
        {
            if (skillBtn == null || skillBtn.skillUnit == null || skillBtn.skillUnit.CurShowSkillInfo == null || !skillBtn.skillUnit.HasShowSkill)
            {
                //SGF.Debuger.LogWarning($"{LOG_TAG} SkillBtn-OnPointerDown  skillBtn=null ");
                return;
            }
            SetCurSkillBtn(skillBtn);
            //SGF.Debuger.Log($"{LOG_TAG}  OnPointerDown nowTime={TimeUtils.ServerNowStampMilli}ms,m_ForceMoveTime={m_MainPlayerChangeMoveForceTime}");


            SetCurSkillInfo(m_currentSkillBtn.skillUnit.CurShowSkillInfo);

            if (curSkillCfg == null)
            {
                return;
            }
            m_entityCtrl.M_UserSkillTag = false;    // 如果是主角：修改使用技能的间隔标识
            m_isIndicator = curSkillCfg.CastMethod == CastMethodType.WheelCast || curSkillCfg.CastMethod == CastMethodType.GatherWheelCast;
            isEnergyIndicator = curSkillCfg.CastMethod == CastMethodType.GatherWheelCast;
            // 指示器技能3s延迟后打开技能指示器
            if (m_isIndicator)
            {
                m_MainPlayerDownSkillBtnTime = GameConfig.System_Skill_Down_Time;
                //SGF.Debuger.LogWarning($"{LOG_TAG} SkillIndicatorView 按下的 设置3s skillid={m_currentSkillBtn.skillUnit.CurSkillInfo.cfg.ID}");
            }
            else
            {
                m_MainPlayerDownSkillBtnTime = 0;
            }

            M_UserSkillStartAngleDir = m_NPCEntityBase.M_EntityAnglesDir;

            // 蓄力技能绑定数据层的委托
            if (isEnergyIndicator)
            {
                m_NPCEntityBase.skillDispatcher.SkillController.ActionOnEnergyStart += m_currentSkillBtn.OnActionOnEnergyStart;
                m_NPCEntityBase.skillDispatcher.SkillController.ActionOnEnergyEnd += m_currentSkillBtn.OnSkillEnergyEndAction;
                m_NPCEntityBase.skillDispatcher.SkillController.ActionOnEndSkillStage += m_currentSkillBtn.OnActionOnEndSkillStage;
            }
            RefreshForbidStickDir(true, "OnPointerDown");

            if (GetIsPointerDownSendSkill())
            {
                SendUserSkillReq(Vector3.zero, false, E_BtnInputType.PointerDown);
            }

        }
        /// <summary> 滑动 </summary>
        private void OnSkillDirChange(UnityEngine.Vector3 dir)
        {
            if (m_currentSkillBtn == null)
            {
                //SGF.Debuger.Log($"{LOG_TAG}  OnSkillDirChange nowTime={TimeUtils.ServerNowStampMilli}ms");
                return;
            }

            /// TODO: 曲
            /// 2023/4/26
            /// 类似于蓄力期间转换朝向的 朝向的方式,目前看代码的做法 是客户端直接先预播朝向改变,同时告知服务器.
            /// 这样可能会有个问题, 服务器收到 朝向改变协议的时候, 并不允许朝向改变. 
            /// 从而导致客户端预播的朝向 跟 服务器的朝向朝向并不一致.
            /// 
            /// 我的想法，对于移动这种，客户端可以完全依赖客户端自己的数据,提前同步告知给服务器.
            /// 对于技能这种 中间原子锁等状态 随时可能会变的情况, 技能中的朝向改变, 最好是 客户端 发送朝向改变请求,
            /// 客户端 收到之后,再 表现. 
            /// 技能中的朝向 和 位移, 应该完全听从服务器的坐标.
            /// 
            {
                // 如果是指示器就发送转向
                if (isEnergyIndicator)
                {
                    SendSkillEnergyRotateSyncMoveMsg(dir);
                }
                // 指示器和蓄力指示器的推送
                m_hostEntityCtrl.SkillDirChangeActions?.Invoke(m_currentSkillBtn.skillUnit, dir);
            }


        }
        /// <summary> 抬起 </summary>
        private void OnPointerUp(UnityEngine.Vector3 dir, E_SkillBtnState buttonState, float arg)
        {
            if (m_currentSkillBtn == null)
            {
                //SGF.Debuger.Log($"{LOG_TAG}  OnPointerUp nowTime={TimeUtils.ServerNowStampMilli}ms");
                return;
            }
            RefreshForbidStickDir(false, "OnPointerUp");

            if (buttonState == E_SkillBtnState.Pressed)
            {
                SendUserSkillReq(dir, false, E_BtnInputType.PointerUp, arg);
            }

            if (isEnergyIndicator)
            {
                m_NPCEntityBase.skillDispatcher.SkillController.ActionOnEnergyStart -= m_currentSkillBtn.OnActionOnEnergyStart;
                m_NPCEntityBase.skillDispatcher.SkillController.ActionOnEnergyEnd -= m_currentSkillBtn.OnSkillEnergyEndAction;
                m_NPCEntityBase.skillDispatcher.SkillController.ActionOnEndSkillStage -= m_currentSkillBtn.OnActionOnEndSkillStage;
                //m_entityCtrl.M_InputMoveDirection = UnityEngine.Vector3.zero;
            }
            if (m_isIndicator)
            {
                //SGF.Debuger.LogError($"{LOG_TAG} SkillIndicatorView 抬起原来的----666666666666666666666 skillid={m_currentSkillBtn.skillUnit.CurSkillInfo.cfg.ID}");

                m_hostEntityCtrl.SkillPointerUpActions?.Invoke(m_currentSkillBtn.skillUnit);
            }
            ReleaseSkillBtnData();
        }

        private void OnPointerUpUI(UniversalButton universalButton)
        {
            if (m_currentSkillBtn == null)
            {
                //SGF.Debuger.Log($"{LOG_TAG}  OnPointerUp nowTime={TimeUtils.ServerNowStampMilli}ms");
                return;
            }
            RefreshForbidStickDir(false, "OnPointerUp");
        }

        /// <summary> 离开 </summary>
        private void OnEndDrag(UnityEngine.Vector3 dir)
        {
            if (m_currentSkillBtn == null)
            {
                //SGF.Debuger.Log($"{LOG_TAG}  OnEndDrag nowTime={TimeUtils.ServerNowStampMilli}ms");
                return;
            }
            RefreshForbidStickDir(false, "OnEndDrag");

            SendUserSkillReq(dir, false, E_BtnInputType.PointerUp);
            if (isEnergyIndicator)
            {
                m_NPCEntityBase.skillDispatcher.SkillController.ActionOnEnergyStart -= m_currentSkillBtn.OnActionOnEnergyStart;
                m_NPCEntityBase.skillDispatcher.SkillController.ActionOnEnergyEnd -= m_currentSkillBtn.OnSkillEnergyEndAction;
                m_NPCEntityBase.skillDispatcher.SkillController.ActionOnEndSkillStage -= m_currentSkillBtn.OnActionOnEndSkillStage;
                //m_entityCtrl.M_InputMoveDirection = UnityEngine.Vector3.zero;
            }
            if (m_isIndicator)
            {
                m_hostEntityCtrl.SkillEndDragActions?.Invoke(m_currentSkillBtn.skillUnit);
            }
            ReleaseSkillBtnData();
        }
        /// <summary> 取消 </summary>
        private void OnCancelSkill()
        {
            RefreshForbidStickDir(false, "OnCancelSkill");

            if (m_isIndicator)
            {
                m_hostEntityCtrl.SkillnCancelActions?.Invoke(m_currentSkillBtn.skillUnit);
            }
            CancelUserSkill();

            ReleaseSkillBtnData();
        }
        /// <summary> 长按 </summary>
        private void OnPressingAction()
        {
            if (m_currentSkillBtn == null)
            {
                SGF.Debuger.LogWarning($"{LOG_TAG} OnPressingAction  m_currentSkillBtn=null,,nowTime={TimeUtils.ServerNowStampMilli}ms");
                return;
            }

            SendUserSkillReq(UnityEngine.Vector3.zero, true, E_BtnInputType.PointerDown);
        }

        /// <summary> AOI [状态] 变化 </summary>
        private void OnAOIStateChange(string key, object val)
        {
            if (m_entityCtrl == null || m_entityCtrl.Data == null)
            {
                return;
            }

            if (curSkillInfo == null || m_currentSkillBtn == null)
            {
                return;
            }

            bool isForbidAttack = m_entityCtrl.Data.IsForbidAttack;
            bool isForbidSkill = m_entityCtrl.Data.IsForbidSkill;

            if (curSkillInfo.IsNormalSkill())
            {
                // 普攻技能
                if (isForbidAttack)
                {
                    // 禁止普通
                    m_currentSkillBtn.OnPointerUp(null);
                }
            }
            else
            {
                if (isForbidSkill)
                {
                    // 禁止技能
                    m_currentSkillBtn.OnPointerUp(null);
                }
            }
        }

        /// <summary> 获得是否按下发送技能 </summary>
        private bool GetIsPointerDownSendSkill()
        {
            bool res = false;

            if (curSkillCfg == null)
            {
                return res;
            }
            // -------- 按下的时候发使用技能的协议
            // 按下释放 类型
            // 蓄力轮盘 类型
            res = curSkillCfg.CastMethod == CastMethodType.GatherWheelCast ||
                  curSkillCfg.CastMethod == CastMethodType.GatherWheelCastNoIndicator ||
                  curSkillCfg.CastMethod == CastMethodType.DirectCast;

            return res;
        }
        /// <summary> 清除技能按钮信息 </summary>
        private void ReleaseSkillBtnData()
        {
            m_MainPlayerDownSkillBtnTime = 0;
            m_isIndicator = false;
            isEnergyIndicator = false;
            //SGF.Debuger.LogError($"{LOG_TAG} SkillIndicatorView 抬起的 skillid={m_currentSkillBtn.skillUnit.CurSkillInfo.cfg.ID}");
            m_currentSkillBtn = null;
        }
        /// <summary> 创建\刷新技能按钮 </summary>
        public void OnActionOnRefreshAllSkill()
        {
            // 获取取消技能按钮
            SkillCanceller SkillCancelBtn = GameInput.GetCancelBtn();
            //uint jobId = m_NPCEntityBase.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Job);
            // 先关闭隐藏所有的技能按钮
            GameInput.ResetSkill();
            // 遍历创建技能信息
            // 异步的，这里的要判断一下了
            if (m_NPCEntityBase == null)
            {
                return;
            }
            foreach (KeyValuePair<int, SkillContainer> item in m_NPCEntityBase.GetSkillContainerDic())
            {
                int posID = item.Key;
                SkillContainer skillContainer = item.Value;

                //if (!item.Value.IsPadding)
                //{
                //    SGF.Debuger.LogWarning($"{LOG_TAG} OnActionOnRefreshAllSkill skillBtn.SkillContainer.isPadding=false ");
                //    continue;
                //}

                SkillPosSetDataCell skillPosSetDataCell = LocalDataManager.Instance.GetSkillPosSetDataCell(item.Key);
                //if (m_NPCEntityBase.Data.isMainPlayer)
                //{
                //    //skillPosSetDataCell = LocalDataManager.Instance.GetSkillPosSetDataCell(item.Key, (int)jobId);
                //    skillPosSetDataCell = LocalDataManager.Instance.GetSkillPosSetDataCell(item.Key);
                //}
                //else
                //{
                //    skillPosSetDataCell = LocalDataManager.Instance.GetSkillPosSetDataCell(item.Key);
                //}

                if (skillPosSetDataCell == null)
                {
                    SGF.Debuger.LogWarning($"{LOG_TAG} OnActionOnRefreshAllSkill skillBtn.SkillContainer.skillPosSetDataCell=null ");
                    continue;
                }

                //// TODO:临时代码，7.15以后要删除
                //if (skillPosSetDataCell.GetSkillType() == (int)E_SkillSetType.Assistant)
                //{
                //    SGF.Debuger.LogWarning($"{LOG_TAG} OnActionOnRefreshAllSkill SkillType==辅助类型 不创建");
                //    continue;
                //}
                // TODO:临时代码，7.15以后要删除

                //string key = $"{item.Key}_{m_NPCEntityBase.EntityId}";
                UniversalButton skillBtn = GameInput.GetSkillBtn(item.Key);
                // 第一次是创建\后面刷新的时候，直接找原来的按钮刷新信息
                if (skillBtn == null)
                {
                    //GameObject go = Service.Resource.ResourceManager.Instance.LoadGameObject(m_modelPath);
                    //skillBtn = go.GetComponent<UniversalButton>();
                    //SGF.Debuger.LogError($"{LOG_TAG} 创建刷新技能按钮 异步加载【请求】 pos={item.Key}");

                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(m_modelPath,
                    (GameObject go) =>
                    {
                        //SGF.Debuger.LogWarning($"{LOG_TAG} 创建刷新技能按钮 异步加载【返回】 pos={item.Key},go={go}");
                        if (go == null)
                        {
                            return;
                        }
                        if (m_entityCtrl != null && m_entityCtrl.Data != null)
                        {
                            skillBtn = GameInput.GetSkillBtn(item.Key);
                            if (skillBtn == null)
                            {
                                var gob = GameObject.Instantiate<GameObject>(go);
                                if (gob != null)
                                {
                                    //SGF.Debuger.LogWarning($"{LOG_TAG} 创建刷新技能按钮 异步加载【返回重新创建】 pos={item.Key},go={go}");
                                    skillBtn = gob.GetComponent<UniversalButton>();
                                    InitCreateSkillBtn(item.Key, skillBtn, skillPosSetDataCell, skillContainer);
                                }
                            }
                            else
                            {
                                InitCreateSkillBtn(item.Key, skillBtn, skillPosSetDataCell, skillContainer);
                            }
                        }
                    });
                }
                else
                {
                    InitCreateSkillBtn(item.Key, skillBtn, skillPosSetDataCell, skillContainer);
                }
            }
        }

        private void InitCreateSkillBtn(int posID, UniversalButton skillBtn, SkillPosSetDataCell skillPosSetDataCell, SkillContainer skillContainer)
        {
            //SGF.Debuger.Log($"{LOG_TAG} 创建刷新技能按钮 异步加载【赋值】 pos={posID}");

            bool res = skillBtn.Create(posID, skillContainer, m_entityCtrl, skillPosSetDataCell);
            if (res)
            {
                GameInput.AddSkillBtn(posID, skillBtn);
                skillBtn.SetSiblingIndex();
                // 添加按钮点击事件
                skillBtn.onPointerDown = OnPointerDown;
                skillBtn.BtnDirChange = OnSkillDirChange;
                skillBtn.onPointerUp = OnPointerUp;
                skillBtn.OnPointerUPUI = OnPointerUpUI;
                skillBtn.onEndDrag = OnEndDrag;
                skillBtn.onCancelSkill = OnCancelSkill;
                // 普攻技能会配置长按回调
                if (skillBtn.skillUnit != null && skillBtn.skillUnit.IsNormalSkill())
                {
                    // 磊哥说的普攻的长按最多100毫秒
                    skillBtn.trrigerIntervalTime = 0.1f;
                    skillBtn.onPressing = OnPressingAction;
                }
                else
                {
                    skillBtn.onPressing = null;
                }
            }
            else
            {
                GameInput.DelSkillBtn(posID);
            }
        }

        private void RemoveSkill()
        {
            // 遍历创建技能信息
            foreach (KeyValuePair<int, SkillContainer> item in m_NPCEntityBase.GetSkillContainerDic())
            {
                //string key = $"{item.Key}_{m_NPCEntityBase.EntityId}";
                GameInput.DelSkillBtn(item.Key);
            }
        }

        #endregion

        #region 发送使用技能
        private ProtoMsg.Vector3 skillMoveMsg = new(); // 使用技能坐标
        private ProtoMsg.RawMsg rawMsg = new();     // 使用技能坐标输入数据
        private ProtoMsg.MoveMsg2 ProtpMoveMsg2 = new();
        private ProtoMsg.Vector3 protoVector3 = new();
        private DateTimeOffset dateTimeOffset;
        private SocketBase M_BattleSocket;

        // 发送使用技能协议
        public void SendUserSkillReq(Vector3 dir, bool isLongPress, E_BtnInputType e_BtnInputType, float arg = -999)
        {
            if (curSkillCfg == null)
            {
                return;
            }

            if (curSkillCfg.CastMethod == CastMethodType.DirectCast && e_BtnInputType == E_BtnInputType.PointerUp)
            {
                EmptySkillCfg();
                return;
            }

            SkillInputType skillInputType = curSkillCfg.SkillInputType;
            // 如果是直接释放的技能
            Vector3 curDir = m_NPCEntityBase.M_EntityAnglesDir.normalized;
            if (skillInputType == SkillInputType.DirInput)
            {
                // 如果是瞬间释放的朝向技能
                // 1.先判断遥感有没有朝向
                // 2.后判断按钮按下去时候的人物朝向
                if (dir == Vector3.zero)
                {
                    if (m_entityCtrl.MoveDirection != Vector3.zero)
                    {
                        curDir = m_entityCtrl.MoveDirection.normalized;
                    }
                    else
                    {
                        curDir = M_UserSkillStartAngleDir;
                    }
                }
            }
            if (dir.magnitude > 0)
            {
                Vector3 VirtualCameraForward = CameraManager.Instance.GetPlayerCameraAnglesY();
                dir = Quaternion.Euler(0, VirtualCameraForward.y, 0) * dir;
            }
            int curAngle = (int)(Math.Atan2(curDir.z, curDir.x) * Mathf.Rad2Deg);
            Vector3 skillPos = m_hostNPCEntityBase.Position();

            /*if (curSkillCfg.CastMethod != CastMethodType.DirectCast)
            {
                curDir = dir.normalized;

                Vector3 n = Vector3.up;
                curAngle = (float)Math.Atan2(Vector3.Dot(n, Vector3.Cross(m_dirInterpolation.normalized, curDir)), Vector3.Dot(m_dirInterpolation.normalized, curDir)) * Mathf.Rad2Deg;
            }*/
            ProtoMsg.SkillUseReq skillUseReq = new();
            skillUseReq.BlackList.Clear();
            Google.Protobuf.Collections.RepeatedField<global::ProtoMsg.BlackBoardNode> blackList = new();
            global::ProtoMsg.BlackBoardNode blackBoardNodeNew = new();
            // 填充数据
            skillUseReq.SkillID = curSkillInfo.skillId;
            skillMoveMsg.X = skillPos.x;
            skillMoveMsg.Y = skillPos.y;
            skillMoveMsg.Z = skillPos.z;
            skillUseReq.Pos = skillMoveMsg;
            skillUseReq.Rot = m_NPCEntityBase.ServerAngles;

            //SGF.Debuger.LogError($"{LOG_TAG} 技能 自动转向 角度={skillUseReq.Rot},我的角度={EulerAngles.y},,输入的朝向={dir},,我的朝向={m_entityAnglesDir.normalized},,使用的朝向={curDir}");
            //SGF.Debuger.LogError($"{LOG_TAG}自动转向 SendUserSkillReq RunTime={skillUseReq.RunTime},SkillID={skillUseReq.SkillID}");

            ShapeRingFan WheelRange = curSkillCfg.WheelRange;   // 轮盘可选范围
            bool isReleaseSucceed = true;   // 是否释放 成功
            bool isAutoLookTarget = false;   // 是否自动查找目标
            NPCEntityBase curAtkEntity = BattleManager.Instance.CurAtkEntity;
            if (!m_NPCEntityBase.Data.isMainPlayer)
            {
                curAtkEntity = null;
            }

            float maxRadius = (float)curSkillCfg.WheelRange.MaxRadius / 100;    // 最大锁敌半径
            bool isClosestTarget = true;    // 是否找目标
            if (curAtkEntity != null)
            {
                // 判断和敌方的距离
                Vector3 curAtkEntityIdPos = curAtkEntity.Position();
                Vector3 dirInterpolation = curAtkEntityIdPos - m_NPCEntityBase.Position();
                dirInterpolation.y = 0;
                float distance = dirInterpolation.magnitude;
                // 如果不是对象输入就增加怪物模型的半径
                float modelRadius = 0f;
                if (skillInputType != SkillInputType.ObjInput)
                {
                    modelRadius = curAtkEntity.ModelRadius;
                }
                isClosestTarget = distance > (maxRadius + modelRadius);
                // 先判断阵营是不是一个
                bool isTargetNtt = EntityFactoryUtils.CheckIsTriggleAOIEntity(curAtkEntity, m_NPCEntityBase.EntityId, m_NPCEntityBase.Faction, TargetSelect);
                //说明实体类型不是目标类型
                if (!isTargetNtt)
                {
                    curAtkEntity = null;
                    isClosestTarget = true;
                }
            }
            // 之前的怪物目标不正确，重新找怪物
            if (isClosestTarget)
            {
                curAtkEntity = null;
                // 找距离最近的敌人---追击目标
                if (m_currentSkillBtn != null && curSkillCfg != null && m_NPCEntityBase != null)
                {
                    // 先判断 是否有正在运行的 输入轴的效果,如果有,就用输入轴的 轮盘配置
                    //SkillWheelInfo skillWheelInfo = m_NPCEntityBase.skillDispatcher.SkillController.GetRunningSkillWheelInfo(curSkillInfo.skillId);

                    int curWheelMaxRadius = curSkillCfg.WheelRange.MaxRadius;

                    //// 如果 输入轴的 轮盘配置存在，就采用 输入轴 中配置的 最大范围
                    //if (skillWheelInfo != null)
                    //{
                    //    curWheelMaxRadius = skillWheelInfo.WheelRange.MaxRadius;
                    //}
                    // 严格判断敌方坐标是否在索敌范围内
                    // 判断敌方模型半径相交也算能锁到怪物
                    bool isAddRadius = skillInputType != SkillInputType.ObjInput;
                    curAtkEntity = SkillUtils.GetClosestTargetIdByTargets(m_NPCEntityBase.EntityId, m_NPCEntityBase.Faction, m_NPCEntityBase.Position(), curWheelMaxRadius, TargetSelect, isAddRadius);
                    isAutoLookTarget = curAtkEntity != null;
                }
            }

            // 如果【坐标输入】【短按】时攻击范围内没有敌人不会在释放
            if (curSkillCfg.CastMethod == CastMethodType.WheelCast && skillInputType == SkillInputType.PosInput && curAtkEntity == null && dir == Vector3.zero && dir.magnitude <= 0)
            {
                return;
            }

            //ulong EnityId = curAtkEntity != null ? curAtkEntity.EnityId : 0;
            //SGF.Debuger.Log($"{LOG_TAG} 技能 自动锁敌 是否自动找目标={isClosestTarget},,是否找到了目标={isAutoLookTarget}，，目标id={EnityId}");
            if (skillInputType == SkillInputType.ObjInput)
            {
                if (curAtkEntity != null && curAtkEntity.EntityId > 0 && dir == Vector3.zero)
                {
                    // 判断和敌方的距离
                    Vector3 curAtkEntityIdPos = curAtkEntity.Position();
                    Vector3 dirInterpolation = curAtkEntityIdPos - m_NPCEntityBase.Position();
                    float distance = dirInterpolation.magnitude;
                    if (WheelRange.MaxRadius > 0 && distance <= (float)WheelRange.MaxRadius / 100)
                    {
                        global::ProtoMsg.BlackBoardNode blackBoardNode = GetBlackBoardNodeNew(GameConfig.SKILL_INPUTTYPE_OBJINPUT);
                        blackBoardNode.Uint64Value = curAtkEntity.EntityId;
                        blackList.Add(blackBoardNode);
                    }
                    else
                    {
                        //Frame.Util.ShowBattleMessage(GameConfig.LocalStr["UserSkillFailTips1"]);
                        Frame.Util.ShowBattleMessage(LanguageManager.Instance.GetLanguageByKey("UserSkillFailTips1"));
                        //SGF.Debuger.Log($"{LOG_TAG} SendUserSkillReq E_SkillInputType.ObjInput PlayerEnityId={EnityId},Pos={CurrentPos},curAtkEntityIdPos={curAtkEntityIdPos},dirInterpolation={dirInterpolation},distance={distance}");
                    }
                }
                else if (dir != Vector3.zero)
                {
                    // 判断手指区域有没有怪物
                    Vector3 rawDir = Vector3.zero;
                    rawDir.x = dir.x * maxRadius;
                    rawDir.z = dir.z * maxRadius;
                    skillPos += rawDir;
                    List<AOIEntityObject> currentTriggetNtts = new();
                    SkillUtils.GetTargetInCircle(ref currentTriggetNtts, m_NPCEntityBase.EntityId, skillPos, 1f, m_NPCEntityBase.Faction, TargetSelect);
                    if (currentTriggetNtts.Count > 0)
                    {
                        curAtkEntity = SkillUtils.GetClosestTargetId(currentTriggetNtts, m_NPCEntityBase.Position());
                        isAutoLookTarget = true;
                        global::ProtoMsg.BlackBoardNode blackBoardNode = GetBlackBoardNodeNew("InputTarget");
                        blackBoardNode.Uint64Value = curAtkEntity.EntityId;
                        blackList.Add(blackBoardNode);
                    }
                    else
                    {
                        isReleaseSucceed = false;
                        //Frame.Util.ShowBattleMessage(GameConfig.LocalStr["UserSkillFailTips2"]);
                        Frame.Util.ShowBattleMessage(LanguageManager.Instance.GetLanguageByKey("UserSkillFailTips2"));
                        //SGF.Debuger.Log($"{LOG_TAG} SendUserSkillReq skill release [invalid],skillId={skillUseReq.SkillID}");
                    }
                }
                else
                {
                    isReleaseSucceed = false;
                    //Frame.Util.ShowBattleMessage(GameConfig.LocalStr["UserSkillFailTips2"]);
                    Frame.Util.ShowBattleMessage(LanguageManager.Instance.GetLanguageByKey("UserSkillFailTips2"));
                    //SGF.Debuger.Log($"{LOG_TAG} SendUserSkillReq skill release [invalid],skillId={skillUseReq.SkillID}");
                }
            }

            if (isReleaseSucceed)
            {
                ///////////------------ 技能转向 规则 --------- start
                bool isAutoTurnToTarget = curSkillCfg.IsAutoTurnToTarget;

                /// 2024/11/14
                /// 自动战斗途中需要索敌. 
                /// 自动战斗 会先判定 地方是否在技能范围内, 在才会 释放技能.
                /// 但是 狗曲 按钮这块会自己 去取 curAtkEntity, 所以可能是 索敌目标为 null。导致 玩家自动战斗空放技能的bug
                /// 
                /// note:
                /// 狗曲前面 curAtkEntity 索敌逻辑写的太鸡儿复杂了, 所以在自动战斗的时候, 再次锁一次敌人, 防止 curAtkEntity 为null
                /// 
                if (BattleManager.Instance.IsAutoBattling)
                {
                    isAutoTurnToTarget = true;
                    // if (false && curAtkEntity == null && BattleManager.Instance.SearchAutoBattleClosetTarget(out ulong findEntity))
                    // {
                    //     // Debug.LogError($"xxxxxxxxxxxxxxx 重新 索敌: {findEntity} ");
                    //     curAtkEntity = GameManager.Instance.GetEntityByEntityID(findEntity);
                    // }
                }

                if (isAutoLookTarget && m_NPCEntityBase.Data.isMainPlayer && curAtkEntity != null)
                {
                    // BattleManager.Instance.SetCurAtkEntity(curAtkEntity);
                    // 攻击的时候, 将索敌目标 指向对应的目标
                    BattleManager.Instance.Switch2TargetEnemy(curAtkEntity.EntityId);
                }




                //SGF.Debuger.Log($"{LOG_TAG} 技能 自动转向---111111111111 自动转向={isAutoTurnToTarget}，，目标id={EnityId},,角度={skillUseReq.Rot},,我的角度={EulerAngles.y}");
                if (curSkillCfg.IsAutoTurnToTarget && GetIsPointerDownSendSkill() && e_BtnInputType == E_BtnInputType.PointerUp)
                {
                    isAutoTurnToTarget = true;
                }
                // 朝向输入 并且 输入的朝向 不为0
                if (skillInputType == SkillInputType.DirInput && dir != Vector3.zero && dir.magnitude > 0)
                {
                    // 只发送自动转向的的朝向
                    // 实际人物转向在，技能播放开始的时候会执行响应的委托
                    curAngle = (int)(Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg);
                }
                else if (isAutoTurnToTarget && curAtkEntity != null)
                {
                    Vector3 dirInterpolation = curAtkEntity.Position() - m_NPCEntityBase.Position();
                    curAngle = (int)(Math.Atan2(dirInterpolation.z, dirInterpolation.x) * Mathf.Rad2Deg);
                }

                // -----根据输入类型不同，传入不同的黑板数据
                switch (skillInputType)
                {
                    case SkillInputType.DirInput:
                        {
                            // 输入坐标 InputRota
                            global::ProtoMsg.BlackBoardNode blackBoardNode = GetBlackBoardNodeNew(GameConfig.SKILL_INPUTTYPE_DIRINPUT);
                            blackBoardNode.Int32Value = curAngle;   // 锁敌的角度
                            blackList.Add(blackBoardNode);
                        }
                        break;
                    case SkillInputType.PosInput:
                        {
                            // 输入坐标 InputCoord
                            rawMsg.MsgValue = ByteString.Empty;
                            rawMsg.MsgID = (uint)(int)MsgIDEnum.Vector3ID;
                            // 自动锁敌的 时： 坐标是 目标的坐标
                            int skillDir = curAngle;
                            if (curAtkEntity != null && curAtkEntity.EntityId > 0 && dir == Vector3.zero)
                            {
                                // 瞬放的技能-》朝向是面向怪物的朝向
                                Vector3 dirInterpolation = curAtkEntity.Position() - m_NPCEntityBase.Position();
                                skillDir = (int)(Math.Atan2(dirInterpolation.z, dirInterpolation.x) * Mathf.Rad2Deg);
                                // 坐标
                                float distance = dirInterpolation.magnitude;
                                isClosestTarget = distance > maxRadius;
                                if (isClosestTarget)
                                {
                                    Vector3 rawDir = Vector3.zero;
                                    rawDir.x = dirInterpolation.normalized.x * maxRadius;
                                    rawDir.z = dirInterpolation.normalized.z * maxRadius;
                                    skillPos += rawDir;
                                }
                                else
                                {
                                    skillPos = curAtkEntity.Position();
                                }
                            }
                            else if (dir != Vector3.zero)
                            {
                                Vector3 rawDir = Vector3.zero;
                                rawDir.x = dir.x * maxRadius;
                                rawDir.z = dir.z * maxRadius;
                                skillPos += rawDir;
                                // 触摸了遥感的技能-》朝向是遥感的朝向
                                skillDir = (int)(Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg);
                            }
                            skillMoveMsg.X = skillPos.x;
                            skillMoveMsg.Y = skillPos.y;
                            skillMoveMsg.Z = skillPos.z;
                            rawMsg.MsgValue = skillMoveMsg.ToByteString();
                            global::ProtoMsg.BlackBoardNode blackBoardNode = GetBlackBoardNodeNew(GameConfig.SKILL_INPUTTYPE_POSINPUT);
                            blackBoardNode.RawValue = rawMsg;
                            blackList.Add(blackBoardNode);
                            ////////////////////--------------------- 坐标输入的技能朝向是输入的朝向
                            global::ProtoMsg.BlackBoardNode blackBoardNodeRota = GetBlackBoardNodeNew(GameConfig.SKILL_INPUTTYPE_DIRINPUT);
                            blackBoardNodeRota.Int32Value = skillDir;   // 锁敌的角度
                            blackList.Add(blackBoardNodeRota);
                            curAngle = skillDir;
                        }
                        break;
                    case SkillInputType.ObjInput:
                        {
                            // 输入目标 InputTarget 的黑板数据在上面已经填充了
                            global::ProtoMsg.BlackBoardNode blackBoardNode = GetBlackBoardNodeNew(GameConfig.SKILL_INPUTTYPE_DIRINPUT);
                            blackBoardNode.Int32Value = curAngle;   // 锁敌的角度
                            blackList.Add(blackBoardNode);
                        }
                        break;
                    default:
                        break;
                }
                // todo:磊子说：没锁到怪物，就用左遥感的朝向或玩家当前的朝向
                if (curAtkEntity == null && curSkillCfg.CastMethod == CastMethodType.GatherWheelCast && curSkillCfg.IsAutoTurnToTarget)
                {
                    skillUseReq.Rot = curAngle;
                }
                else if (curAtkEntity == null)
                {
                    skillUseReq.Rot = m_entityCtrl.GetCurLeftJoyStickAngle();
                }
                //int curUserSkillRot = isAutoTurnToTarget ? curAngle : skillUseReq.Rot;
                //SGF.Debuger.Log($"{LOG_TAG} 技能 自动转向---2222222222222 自动转向={isAutoTurnToTarget}，，目标id={m_NPCEntityBase.EntityId},,角度={skillUseReq.Rot},,我的角度={m_NPCEntityBase.EulerAngles.y}");
                ///----------- 释放技能的朝向
                //if (nextPointBox == null)
                //{
                ////    nextPointBox = UnityEngine.Object.Instantiate(Resources.Load("Perfab/TestPoint/3") as GameObject);
                //    nextPointBox = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject("Prefabs/GMPoint/3");
                //}
                //nextPointBox.name = "NextPoint";
                //nextPointBox.transform.position = skillPos;
                //int curAngle22 = (int)(Math.Atan2(curDir.x, curDir.z) * Mathf.Rad2Deg);
                //nextPointBox.transform.eulerAngles = new Vector3(0, curAngle22, 0);
                ///----------- 释放技能的朝向
                //SGF.Debuger.LogWarning($"{LOG_TAG} 技能 自动转向---2222222 自动转向={isAutoTurnToTarget}，，目标id={EnityId},,角度={skillUseReq.Rot},,我的角度={EulerAngles.y},,传入的角度={(Math.Atan2(dir.x, dir.z) * Mathf.Rad2Deg)}");

                ///////////------------ 技能转向 规则 --------- end
                skillUseReq.LockTargetID = curAtkEntity != null ? curAtkEntity.EntityId : 0;     // 锁定目标ID。有就传，没有就传0
                if (skillInputType != SkillInputType.ObjInput && curAtkEntity != null)
                {
                    global::ProtoMsg.BlackBoardNode blackBoardNode = GetBlackBoardNodeNew(GameConfig.SKILL_INPUTTYPE_OBJINPUT);
                    blackBoardNode.Uint64Value = curAtkEntity.EntityId;
                    blackList.Add(blackBoardNode);
                }
                try
                {
                    foreach (var item in blackList)
                    {
                        skillUseReq.BlackList.Add(item);
                    }
                    E_UseSkillResult e_UseSkillRes = m_NPCEntityBase.skillDispatcher.SkillController.ClientUseSkill(m_currentSkillBtn.skillUnit, skillUseReq, curSkillCfg.CastMethod, e_BtnInputType, arg);
                    if (e_UseSkillRes == E_UseSkillResult.Succeed)
                    {
                        if (m_currentSkillBtn != null)
                        {
                            m_currentSkillBtn.PlayerClickAnim();
                        }
                        //释放技能
                        GlobalEvent.OnReciveEvent.Invoke(E_EventDefine.ReleaseSkill, m_currentSkillBtn.SkillBtnPos.ToString());
                        // curSkillInfo.cfg.ID
                        //SGF.Debuger.Log($"{LOG_TAG} SendUserSkillReq [Succeed] PlayerEnityId={EnityId},skillID={skillUseReq.SkillID},Pos={CurrentPos},sendPos={CurrentPos},anglesY={m_angles.y},rot={curAngle},e_UseSkillRes={e_UseSkillRes}");
                    }
                    else
                    {
                        //SGF.Debuger.LogWarning($"{LOG_TAG} SendUserSkillReq [failure] PlayerEnityId={EnityId},skillID={skillUseReq.SkillID},Pos={CurrentPos},sendPos={CurrentPos},anglesY={m_angles.y},rot={curAngle},e_UseSkillRes={e_UseSkillRes}");
                    }
                    // 返回这个 技能是 普通技能
                    if (!m_currentSkillBtn.skillUnit.IsNormalSkill())
                    {
                        PlayUseSkillTips(e_UseSkillRes, skillUseReq);
                    }

                }
                catch (Exception t)
                {
                    SGF.Debuger.LogError($"{LOG_TAG} SendUserSkillReq blackBoardNode={blackBoardNodeNew},,,,,t={t},Stack={t.StackTrace}");
                }
            }

            // 如果是【蓄力技能】或【长按技能】 不在这里清空
            if (curSkillCfg != null && !isLongPress && e_BtnInputType == E_BtnInputType.PointerUp)
            {
                EmptySkillCfg();
            }
        }

        private global::ProtoMsg.BlackBoardNode GetBlackBoardNodeNew(string key)
        {
            global::ProtoMsg.BlackBoardNode blackBoardNodeNew = new();
            blackBoardNodeNew.Key = key;
            return blackBoardNodeNew;
        }

        private void PlayUseSkillTips(E_UseSkillResult e_UseSkillRes, ProtoMsg.SkillUseReq skillUseReq)
        {
            switch (e_UseSkillRes)
            {
                case E_UseSkillResult.Succeed:
                    break;
                case E_UseSkillResult.CD_Not_Enough:
                    Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_skill);
                    break;
                case E_UseSkillResult.Status_Error:
                    //Frame.Util.ShowBattleMessage(GameConfig.LocalStr["UserSkillFailTips3"]);
                    Frame.Util.ShowBattleMessage(LanguageManager.Instance.GetLanguageByKey("UserSkillFailTips3"));
                    break;
                case E_UseSkillResult.MP_Not_Enough:
                    //Frame.Util.ShowBattleMessage(GameConfig.LocalStr["UserSkillFailTips4"]);
                    Frame.Util.ShowBattleMessage(LanguageManager.Instance.GetLanguageByKey("UserSkillFailTips4"));
                    break;
                case E_UseSkillResult.Priority_Not_Enough:
                    //Frame.Util.ShowBattleMessage(GameConfig.LocalStr["UserSkillFailTips5"]);
                    Frame.Util.ShowBattleMessage(LanguageManager.Instance.GetLanguageByKey("UserSkillFailTips5"));
                    break;
                case E_UseSkillResult.ForbidAttack:
                    // 原子锁 禁止使用技能,一般不需要提示,点了 没反应即可,没必要弹tips
                    // Frame.Util.ShowBattleMessage("原子锁禁止使用技能");
                    SGF.Debuger.Log($"{LOG_TAG}  原子锁 禁止使用技能");
                    break;
                default:
                    break;
            }

            //SGF.Debuger.Log($"{LOG_TAG} 技能 自动转向 使用技能的结果 e_UseSkillRes={e_UseSkillRes},ID={skillUseReq.SkillID},rot={skillUseReq.Rot},我的角度={EulerAngles.y}");

        }

        /// <summary>
        /// 发送蓄力轮盘的旋转移动同步消息
        /// </summary>
        private void SendSkillEnergyRotateSyncMoveMsg(Vector3 dir)
        {
            if (m_currentSkillBtn == null || curSkillCfg == null || dir == Vector3.zero)
            {
                return;
            }
            if (curSkillCfg.IsAutoTurnToTarget && !m_NPCEntityBase.Data.Is___ForbidDir)
            {

                SendSkillMoveMsg(dir);
            }
        }
        private void SendSkillMoveMsg(Vector3 dir)
        {
            if (m_currentSkillBtn == null || curSkillCfg == null || dir == Vector3.zero)
            {
                return;
            }

            m_NPCEntityBase.Is___SkillJoyStick = true;
            if (dir.magnitude > 0)
            {
                Vector3 VirtualCameraForward = CameraManager.Instance.GetPlayerCameraAnglesY();
                dir = Quaternion.Euler(0, VirtualCameraForward.y, 0) * dir;
            }
            m_NPCEntityBase.ClientSetRotationByDir(dir, false, 0);//技能手柄不需要过度

            // 计算角度
            // 目前服务器是做的左手遥感控制朝向
            // 服务器做的---并不是钟馗的钩子，左手遥感是控制自身的朝向，右手技能遥感是控制 钩子的朝向
            // 蓄力轮盘的转向需要以后加其他协议做
            /*Vector3 n = Vector3.up;
            float angle = Mathf.Atan2(
                    Vector3.Dot(n, Vector3.Cross(m_dirInterpolation.normalized, dir.normalized)), 
                    Vector3.Dot(m_dirInterpolation.normalized, dir.normalized)
            ) * Mathf.Rad2Deg;*/
            float angle = (float)(Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg);
            //Debug.LogError($"发送技能 skillMove : {angle}");
            Vector3 currentPos = m_NPCEntityBase.Position();
            protoVector3.X = currentPos.x;
            protoVector3.Y = currentPos.y;
            protoVector3.Z = currentPos.z;
            ProtpMoveMsg2.IsStart = false;
            ProtpMoveMsg2.Rot = (int)angle;
            ProtpMoveMsg2.Pos = protoVector3;
            ProtpMoveMsg2.TimeStamp = dateTimeOffset.ToUnixTimeSeconds();
            M_BattleSocket.SendRPCMsg(ServerType.ServerTypeScene, ProtpMoveMsg2, false);
        }

        /// <summary> 取消使用技能 </summary>
        private void CancelUserSkill()
        {
            if (m_currentSkillBtn == null || curSkillCfg == null)
            {
                return;
            }

            if (curSkillCfg.CastMethod == CastMethodType.GatherWheelCast || curSkillCfg.CastMethod == CastMethodType.GatherWheelCastNoIndicator)
            {
                // 如果是蓄力轮盘就在取消的时候 发强制终结技能的协议
                m_NPCEntityBase.skillDispatcher.SkillController.ClientBreakActiveSkill(E_ClientSkillEndType.ClientQuit);
                //SGF.Debuger.Log($"{LOG_TAG} CancelUserSkill");
            }
            EmptySkillCfg();
        }

        private void EmptySkillCfg()
        {
            SetCurSkillInfo(null);
            M_UserSkillStartAngleDir = Vector3.zero;
            m_NPCEntityBase.Is___SkillJoyStick = false;
        }

        #endregion

        /// <summary>
        /// 检查是否使用 右边技能按钮的 朝向
        /// 如果 是 蓄力 技能, 并且 技能中 配置了 使用技能朝向(释放技能 时 人物朝向使用 技能按钮朝向)，
        /// 那么就是 使用 技能按钮轮盘朝向
        /// </summary>
        /// <returns></returns>
        private bool CheckIsUskRightBtnDir()
        {
            if (curSkillCfg != null && isEnergyIndicator && curSkillCfg.IsAutoTurnToTarget)
            {
                return true;
            }

            return false;
        }

        private void ForbidJoyStkickDir(bool forbid)
        {
            m_entityCtrl.M_Curr.IsForbidJoyStickDir = forbid;
        }

        private void RefreshForbidStickDir(bool forbid, string tag)
        {
            if (CheckIsUskRightBtnDir())
            {
                ForbidJoyStkickDir(forbid);
                //SGF.Debuger.LogError($"[ForbidDir] 刷新禁止左摇杆朝向: {tag} forbid: {forbid} ");
            }
        }


#if UNITY_EDITOR

        public void RefreshSkill()
        {
            OnActionOnRefreshAllSkill();

            // 遍历创建技能信息
            foreach (var item in m_NPCEntityBase.skillDispatcher.SkillUnitController.SkillContainerDic)
            {
                SkillPosSetDataCell skillPosSetDataCell = LocalDataManager.Instance.GetSkillPosSetDataCell(item.Key);
                if (skillPosSetDataCell != null)
                {
                    if (!item.Value.IsPadding)
                    {
                        SGF.Debuger.LogWarning($"{LOG_TAG} OnActionOnRefreshAllSkill skillBtn.SkillContainer.isPadding=false ");
                        continue;
                    }
                    //string key = $"{item.Key}_{m_NPCEntityBase.EntityId}";
                    UniversalButton skillBtn = GameInput.GetSkillBtn(item.Key);

                    int skillId = item.Value.CurSkillId;
                    skillBtn.transform.name = $"Skill_{skillId}";
                    skillBtn.transform.GetComponent<RectTransform>().anchoredPosition3D = new UnityEngine.Vector3(skillPosSetDataCell.SkillPos[0], skillPosSetDataCell.SkillPos[1], skillPosSetDataCell.SkillPos[2]);
                    skillBtn.UpdateBound();
                }
            }
        }
#endif


    }
}
