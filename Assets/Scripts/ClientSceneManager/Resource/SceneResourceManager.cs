using SGF.Module.Framework;

namespace StarProject.Service.Resource
{
    /// <summary>
    /// 抽象的场景资源的管理(由各个子类型自己去管理自己的类型,参考 LocalFxManager). 
    /// 目前 resouceManagerExtend 在切换场景的时候, 会触发一次 资源的 释放.
    /// 但是 对于各种池 中的资源, resouceManagerExtend 没办法统一(资源引用还是会存在于池中).
    /// 
    /// 所以此处增加 SceneResourceManager, 是为了 细化资源的管理. 
    /// 资源 目前会有以下几种形式:
    /// 1.没有loading, 正常 进入 sceneA;
    /// 2.loading 进入 SceneA 过程中 预加载的资源, 需要在 SceneA 使用, 切出SceneA 后，需要释放(即loading 加载的资源需要计入 sceneA 中);
    /// 3.不切换场景,切换地图(如镜像副本), 当地图切出后, 需要释放之前地图的资源.
    /// 
    /// 综上:
    /// 资源 目前 会绑定与 Scene 和 Map 中。 当其中一个发生变化, 都去释放 与之相关的无用的资源.
    /// </summary>
    public abstract class SceneResourceManager<T> : ServiceModule<T> where T : SceneResourceManager<T>, new()
    {
        /// <summary>
        /// 场景变化时, 释放旧场景相关的资源
        /// </summary>
        /// <param name="newScene"></param>
        /// <param name="oldScene"></param>
        public abstract void OnSceneChange(string oldScene, string newScene);
        /// <summary>
        /// 地图切换时,释放旧地图相关在资源
        /// </summary>
        /// <param name="oldMapId"></param>
        /// <param name="newMapId"></param>
        public abstract void OnMapIdChange(int oldMapId, int newMapId);

        /// <summary>
        /// serverID 切换时, AOI相关的 资源 需要释放
        /// </summary>
        /// <param name="oldServerID"></param>
        /// <param name="newServerID"></param>
        public abstract void OnServerIDChange(ulong oldServerID, ulong newServerID);

        public SceneResourceManager()
        {
            // SGF.Debuger.Log($"[SceneResourceManager] reg listener");
            GlobalEvent.OnMapChange.AddListener(OnMapIdChange);
            GlobalEvent.OnDiffMap_SceneChange.AddListener(OnSceneChange);
            GlobalEvent.OnServerIDChange.AddListener(OnServerIDChange);
        }

    }
}