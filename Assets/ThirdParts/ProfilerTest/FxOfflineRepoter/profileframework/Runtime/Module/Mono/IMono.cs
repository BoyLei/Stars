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
using Yoka.Galaxy.Framework;


namespace Yoka.Galaxy.Mono
{
    public interface IUpdatable : IAspect
    {
        void OnUpdate(float deltaTime);
    }

    public interface ILateUpdatable : IAspect
    {
        void OnLateUpdate(float deltaTime);
    }

    /// <summary>
    /// 封装返回协程的接口
    /// </summary>
    public interface ICoroutine
    {

    }

    public interface IMono : IModule
    {
        void InvokeUpdate();

        void InvokeLateUpdate();

        void InvokeDestroy();

        ICoroutine StartCoroutine(IEnumerator enumerator);

        void StopCoroutine(ICoroutine coroutine);
    }

    public class QMono : QI<IMono> { }
}
#endif