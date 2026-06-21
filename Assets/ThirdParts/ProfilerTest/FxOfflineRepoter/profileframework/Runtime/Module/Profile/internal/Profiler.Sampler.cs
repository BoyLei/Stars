// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/21 15:29:22)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;


namespace Yoka.Galaxy.Profile
{

    public sealed partial class Profiler
    {
        private IProfilerOutputFormatter _tableFormatter = null;
        private IProfilerOutputData _runningOutputData = null;
        private List<IProfilerSampler> _samplerList = new List<IProfilerSampler>();
        private Type _samplerInterface = typeof(IProfilerSampler);
        private float _timeCounter = 0;
        private float _sampleTime = 1f;
        private int _frameCount = 0;
        private bool _sampling = false;

        private void ResolveSampler()
        {
        }

        private void BeginSampler(string outputPath)
        {
            _timeCounter = 0;

            _runningOutputData = new ProfilerStreamOutputData();
            _runningOutputData.Begin(outputPath);

            _sampling = true;

            _tableFormatter = new ProfilerTableFormatter();
            _tableFormatter.Begin(_runningOutputData, _samplerList);

            _samplerList.ForEach((sampler) =>
            {
                sampler.Begin();
            });
        }

        private void EndSampler()
        {
            _sampling = false;

            _samplerList.ForEach((sampler) =>
            {
                sampler.End();
            });

            _tableFormatter.End(_runningOutputData);

            _runningOutputData.End();
        }

        private void ClearSampler()
        {
            _runningOutputData = null;
        }

        private void OnMethodBegin(string methodName)
        {
            _tableFormatter.Tag(methodName);
        }

        private void TryParseSampler(Type type)
        {
            if(!IsSampler(type)) return;
            var instance = type.Assembly.CreateInstance(type.FullName);
            IProfilerSampler sampler = instance as IProfilerSampler;
            _samplerList.Add(sampler);
        }

        public bool IsSampler(Type type)
        {
            if (type.IsAbstract)
            {
                return false;
            }
            if (!_samplerInterface.IsAssignableFrom(type))
            {
                return false;
            }

            return true;
        }


        public void OnUpdate(float deltaTime)
        {
            if(_sampling)
            {
                foreach (var sampler in _samplerList)
                {
                    sampler.Frame(deltaTime);
                }

                _timeCounter += deltaTime;
                _frameCount++;
                if (_timeCounter >= _sampleTime)
                {
                    _timeCounter -= _sampleTime;
                    _tableFormatter.TakeSample(_runningOutputData, _samplerList, GetRunningRootCase(), _frameCount);
                }
            }
        }
    }

}
#endif