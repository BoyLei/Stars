using Animancer;
using Animancer.FSM;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.OffLine;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这里全是显示层，，mono显示层
/// </summary>
namespace StarProject.ArtHelper
{
    [XLua.LuaCallCSharp]
    public class RoleViewDisplay : MonoBehaviour, I_VVitalAnim
    {
        [HideInInspector]
        private AnimancerComponent _Animancer;
        public AnimancerComponent Animancer => _Animancer;

        private readonly StateMachine<VitalState>.WithDefault _StateMachine = new();
        public StateMachine<VitalState>.WithDefault StateMachine => _StateMachine;


        private VitalState Stage_Idle;
        private List<VitalState> CommonVitalStates = new();

        private bool m_isPoolsLoad = true;

        private AvatarDataCell m_AvatarDataCell;

        //离线绑点数据
        private ModelOffLineData m_ModelOffLineData = null;
        public ModelOffLineData M_ModelOffLineData
        {
            get
            {
                if (m_ModelOffLineData == null)
                {
                    m_ModelOffLineData = transform.GetComponentInChildren<ModelOffLineData>();
                }
                return m_ModelOffLineData;
            }
            set
            {
                m_ModelOffLineData = value;
            }
        }

        private Transform ModelRoot = null;
        private string ModelPath = string.Empty;
        private string AnimsPath = string.Empty;
        private string key = string.Empty;

        private void Awake()
        {
            BindState();
            ModelRoot = transform.Find("ModelRoot");
        }

        private void OnDestroy()
        {
            CommonVitalStates.Clear();
            Reset();
        }

        private void OnDisable()
        {
            Reset();
        }

        private void Reset()
        {
            if (ModelRoot != null && _Animancer != null && !string.IsNullOrEmpty(ModelPath))
            {
                PushModelGameObject(ModelPath, _Animancer.gameObject);
                //ModelRoot.DestroyChildren();
            }
            m_ModelOffLineData = null;
            _Animancer = null;
            m_AvatarDataCell = null;
            ModelPath = string.Empty;
            AnimsPath = string.Empty;
            key = string.Empty;
        }

        private void BindState()
        {
            Stage_Idle = transform.Find("StateMachines/Idle").GetComponent<VitalState>();
            Stage_Idle.Character = this;

            var Common_1 = transform.Find("StateMachines/Common_1").GetComponent<VitalState>();
            var Common_2 = transform.Find("StateMachines/Common_2").GetComponent<VitalState>();
            Common_1.Character = this;
            Common_2.Character = this;
            CommonVitalStates.Add(Common_1);
            CommonVitalStates.Add(Common_2);
        }

        public void SetIsPoolsLoad(bool isPoolsLoad)
        {
            m_isPoolsLoad = isPoolsLoad;
        }

        public void InitData(AvatarDataCell avatarDataCell, string animName, string targetLayer, bool isHighModel = true, Action<float> excb = null)
        {
            //SGF.Debuger.Log($"模型显示 初始化 id={GetHashCode()},key={key},AvatarID={avatarDataCell.GetAvatarID()},animName={animName},ModelPath={ModelPath},AnimsPath={AnimsPath}");
            if (m_AvatarDataCell != null)
            {
                //SGF.Debuger.LogWarning($"模型显示 初始化 id={GetHashCode()},key={key},AvatarID={m_AvatarDataCell.GetAvatarID()},ModelPath={ModelPath},AnimsPath={AnimsPath}");
            }
            Reset();
            key = Fire.Utils.GenerateCheckCode(16);
            m_AvatarDataCell = avatarDataCell;
            Action<VitalState> cb = (vitalState) =>
            {
                if (excb != null && vitalState != null)
                {
                    excb?.Invoke(vitalState.Clip.length);
                }
                CreateModel(targetLayer, vitalState, isHighModel);
            };
            bool isAnimNameByIdle = animName.IndexOf("idle") != -1 || animName.IndexOf("Idle") != -1;
            InitLoadIdleAnimAsync(animName, isAnimNameByIdle ? cb : null);
            // 如果默认动画是不是【idle】那就加载其他动画
            if (!isAnimNameByIdle)
            {
                SGF.Debuger.LogWarning($"模型显示 初始化 id={GetHashCode()},key={key},AvatarID={m_AvatarDataCell.GetAvatarID()},ModelPath={ModelPath},AnimsPath={AnimsPath},isAnimNameByIdle={isAnimNameByIdle},animName={animName},显示其他动画文件");
                InitLoadOtherAnimAsync(animName, cb);
            }
        }

