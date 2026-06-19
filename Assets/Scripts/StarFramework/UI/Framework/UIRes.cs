using System;
using UnityEngine;

namespace SGF.UI.Framework
{
    [XLua.LuaCallCSharp]
    public static class UIRes
    {
        //UI文件夹里面的东西不用单写。
        public static string UIResRoot = "UI/";

        [Obsolete("请使用LoadPrefabAsync", false)]//标记该方法已弃用
        /// <summary>
        /// 加载UI的Prefab
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject LoadPrefab(string name)
        {
            name = UIResRoot + name;
            SGF.Debuger.Log("加载界面资源：" + name);
            // GameObject asset = (GameObject)Resources.Load(name);
            GameObject asset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadGameObject(name);

            return asset;
        }

        public static void LoadPrefabAsync(string name, Action<GameObject> cb)
        {
            name = UIResRoot + name;
            SGF.Debuger.Log("加载界面资源：" + name);

            //资源加载
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(name,
                (GameObject go) =>
                {
                    if (go == null)
                    {
                        cb?.Invoke(null);
                        return;
                    }

                    var gob = GameObject.Instantiate<GameObject>(go);
                    if (gob != null)
                    {
                        cb?.Invoke(gob);
                    }
                });
        }

        private static DictionaryEx<string, Action<GameObject>> _asyncLoadCb = new();
        /// <summary>
        /// 重复互斥的异步加载资源接口,  在loading A 期间重复多次加载 A, 只有最后一次加载 执行回调.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cb"></param>
        public static void LoadPrefabAsyncRepetMutex(string name, Action<GameObject> cb)
        {
            name = UIResRoot + name;
            SGF.Debuger.Log("加载界面资源：" + name);

            // 重新生成 key, 防止 其它的 接口加载相同的资源 写入相同的key
            string key = $"rep_mut_{name}";
            _asyncLoadCb[key] = cb;

            //资源加载
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(name,
                (GameObject go) =>
                {
                    // 如果没有加载的回调了, 说明 这个接口资源加载的回调被后面相同的资源加载 互斥干掉, 所以可以直接return
                    if (!_asyncLoadCb.ContainsKey(key))
                    {
                        return;
                    }
                    var loadCb = _asyncLoadCb[key];
                    // 移除对应的 loadCB
                    _asyncLoadCb.Remove(key);


                    if (go == null)
                    {
                        loadCb?.Invoke(null);
                        return;
                    }

                    var gob = GameObject.Instantiate<GameObject>(go);
                    if (gob != null)
                    {
                        loadCb?.Invoke(gob);
                    }
                });
        }

        public static void LoadPrefabTest(string name, Action<GameObject> cb)
        {
            name = UIResRoot + name;
            SGF.Debuger.Log("加载界面资源：" + name);

            //资源加载
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(name,
                (GameObject go) =>
                {
                    if (go == null)
                    {
                        return;
                    }

                    var gob = GameObject.Instantiate<GameObject>(go);
                    if (gob != null)
                    {
                        cb?.Invoke(gob);
                    }
                });
        }
    }
}
