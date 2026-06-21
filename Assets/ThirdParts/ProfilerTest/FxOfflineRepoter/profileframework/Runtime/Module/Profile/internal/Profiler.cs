// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/20 13:53:50)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Yoka.Galaxy.Framework;
using Cysharp.Threading.Tasks;
using Yoka.Galaxy.Mono;

namespace Yoka.Galaxy.Profile
{
    public sealed partial class Profiler : IProfiler, Mono.IUpdatable
    {

        public bool OnResolve(App app)
        {
            TraverseAllTypes();
            ResolveSampler();

            return true;
        }

        public void Dispose()
        {
            Clear();
        }

        public UniTask Begin(string outputPath, string groups)
        {
            QualitySettings.vSyncCount = 0;

            BeginSampler(outputPath);

            return BeginUseCase();
            //return BeginAspects(groups);
        }

        public void End()
        {
            EndSampler();
        }

        private void Clear()
        {
            ClearSampler();
        }

        private void TraverseAllTypes()
        {
            List<Assembly> assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
            Assembly assembly = Assembly.GetExecutingAssembly();
            assemblies.Remove(assembly);
            assemblies.Insert(0, assembly);
            for (int i = 0; i < assemblies.Count; i++)
            {
                Type[] types = assemblies[i].GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];
                    TryParseSampler(type);
                }
            }
        }
    }

}
#endif