using SGF.Module.Framework;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Player.Component;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Entity
{
    [XLua.LuaCallCSharp]
    public abstract class EntityLocalDynamic : EntityObject
    {
        protected string TagFlag = "EntityLocalDynamic";

        protected GameObject m_container;
        public GameObject Container { get { return m_container; } }


        public string EntityKey = string.Empty; // 特殊的实体key

        public E_LocalEntityType Type;
        public int GetTypeInt()
        {
            return (int)Type;
        }

        public long ID;

        public string TriggerKey;

        private Vector3 m_StartPos;
        public void SetStartPosition(Vector3 pos)
        {
            m_StartPos = pos;
        }
        public Vector3 StartPosition()
        {
            return m_StartPos;
        }

        private Vector3 m_CurrentPos;
        public void SetPosition(Vector3 pos)
        {
            m_CurrentPos = pos;
        }
        public override Vector3 Position()
        {
            return m_CurrentPos;
        }

        private float EulerAngles;
        public float GetEulerAngles()
        {
            return EulerAngles;
        }


        private ObjectSubState m_eSubState = ObjectSubState.Idle;   //当前状态
        public ObjectSubState M_eSubState
        {
            get => m_eSubState;
            set
            {
                m_eSubState = value;
            }
        }

        /// <summary>
        /// 作为Player实体，有些功能需要用组合的方式去实现，从而需要有组件的概念
        /// 并不是每一个Entity都会有组件的
        /// </summary>
        public List<PlayerComponent> m_listCompoent = new();

        public bool IsEntryTrigger = false; // 是否进入触发
        private bool isHide = false;  // 是否隐藏

        public bool IsHide
        {
            get
            {
                return isHide;
            }
            set
            {
                if (!string.IsNullOrEmpty(EntityKey))
                {
                    //SGF.Debuger.LogError($"通缉实体 设置 EntityKey={EntityKey},value={value}");
                }
                isHide = value;
            }
        }

        public ModelDataCell modelDataCell;
        public AvatarDataCell avatarDataCell;
        public float ModleScale = 1.0f;

        ////// ------------ 交互物件配置信息
        public InteractDataCell interactDataCell;
        public int SpecialEffect;
        public string EffectAddress;
        public float Range;
        public string ObjectName;
        public int m_AssetIndex;
        public string EndinterIdle;
        ////// ------------ 交互物件配置信息

        public Action<bool> ActionModelFesnel;	// 给模型菲涅尔效果
        public Action<bool> ActionModelInteractiveEff;  // 交互提示特效
        public Action<I_FxParam, string, string> ActionOnPlaySpecialEffects;    // 播放特效
        public Action<I_FxParam, string> ActionOnStopSpecialEffects;    // 关闭特效
        public Action<bool> ActionOnHidden;   // 模型隐藏

        public Func<I_AnimParam, string, VitalState> PlayAnimationAction;
        public Action<string> SetIdleAnimation; // 设置实体模型idle动作文件名

        public Action ActionOnViewCreateFinish; // 显示层加载完成

        public virtual void Create(string entityKey, E_LocalEntityType type, long id, string triggerKey, Vector3 pos, float scale)
        {
            EntityKey = entityKey;
            Type = type;
            ID = id;
            TriggerKey = triggerKey;
            ActionOnViewCreateFinish += OnActionOnViewCreateFinish;

            SetPosition(pos);
            RandomEulerAngles();
        }

        protected virtual void Reset()
        {
            EntityKey = string.Empty;
            Type = E_LocalEntityType.None;
            ID = 0;
            TriggerKey = string.Empty;
            IsEntryTrigger = false;
            IsHide = false;

            m_eSubState = ObjectSubState.Idle;

            modelDataCell = null;
            avatarDataCell = null;
            ModleScale = 1.0f;

            interactDataCell = null;
            SpecialEffect = -1;
            EffectAddress = string.Empty;
            Range = 1.0f;
            ObjectName = string.Empty;
            m_AssetIndex = -1;
            EndinterIdle = string.Empty;

            ActionModelFesnel = null;
            ActionModelInteractiveEff = null;
            ActionOnPlaySpecialEffects = null;
            ActionOnStopSpecialEffects = null;
            ActionOnHidden = null;
            PlayAnimationAction = null;
            SetIdleAnimation = null;
            ActionOnViewCreateFinish = null;
        }

        protected override void Release()
        {
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].Release();
            }
            m_listCompoent.Clear();

            base.Release();
        }

        public virtual void EnterFrame(int frameIndex)
        {
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                m_listCompoent[i].EnterFrame(frameIndex);
            }
        }

        // 创建显示层容器
        protected void CreateMContainer()
        {
            m_container = new GameObject(TagFlag);
            m_container.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
        }

        /// <summary>
        /// 壳子加载完成
        /// </summary>
        protected virtual void OnTemplateCreateFinifh(GameObject gob)
        {

        }

        /// <summary>
        /// 加载模型完成
        /// </summary>
        protected virtual void OnActionOnViewCreateFinish()
        {
            CompoentInitRefreshState();
        }

        /// <summary>
        /// 异步加载完成后初始化刷新状态
        /// </summary>
        protected void CompoentInitRefreshState()
        {
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                PlayerComponent playerComponent = m_listCompoent[i];
                if (playerComponent != null)
                {
                    MonoBehaviour view = playerComponent.GetView();
                    if (view != null)
                    {
                        playerComponent.InitRefreshState();
                    }
                }
            }
        }

        /// <summary>
        /// 刷新 avatarID
        /// </summary>
        /// <param name="avatarID"></param>
        protected void RefreshAvatarID(int avatarID)
        {
            // 刷新 模型的时候，如果之前的模型配置了 音频bank, 那就先卸载之前的 音频bank
            if (avatarDataCell != null && avatarDataCell.SoundBank != null)
            {
                SoundManager.Instance.UnLoadBank(avatarDataCell.SoundBank);
            }

            avatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(avatarID);
            ModleScale = avatarDataCell.GetModelScaling() / 100f;
            if (avatarDataCell != null)
            {
                modelDataCell = LocalDataManager.Instance.GetModelDataCell(avatarDataCell.GetModelId());
                SoundManager.Instance.LoadBank(avatarDataCell.SoundBank);
            }
            else
            {
                SGF.Debuger.LogError($"{TagFlag} Create() avatarId={avatarID},avatarDataCell=null  err!!!");
            }
            if (modelDataCell == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} Create() [{Type}] EnityId=>{ID},TriggerKey={TriggerKey},APpearanceID={avatarID},modelDataCell=null");
            }
        }

        public void SetHide(bool hide)
        {
            //SGF.Debuger.LogWarning($"{TagFlag} 通缉任务 单个 显隐 1111 hide={hide},IsHide={IsHide},TriggerKey={TriggerKey}");

            if (IsHide == hide)
            {
                return;
            }
            //SGF.Debuger.LogWarning($"{TagFlag} 通缉任务 单个 显隐 2222 hide={hide},IsHide={IsHide},TriggerKey={TriggerKey}");

            IsHide = hide;
            // 隐藏按钮框
            if (IsHide)
            {
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnRefeshLocalObjectInteractive", new object[] { (int)Type, 0, TriggerKey, EntityKey });
            }
            ActionOnHidden?.Invoke(IsHide);
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                PlayerComponent playerComponent = m_listCompoent[i];
                if (playerComponent != null)
                {
                    MonoBehaviour view = playerComponent.GetView();
                    if (view != null)
                    {
                        //bool isHide = Type == E_LocalEntityType.WantedEnity ? !IsHide : IsHide;
                        playerComponent.SetFlashHide(IsHide);
                    }
                }
            }
        }

        #region 动画状态机

        public AnimParam GetAnimParam(string AnimationName, float speed = 1)
        {
            AnimParam animParam = new();
            animParam.SetBaseAnimName(AnimationName);
            animParam.SetBaseAnimSpeed(speed);
            return animParam;
        }

        public VitalState PlayAnimation(I_AnimParam param, string animationName = "")
        {
            return PlayAnimationAction?.Invoke(param, animationName);
        }

        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="newState">新状态</param>
        /// <param name="canSetSameState">是否可以设置相同的状态</param>
        public void ChangeState(ObjectSubState newState, bool canSetSameState = false)
        {
            if (M_eSubState != newState || canSetSameState)
            {
                M_eSubState = newState;
            }
        }

        #endregion

        #region 播放特效的接口
        /// <summary>
        /// 播放 特效的接口
        /// key : 预留的一个 参数key
        /// </summary>
        public void PlaySpecialEffect(string EffectPath, HangPoint _hangPoint = HangPoint.Root, string key = "")
        {
            AvatarDataCell avatarData = avatarDataCell;
            if (avatarData == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} PlaySpecialEffect() [{Type}] EnityId=>{ID},TriggerKey={TriggerKey},Type={Type},avatarData=null");
                return;
            }

            FxParam fxParam = new();
            fxParam.InitWithLogic(EffectPath, _hangPoint);
            ActionOnPlaySpecialEffects?.Invoke(fxParam, key, avatarData.EffectsPath);
        }

        #endregion

        private void RandomEulerAngles()
        {
            EulerAngles = UnityEngine.Random.Range(0, 360);
        }



    }
}