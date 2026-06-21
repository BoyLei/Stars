// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/22 8:36:8)
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
    public class FPSSampler : IProfilerSampler
    {
        private int _frameCount = 0;
        private float _frameTime = 0;

        public void Begin()
        {
        }

        public void End()
        {
        }

        public void Frame(float frameTime)
        {
            ++_frameCount;
            _frameTime += frameTime;
        }

        public string Lable()
        {
            return "frame cost(s)";
        }

        public string TakeSample()
        {
            if(_frameCount > 0)
            {
                float avg = _frameTime / _frameCount;

                _frameCount = 0;
                _frameTime = 0;

                return string.Format("{0:f4}", avg);
            }
            return string.Empty;
        }
    }

}
#endif