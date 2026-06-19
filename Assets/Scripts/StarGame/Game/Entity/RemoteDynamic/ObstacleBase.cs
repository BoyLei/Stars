///--------------------------------------------------------------------
/// 文件名   :   ObjectSignData
/// 内  容   :   
/// 说  明   :   交互物件，小物件
/// 创建日期 :   2022/07/29 14:07:56
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProjectDef;
using System;
using UnityEngine;
namespace StarProject.Game.Entity.VitalSigns
{
    public class ObstacleBase : AOIEntityObject
    {
        private string TagFlag = "ObjectSignDataBase";

        protected NoneVitalSignData m_data;           //玩家基础数据,playerD

        public VitalSignViewShowData viewEnityData = new VitalSignViewShowData();
        public Vector3 m_currentPos;                        //缓存当前坐标
        protected override VitalSignAttrData AttrData => m_data.Attrs;

        private bool isBorn = false;

        public Action<bool, bool> ActionModelVisiable;	// 隐藏模型
        public Action<bool> ActionModelFesnel;	// 给模型菲涅尔效果
        public Action<bool> ActionModelInteractiveEff;  // 交互提示特效

        public Func<I_AnimParam, string, VitalState> PlayAnimationAction;

        private ObjectSubState m_eSubState = ObjectSubState.Idle;   //当前状态
        public new ObjectSubState M_eSubState
        {
            get => m_eSubState;
            set
            {
                m_eSubState = value;
            }
        }

        public VitalState PlayAnimation(I_AnimParam param, string animationName = "")
        {
            return PlayAnimationAction?.Invoke(param, animationName);
        }

        public void PlaySound(AudioClip audioClip, bool loop)
        {
            ActionOnPlayAudio?.Invoke(audioClip, loop);
        }

        /// <summary>
        /// 创建逻辑数据必要的，需要玩家数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="playerData"></param>
        /// <param name="container">角色们找个通用的角色挂点就行了</param>
        //public void Create(int index, NoneVitalSignData playerData, Transform container)
        //{
        //    base.Init(playerData.M_EntityID, playerData.EntityType);

        //    m_data = playerData;
        //    isBorn = true;
        //    IgnoreGravity = false;

        //    // 根据ID，在model表里面获取资源路径下载
        //    uint _index = ConfigIndex;
        //    InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell((int)_index);
        //    if (interactDataCell != null)
        //    {
        //        ModleScale = (float)interactDataCell.GetModelScaling() / 100f;

        //        avatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(interactDataCell.GetAvatarID());
        //        if (avatarDataCell != null)
        //        {
        //            ModleScale *= avatarDataCell.GetModelScaling() / 100f;
        //            modelDataCell = LocalDataManager.Instance.GetModelDataCell(avatarDataCell.GetModelId());
        //            if (modelDataCell == null)
        //            {
        //                SGF.Debuger.LogError($"{TagFlag} Create modelDataCell=null,,error!!");
        //            }
        //            SoundManager.Instance.LoadBank(avatarDataCell.SoundBank);
        //        }
        //        else
        //        {
        //            SGF.Debuger.LogError($"{TagFlag} Create avatarId={interactDataCell.GetAvatarID()},avatarDataCell=null,,error!!");
        //        }

        //        // 在地图配置里面查找是否忽略重力
        //        IgnoreGravity = GameManager.Instance.GetEntityIDIsIgnoreGravity(E_EntityDataType.Interact, ConfigIndex);
        //    }
        //    else
        //    {
        //        SGF.Debuger.LogError($"{TagFlag} Create interactDataCell=null,_index={_index},error!!");
        //    }
        //    ViewFactory.CreateViewAddressables("Roles/Template/Interact_Model", "Roles/Template/Interact_Model", this, container);
        //    OnModelCreateComplete?.Invoke();
        //    InitRegisterAttribute();
        //}

