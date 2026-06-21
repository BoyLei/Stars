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
    public struct MonoCoroutine : ICoroutine
    {
        private readonly object _coroutine;

        public MonoCoroutine(object coroutine)
        {
            _coroutine = coroutine;
        }

        public object CoroutineObject
        {
            get
            {
                return _coroutine;
            }
        }
    }

    public class Mono : IMono
    {
        AspectContainer<IUpdatable> _updateContainer = null;
        AspectContainer<ILateUpdatable> _lateUpdateContainer = null;
        UnityMono _mono;

        public void Dispose()
        {
        }

        public bool OnResolve(App app)
        {
            var exist = GameObject.FindObjectOfType<UnityMono>();
            if (exist)
            {
                exist.MonoModule = this;
                _mono = exist;
            }
            else
            {
                GameObject monoRoot = new GameObject("monoRoot");
                UnityMono unityMono = monoRoot.AddComponent<UnityMono>();
                unityMono.MonoModule = this;
                if (Application.isPlaying)   //编辑器模式不允许DontDestroyOnLoad操作
                {
                    GameObject.DontDestroyOnLoad(monoRoot);
                }
                _mono = unityMono;
            }

            _updateContainer = new AspectContainer<IUpdatable>();
            app.AddAspectContainer<IUpdatable>(_updateContainer);
            _lateUpdateContainer = new AspectContainer<ILateUpdatable>();
            app.AddAspectContainer<ILateUpdatable>(_lateUpdateContainer);

            return true;
        }

        public void InvokeLateUpdate()
        {
            _lateUpdateContainer.Fetch().ForEach((updatable) =>
            {
                updatable.OnLateUpdate(Time.deltaTime);
            });
        }

        public void InvokeUpdate()
        {
            _updateContainer.Fetch().ForEach((updatable) =>
            {
                updatable.OnUpdate(Time.deltaTime);
            });
        }

        public void InvokeDestroy()
        {
            var app = App.GetInstance();
            if (app != null)
            {
                app.Destroy();
            }
        }

        public ICoroutine StartCoroutine(IEnumerator enumerator)
        {
            if (Application.isPlaying)
            {
                return new MonoCoroutine(_mono.StartCoroutine(enumerator));
            }
            else
            {
#if UNITY_EDITOR
                return new MonoCoroutine(Yoka.EditorExtension.EditorCoroutineUtility.StartCoroutineOwnerless(enumerator));
#else
                //GLog.Error("StartCoroutine should not reach here!");
                return null;
#endif
            }
        }

        public void StopCoroutine(ICoroutine coroutine)
        {
            if (Application.isPlaying)
            {
                if (coroutine != null)
                {
                    var c = ((MonoCoroutine)coroutine).CoroutineObject as Coroutine;
                    if (c != null)
                    {
                        _mono.StopCoroutine(c);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                Yoka.EditorExtension.EditorCoroutineUtility.StopCoroutine(((MonoCoroutine)coroutine).CoroutineObject as EditorExtension.EditorCoroutine);
#else
                //GLog.Error("StopCoroutine should not reach here!");
#endif
            }
        }
    }

}
#endif