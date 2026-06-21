// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/21 11:16:9)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Yoka.Galaxy.Profile
{
    public abstract class ProfilerUseCase : ScriptableObject
    {
        [VerticalGroup("UseCase")]
        [TableColumnWidth(200, Resizable = false)]
        public string tag;

        [VerticalGroup("UseCase")]
        public bool active;

        public abstract UniTask Run();

        public virtual string ReportStatus(IProfilerOutputFormatter formatter)
        {
            return tag;
        }

        public virtual void Sync(HashSet<ProfilerUseCase> usingCases)
        {
            usingCases.Add(this);
            name = tag;
        }

    }
}
#endif