        private void CreateModel(string targetLayer, VitalState vitalState, bool isHighModel = true)
        {
            if (m_AvatarDataCell != null)
            {
                int modelID = isHighModel ? m_AvatarDataCell.GetHighModelId() : m_AvatarDataCell.GetModelId();
                ModelDataCell modelDataCell = LocalDataManager.Instance.GetModelDataCell(modelID);
                if (modelDataCell == null)
                {
                    modelDataCell = LocalDataManager.Instance.GetModelDataCell(m_AvatarDataCell.GetModelId());
                }
                if (modelDataCell != null)
                {
                    string path = modelDataCell.ModelsPath;
                    path = path.Replace("Assets/Res/", string.Empty);
                    ModelPath = path;
                    Action<GameObject> cb = (go) =>
                    {
                        if (go == null)
                        {
                            return;
                        }
                        GameObject gob = go;
                        if (!m_isPoolsLoad)
                        {
                            gob = GameObject.Instantiate<GameObject>(go);
                        }
                        SGF.Debuger.Log($"模型显示 加载模型 id={GetHashCode()},key={key},path={path},ModelPath={ModelPath}");
                        if (path != ModelPath && ModelPath != string.Empty)
                        {
                            SGF.Debuger.LogWarning($"模型显示 加载模型 id={GetHashCode()},key={key},path={path},ModelPath={ModelPath}, 跟我要的不一样，回收回去吧");
                            PushModelGameObject(path, gob);
                        }
                        else
                        {
                            if (ModelRoot != null)
                            {
                                //if (_Animancer != null && !string.IsNullOrEmpty(ModelPath))
                                //{
                                //    Service.Resource.ResourceFormalManager.Instance.PushGameObject(ModelPath, _Animancer.gameObject);
                                //    SGF.Debuger.LogWarning($"模型显示 加载模型 id={GetHashCode()},key={key},path={path},ModelPath={ModelPath}, 直接有模型了，先把之前的还回去");
                                //    //ModelRoot.DestroyChildren();
                                //}
                                SGF.Debuger.Log($"模型显示 加载模型 id={GetHashCode()},key={key},path={path},成功");
                                LoadModelCB(modelDataCell, targetLayer, vitalState, gob, path);
                            }
                            else
                            {
                                SGF.Debuger.LogWarning($"模型显示 加载模型 id={GetHashCode()},key={key},path={path}, root不存在了");
                                PushModelGameObject(path, gob);
                            }
                        }
                    };
                    Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(path, E_AssetType.Roles, m_isPoolsLoad, cb);
                }
            }
        }

