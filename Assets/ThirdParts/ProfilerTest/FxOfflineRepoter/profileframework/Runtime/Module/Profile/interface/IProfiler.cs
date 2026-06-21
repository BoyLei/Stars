// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/20 13:51:8)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yoka.Galaxy.Framework;


namespace Yoka.Galaxy.Profile
{
    public interface IProfiler : IModule
    {
        UniTask Begin(string outputPath, string groups);

        void End();
    }

}
#endif