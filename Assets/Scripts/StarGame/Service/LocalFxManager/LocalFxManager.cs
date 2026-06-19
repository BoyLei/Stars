using SGF.UI.Framework;
using SGF.Unity;
using Sirenix.Utilities;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Service.Cam;
using StarProject.Service.LocalData;
using StarProject.Service.LocalDynamic.Fx;
using StarProject.Service.Resource;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StarProject.Service.LocalDynamic
{
    /// <summary>
    /// 这里主要处理。本地动态
    /// 特点：特效复用性不强，不同于gob的子弹，所以加载
    /// </summary>
    public class LocalFxManager : SceneResourceManager<LocalFxManager>
    {
        // 最大的 特效数量, 目前博哥 定下来的是 50 个
        public int MaxFxCount = 30;
        public int MiddleFxCount
        {
            get
            {
                return (int)(MaxFxCount * 0.8);

            }
        }
        public Transform FxSceneRoot;//特效不基于场景：人身上能挂，【实体特效】；【特效身上不能挂实体特效】
        Transform UIfxDynamicRoot;

        ///1【远程动态：TriggerEntityManager】实体类：用于模拟战场实体的战斗【足球经理，小地图，中地图】：可能有池化，甚至数据项的，战场小地图UI类的实体
        ///2【远程静态】：UI的赋值项的，一次数据直接赋值：那你自己Business->resource功能加载
        ///3【本地静态】，你拼到你perfab里面

        //4,本地动态:【这里】


        //UI
        //场景
        public void Init(Transform fxSceneRoot/*, Transform uiRoot*/)
        {
            CheckSingleton();
            FxSceneRoot = fxSceneRoot;
            //UIRoot = uiRoot;

            // GlobalEvent.TestEvent.AddListener((UnityEngine.Events.UnityAction<object>)((aa) =>
            // {
            //     // ClearActorFx();
            //     OnMapIdChange(GameManager.Instance.GetCurMapId(), 0);

            //     DelayInvoker.DelayInvoke(1, (DelayFunction)((args) =>
            //     {
            //         OnSceneChange((string)StarScenesManager.Instance.CurSceneName, "");
            //     }));
            // }));
        }

        public override void OnMapIdChange(int oldMapId, int newMapId)
        {
            return;//都return,这里分类需要对应通知，[有一些清理是没必要的所以删除]没必要删除，保留结构，和表里世界波纹一样需要删掉return告诉我一声
            ClearMapFxEffects(oldMapId);
        }

        public override void OnSceneChange(string oldScene, string newScene)
        {
            ClearSceneFxEffects(oldScene);
        }

        public override void OnServerIDChange(ulong oldServerID, ulong newServerID)
        {
            return;//都return,这里分类需要对应通知，[有一些清理是没必要的所以删除]没必要删除，保留结构，和表里世界波纹一样需要删掉return告诉我一声
            ClearServerIDFxEffects(oldServerID);
        }

        #region 特效相关的资源管理
        /// <summary>
        /// 特效资源的 caches
        /// </summary>
        private Dictionary<string, List<FxResourceInfo>> FxResouceCaches = new();

        private bool ContainFxCache(string path, LocalEffectTags tag, out GameEffect ge)
        {
            ge = null;
            if (!FxResouceCaches.ContainsKey(path))
            {
                return false;
            }
            List<FxResourceInfo> pool = FxResouceCaches[path];
            for (int i = 0; i < pool.Count; i++)
            {
                var fxRes = pool[i];
                if (fxRes.CheckFxCanReUse(tag))
                {
                    // 重新复用 特效池的 资源
                    ge = fxRes.ge;
                    fxRes.ReUse();
                    return true;
                }
            }
            return false;
        }

        [Obsolete("请使用 GetFxGameEffectAsync", false)]//标记该方法已弃用
        /// <summary>
        /// 获取 gameEffect 的同步接口
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        private GameEffect GetFxGameEffect(string path, LocalEffectTags tag, ResoruceReleaseType releaseType)
        {
            if (ContainFxCache(path, tag, out GameEffect ge))
            {
                return ge;
            }
            // 实例化一个 prefab
            GameObject obj = InstantiateSync(path, "Perfab/Fx/Default/0");

            // 可用缓存不存在的时候, 重新创建一个新的特效
            return CreateFxGameEffect(path, obj, releaseType);
        }

        /// <summary>
        /// 获得 GameEffect 的异步接口
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        private void GetFxGameEffectAsync(string path, LocalEffectTags tag, ResoruceReleaseType releaseType, Action<GameEffect> callBack)
        {
            // 如果有可用缓存, 直接采用缓存
            if (ContainFxCache(path, tag, out GameEffect ge))
            {
                callBack?.Invoke(ge);
                return;
            }

            // 可用缓存不存在的时候, 重新创建一个新的特效
            CreateFxGameEffectAsync(path, releaseType, callBack);
        }

        private FxResourceInfo CreateFxResource(string file_path, GameObject go, ResoruceReleaseType releaseType = ResoruceReleaseType.MapSceneAndServerID)
        {
            if (go == null)
            {
                SGF.Debuger.LogWarning("cant find file ! : " + file_path);
                return null;
            }
            //设计上就不要重复播放，迁移位置，同资源不同类型就不可以公用
            //重复出现的资源因为使用不同情况较少不合并池
            GameEffect ge = GameObjectUtils.EnsureComponent<GameEffect>(go);

            FxResourceInfo fxResourceInfo = SimpleDataFactory.InstanceData<FxResourceInfo>();

            fxResourceInfo.Create(file_path, ge, releaseType);

            //代替资源不影响分类
            if (!FxResouceCaches.ContainsKey(file_path))
            {
                FxResouceCaches.Add(file_path, new List<FxResourceInfo>());
            }
            FxResouceCaches[file_path].Add(fxResourceInfo);
            return fxResourceInfo;
        }

        private GameEffect CreateFxGameEffect(string file_path, GameObject obj, ResoruceReleaseType releaseType)
        {
            if (obj == null)
            {
                SGF.Debuger.LogWarning("cant find file ! : " + file_path);
                return null;
            }
            FxResourceInfo fxResourceInfo = CreateFxResource(file_path, obj, releaseType);

            // 每次创建一个新特效,均触发一次 特效的超时检测
            TriggerOverTimeClearFxEffects();

            return fxResourceInfo.ge;
        }

        private void CreateFxGameEffectAsync(string file_path, ResoruceReleaseType releaseType, Action<GameEffect> callBack)
        {
            LoadPrefabAsync(file_path, (prefab) =>
            {
                if (prefab == null)
                {
                    SGF.Debuger.LogWarning("cant find file ! : " + file_path);
                    callBack?.Invoke(null);
                    return;
                }
                GameObject gob = GameObject.Instantiate(prefab);

                var ge = CreateFxGameEffect(file_path, gob, releaseType);

                callBack?.Invoke(ge);
            });
        }

        #endregion

        /*/// <summary>
        /// 添加特效到世界
        /// </summary>
        /// <returns>The world effect.</returns>
        /// <param name="effectid">配置表中的id.</param>
        /// <param name="pos">出生位置.</param>
        /// <param name="scale">特效的尺寸.</param>
        //public GameEffect AddWorldEffect(int effectid, Vector3 pos, float scale)
        //{
        //    GameEffect ge = GetEffect(effectid);
        //    if (ge == null) return null;
        //    ge.transform.position = pos;
        //    ge.Play(scale);
        //    return ge;
        //}*/

        [Obsolete("请使用 AddPlayerEffectAsync", false)]//标记该方法已弃用
        /// <summary>
        /// 添加特效到本地场景特效父节点下
        /// </summary>
        /// <param name="effectName">特效文件名</param>
        /// <param name="startWorldPos">世界坐标系下的坐标</param>
        /// <param name="startWorldRotate">世界坐标系下的角度</param>
        /// <param name="localEffectTags">特效标识</param>
        /// <param name="scale">缩放</param>
        /// <param name="playTime">配置了策划说的算,大于特效时间就循环,少于就提前干了</param>
        /// <param name="isFollowMove">是否跟随移动</param>
        /// <param name="isFollowRotate">是否跟随旋转</param>
        /// <param name="startDelay">起始播放延迟</param>
        /// <param name="isLoop">是否循环播放</param>
        /// <param name="speedMultiplier">特效播放速度</param>
        /// <param name="startTime">特效开始播放的时间点</param>
        /// <returns></returns>
        public GameEffect AddEnvEffect(
            string effectName,
            Vector3 startWorldPos,
            Vector3 startWorldRotate,
            LocalEffectTags localEffectTags/*, GameObject header*//*, Vector3 offsetPos*/,
            Vector3 scale,
            float playTime = 0,
            bool isFollowMove = false,
            bool isFollowRotate = false,
            int startDelay = 0,
            bool isLoop = false,
            float speedMultiplier = 1f,
            int startTime = 0,
            bool isFollowBuilderHide = true
            )
        {
            if (!CheckEffectNeedPlay(effectName, startTime))
            {
                SGF.Debuger.LogWarning($"fx recover : effectName: {effectName}, startTime: {startTime} , skip play !!!");
                return null;
            }

            //if (header == null)
            //{
            //    return AddWorldEffect(effectid, offsetPos, scale);
            //}
            //playTime = 0;//现在不用PlayTime了暂时

            //GameEffect ge = GetEffect(effectName, LocalEffectTags.Env, playTime, (float)startDelay / 1000f);
            GameEffect ge = GetEffect(effectName, localEffectTags, ResoruceReleaseType.MapSceneAndServerID, playTime, startDelay / 1000f, isFollowBuilderHide);
            if (ge == null)
            {
                return null;
            }
            //ge.SetHeader(header);
            ge.transform.parent = FxSceneRoot;
            ge.gameObject.name = effectName;

            //if (isFollowMove)
            //{
            //    ge.ps.simulationSpace = ParticleSystemSimulationSpace.Local;
            //}
            //else
            //{
            //    ge.ps.simulationSpace = ParticleSystemSimulationSpace.World;
            //}
            //ge.transform.localPosition = Vector3.zero;
            ge.transform.position = startWorldPos;
            //if (isFollowRotate)
            {
                ge.transform.eulerAngles = startWorldRotate;
            }
            //else
            //{
            //    ge.transform.localEulerAngles = Vector3.zero;
            //}
            ge.SetSpeed(speedMultiplier);
            ge.Play(startTime / 1000f);
            //Debug.LogError($" Fx play : {effectName} , startTime : {startTime} ms ");
            ge.SetScale(scale);
            if (ge.mainPs != null)
            {
                var main = ge.mainPs.main;
                main.loop = isLoop;
            }
            return ge;
        }

        public void AddEnvEffectAsync(
            string effectName,
            Vector3 startWorldPos,
            Vector3 startWorldRotate,
            LocalEffectTags localEffectTags/*, GameObject header*//*, Vector3 offsetPos*/,
            Vector3 scale,
            float playTime = 0,
            bool isFollowMove = false,
            bool isFollowRotate = false,
            int startDelay = 0,
            bool isLoop = false,
            float speedMultiplier = 1f,
            int startTime = 0,
            bool isFollowBuilderHide = true
            )
        {
            if (!CheckEffectNeedPlay(effectName, startTime))
            {
                SGF.Debuger.LogWarning($"AddEnvEffectAsync() recover : effectName: {effectName}, startTime: {startTime} , skip play !!!");
                return;
            }

            int fxTime = GetFxDetailTime(effectName);

            Action<GameEffect> cb = (ge) =>
            {
                if (ge == null)
                {
                    return;
                }

                ge.Init(localEffectTags, false, Vector3.zero, playTime, startDelay / 1000f, isFollowBuilderHide, false);

                ge.transform.parent = FxSceneRoot;
                ge.gameObject.name = effectName;

                ge.transform.position = startWorldPos;
                ge.transform.eulerAngles = startWorldRotate;
                ge.SetSpeed(speedMultiplier);
                ge.Play(startTime / 1000f);
                ge.SetScale(scale);

                ge.SetLoop(isLoop, fxTime);
            };
            GetEffectAsync(effectName, localEffectTags, ResoruceReleaseType.MapSceneAndServerID, cb);
        }

        /// <summary>
        /// 添加播放特效到指定节点下
        /// </summary>
        /// <param name="effectName">特效文件名</param>
        /// <param name="root">父节点</param>
        /// <param name="scale">缩放</param>
        /// <param name="playTime">配置了策划说的算,大于特效时间就循环,少于就提前干了</param>
        /// <param name="startDelay">起始播放延迟</param>
        /// <param name="isLoop">是否循环播放</param>
        /// <param name="speedMultiplier">特效播放速度</param>
        ///  <param name="initRotateFollowWorld">是否需要世界坐标为0</param>
        ///  <param name="isFaceToCamera">是否面向摄像机</param>
        ///  <param name="startTime">特效开始时间</param>
        ///  <param name="baseRotate">特效基础的 偏转朝向, 如果 挂点朝 右, 特效需要 面向施法者,此时特效需要朝左, 那么特效就使用朝左 的基础朝向</param>
        ///  <param name="isFollowBuilderHide">是否跟随隐藏</param>
        ///  <param name="moveOffset">偏移量</param>
        ///  <param name="entityType">实体类型</param>
        ///  <param name="fxGeParam">特效数据层</param>
        ///  <param name="callBack">回调</param>
        /// <returns></returns>
        public void AddPlayerEffectAsync(
                string effectName,
                Transform root,
                Vector3 scale = new Vector3(),
                float playTime = 0,
                int startDelay = 0,
                bool isLoop = false,
                float speedMultiplier = 1f,
                bool initRotateFollowWorld = false,
                bool isFaceToCamera = false,
                int startTime = 0,
                Vector3 baseRotate = new Vector3(),
                bool isFollowBuilderHide = true,
                Vector3 moveOffset = new Vector3(),
                E_EntityType entityType = E_EntityType.None,
                FxGeParam fxGeParam = null,
                Action<GameEffect> callBack = null,
                ResoruceReleaseType releaseType = ResoruceReleaseType.MapSceneAndServerID

            )
        {
            if (!CheckEffectNeedPlay(effectName, startTime))
            {
                SGF.Debuger.LogWarning($"fx recover : effectName: {effectName}, startTime: {startTime} , skip play !!!");
                callBack?.Invoke(null);
                return;
            }
            int fxTime = GetFxDetailTime(effectName);

            Action<GameEffect> cb = (ge) =>
            {
                callBack?.Invoke(ge);

                if (ge == null)
                {
                    return;
                }

                if (fxGeParam == null || fxGeParam.IsClosed)
                {
                    return;
                }

                ge.Init(LocalEffectTags.ActorFx, false, Vector3.zero, playTime, startDelay / 1000f, isFollowBuilderHide, isFaceToCamera);
                ge.gameObject.SetActive(true);

                if (entityType == E_EntityType.BulletEntity)
                {
                    UIUtilsFrameWork.ChangeLayer(ge.transform, E_LayerType.Bullet.ToString());
                }
                else
                {
                    UIUtilsFrameWork.ChangeLayer(ge.transform, E_LayerType.Entity.ToString());
                }

                //挂点调用已经决定旋转的可能性了。
                // if (isFollowRotate2th)
                // {
                //     ge.transform.parent = root;//应该是ModelOffset（绑点下)
                // }
                // else
                // {
                //     ge.transform.parent = root;//root应该是m_ModelNoRotationRoot下
                // }
                ge.transform.parent = root;
                ge.transform.localPosition = moveOffset;
                ge.transform.localEulerAngles = baseRotate; //永远朝向绑点当前，且应用特效所做的方向。
                //if (initRotateFollowWorld)
                //{
                //    //通常适配世界，绑点也不跟着人，初始化的时候应用特效的默认朝向在世界中生成
                //    ge.transform.eulerAngles = baseRotate;
                //}
                // else
                // {
                //     //跟着转，人的方向可能是世界的某一个方向，这时候约束特效是根人的当前朝向
                //     //所以设定当前旋转和角色一致
                //     //适配特效再在人身上跟随，挂在人身上类型
                //     ge.transform.localEulerAngles = Vector3.zero;
                // }
                ge.transform.localScale = Vector3.one;
                ge.SetSpeed(speedMultiplier);
                ge.Play(startTime / 1000f);
                ge.SetScale(scale);

                ge.SetLoop(isLoop, fxTime);
            };
            GetEffectAsync(effectName, LocalEffectTags.ActorFx, releaseType, cb);
        }

        [Obsolete("请使用 ", false)]//标记该方法已弃用
        /// <summary>
        /// 添加播放特效到指定节点下
        /// </summary>
        /// <param name="effectName">特效文件名</param>
        /// <param name="root">父节点</param>
        /// <param name="scale">缩放</param>
        /// <param name="playTime">配置了策划说的算,大于特效时间就循环,少于就提前干了</param>
        /// <param name="isFollowMove">是否跟随移动</param>
        /// <param name="isFollowRotate">是否跟随旋转过程（再移动是否原来特效保持在空气中）</param>
        /// <param name="startDelay">起始播放延迟</param>
        /// <param name="isLoop">是否循环播放</param>
        /// <param name="speedMultiplier">特效播放速度</param>
        ///  <param name="InitRotateFollowWorld">是否需要世界坐标为0</param>
        /// <returns></returns>
        public GameEffect AddPlayerEffect(
                string effectName/*, GameObject header*//*, Vector3 offsetPos*/,
                Transform root,
                Vector3 scale = new Vector3(),
                float playTime = 0,
                int startDelay = 0,
                bool isLoop = false,
                float speedMultiplier = 1f,
                bool InitRotateFollowWorld = false,
                int startTime = 0,
                Vector3 baseRotate = new Vector3(), // 特效基础的 偏转朝向, 如果 挂点朝 右, 特效需要 面向施法者,此时特效需要朝左, 那么特效就使用朝左 的基础朝向
                bool isFollowBuilderHide = true,
                Vector3 moveOffset = new Vector3(),
                ResoruceReleaseType releaseType = ResoruceReleaseType.MapSceneAndServerID
            )
        {
            if (!CheckEffectNeedPlay(effectName, startTime))
            {
                SGF.Debuger.LogWarning($"fx recover : effectName: {effectName}, startTime: {startTime} , skip play !!!");
                return null;
            }
            //if (header == null)
            //{
            //    return AddWorldEffect(effectid, offsetPos, scale);
            //}
            //playTime = 0;//现在不用PlayTime了暂时

            /// note: 
            ///     1.主角的 特效 需要单独设置为 ResoruceReleaseType.OnlyForce . 只有在返回登录 才需要释放.
            ///     2.主角变身 , 切换场景/地图 是否 释放特效, 要看后续 技能规划;
            ///           如果像 杰斯/暴女 这种本身有多套技能的变身, 那 特效切 地图和场景, 都不需要释放.
            ///           如果像 roguelike 这种可以捡技能, 那切换地图 和场景 都可以 释放.

            GameEffect ge = GetEffect(effectName, LocalEffectTags.ActorFx, releaseType, playTime, startDelay / 1000f, isFollowBuilderHide);
            if (ge == null)
            {
                return null;
            }
            //ge.SetHeader(header);

            //挂点调用已经决定旋转的可能性了。
            // if (isFollowRotate2th)
            // {
            //     ge.transform.parent = root;//应该是ModelOffset（绑点下)
            // }
            // else
            // {
            //     ge.transform.parent = root;//root应该是m_ModelNoRotationRoot下
            // }

            ge.transform.parent = root;
            ge.transform.localPosition = moveOffset;
            ge.transform.localEulerAngles = baseRotate; //永远朝向绑点当前，且应用特效所做的方向。
            if (InitRotateFollowWorld)
            {
                //通常适配世界，绑点也不跟着人，初始化的时候应用特效的默认朝向在世界中生成
                ge.transform.eulerAngles = baseRotate;
            }
            // else
            // {
            //     //跟着转，人的方向可能是世界的某一个方向，这时候约束特效是根人的当前朝向
            //     //所以设定当前旋转和角色一致
            //     //适配特效再在人身上跟随，挂在人身上类型
            //     ge.transform.localEulerAngles = Vector3.zero;
            // }

            ge.transform.localScale = Vector3.one;
            ge.SetSpeed(speedMultiplier);
            ge.Play(startTime / 1000f);
            ge.SetScale(scale);

            if (ge.mainPs != null)
            {
                var main = ge.mainPs.main;
                main.loop = isLoop;
            }
            return ge;
        }

        [Obsolete("请使用 ", false)]//标记该方法已弃用
        public GameEffect AddUIEffect(string effectName,
            Vector3 scale,
            float playTime = 0,
            bool isFollowMove = false,
            bool isFollowRotate = false,
            int startDelay = 0,
            bool isLoop = false,
            int startTime = 0,
             bool isFollowBuilderHide = true
            )
        {
            if (UIfxDynamicRoot == null)
            {
                UIfxDynamicRoot = UIFXRoot.UINttRoot.transform;
            }
            if (UIfxDynamicRoot == null)
            {
                SGF.Debuger.LogWarning("没找到ui节点，或者尚未启动");
                return null;
            }
            if (!CheckEffectNeedPlay(effectName, startTime))
            {
                SGF.Debuger.LogWarning($"fx recover : effectName: {effectName}, startTime: {startTime} , skip play !!!");
                return null;
            }

            //playTime = 0;//现在不用PlayTime了暂时
            GameEffect ge = GetEffect(effectName, LocalEffectTags.UI, ResoruceReleaseType.MapSceneAndServerID, playTime, startDelay / 1000f, isFollowBuilderHide);
            if (ge == null)
            {
                return null;
            }

            //ge.SetHeader(header);
            ge.transform.parent = UIfxDynamicRoot;
            ge.transform.localPosition = Vector3.zero;
            if (isFollowRotate)
            {
                ge.transform.eulerAngles = Vector3.zero;
            }
            else
            {
                ge.transform.localEulerAngles = Vector3.zero;
            }
            ge.transform.localScale = Vector3.one;
            ge.Play(startTime / 1000f);
            ge.SetScale(scale);

            if (ge.mainPs != null)
            {
                var main = ge.mainPs.main;
                main.loop = isLoop;
            }
            return ge;
        }

        #region 预警圈

        private Dictionary<string, List<WarningRingEffect>> m_WarningRingEffectPool = new();

        /// <summary>
        /// 添加预警圈特特效
        /// </summary>
        /// <param name="effectName">特效名字</param>
        /// <param name="startWorldPos">世界坐标</param>
        /// <param name="startWorldRotate">世界角度</param>
        /// <param name="startTime">开始时间（毫秒）</param>
        /// <param name="playMaxTime">最大时间（毫秒）</param>
        /// <param name="shap">形状</param>
        public void AddWarningRingEffect(string effectName, Vector3 startWorldPos, Vector3 startWorldRotate, float startTime, float playMaxTime, ShapeSerialize shap)
        {
            Action<WarningRingEffect> cb = (ge) =>
            {
                if (ge == null)
                {
                    return;
                }
                ge.transform.parent = FxSceneRoot;
                ge.gameObject.name = effectName;
                ge.transform.position = startWorldPos;
                ge.transform.eulerAngles = startWorldRotate;

                ge.Init(shap, startTime, playMaxTime);
            };
            GetWarningRingEffect(effectName, cb);
        }

        private void GetWarningRingEffect(string effectName, Action<WarningRingEffect> callBack)
        {
            string effectPath = "Effects/Skill/WarningRing/" + effectName;
            //对应池是否存在：初始化为空，首次建立
            if (!m_WarningRingEffectPool.ContainsKey(effectPath))
            {
                CreateWarningRingEffect(effectPath, callBack);
            }
            else
            {
                //寻找空闲特效:不用两层了，别加queue了
                List<WarningRingEffect> pool = m_WarningRingEffectPool[effectPath];
                for (int i = 0; i < pool.Count; i++)
                {
                    WarningRingEffect eff = pool[i];
                    //没有池要新增，没有可用的要新增，但是否可用这里限定
                    //同特效，不同应用途径不可池共享属性，因为不想做重生
                    //不想做迁移，情况少，如共享池处理都不如你把资源分成两部分
                    if (eff != null && !eff.Playing)
                    {
                        callBack?.Invoke(eff);
                        return;
                    }
                }
                //如果没有可用特效，则创建一个新的：
                CreateWarningRingEffect(effectPath, callBack);
            }
        }

        private void CreateWarningRingEffect(string file_path, Action<WarningRingEffect> callBack)
        {
            Action<GameObject> cb = (go) =>
            {
                if (go == null)
                {
                    SGF.Debuger.LogWarning("CreateWarningRingEffect() cant find file ! : " + file_path);
                    callBack?.Invoke(null);
                    return;
                }
                GameObject gob = GameObject.Instantiate(go);
                //设计上就不要重复播放，迁移位置，同资源不同类型就不可以公用
                //重复出现的资源因为使用不同情况较少不合并池
                WarningRingEffect ge = GameObjectUtils.EnsureComponent<WarningRingEffect>(gob);
                //代替资源不影响分类
                if (!m_WarningRingEffectPool.ContainsKey(file_path))
                {
                    m_WarningRingEffectPool.Add(file_path, new List<WarningRingEffect>());
                }
                m_WarningRingEffectPool[file_path].Add(ge);
                callBack?.Invoke(ge);
            };
            LoadPrefabAsync(file_path, cb);
        }

        #endregion

        [Obsolete("请使用 GetEffectAsync", false)]//标记该方法已弃用
        private GameEffect GetEffect(string effectName, LocalEffectTags tag, ResoruceReleaseType releaseType, float playerTime = 0, float starDelay = 0f, bool isFollowBuilderHide = true)
        {
            string effectPath = "";
            switch (tag)
            {
                case LocalEffectTags.UI:
                    effectPath = "Perfab/Fx/UIFX_ABS/" + effectName;
                    break;
                case LocalEffectTags.Env:
                    effectPath = "Perfab/Fx/SceneFx/" + effectName;
                    break;
                case LocalEffectTags.ActorFx:
                    //effectPath = "Perfab/Fx/ActorFx/CommonSkill/" + effectName;//这里暂时先不继续分类拓展
                    effectPath = effectName;
                    break;
                case LocalEffectTags.Weapon:
                    effectPath = "Perfab/Fx/WeaponFx_ABS/" + effectName;
                    break;
                default:
                    break;
            }
            //整体思路是保证使用
            GameEffect ret = GetFxGameEffect(effectPath, tag, releaseType);

            if (ret != null)
            {
                ret.Init(tag, false, Vector3.zero, playerTime, starDelay, isFollowBuilderHide);
                ret.gameObject.SetActive(true);
            }
            return ret;
        }


        private void GetEffectAsync(string effectName, LocalEffectTags tag, ResoruceReleaseType releaseType, Action<GameEffect> cb)
        {
            string effectPath = "";
            switch (tag)
            {
                case LocalEffectTags.UI:
                    effectPath = "Perfab/Fx/UIFX_ABS/" + effectName;
                    break;
                case LocalEffectTags.Env:
                    effectPath = "Perfab/Fx/SceneFx/" + effectName;
                    break;
                case LocalEffectTags.ActorFx:
                    //effectPath = "Perfab/Fx/ActorFx/CommonSkill/" + effectName;//这里暂时先不继续分类拓展
                    effectPath = effectName;
                    break;
                case LocalEffectTags.Weapon:
                    effectPath = "Perfab/Fx/WeaponFx_ABS/" + effectName;
                    break;
                default:
                    break;
            }
            GetFxGameEffectAsync(effectPath, tag, releaseType, cb);

        }

        [Obsolete("请使用 LoadPrefabAsync", false)]//标记该方法已弃用
        /// <summary>
        /// 同步加载并且实例化一个prefab
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="defaultPrefabName"></param>
        /// <returns></returns>
        private GameObject InstantiateSync(string prefabName, string defaultPrefabName)
        {
            GameObject prefab = Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(prefabName, E_AssetType.Effects);
            return prefab;
        }

        /// <summary>
        /// 异步加载预制体
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="cb"></param>
        private void LoadPrefabAsync(string prefabName, Action<GameObject> cb)
        {
            Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(prefabName, E_AssetType.Effects, false, cb);
        }

        public bool CheckEffectNeedPlay(FXJson fXJson, int startTime)
        {
            return CheckEffectNeedPlay(fXJson.EffectPath, startTime);
        }

        public int GetFxDetailTime(string effectPath)
        {
            string path = ResourcesUtli.GetReadPath(effectPath.ToLower(), E_AssetType.Effects, LocalDataManager.Instance.CehckHasFxDetail);
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            {
                return -1;
            }
            int effectTime = 0;

            // 以下其实是同步接口
            LocalDataManager.Instance.GetFxDetailJson((FXDetailJson fXDetailJson) =>
            {
                effectTime = fXDetailJson.fxFileDurationDic[path];
            });

            return effectTime;

        }

        public bool CheckEffectNeedPlay(string effectPath, int startTime)
        {
            bool result = true;
            string path = ResourcesUtli.GetReadPath(effectPath.ToLower(), E_AssetType.Effects, LocalDataManager.Instance.CehckHasFxDetail);

            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            {
                // 虽然 特效 在 fxDetail.json中 没找到,但是 还是 要播,因为 fxDetail 中可能漏了 一些资源
                SGF.Debuger.LogWarning($"fx: CheckEffectNeedPlay path: {effectPath}, not find in fxDetail!!!");
                ResourcesUtli.GetReadPath(effectPath.ToLower(), E_AssetType.Effects, LocalDataManager.Instance.CehckHasFxDetail);
                return true;
            }

            // fxDetail 的加载 在loading 时已经加载完成, 所以 GetFxDetailJson 是能够同步获取
            LocalDataManager.Instance.GetFxDetailJson((FXDetailJson fXDetailJson) =>
            {
                int effectTime = fXDetailJson.fxFileDurationDic[path];

                // 如果特效的 粒子时间 为 0, 表明 这个特效 只是用了 gameEffect的壳子,
                // 在根节点 挂了一个 粒子组件,但内部实现 肯能是动画 或者一些其它的东西(例如预警圈)
                // 对于这种 特效, 还是要播的, 只不过 要沟通一下,增加一个恢复接口
                if (effectTime == 0)
                {
                    result = true;
                }
                else
                {
                    result = effectTime > startTime;
                }
            });

            return result;
        }

        public void ReturnEffect(GameEffect ge)
        {
            ge.OnReturnEffect();
            NodePool.Put(ge.transform, NodePool.NodePoolType.Fx, false);
            // ge.transform.gameObject.SetActive(false);
            // ge.transform.parent = EntityRoot.Instance.DotRemoveRoot.transform;
        }

        public void OnEndGame()
        {
            ClearAll();
        }

        //结束清理SceneRoot，工厂倒闭
        public void ClearAll()
        {
            ClearEffectPool();
            ClearWarningRingEffectPool();

            ClearGeRecords();
        }

        public void ClearEffectPool()
        {
            FxResouceCaches.ForEach((item) =>
            {
                var listFxResourceInfos = item.Value;
                for (int i = 0; i < listFxResourceInfos.Count; i++)
                {
                    SimpleDataFactory.ReleaseData(listFxResourceInfos[i]);
                }
                listFxResourceInfos.Clear();
            });
            FxResouceCaches.Clear();
        }

        public void ClearWarningRingEffectPool()
        {
            if (m_WarningRingEffectPool != null && m_WarningRingEffectPool.Count != 0)
            {
                foreach (var lists in m_WarningRingEffectPool)
                {
                    if (lists.Value != null && lists.Value.Count != 0)
                    {
                        for (int i = lists.Value.Count; i >= 0; i--)
                        {
                            lists.Value[i].DestroyRelease();
                        }
                        lists.Value.Clear();
                    }
                }
                m_WarningRingEffectPool.Clear();
            }
        }



        public void ClearEnv()
        {
            ClearTagFxEffects(LocalEffectTags.Env);
        }

        public void ClearUI()
        {
            ClearTagFxEffects(LocalEffectTags.UI);
        }

        public void ClearActorFx()
        {
            ClearTagFxEffects(LocalEffectTags.ActorFx);
        }

        /// <summary>
        /// 根据 条件清除 对应的 特效
        /// </summary>
        /// <param name="checkCondition"></param> 
        private void ClearFxEffectWithCondition(Func<FxResourceInfo, bool> checkCondition)
        {
            FxResouceCaches.ForEach((item) =>
            {
                var listFxResourceInfos = item.Value;
                for (int i = 0; i < listFxResourceInfos.Count; i++)
                {
                    FxResourceInfo fxResourceInfo = listFxResourceInfos[i];

                    // 如果 满足检查条件, 释放对于tag 的 特效
                    if (checkCondition(fxResourceInfo))
                    {
                        listFxResourceInfos.RemoveAt(i);
                        i--;
                        SimpleDataFactory.ReleaseData(fxResourceInfo);
                    }
                }
            });
        }

        /// <summary>
        /// 清除 过量的 特效. 目前博哥的说法是 池内相同的 特效 多的就干掉
        /// </summary>
        private void ClearOverCountFxResouces(int maxCount = 1)
        {
            FxResouceCaches.ForEach((item) =>
            {
                var listFxResourceInfos = item.Value;

                // 清除 超过 maxCount 数量的 相同的 特效
                for (int i = maxCount; i < listFxResourceInfos.Count; i++)
                {
                    FxResourceInfo fxResourceInfo = listFxResourceInfos[i];

                    listFxResourceInfos.RemoveAt(i);
                    i--;
                    SimpleDataFactory.ReleaseData(fxResourceInfo);
                }
            });
        }

        /// <summary>
        /// 清除 tag 相对应的 特效
        /// </summary>
        /// <param name="tag"></param>
        private void ClearTagFxEffects(LocalEffectTags tag)
        {
            ClearFxEffectWithCondition((fxResourceInfo) =>
            {
                // 如果 tag 相同, 释放对于tag 的 特效
                return fxResourceInfo.ge.EqualTags(tag) && fxResourceInfo.CheckCanRelease(TryReleaseResouceType.Force); ;
            });
        }

        /// <summary>
        /// 清除 与 mapId 相关的 特效
        /// </summary>
        /// <param name="oldMapId"></param>
        private void ClearMapFxEffects(int oldMapId)
        {
            // SGF.Debuger.Log("=================================");
            // SGF.Debuger.Log($"ClearMapFxEffects: oldMapId : {oldMapId}");
            ClearFxEffectWithCondition((fxResourceInfo) =>
            {
                // 如果 tag 相同, 释放对于tag 的 特效
                return fxResourceInfo.EqualMap(oldMapId) && fxResourceInfo.CheckCanRelease(TryReleaseResouceType.Map);
            });

            ClearOverCountFxResouces();
        }

        /// <summary>
        /// 清除 与 scene 相关的 特效
        /// </summary>
        /// <param name="oldMapId"></param>
        private void ClearSceneFxEffects(string oldScene)
        {
            // SGF.Debuger.Log("=================================");
            // SGF.Debuger.Log($"ClearSceneFxEffects: oldScene : {oldScene}");
            ClearFxEffectWithCondition((fxResourceInfo) =>
            {
                // 如果 tag 相同, 释放对于tag 的 特效
                return fxResourceInfo.EqualScene(oldScene) && fxResourceInfo.CheckCanRelease(TryReleaseResouceType.Scene);
            });

            ClearOverCountFxResouces();
        }

        private void ClearServerIDFxEffects(ulong oldServerID)
        {
            // SGF.Debuger.Log("=================================");
            // SGF.Debuger.Log($"ClearServerIDFxEffects: oldServerID : {oldServerID}");
            ClearFxEffectWithCondition((fxResourceInfo) =>
            {
                // 如果 oldServerID 相同, 释放对于 oldServerID 的 特效
                return fxResourceInfo.EqualServerID(oldServerID) && fxResourceInfo.CheckCanRelease(TryReleaseResouceType.ServerID);
            });

            ClearOverCountFxResouces();

        }

        /// <summary>
        /// 按博哥的策略,每次 添加一个新的特效的时候,检查池内特效是否超时
        /// </summary>
        private void TriggerOverTimeClearFxEffects()
        {
            // SGF.Debuger.Log("=================================");
            // SGF.Debuger.Log($"TriggerOverTimeClearFxEffects: 准备特效的超时检测");

            ClearFxEffectWithCondition((fxResourceInfo) =>
            {
                // 释放超时的特效
                return fxResourceInfo.IsOverTime();
            });

        }


        /// <summary>
        /// 播放的特效的 记录.
        /// note:
        ///     1. 在每次 add 的时候做排序,那么检索的时候, 就不需要再去排序;
        ///     2. 每个特效 stop的时候, 都去 remove;
        ///     3. 特效的 FxRenderType 枚举类型在存入时 直接转成对应的 int 类型，防止 排序时重复去 枚举类型转换;
        ///     4. 场景中 特效最多的类型 是 other 类型. 所以在 类型 add/remove 的时候, 类型最多的 放在前面.  
        ///        这样能够优先查找到目标;
        ///     5. 同样类型的特效 按队列的方式存入, 先进先出. (存在链表中时, 新加的特效存放在同类型特效的最后面).
        ///     6. 特效类型 从 大到小排序. 最大的类型为 other. 放在最前面
        /// </summary> 
        private LinkedList<FxGeParam> fxGeParamRecords = new();

        public bool CheckCanAdd(FxGeParam fxGeParam)
        {
            var head = fxGeParamRecords;

            if (ForbidOtherPlayerFX)
            {
                bool isOhterPlayerFx = fxGeParam.FxRenderType == E_Render_PRI.PartyATTACK_FX
                                    || fxGeParam.FxRenderType == E_Render_PRI.PartyHIT_FX
                                    || fxGeParam.FxRenderType == E_Render_PRI.GuildATTACK_FX
                                    || fxGeParam.FxRenderType == E_Render_PRI.GuildHIT_FX
                                    || fxGeParam.FxRenderType == E_Render_PRI.Other_Player_FX;
                if (isOhterPlayerFx)
                {
                    return false;
                }

            }

            if (OnlyShowMainPlayerFX)
            {
                bool isMainPlayerFx = fxGeParam.FxRenderType == E_Render_PRI.PlayerATTACK_FX
                                    || fxGeParam.FxRenderType == E_Render_PRI.PlayerHIT_FX
                                    || fxGeParam.FxRenderType == E_Render_PRI.TeammateATTACK_FX
                                    || fxGeParam.FxRenderType == E_Render_PRI.TeammateHIT_FX;
                if (!isMainPlayerFx)
                {
                    return false;
                }
            }

            //最大数量限制优先级最高，如果是0，显示自己都不显示出来===策划目的
            if (head.Count < MaxFxCount)
            {
                return true;
            }

            if (head.Count >= MiddleFxCount)
            {
                // 如果数量 > MiddleFxCount 并且优先级也 比第一个优先级要高, 
                // 那此时就进一步做过滤, 判断特效是否在屏幕内, 如果都不在屏幕内, 那都不需要再去播放

                bool isInCamera = CameraManager.Instance.IsCameraPoint(E_CameraType.StarWorldCam, fxGeParam.CurPos);
                // 如果都不在屏幕内, 那就 直接不用播放
                if (!isInCamera)
                {
                    return false;
                }
            }



            // 优先级 比 第一个特效的优先级高的化，那就可以添加
            if (fxGeParam.FxRenderTypeValue < head.First.Value.FxRenderTypeValue)
            {
                return true;
            }




            return false;
        }
        public bool OnlyShowMainPlayerFX = false;
        public bool ForbidOtherPlayerFX = false;

        /// <summary>
        /// 增加特效播放的 记录
        /// </summary>
        /// <param name="fxGeParam"></param>
        public void AddFxGeRecord(FxGeParam fxGeParam)
        {
            // SGF.Debuger.Log($"[Fx] add fx: {fxGeParam.EffectName} , count: {fxGeParamRecords.Count}");


            var head = fxGeParamRecords;

            // 如果特效的 数量大于 最大的特效数量, 那就删除 之前播放的 特效.
            // 目前来说, fxGeParamRecords 的类型 由大--->小, 时间由 旧--->新排序.
            // 所以只需要删除 链表的首节点 即可
            if (head.Count >= MaxFxCount)
            {

                // SGF.Debuger.Log($"[Fx] 大于max, remove fx: {head.First.Value.EffectName} ,  count: {fxGeParamRecords.Count}");

                // 关闭 这个特效
                head.First.Value.Close();
            }

            LinkedListNode<FxGeParam> node = head.First;
            // 从头往后找
            while (node != null)
            {

                var fxGe = node.Value;
                if (fxGeParam.FxRenderTypeValue > fxGe.FxRenderTypeValue)
                {
                    break;
                }
                // 继续往后面找
                node = node.Next;
            }

            // 如果找到了 一个 node 的类型要小于新加的特效的类型,那就要插入这个节点的前面
            if (node != null)
            {
                head.AddBefore(node, fxGeParam);
            }
            else
            {
                // 如果找不到的时候, 那就直接插入队列的尾部
                head.AddLast(fxGeParam);
            }





            LogCache();

        }

        StringBuilder sb = new();
        private void LogCache()
        {
            return;
            LinkedListNode<FxGeParam> node = fxGeParamRecords.First;

            sb.Clear();
            // sb.Append($"[Fx] count : {fxGeParamRecords.Count} , ");
            // 从头往后找
            while (node != null)
            {

                var fxGe = node.Value;
                // sb.Append($" [{fxGe.EffectName}] ");
                // 继续往后面找
                node = node.Next;
            }
            // SGF.Debuger.Log(sb);
        }

        /// <summary>
        /// 直接删除指定的特效
        /// </summary>
        /// <param name="fxGeParam"></param>
        public void RemoveFxGeRecord(FxGeParam fxGeParam)
        {
            fxGeParamRecords.Remove(fxGeParam);
            // SGF.Debuger.Log($"[Fx] remove fx: {fxGeParam.EffectName} , count: {fxGeParamRecords.Count}");
            LogCache();
        }

        /// <summary>
        /// 清除所有的特效记录
        /// </summary>
        private void ClearGeRecords()
        {
            fxGeParamRecords.Clear();
        }
    }
}