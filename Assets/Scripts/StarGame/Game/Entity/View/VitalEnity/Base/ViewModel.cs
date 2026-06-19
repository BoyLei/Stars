using Animancer;
using SGF.Unity;
using StarProject.OffLine;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

//项目,游戏,实体,工厂  管理
namespace StarProject.Game.Entity.Factory
{
    //有模型的层
    public abstract class ViewModel : ViewObject, IRecyclableObject
    {
        //path
        //绑定模型重写

        //基础生命周期

        /// <summary>
        /// 模型的父节点，专门用来做模型的 角度和 偏移。角色控制器 控制的是 modelOffse 的父结点
        /// </summary>
        protected Transform m_ModelOffset;
        protected int OutlineActiveCount;
        public virtual string ModelPath { get; }
        public virtual float ModleScale { get; }

        [SerializeField]
        protected AnimancerComponent _Animancer;
        public AnimancerComponent Animancer => _Animancer;

        public ParticleSystem SelfParticleSystem = null;

        //离线绑点数据
        public ModelOffLineData M_ModelOffLineData = null;

        protected Dictionary<string, GameObject> ModelRecord = new();

        protected bool IsRelease = false;
        private Action<GameObject> LoadModelCB = null;

        protected override void Create(EntityObject entity)
        {
            //提取自己写
            IsRelease = false;
        }

        protected override void Create(EntityObject entity, string modelPath)
        {
            //提取自己写
            IsRelease = false;
        }

        protected virtual void Reset()
        {

        }

        protected override void Release()
        {
            if (OutlineActiveCount > 0)
            {
                DynamicRenderQueueManager.Instance.RemoveNewRender(OutlineActiveCount);
            }

            foreach (var item in ModelRecord)
            {
                Service.Resource.ResourceFormalManager.Instance.PushGameObject(item.Key, item.Value);
            }
            LoadModelCB=null;
            M_ModelOffLineData = null;
            SelfParticleSystem = null;
            ModelRecord?.Clear();
            //剩下的自己写
            //base.Release();
            IsRelease = true;
        }

        [Obsolete("请使用 CreateModelAsync", false)]//标记该方法已弃用
        public GameObject CreateModel(string modelPath, E_EntityType e_EntityType)
        {
            GameObject gob = Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(modelPath, E_AssetType.Roles);
            if (gob == null)
            {
                if (e_EntityType == E_EntityType.Player)
                {
                    gob = Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject("Roles/World/Character/cm/CM_Model", E_AssetType.Roles);
                }
                else
                {
                    gob = Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject("Roles/World/Monster/cm/CM_Model", E_AssetType.Roles);
                }
            }
            gob.transform.SetParent(m_ModelOffset, false);
            gob.transform.localPosition = Vector3.zero;

            return gob;
        }

        public void CreateModelAsync(string modelPath, Action<GameObject> cb)
        {
            LoadModelCB = (GameObject go) =>
            {
                LoadModelCB = null;
                if (go != null && IsRelease)
                {
                    Debug.LogError("模型被释放了" +modelPath);
                    Service.Resource.ResourceFormalManager.Instance.PushGameObject(modelPath, go);
                    return;
                }
                if (go != null)
                {
                    go.transform.SetParent(m_ModelOffset, false);
                    go.transform.localPosition = Vector3.zero;
                }
                cb?.Invoke(go);
                cb = null;
            };
            Service.Resource.ResourceFormalManager.Instance.LoadTagGuidGameObject(modelPath, E_AssetType.Roles, true, LoadModelCB, this.GetHashCode());

        }

        /// <summary>
        /// 刷新动画组件
        /// </summary>
        /// <param name="modelGob"></param>
        protected void RefreshModelAnimancer(GameObject modelGob)
        {
            Animator animator = modelGob.GetComponent<Animator>();
            if (animator == null)
            {
                modelGob.AddComponent<Animator>();
            }
            // _Animancer = GameObjectUtils.EnsureComponent<AnimancerComponent>(gob);
            _Animancer = GameObjectUtils.EnsureComponent<AnimancerExtend>(modelGob);
            if (_Animancer.Animator == null || _Animancer.Animator != animator)
            {
                _Animancer.Animator = animator;
            }
            // 节点 active =false 并不会清除 action, 所以 设置节点 active = true 后 并不需要重新 设置 action
            (_Animancer as AnimancerExtend).ActionOnPlayWwise = OnActionPlayWwise;
        }

        /// <summary>
        /// 刷新 模型 节点的 骨骼离线数据
        /// </summary>
        /// <param name="modelGob"></param>
        protected void RefreshModelOfflineData(GameObject modelGob)
        {
            var modelOffLineData = modelGob.GetComponent<ModelOffLineData>();
            if (modelOffLineData == null)
            {
                modelOffLineData = modelGob.AddComponent<ModelOffLineData>();
                modelOffLineData.BindDummyPos.Clear();
                modelOffLineData.BindDummyPos.Add("Root", modelGob.transform);
            }
            M_ModelOffLineData = modelOffLineData;
            if (M_ModelOffLineData.BindDummyPos.Count <= 0)
            {
                modelOffLineData.BindDummyPos.Add("Root", modelGob.transform);
            }
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