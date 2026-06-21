// -----------------------------------------------------------------------
// This file is part of framework.
// 
// (c) rick <ninglei@dobest.com>
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoka.Galaxy.Framework
{
    public abstract class App : IModuleContainer
    {

        protected static App _baseGameInstatnce;
        public static App GetInstance()
        {
            return _baseGameInstatnce;
        }

        public void Init()
        {
            _baseGameInstatnce = this;
            PreRegisterModules();
        }

        public void Destroy()
        {
            OnDestroy();
            _baseGameInstatnce = null;
        }

        public virtual void ExitGame(bool forceQuit)
        {
            Application.Quit();
        }

        #region Unity MonoBehaviour调用函数

        /// <summary>
        /// 清除对象
        /// </summary>
        public abstract void OnDestroy();
        #endregion

        protected abstract void PreRegisterModules();

        public abstract I Resolve<I>() where I : IModule;

        public abstract bool Register<I, T>(bool resolveInstantly)
            where I : IModule
            where T : I;

        public abstract bool HasRegister<I>() where I : IModule;

        public abstract void AddAspectContainer<A>(IAspectContainer aspectContainer) where A : IAspect;

        public abstract void AssignPriorityForAspect<A>(bool undefinedToTail, List<Type> priorityList) where A : IAspect;
    }


}
#endif