        private void LoadModelCB(ModelDataCell modelDataCell, string targetLayer, VitalState vitalState, GameObject gob, string path)
        {
            if (gob == null)
            {
                SGF.Debuger.LogWarning($"模型显示 RoleViewDisplay CreateModel gob=null,modelName={modelDataCell.Desc},HighModelsPath={modelDataCell.ModelsPath}");
                return;
            }
            if (path != ModelPath && ModelPath != string.Empty)
            {
                SGF.Debuger.LogWarning($"模型显示 加载模型 id={GetHashCode()},key={key},path={path},ModelPath={ModelPath}, 跟我要的不一样，回收回去吧LoadModelCB");
                PushModelGameObject(path, gob);
                return;
            }
            if (ModelRoot.childCount > 0)
            {
                //Service.Resource.ResourceFormalManager.Instance.PushGameObject(path, go);

                SGF.Debuger.LogWarning($"模型显示 加载模型 id={GetHashCode()},key={key},path={path},ModelPath={ModelPath}, 虽然一样了,但还是有子节点，删除了");
                ModelRoot.DestroyChildren();
            }
            gob.transform.SetParent(ModelRoot);
            gob.SetActive(true);
            //gob.transform.localScale = Vector3.one;
            gob.transform.localEulerAngles = Vector3.up * 180;
            gob.transform.localPosition = Vector3.zero;

            Renderer[] gobRender = gob.GetComponentsInChildren<Renderer>();
            foreach (var item in gobRender)
            {
                if (item != null)
                {
                    item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

            UIUtilsFrameWork.ChangeLayer(transform, targetLayer);

            #region 防错添加组件

            Animator animator = gob.GetComponent<Animator>();
            if (animator == null)
            {
                gob.AddComponent<Animator>();
            }
            // _Animancer = GameObjectUtils.EnsureComponent<AnimancerComponent>(gob);
            _Animancer = GameObjectUtils.EnsureComponent<AnimancerExtend>(gob);
            if (_Animancer.Animator != null && _Animancer.Animator != animator)
            {
                SGF.Debuger.LogWarning($"模型显示 加载模型 id={GetHashCode()},key={key},设置新的animator");
            }
            if (_Animancer.Animator == null || _Animancer.Animator != animator)
            {
                _Animancer.Animator = animator;
            }
            (_Animancer as AnimancerExtend).ActionOnPlayWwise = OnActionPlayWwise;

            ModelOffLineData modelOffLineData = gob.GetComponent<ModelOffLineData>();
            if (modelOffLineData == null)
            {
                M_ModelOffLineData = gob.AddComponent<ModelOffLineData>();
                M_ModelOffLineData.BindDummyPos.Clear();
                M_ModelOffLineData.BindDummyPos.Add("Root", gob.transform);
            }
            if (M_ModelOffLineData != null && M_ModelOffLineData.BindDummyPos.Count <= 0)
            {
                modelOffLineData.BindDummyPos.Add("Root", gob.transform);
            }

            #endregion

            StateMachine.TryResetState(vitalState);
        }

        public void SetPos(Vector3 pos)
        {
            transform.position = pos;
        }

        public void SetRotation(Quaternion quaternion)
        {
            transform.rotation = quaternion;
        }

        public void PlayIAnimImmediateAsync(string VitalStateName)
        {
            bool find = false;
            VitalState vitalState = GetVitalState(VitalStateName, out find);
            if (vitalState != null)
            {
                if (m_AvatarDataCell != null)
                {
                    string path = $"{m_AvatarDataCell.AnimsPath}/{VitalStateName}";
                    path = path.Replace("Assets/Res/", string.Empty);
                    path = path.Replace(".anim", string.Empty);

                    StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<AnimationClip>(path, E_AssetType.Animation,
                    (AnimationClip animationClip) =>
                    {
                        if (animationClip != null)
                        {
                            vitalState.SetClip(animationClip);
                            vitalState.speed = 1;
                            StateMachine.TryResetState(vitalState);
                        }
                    });
                }
            }
        }

        public void InitLoadIdleAnimAsync(string idleStateName, Action<VitalState> cb)
        {
            bool find = false;
            VitalState idleVitalState = GetVitalState(idleStateName, out find);
            if (idleVitalState != null)
            {
                if (m_AvatarDataCell != null)
                {
                    string path = $"{m_AvatarDataCell.AnimsPath}/{idleStateName}";
                    path = path.Replace("Assets/Res/", string.Empty);
                    path = path.Replace(".anim", string.Empty);
                    SGF.Debuger.Log($"模型显示 加载动画文件 InitLoadIdleAnimAsync id={GetHashCode()},key={key},path={path},AnimsPath={AnimsPath}");
                    AnimsPath = path;
                    StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<AnimationClip>(path, E_AssetType.Animation,
                    (AnimationClip animationClip) =>
                    {
                        if (animationClip != null)
                        {
                            if (path != AnimsPath && AnimsPath != string.Empty)
                            {
                                SGF.Debuger.LogWarning($"模型显示 加载动画文件 成功 id={GetHashCode()},key={key},path={path},AnimsPath={AnimsPath}, 跟我要的不一样，回收回去吧");
                                return;
                            }
                            else
                            {
                                idleVitalState.SetClip(animationClip);
                                idleVitalState.speed = 1;
                                //StateMachine.TryResetState(vitalState);
                                Stage_Idle.FadeTimeMilSeconds = -1;
                                StateMachine.DefaultState = Stage_Idle;
                                SGF.Debuger.Log($"模型显示 加载动画文件 成功 id={GetHashCode()},key={key},path={path}");
                            }
                        }
                        else
                        {
                            SGF.Debuger.LogWarning($"模型显示 加载动画文件 失败 id={GetHashCode()},key={key},path={path}");
                        }
                        cb?.Invoke(idleVitalState);
                    });
                }
                else
                {
                    SGF.Debuger.LogWarning($"模型显示 加载动画文件 失败 id={GetHashCode()},key={key},m_AvatarDataCell=null");
                    cb?.Invoke(idleVitalState);
                }
            }
            else
            {
                SGF.Debuger.LogWarning($"模型显示 加载动画文件 失败 id={GetHashCode()},key={key},idleVitalState=null 动作状态机没找到");
                cb?.Invoke(null);
            }
        }

        public void InitLoadOtherAnimAsync(string vitalStateName, Action<VitalState> cb)
        {
            bool find = false;
            VitalState vitalState = GetVitalState(vitalStateName, out find);
            if (vitalState != null)
            {
                if (m_AvatarDataCell != null)
                {
                    string path = $"{m_AvatarDataCell.AnimsPath}/{vitalStateName}";
                    path = path.Replace("Assets/Res/", string.Empty);
                    path = path.Replace(".anim", string.Empty);
                    SGF.Debuger.Log($"模型显示 加载动画文件 InitLoadOtherAnimAsync id={GetHashCode()},key={key},path={path},AnimsPath={AnimsPath}");
                    AnimsPath = path;
                    StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<AnimationClip>(path, E_AssetType.Animation,
                    (AnimationClip animationClip) =>
                    {
                        if (animationClip != null)
                        {
                            if (path != AnimsPath && AnimsPath != string.Empty)
                            {
                                SGF.Debuger.LogWarning($"模型显示 加载动画文件 成功1 id={GetHashCode()},key={key},path={path},AnimsPath={AnimsPath}, 跟我要的不一样，回收回去吧");
                                return;
                            }
                            else
                            {
                                vitalState.SetClip(animationClip);
                                vitalState.speed = 1;
                            }
                        }
                        cb?.Invoke(vitalState);
                    });
                }
                else
                {
                    cb?.Invoke(vitalState);
                }
            }
            else
            {
                cb?.Invoke(null);
            }
        }


        private void PushModelGameObject(string path, GameObject gob)
        {
            Renderer[] gobRender = gob.GetComponentsInChildren<Renderer>();
            foreach (var item in gobRender)
            {
                if (item != null)
                {
                    item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
            }
            if (m_isPoolsLoad)
            {
                Service.Resource.ResourceFormalManager.Instance.PushGameObject(path, gob);
            }
            else
            {
                GameObject.Destroy(gob);
            }
        }

        /// <summary>
        /// 获取VitalState  获取空闲的VitalState
        /// </summary>
        /// <param name="VitalStateName"></param>
        /// <returns></returns>
        private VitalState GetVitalState(string VitalStateName, out bool find)
        {
            VitalState vitalState = null;
            find = false;
            string stateName = VitalStateName.ToLower();
            if (VitalStateName.IndexOf("idle") != -1)
            {
                vitalState = Stage_Idle;
                find = vitalState != null;
            }

            if (vitalState == null)
            {
                // 使用通用的
                if (CommonVitalStates != null)
                {
                    foreach (var item in CommonVitalStates)
                    {
                        if (!item.enabled)
                        {
                            vitalState = item;
                            break;
                        }
                    }
                }
            }
            return vitalState;
        }

        public void PlayAnim(E_ULayerSubState subState, I_AnimParam animParam, Action action)
        {

        }

        #region 播放 音频事件帧的接口
        public void OnActionPlayWwise(string soundBank, string eventName)
        {
            PlayerWWise(soundBank, eventName);
        }

        /// <summary>
        /// 播放 PlayerWWise 的 虚方法
        /// </summary>
        /// <param name="soundBank"></param>
        /// <param name="eventName"></param>
        public virtual void PlayerWWise(string soundBank, string eventName)
        {
            // 子类实现
        }
        #endregion
    }
}
