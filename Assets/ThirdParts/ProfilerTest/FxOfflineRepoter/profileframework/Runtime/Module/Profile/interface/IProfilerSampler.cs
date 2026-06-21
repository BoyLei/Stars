// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/21 15:52:21)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoka.Galaxy.Profile
{
    public interface IProfilerSampler
    {
        void Begin();

        void Frame(float frameTime);

        string Lable();

        string TakeSample();

        void End();
    }

}
#endif