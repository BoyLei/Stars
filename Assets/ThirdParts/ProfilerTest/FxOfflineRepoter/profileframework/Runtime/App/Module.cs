// -----------------------------------------------------------------------
// This file is part of framework.
// 
// (c) rick <ninglei@dobest.com>
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Yoka.Galaxy.Framework
{

    /// <summary>
    /// 管理器接口
    /// 生命周期：Init -> Boot -> Shutdown -> Dispose
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// 生命周期函数，初始化，整个游戏生命期只执行一遍
        /// </summary>
        /// <returns></returns>
        bool OnResolve(App app);

        /// <summary>
        /// 生命周期函数，清理，整个游戏生命期只执行一遍
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// 模块容器
    /// </summary>
    public interface IModuleContainer
    {
        /// <summary>
        /// 注册
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="resolveInstantly"></param>
        /// <returns></returns>
        bool Register<I, T>(bool resolveInstantly) where I : IModule where T : I;

        /// <summary>
        /// 获取
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <returns></returns>
        I Resolve<I>() where I : IModule;
    }

    public abstract class QI<I> where I : IModule
    {
        private static I _instance = default;

        public static I Instance
        {
            get
            {
                if (_instance == null)
                {
                    var app = App.GetInstance();
                    if (app != null)
                    {
                        _instance = app.Resolve<I>();
                    }
                }
                return _instance;
            }
        }
    }

}
#endif