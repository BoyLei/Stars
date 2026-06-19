using Sirenix.Utilities;
using SkillEditor;
using StarProject.Game.Data;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player.Component;
using StarProject.Game.Skill;
using StarProject.Game.TypeEffect;
using StarProject.Service.LocalData;
using StarProject.Service.WorldToUI;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Player
{
    /// <summary>
    /// 实体的控制层基类
    /// 类似主角会有三位一体这种情况，所以会有PlayerCtrlGroup
    /// 怪物会有MonsterCtrlGroup
    /// </summary>
    public abstract class EntityCtrlBase : EntityCtrlMsgBase
    {
        protected string LOG_TAG = "EntityBaseCtrl";

        public EntityBaseData entityBaseData;

        protected override EntityBaseData EntityData => entityBaseData;

        protected VitalSignData m_data;

        protected GameContext m_context;

        protected GameObject m_container;

        public GameObject Container { get { return m_container; } }
        public VitalSignData Data { get { return m_data; } }  //player,Npc
        public VitalSignViewShowData ViewEnityData { get { return m_data.viewEnityData; } }//Model
        public int TeamId { get { return m_data.teamId; } }
        public float HitDistance { get { return m_data.viewEnityData.size * m_data.viewEnityData.viewScale; } }

        public E_EntityType EntityType { get { return entityBaseData.EntityType; } }

        protected Vector3 m_createPos = Vector3.zero;   // 创建坐标

        /// <summary>
        /// 作为Player实体，有些功能需要用组合的方式去实现，从而需要有组件的概念
        /// 并不是每一个Entity都会有组件的
        /// </summary>
        public List<PlayerComponent> m_listCompoent = new();

        /// <summary>
        /// 抽象逻辑层Enity
        /// </summary>
        public abstract AOIEntityObject M_Curr { get; }


        public UnityEngine.Vector3 M_InputMoveDirection = new();
        /// <summary> 【主角自己】技能切遥感位移的【强制时间（毫秒）】 </summary>
        public float M_MainPlayerChangeMoveForceTime = 0f;
        public bool M_UserSkillTag = false;

        private bool m_IsHide = false;
        public bool IsHid { get { return m_IsHide; } }

        // 遥感的朝向
        public UnityEngine.Vector3 MoveDirection = new();

        public bool IsServerAOI { get; internal set; }

        public virtual bool CheckCanSkillBtn(SkillContainer skillContainer)
        {
            return true;
        }

        // --------技能的委托
        public Action<SkillContainer> SkillPointerDownActions;
        public Action<SkillContainer, UnityEngine.Vector3> SkillDirChangeActions;
        public Action<SkillContainer> SkillPointerUpActions;
        public Action<SkillContainer> SkillEndDragActions;
        public Action<SkillContainer> SkillnCancelActions;
        // --------技能的委托

        /// <summary>
        /// 本地服 同步 黑板的 action
        /// </summary>
        public Action<EditorModeTest.CusBlackBoardData> ActionOnUpdateCusBlackBoard;

        public Action ActionPlayMvpAnim;

        public virtual void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        {
            entityBaseData = data;
            m_context = GameManager.Instance.Context;
            m_createPos = pos;
            IsServerAOI = entityBaseData.IsServerAOI;
            // 头顶信息移动创建坑位
            //if (Data != null && Data.isMainPlayer)
            //{
            //    SGF.Debuger.LogError($"名字测试 主角创建 1逻辑层坑位创建 EntityId={entityBaseData.M_EntityID}");
            //}
            WorldItemChecker.Instance.RegToPendRoleMaps(entityBaseData.M_EntityID);

            CreateMContainer(entityBaseData);
            Container.SetActive(false);
            // 创建消息监听
            InitMsgListen();

            GlobalEvent.OnNpcCreateFinished?.Invoke(0); 
        }

        public virtual void Release()
        {
            m_data = null;
            WorldItemChecker.Instance.UnRegToRectTransformPend(entityBaseData.M_EntityID);
            entityBaseData = null;
            m_context = null;
            m_createPos = Vector3.zero;

            //m_container = null;

            ActionPlayMvpAnim = null;
            m_IsHide = true;
            avatarChangeEffects.Clear();
            hiddenSkillSlotEffects.Clear();
            ClearMsgListen();

        }

        public abstract void EnterFrame(int frameIndex);

        //创建用来显示视图的容器

        protected abstract void CreateMContainer(EntityBaseData data);
        /// <summary> 强制同步角度（服务器角度） </summary>
        public virtual void ForceSyncRot(int rot)
        {

        }

        /// <summary> 强制同步坐标 </summary>
        public virtual void ForceSyncPos(UnityEngine.Vector3 pos)
        {
            //Debug.Log("服务器强制同步" + pos);
        }


        /// <summary>
        /// 2024/1/31
        /// DL:
        ///     同步一个 出生点 坐标, 强同步坐标 跟出生点坐标应该分离.很多时候，在 出点坐标 需要有单独的 特效表现. 
        ///     之前 曲的 代码 逻辑 太好的区分这两点。 比如 playerCtr 走的 是 forceSyncPos。
        /// </summary>
        /// <param name="pos"></param>
        protected abstract void SyncBornPos(Vector3 pos);
        protected void SyncBornPos()
        {
            SyncBornPos(m_createPos);
        }

        /// <summary> 打断自动寻路 </summary>
        public virtual void BreakFindPath()
        {
            if (M_Curr != null)
            {
                M_Curr.BreakFindPath();
            }
        }

        /// <summary>
        /// 获取交互是否能打断
        /// </summary>
        /// <returns></returns>
        public bool GetInterCanBreak()
        {
            ulong value = M_Curr.CurInteractID;
            ulong uid = Convert.ToUInt64(value);
            if (uid > 0)
            {
                //交互中 
                var entity = (ObjectCtrlGroup)GameManager.Instance.GetEntityCtr(uid);
                if (entity != null)
                {
                    InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(entity.ConfigID);
                    if (interactDataCell != null)
                    {
                        //交互能否被打断
                        return interactDataCell.GetCanBreak();
                    }
                }
            }

            return true;
        }

        public virtual void BreakCreate()
        {

        }

        public virtual void BreakInteract()
        {

        }

        /// <summary>
        /// 壳子加载完成
        /// </summary>
        protected virtual void OnTemplateCreateFinifh(GameObject gob)
        {
            if (gob == null)
            {
                return;
            }
            //目前只有一个角色
            //CurCtrlPlayerFxRoot = m_container.transform.Find("Character_Model/ModelOffset/FxRoot");
        }

        /// <summary>
        /// 加载模型完成
        /// </summary>
        protected virtual void OnActionOnViewCreateFinifh()
        {
            //【我，其他人】放置在出生坐标 异步加载完后执行
            if (m_createPos.x != -999)//AOI的Default，不能进
            {
                // SyncBornPos(m_createPos);//客户端本地创建的，默认负数（000有用），则不进
            }

            Container.SetActive(true);
            CompoentInitRefreshState();
        }


        /// <summary>
        /// 组件显示委托回调
        /// </summary>
        /// <param name="isHide"></param>
        protected void OnCompoentShowAction(bool isHide)
        {
            m_IsHide = isHide;
            for (int i = 0; i < m_listCompoent.Count; ++i)
            {
                PlayerComponent playerComponent = m_listCompoent[i];
                if (playerComponent != null)
                {
                    MonoBehaviour view = playerComponent.GetView();
                    if (view != null)
                    {
                        playerComponent.SetFlashHide(isHide);
                    }
                }
            }
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
        /// 获得当前服务器角度
        /// </summary>
        /// <returns></returns>
        public int GetCurLeftJoyStickAngle()
        {
            int angle = 0;

            if (M_InputMoveDirection.magnitude > 0.5f)
            {
                angle = (int)(Math.Atan2(M_InputMoveDirection.z, M_InputMoveDirection.x) * Mathf.Rad2Deg); ;
            }
            else if (M_Curr != null)
            {
                // 只要 <= 抖动范围,就拿 玩家当前的角度
                angle = M_Curr.ServerAngles;
            }
            return angle;
        }


        public virtual bool GetChatShow()
        {
            return false;
        }



        #region BUFF效果处理

        /// <summary>
        /// 栈结构的 globalShow 效果
        /// </summary>
        private Dictionary<GlobalShowType, List<BaseTypeEffect>> S_GlobalShowEffects = new();



        /// <summary>
        /// 处理 类似于栈 结构的效果. 
        /// 每一个新效果 都是入栈. 只有当前 在栈顶的 效果才会生效.
        /// 
        /// 类似于变身 这种效果， 它的效果 都是 采用最后的一个效果，依次出栈.
        /// 
        /// 对于 CharStateController 里面的 shader效果来说, 也是 同时只有一个 效果生效.
        /// </summary>
        public void HandleStackEffect(BaseTypeEffect effect, TypeEffectUpdateType updateType)
        {
            GlobalShowType showType = effect.ShowType;
            switch (updateType)
            {
                case TypeEffectUpdateType.OnEnter:
                    {
                        HandleEnterStatckEffect(showType, effect);
                    }
                    break;
                case TypeEffectUpdateType.OnExit:
                    {
                        HandleExitStatckEffect(showType, effect);
                    }
                    break;
                case TypeEffectUpdateType.OnUpdate:
                    {
                        HandleUpdateStatckEffect(showType, effect);
                    }
                    break;
                default: break;
            }

        }

        private BaseTypeEffect GetCurShowTypeEffect(List<BaseTypeEffect> baseTypeEffects)
        {
            return baseTypeEffects.Count > 0 ? baseTypeEffects[baseTypeEffects.Count - 1] : null;
        }

        private bool IsCurShowTypeEffect(List<BaseTypeEffect> baseTypeEffects, BaseTypeEffect effect)
        {

            return baseTypeEffects.Count > 0 && baseTypeEffects[baseTypeEffects.Count - 1] == effect;
        }

        /// <summary>
        /// 处理效果 进入的 接口
        /// </summary>
        /// <param name="showType"></param>
        /// <param name="effect"></param>
        private void HandleEnterStatckEffect(GlobalShowType showType, BaseTypeEffect effect)
        {
            if (!S_GlobalShowEffects.TryGetValue(showType, out var baseTypeEffects))
            {
                baseTypeEffects = new();
                S_GlobalShowEffects.Add(showType, baseTypeEffects);
            }
            var curEffect = GetCurShowTypeEffect(baseTypeEffects);

            baseTypeEffects.Add(effect);

            M_Curr.ActionOnStackShowTypeChange?.Invoke(TypeEffectUpdateType.OnEnter, effect, curEffect);
        }

        private void HandleExitStatckEffect(GlobalShowType showType, BaseTypeEffect effect)
        {
            if (S_GlobalShowEffects.TryGetValue(showType, out var baseTypeEffects))
            {
                // 如果是 栈顶的 效果,那就 删除这个效果 并且触发下个效果的刷新
                bool isTriggerRefresh = IsCurShowTypeEffect(baseTypeEffects, effect);

                // 先刷新, 再出发
                // 如果准备死亡了, 那当前的表现标签先保留， 需要在死亡动作的时候 依旧保持这个shder效果
                if (!M_Curr.IsReadyDead && baseTypeEffects.Remove(effect) && isTriggerRefresh)
                {
                    M_Curr.ActionOnStackShowTypeChange?.Invoke(TypeEffectUpdateType.OnExit, effect, null);
                }
            }
        }

        private void HandleUpdateStatckEffect(GlobalShowType showType, BaseTypeEffect effect)
        {
            if (S_GlobalShowEffects.TryGetValue(showType, out var baseTypeEffects))
            {
                // 如果是 栈顶的 效果,那就 删除这个效果 并且触发下个效果的刷新
                bool isTriggerRefresh = IsCurShowTypeEffect(baseTypeEffects, effect);

                // 先刷新, 再出发
                if (isTriggerRefresh)
                {
                    M_Curr.ActionOnStackShowTypeChange?.Invoke(TypeEffectUpdateType.OnUpdate, effect, null);
                }
            }
        }

        /// <summary>
        /// 复活的话,清除身上的shader 效果
        /// </summary>
        public void HandleOnRevive()
        {
            S_GlobalShowEffects.ForEach((item) =>
            {
                var effect = GetCurShowTypeEffect(item.Value);
                M_Curr.ActionOnStackShowTypeChange?.Invoke(TypeEffectUpdateType.OnExit, effect, null);
            });
            S_GlobalShowEffects.Clear();
        }

        #region shader state 变化的 效果
        public void OnShaderStateChange(CharStateController.CharacterState characterState, bool isShow)
        {

            if (M_Curr == null)
            {
                return;
            }
            M_Curr.OnCharStateChange?.Invoke(characterState, isShow);
        }

        #endregion

        #region BUFF【冰冻】

        /// <summary>
        /// 冰冻，恢复模型
        /// </summary>
        /// <param name="free"></param>
        public void Freez(bool free)
        {
            if (M_Curr == null)
            {
                return;
            }
            M_Curr.FreezModel?.Invoke(free);
        }

        //public void Translucent(bool change)
        //{
        //    if (M_Curr == null)
        //    {
        //        return;
        //    }
        //    M_Curr.ShaderChange?.Invoke(SkillEditor.ShaderEnum.Translucent, change);
        //}


        /// <summary>
        /// 外发光 接口
        /// </summary>
        /// <param name="change"></param>
        /// <param name="edgeLightData"></param>
        public void EdgeLight(bool change, TypeEffect.BaseTypeEffect edgeLightData)
        {
            // 外发光的效果
            if (M_Curr == null)
            {
                return;
            }
            M_Curr.ShaderChange2?.Invoke(change, edgeLightData);
        }

        /// <summary>
        /// 暂停，恢复动画
        /// </summary>
        /// <param name="pause"></param>
        public void PauseAniamtion(bool pause)
        {
            if (M_Curr == null)
            {
                return;
            }
            M_Curr.PauseAnimation?.Invoke(pause);
        }

        #endregion

        #region BUFF【改变状态动画】

        /// <summary>
        /// 注册动作替换数组
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="changeAnims"></param>
        public void RegisterChangeAnim(string tag, List<SkillEditor.ChangeAnim> changeAnims)
        {
            Data.RegisterChangeAnim(tag, changeAnims);
        }

        /// <summary>
        /// 取消动作替换数组
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="changeAnims"></param>
        public void UnRegisterChangeAnim(string tag, List<SkillEditor.ChangeAnim> changeAnims)
        {
            Data.UnRegisterChangeAnim(tag, changeAnims);
        }

        #endregion

        #region BUFF【隐身】
        public void SetModelVisiable(bool isShow)
        {
            if (M_Curr == null)
            {
                return;
            }
            //M_Curr.ActionModelVisiableTime?.Invoke(isShow, true, 0, "BuffModelVisiable");
            if (isShow)
            {
                M_Curr.ActionOnStopHidden?.Invoke(false);
            }
            else
            {
                M_Curr.ActionOnStartHidden?.Invoke(false);
            }
        }

        #endregion

        #region BUFF【影子】
        public virtual void CreateShadowView(string path)
        {
            (M_Curr as NPCEntityBase).CreateShadowView(m_container.transform, path);
        }

        public void HiddenShadowView()
        {
            (M_Curr as NPCEntityBase).HiddenShadowView();
        }

        #endregion

        #region BUFF【镜像影子】

        public virtual void CreateMirrorShadowView(string path)
        {
            (M_Curr as NPCEntityBase).CreateMirrorShadowView(m_container.transform, path);
        }

        public void HiddenMirrorShadowView()
        {
            (M_Curr as NPCEntityBase).HiddenMirrorShadowView();
        }

        #endregion

        #region 播放 lineRender 效果 接口

        /// <summary>
        /// 控制 start 播放  到 target 目标的 lineRender 效果.
        /// </summary>
        /// <param name="lineRenderCfg"></param>
        /// <param name="starts"></param>
        /// <param name="targets"></param>
        /// <param name="startTime"></param>
        /// <param name="tagKey">此次效果 的唯一 tagKey. 它用来追踪 这一批 lineRender  </param> 
        public void ControlPlayLineRender(LineRendererConfig lineRenderCfg, List<ulong> starts, List<ulong> targets, int startTime, string tagKey)
        {
            if (starts.Count > 0 && targets.Count > 0)
            {
                starts.ForEach((starEtt) =>
                {
                    var ett = GameManager.Instance.GetEntityCtr(starEtt);
                    if (ett != null)
                    {
                        ett.PlayLineRender(lineRenderCfg, targets, startTime, tagKey);
                    }
                });

            }
        }

        /// <summary>
        /// 控制 关闭 lineRender. 
        /// </summary>
        /// <param name="lineRenderCfg"></param>
        /// <param name="starts">具体 关闭的 lineRender 的 出发点</param>
        /// <param name="targets">具体 关闭的 lineRender 的 结束点</param>
        /// <param name="tagKey">tagKey 用来 确定 要关的 是 那一批的 lineRender[不同的 tagKey 会产生 不同的 新的 lineRender 节点]</param>
        public void ControlCloseLineRender(LineRendererConfig lineRenderCfg, List<ulong> starts, List<ulong> targets, string tagKey)
        {
            if (starts.Count > 0 && targets.Count > 0)
            {
                starts.ForEach((starEtt) =>
                {
                    var ett = GameManager.Instance.GetEntityCtr(starEtt);
                    if (ett != null)
                    {
                        ett.StopPlayLineRender(lineRenderCfg, targets, tagKey);
                    }
                });
            }
        }

        /// <summary>
        /// 自己 播放 lineRender 的接口
        /// </summary>
        public void PlayLineRender(LineRendererConfig lineRenderCfg, List<ulong> targets, int startTime, string tagKey)
        {
            (M_Curr as NPCEntityBase).PlayLineRender(lineRenderCfg, targets, startTime, tagKey);
        }

        /// <summary>
        /// 自己 停止 播放 lineRender
        /// </summary>
        /// <param name="lineRenderCfg"></param>
        /// <param name="targets"></param>
        /// <param name="tagKey"></param>
        public void StopPlayLineRender(LineRendererConfig lineRenderCfg, List<ulong> targets, string tagKey)
        {
            (M_Curr as NPCEntityBase).StopLineRenders(lineRenderCfg, tagKey);
        }

        #endregion

        #region 变身效果刷新 Avatar 
        /// <summary>
        /// 变身效果 可能有多个, 目前 夏哥的意思, 是后一个变身效果 会替换前一个.
        /// 比如 大招变身,然后 变羊.
        /// </summary>
        private List<AvatarChangeEffect> avatarChangeEffects = new();

        /// <summary>
        /// 开始 一个 变换 avatar 的效果
        /// </summary>
        /// <param name="avatarChangeEffect"></param> 
        public void StartChangeAvatarEffect(AvatarChangeEffect avatarChangeEffect)
        {
            avatarChangeEffects.Add(avatarChangeEffect);
            (M_Curr as NPCEntityBase).RefreshAvatarEffect(avatarChangeEffect);
        }

        public void StopChangeAvatarEffect(AvatarChangeEffect avatarChangeEffect)
        {
            // 如果stop 的是 最后一个标签,才需要 通知刷新
            bool isPostRefresh = avatarChangeEffects.Count > 0 && avatarChangeEffects[avatarChangeEffects.Count - 1] == avatarChangeEffect;

            if (avatarChangeEffects.Remove(avatarChangeEffect) && isPostRefresh)
            {
                if (avatarChangeEffects.Count > 0)
                {
                    (M_Curr as NPCEntityBase).RefreshAvatarEffect(avatarChangeEffects[avatarChangeEffects.Count - 1]); ;
                }
                else
                {
                    (M_Curr as NPCEntityBase).RefreshAvatarEffect(null); ;
                }
            }
        }

        #endregion


        #region  隐藏技能槽的效果, 一般跟变身效果在一起, 所以需要考虑多个 隐藏技能槽效果的情况
        private List<HiddenSkillSlotEffect> hiddenSkillSlotEffects = new();

        /// <summary>
        /// 隐藏技能槽的效果 先屏蔽 不处理.
        /// 原始设计:
        ///     变身会有2种 技能标签:
        ///         1.隐藏对应的 技能槽位;
        ///         2.刷新对应槽位的技能(需要将这个技能显示).
        ///     但是发现在判断 一个槽位是否 需要显示的时候,如果变身之后, 又收到了一个 隐藏槽位的buf.
        ///     变身隐藏 [1,2,3], 刷新 [4] . buff 隐藏 [4].
        ///     由于不同同一个 效果标签,  隐藏标签 [4] 会去 替代变身隐藏标签 [1,2,3].
        ///     所以 此时 存在两个标签, 刷新标签 [4] 和 隐藏标签 [4] . 
        ///     按 gl 一开始的意思, 只要有 刷新技能标签, 不管什么情况,都要显示(比如新手引导变身,槽位未开启,仍需要显示变身技能).
        ///     此时, 技能刷新标签 需要显示，但是 隐藏标签又需要隐藏。 没有优先级， 也没办法通过时间先后判断.
        ///  
        /// note:
        ///     所以, 改了设计。 变身效果 不再去 设置隐藏标签. 刷新技能 标签 改为针对所有的 技能槽. 如果未配置，就默认隐藏 技能槽.
        /// </summary>
        /// <param name="hiddenSkillSlotEffect"></param> 
        public void StartHiddenSkillSlotEffects(HiddenSkillSlotEffect hiddenSkillSlotEffect)
        {
            return;// 先屏蔽
            hiddenSkillSlotEffects.Add(hiddenSkillSlotEffect);
            (M_Curr as NPCEntityBase).RefreshHiddenSkillSlotEffect(hiddenSkillSlotEffect);
        }

        public void StopHiddenSkillSlotEffects(HiddenSkillSlotEffect hiddenSkillSlotEffect)
        {
            return;// 先屏蔽
            bool isPostRefresh = hiddenSkillSlotEffects.Count > 0 && hiddenSkillSlotEffects[hiddenSkillSlotEffects.Count - 1] == hiddenSkillSlotEffect;

            if (hiddenSkillSlotEffects.Remove(hiddenSkillSlotEffect) && isPostRefresh)
            {
                if (hiddenSkillSlotEffects.Count == 0)
                {
                    (M_Curr as NPCEntityBase).RefreshHiddenSkillSlotEffect(null);
                }
                else
                {
                    (M_Curr as NPCEntityBase).RefreshHiddenSkillSlotEffect(hiddenSkillSlotEffects[hiddenSkillSlotEffects.Count - 1]);
                }
            }

        }

        #endregion

        #region 变身技能 量普变化的效果, 一般跟变身技能绑在一起,需要考虑多个的情况. 不过作为效果标签, 都可以单独使用
        //量普效果标签
        private List<SpectralChangeEffect> spectralChangeEffects = new();

        public void StartSpectralChangeEffect(SpectralChangeEffect spectralChangeEffect)
        {
            spectralChangeEffects.Add(spectralChangeEffect);
            (M_Curr as NPCEntityBase).RefreshSpectralChangeEffect(spectralChangeEffect);
        }

        public void StopSpectralChangeEffect(SpectralChangeEffect spectralChangeEffect)
        {
            bool isPostRefresh = spectralChangeEffects.Count > 0 && spectralChangeEffects[spectralChangeEffects.Count - 1] == spectralChangeEffect;

            if (spectralChangeEffects.Remove(spectralChangeEffect) && isPostRefresh)
            {
                if (spectralChangeEffects.Count == 0)
                {
                    (M_Curr as NPCEntityBase).RefreshSpectralChangeEffect(null);
                }
                else
                {
                    (M_Curr as NPCEntityBase).RefreshSpectralChangeEffect(spectralChangeEffects[spectralChangeEffects.Count - 1]);
                }
            }

        }
        #endregion

        #region 变身技能 技能变化的效果, 一般跟变身技能绑在一起,需要考虑多个的情况. 不过作为效果标签, 都可以单独使用
        private List<ChangeSkillEffect> ChangSkillEffects = new();

        public void StartChangSkillEffect(ChangeSkillEffect changSkillEffect)
        {
            ChangSkillEffects.Add(changSkillEffect);
            (M_Curr as NPCEntityBase).RefreshChangeSkillSlots(changSkillEffect);
        }

        public void StopChangSkillEffect(ChangeSkillEffect changSkillEffect)
        {
            bool isPostRefresh = ChangSkillEffects.Count > 0 && ChangSkillEffects[ChangSkillEffects.Count - 1] == changSkillEffect;

            if (ChangSkillEffects.Remove(changSkillEffect) && isPostRefresh)
            {
                if (ChangSkillEffects.Count == 0)
                {
                    (M_Curr as NPCEntityBase).RefreshChangeSkillSlots(null);
                }
                else
                {
                    (M_Curr as NPCEntityBase).RefreshChangeSkillSlots(ChangSkillEffects[ChangSkillEffects.Count - 1]);
                }
            }

        }
        #endregion

        #region  半透效果, 策划gl要求半透效果 也需要考虑多个的情况
        private List<TranslucentEffect> TranslucentEffects = new();

        public void StartTranslucentEffect(TranslucentEffect effect)
        {
            TranslucentEffects.Add(effect);
            (M_Curr as NPCEntityBase).OnTranslucentEffect(effect, true);
        }

        /// <summary>
        /// 更新当前的半透效果, 半透效果存在多个, 所以 更新的 是的当前的最后添加 一个
        /// </summary>
        /// <param name="effect"></param>
        public void UpdateTranslucent(TranslucentEffect effect)
        {
            // 更新的是  最新的半透效果
            if (TranslucentEffects.Count > 0 && TranslucentEffects[TranslucentEffects.Count - 1] == effect)
            {
                (M_Curr as NPCEntityBase).OnTranslucentEffect(effect, true);
            }
        }

        public void StopTranslucentEffect(TranslucentEffect effect)
        {
            bool isPostRefresh = TranslucentEffects.Count > 0 && TranslucentEffects[TranslucentEffects.Count - 1] == effect;

            if (TranslucentEffects.Remove(effect) && isPostRefresh)
            {
                if (TranslucentEffects.Count == 0)
                {
                    (M_Curr as NPCEntityBase).OnTranslucentEffect(effect, false);
                }
                else
                {
                    (M_Curr as NPCEntityBase).OnTranslucentEffect(TranslucentEffects[TranslucentEffects.Count - 1], false);
                }
            }
        }
        #endregion

        #endregion

        #region  创建本地召唤物逻辑
        public void OnCreateSimulateSummon(ulong runtimeID, string key, Vector3 pos, int avatarID, bool isFollowRotate)
        {
            (M_Curr as NPCEntityBase).CreateSimulateSummon(runtimeID, key, pos, avatarID, isFollowRotate);
        }

        /// <summary>
        /// 本地召唤物角度的偏转
        /// </summary>
        /// <param name="angle"></param> <summary>
        public void OnSimulateSummonTurn(ulong runtimeID, int angle, bool isFollowRotate, bool isPlay)
        {
            (M_Curr as NPCEntityBase).TurnSimulateSummon(runtimeID, angle, isFollowRotate, isPlay);
        }

        #endregion
    }
}