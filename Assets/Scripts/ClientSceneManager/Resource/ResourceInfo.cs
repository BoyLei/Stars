using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProject.Game.Entity.Factory;
using UnityEngine.SceneManagement;
using StarProject.Game;
using StarProjectDef;

namespace StarProject.Service.Resource
{
    /// <summary>
    /// 资源信息的基类, 存储了 资源所在的路径/场景/和地图.
    /// 不同的 资源类型 通过子类继承的方式, 去 存储 相应类型的 资源.
    /// 比如特效类型,  存储的 是特效prefab 实例化的 节点
    /// </summary>
    public abstract class ResourceInfo : SimpleDataObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// 资源路径
        /// </summary>
        protected string Path;
#endif
        /// <summary>
        /// 资源 存在于 什么场景
        /// </summary>
        // protected string SceneName;

        // /// <summary>
        // /// 资源存在于什么地图上
        // /// </summary>
        // protected int MapId;

        // /// <summary>
        // /// 资源存在于 什么serverID 线上, 当 切线的时候，地图一般不需要切换，但是AOI相关的资源需要释放
        // /// </summary>
        // protected ulong ServerID;

        /// <summary>
        /// 资源的 释放类型
        /// </summary>
        protected ResoruceReleaseType ReleaseType = ResoruceReleaseType.MapSceneAndServerID;

        /// <summary>
        /// 是否是 预加载loading 场景过程中的资源.
        /// 如果是 loading 过程中的资源,这个资源 归属于 后面需要切换的场景
        /// </summary>
        // public bool isLoadingScene = false;

        public virtual void Create(string path, ResoruceReleaseType releaseType = ResoruceReleaseType.MapSceneAndServerID)
        {
            // SceneName = StarScenesManager.Instance.CurSceneName;
            // MapId = GameManager.Instance.GetCurMapId();
            // ServerID = GameManager.Instance.GetCurServerID();
#if UNITY_EDITOR
            Path = path;
#endif
            ReleaseType = releaseType;

        }

        protected override void Release()
        {
            // MapId = -1;
            // ServerID = 0;
#if UNITY_EDITOR
            Path = string.Empty;
#endif
            // SceneName = string.Empty;
            ReleaseType = ResoruceReleaseType.MapSceneAndServerID;
        }

        public bool EqualMap(int mapId)
        {
            return true;
            // return MapId == mapId;
        }

        public bool EqualServerID(ulong serverID)
        {
            return true;
            // return ServerID == serverID;
        }

        public bool EqualScene(string scene)
        {
            return true;
            // return SceneName.Equals(scene);
        }

        public bool CheckCanRelease(TryReleaseResouceType tryReleaseType)
        {

            switch (tryReleaseType)
            {
                // 如果是 force 类型, 那就 可以释放
                case TryReleaseResouceType.Force:
                    {
                        return true;
                    }
                // 如果是 切场景触发的释放, 那应该 只要不是 force, 都可以释放
                case TryReleaseResouceType.Scene:
                    {
                        return ReleaseType != ResoruceReleaseType.Force;
                    }
                case TryReleaseResouceType.Map:
                    {
                        return ReleaseType == ResoruceReleaseType.MapSceneAndServerID || ReleaseType == ResoruceReleaseType.MapAndScene;
                    }
                case TryReleaseResouceType.ServerID:
                    {
                        return ReleaseType == ResoruceReleaseType.MapSceneAndServerID;
                    }
                default: break;
            }

            return true;
        }
    }
}
