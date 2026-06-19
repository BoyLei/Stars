using System;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProjectDef;
using UnityEngine;


namespace StarProject.Game.Entity
{
    /// <summary>
    /// 提出 这个 EntityLocalStatic 的原因是 EntityLocalDynamic 中包含了太多 不必要的数据
    /// </summary>
    public abstract class EntityLocalStatic : EntityObject
    {
        protected string TagFlag = "EntityLocalStatic";

        protected GameObject m_container;
        public GameObject Container { get { return m_container; } }

        public abstract E_LocalEntityType LocalEntityType { get; }

        public ulong EntityID;

        private Vector3 m_CurrentPos;

        public ModelDataCell modelDataCell;
        public AvatarDataCell avatarDataCell;

        public int BaseAnimID
        {
            get
            {
                if (avatarDataCell == null)
                {
                    return 0;
                }
                return avatarDataCell.BaseAnims;
            }
        }

        /// <summary>
        /// 模型动作配置
        /// </summary> 
        protected ModelAnimancerDataCell AnimCfg
        {
            get
            {
                if (avatarDataCell == null)
                {
                    return null;
                }
                return LocalDataManager.Instance.GetModelAnimancerDataCell(BaseAnimID);
            }
        }

        private float m_modleScale = 1f;

        /// <summary>
        /// 模型scale, 子类 可以自定义对应的 模型scale
        /// </summary>
        public virtual float ModleScale { get => m_modleScale; }

        private int m_avatarID = 0;
        public int AvatarID { get => m_avatarID; }

        /// <summary>
        /// 模型的路径, 需要子类去实现 
        /// </summary>
        public abstract string ModelPath { get; }

        public Action<I_FxParam, string, string> ActionOnPlaySpecialEffects;    // 播放特效
        public Action<I_FxParam, string> ActionOnStopSpecialEffects;    // 关闭特效
        public Func<I_AnimParam, string, VitalState> PlayAnimationAction;

        public Action<string> SetIdleAnimation; // 设置实体模型idle动作文件名

        public Action<ulong, float, float> ActionOnTurn2Target; // 始终转向目标

        public Action<ulong> ActionFollowTargetForward; // 跟随目标的 朝向

        public Action ActionOnViewCreateFinifh; // 显示层加载完成


        public virtual void Create(ulong id, Vector3 pos)
        {
            EntityID = id;

            SetPosition(pos);

            ActionOnViewCreateFinifh += OnActionOnViewCreateFinish;

        }

        protected virtual void Reset()
        {
            EntityID = 0;

            modelDataCell = null;
            avatarDataCell = null;
            m_modleScale = 1.0f;


            ActionOnPlaySpecialEffects = null;
            ActionOnStopSpecialEffects = null;
            PlayAnimationAction = null;
            SetIdleAnimation = null;

        }

        protected override void Release()
        {
            ViewFactory.ReleaseView(this);

            Reset();

            base.Release();

        }

        /// <summary>
        /// 加载模型完成
        /// </summary>
        protected virtual void OnActionOnViewCreateFinish()
        {
            if (AnimCfg != null && AnimCfg.Idle != null)
            {
                SetIdleAnimation?.Invoke(AnimCfg.Idle);
            }
        }


        public void SetPosition(Vector3 pos)
        {
            m_CurrentPos = pos;
        }
        public override Vector3 Position()
        {
            return m_CurrentPos;
        }

        /// <summary>
        /// 创建一个 本地实体的显示容器的 根节点, 默认放在 RemoveRoot 的子节点下
        /// </summary>
        /// <param name="containerParent">容器的 parent</param>
        public void CreateMContainer(Transform containerParent = null)
        {
            m_container = new GameObject(TagFlag);

            if (containerParent == null)
            {

                m_container.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
            }
            else
            {
                m_container.transform.SetParent(containerParent, false);
            }
        }

        /// <summary>
        /// 刷新 avatarID 以及模型 相关的配置
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
            m_modleScale = avatarDataCell.GetModelScaling() / 100f;
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
                SGF.Debuger.LogWarning($"{TagFlag} Create() EnityId=>{EntityID},APpearanceID={avatarID},modelDataCell=null");
            }
        }

        public void CreateView(int avatarID, Transform containerParent = null)
        {
            m_avatarID = avatarID;

            if (m_avatarID == 0)
            {
                return;
            }

            CreateMContainer(containerParent);

            RefreshAvatarID(avatarID);
            ViewFactory.CreateViewAsync(ModelPath, this, m_container.transform, null);
        }




        #region 播放动画

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

        public void PlayAnimation(string animationName)
        {
            PlayAnimation(GetAnimParam(animationName));
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
                SGF.Debuger.LogWarning($"{TagFlag} PlaySpecialEffect() EnityId=>{EntityID}, avatarData=null");
                return;
            }

            FxParam fxParam = new();
            fxParam.InitWithLogic(EffectPath, _hangPoint);
            ActionOnPlaySpecialEffects?.Invoke(fxParam, key, avatarData.EffectsPath);
        }

        /// <summary>
        /// 根据 fxParam 播放特效
        /// </summary>
        /// <param name="fxParam"></param>
        /// <param name="isCreate">是创建还是 关闭</param>
        public void PlaySpecialEffect(FxParam fxParam, bool isCreate)
        {

            AvatarDataCell fxAvatarData = avatarDataCell;
            if (fxParam.BuilderID != EntityID)
            {
                AvatarDataCell model = GameManager.Instance.GetEntityAvatarById(fxParam.BuilderID);
                if (model != null)
                {
                    fxAvatarData = model;
                }
            }
            if (fxAvatarData == null)
            {
                SGF.Debuger.LogWarning($"{TagFlag} PlaySpecialEffect PlayerEnityId=>{fxParam.BuilderID},i_FxParam={fxParam.EffectName},avatarData=null");
                return;
            }

            if (isCreate)
            {
                ActionOnPlaySpecialEffects?.Invoke(fxParam, "", fxAvatarData.EffectsPath);
            }
            else
            {
                ActionOnStopSpecialEffects?.Invoke(fxParam, "");
            }
        }

        #endregion

        public void Turn2Target(ulong targetEntityID, float trunLerpTime, float totalTime)
        {
            ActionOnTurn2Target?.Invoke(targetEntityID, trunLerpTime, totalTime);
        }

    }


}