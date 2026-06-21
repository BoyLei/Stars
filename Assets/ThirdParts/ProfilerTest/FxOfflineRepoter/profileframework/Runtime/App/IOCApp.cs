// -----------------------------------------------------------------------
// This file is part of framework.
// 
// (c) rick <ninglei@dobest.com>
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Yoka.Galaxy.Framework
{
    public class IOCApp : App
    {
        Dictionary<Type, Type> _resolveTypeDict = null;
        private Dictionary<Type, IModule> _managerDict = null;
        Stack<IModule> _moduleByResolveOrder = null;
        List<IAspectContainer> _aspectContainers = null;
        Dictionary<Type, IAspectContainer> _aspectContainerDict = null;

        public override void OnDestroy()
        {
            //清理
            while (_moduleByResolveOrder.Count > 0)
            {
                var module = _moduleByResolveOrder.Pop();
                module.Dispose();
            }
        }

        protected override void PreRegisterModules()
        {
            Debug.AssertFormat(_resolveTypeDict == null, "_resolveTypeDict should keep uninitialized on init");
            Debug.AssertFormat(_managerDict == null, "_managerDict should keep uninitialized on init");
            Debug.AssertFormat(_moduleByResolveOrder== null, "_moduleByResolveOrder should keep uninitialized on init");
            Debug.AssertFormat(_aspectContainers == null, "_aspectContainers should keep uninitialized on init");
            Debug.AssertFormat(_aspectContainerDict == null, "_aspectContainerDict should keep uninitialized on init");
            _resolveTypeDict = new Dictionary<Type, Type>();
            _managerDict = new Dictionary<Type, IModule>();
            _moduleByResolveOrder = new Stack<IModule>();
            _aspectContainers = new List<IAspectContainer>();
            _aspectContainerDict = new Dictionary<Type, IAspectContainer>();
        }

        public override bool Register<I, T>(bool resolveInstantly)
        {
            Type tI = typeof(I);
            if (_resolveTypeDict.ContainsKey(tI))
            {
                if(Application.isPlaying)
                {
                    Debug.LogErrorFormat("interface type already registered: {0}", tI.ToString());
                }
                else
                {
                    Debug.LogFormat("Editor mode: interface type already registered: {0}", tI.ToString());
                }
                return false;
            }
            Type tT = typeof(T);
            _resolveTypeDict.Add(tI, tT);
            Debug.LogFormat("{0} registered to {1}", tI.ToString(), tT.ToString());
            if(resolveInstantly)
            {
                Resolve<I>();
            }
            return true;
        }

        public override I Resolve<I>()
        {
            Type tI = typeof(I);
            if (_managerDict.TryGetValue(tI, out var output))
            {
                if(output is I)
                {
                    return (I)output;
                }
                else
                {
                    Debug.LogErrorFormat("Resolve failed, {0} is not {1}", output.GetType().ToString(), tI.ToString());
                    return default(I);
                }
            }
            if(_resolveTypeDict.TryGetValue(tI, out Type resolveType))
            {
                var moduleObject = resolveType.Assembly.CreateInstance(resolveType.ToString());
                IModule module = moduleObject as IModule;
                if(module is I)
                {
                    _managerDict.Add(tI, module);
                    bool success = module.OnResolve(this);
                    if(success)
                    {
                        _moduleByResolveOrder.Push(module);

                        foreach(var aspectContainer in _aspectContainers)
                        {
                            aspectContainer.Recieve(module);
                        }

                        return (I)module;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Resolve failed, {0} OnResolve failed", module.GetType().ToString());
                        return default(I);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Resolve failed, {0} is not {1}", module.GetType().ToString(), tI.ToString());
                    return default(I);
                }
            }
            else
            {
                Debug.LogErrorFormat("{0} is not registered", tI.ToString());
                return default(I);
            }
        }

        public override bool HasRegister<I>()
        {
            return _resolveTypeDict.ContainsKey(typeof(I));
        }

        public override void AddAspectContainer<A>(IAspectContainer aspectContainer)
        {
            _aspectContainers.Add(aspectContainer);
            var type = typeof(A);
            Debug.AssertFormat(!_aspectContainerDict.ContainsKey(type), "_aspectContainerDict already has registered for type {0}", type.Name);
            _aspectContainerDict.Add(type, aspectContainer);
        }

        public override void AssignPriorityForAspect<A>(bool undefinedToTail, List<Type> priorityList)
        {
            var type = typeof(A);
            Debug.AssertFormat(_aspectContainerDict.ContainsKey(type), "_aspectContainerDict contains nothing for type {0}", type.Name);
            var container = _aspectContainerDict[type];
            container.AssginPriority(undefinedToTail, priorityList);
        }
    }
}
#endif