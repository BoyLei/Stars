// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/3/1 14:13:24)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Yoka.Galaxy.Profile
{
    public sealed partial class Profiler
    {
        private ProfilerUseCase _runningRootCase = null;

        private UniTask BeginUseCase()
        {
            var task = UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                var cases = UseCaseSetting.Fetch();
                foreach(var c in cases.cases)
                {
                    if (!c.active)
                        continue;
                    _runningRootCase = c;
                    OnMethodBegin(c.tag);
                    try
                    {
                        await c.Run();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    PostUseCase();
                    _runningRootCase = null;
                }
            });
            return task;
        }

        private ProfilerUseCase GetRunningRootCase()
        {
            return _runningRootCase;
        }

        private void PostUseCase()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }

}
#endif