        public void Create(int index, NoneVitalSignData playerData)
        {
            base.Init(playerData.M_EntityID, playerData.EntityType);

            m_data = playerData;
            isBorn = true;
            IgnoreGravity = false;

            // 根据ID，在model表里面获取资源路径下载
            uint _index = ConfigIndex;
            InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell((int)_index);
            if (interactDataCell != null)
            {
                ModleScale = (float)interactDataCell.GetModelScaling() / 100f;

                avatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(interactDataCell.GetAvatarID());
                if (avatarDataCell != null)
                {
                    ModleScale *= avatarDataCell.GetModelScaling() / 100f;
                    modelDataCell = LocalDataManager.Instance.GetModelDataCell(avatarDataCell.GetModelId());
                    if (modelDataCell == null)
                    {
                        SGF.Debuger.LogWarning($"{TagFlag} Create ModelId={avatarDataCell.GetModelId()},modelDataCell=null,,error!!");
                    }
                    SoundManager.Instance.LoadBank(avatarDataCell.SoundBank);
                }
                else
                {
                    SGF.Debuger.LogWarning($"{TagFlag} Create avatarId={interactDataCell.GetAvatarID()},avatarDataCell=null,,error!!");
                }

                // 在地图配置里面查找是否忽略重力
                IgnoreGravity = GameManager.Instance.GetEntityIDIsIgnoreGravity(E_EntityType.Interact, ConfigIndex);
            }
            else
            {
                SGF.Debuger.LogError($"{TagFlag} Create interactDataCell=null,_index={_index},error!!");
            }

            InitRegisterAttribute();

        }

        public void CreateModel(Transform container, Action<GameObject> cb)
        {
            ViewFactory.CreateViewAsync("Roles/Template/Interact_Model", this, container, cb);
        }

        protected override void InitRegisterAttribute()
        {
            //base.InitRegisterAttribute();
            //OnAOIRotChange(AOIAttrDefine.Rot, null);
            //OnAOITruthSpeedChange(AOIAttrDefine.TruthSpeed, null);
            //OnAOIFactionChange(AOIAttrDefine.Faction, null);

            m_data.RegisterAttribute(AOIAttrDefine.Position, OnAOIPositionChange);
            m_data.RegisterAttribute(AOIAttrDefine.PathPoses, OnAOIWaypointsChange);
            m_data.RegisterAttribute(AOIAttrDefine.CurrPathIndex, OnAOICurrPathIndexChange);
            m_data.RegisterAttribute(AOIAttrDefine.Rot, OnAOIRotChange);
            m_data.RegisterAttribute(AOIAttrDefine.TruthSpeed, OnAOITruthSpeedChange);
            m_data.RegisterAttribute(AOIAttrDefine.Faction, OnAOIFactionChange);
            m_data.RegisterAttribute(AOIAttrDefine.OfflineRot, OnAOIRotChange);
        }

        /// <summary> AOI [坐标] 变化 </summary>
        /// 物件直接设置位置坐标即可 
        protected override void OnAOIPositionChange(string key, object val)
        {
            object value = m_data.Attrs.GetProtoValue(key);
            ServerSetPosition(value, isBorn, true);
            if (isBorn)
            {
                isBorn = false;
            }
        }

        /// <summary>
        /// 1客户端控制
        /// 2dl自己计算的相信
        /// 3第三方的默认idle
        /// </summary>
        public void ForceSetDefaultState()
        {
            // SGF.Debuger.LogError("To_Default_State");
            M_eSubState = ObjectSubState.Idle;
        }

        internal override void MoveByServer(Vector3 pos, bool isBornOrForceSet)
        {
            if (isBornOrForceSet)
            {
                DoForceMove?.Invoke(pos);
                //ActionOnBirthPos?.Invoke(pos);
            }
            m_currentPos = pos;
        }

        internal override void MoveByServerNew(Vector3 pos, bool ServerForce, bool isBorn)
        {
            /// 2024/1/31
            /// 召唤物 都不需要关系 出生点嘛？？？？
            if (ServerForce || isBorn)
            {
                DoForceMove?.Invoke(pos);
                //ActionOnBirthPos?.Invoke(pos);
            }
            ServerPosition = pos;
            m_currentPos = pos;
        }

        #region 动画状态机

        public void ChangeState(ObjectSubState newState, bool canSetSameState = false)
        {
            if (M_eSubState != newState || canSetSameState)
            {
                M_eSubState = newState;
            }
        }

        #endregion

        //#endregion
        protected override void Release()
        {
            base.Release();
            ActionModelVisiable = null;
            ActionModelFesnel = null;
            ActionModelInteractiveEff = null;
            ViewFactory.ReleaseView(this);
        }

    }

}
