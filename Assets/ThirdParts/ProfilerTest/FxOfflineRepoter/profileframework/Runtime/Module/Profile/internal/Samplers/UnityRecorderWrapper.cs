// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/3/8 16:11:32)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using System;


namespace Yoka.Galaxy.Profile
{
    public enum RecorderValueType
    {
        Long = 0,
        Double,
    }

    public enum RecorderValueUnit
    {
        One = 0,
        K,
        M,
    }

    public class UnityRecorderWrapper : IDisposable
    {
        private ProfilerRecorder _generalRecorder;
        private bool _everyFrame;
        private ProfilerCategory _profilerCategory;
        private string _statName;
        private uint _frameCount;
        private double _accumulatedDoubleValue;
        private long _accumulatedLongValue;
        private RecorderValueType _valueType;


        public UnityRecorderWrapper(bool everyFrame, ProfilerCategory profilerCategory, string statName, RecorderValueType valueType)
        {
            _everyFrame = everyFrame;
            _profilerCategory = profilerCategory;
            _statName = statName;
            _generalRecorder = ProfilerRecorder.StartNew(_profilerCategory, _statName);
            _frameCount = 0;
            _valueType = valueType;
        }

        public void Dispose()
        {
            _generalRecorder.Dispose();
        }

        public void FrameUpdate()
        {
            if(_everyFrame)
            {
                _frameCount++;
                switch(_valueType)
                {
                    case RecorderValueType.Long:
                        _accumulatedLongValue += _generalRecorder.LastValue;
                        //Debug.Log($"FrameUpdate _statName:{_statName}, LastValue:{_generalRecorder.LastValue}");
                        break;
                    case RecorderValueType.Double:
                        _accumulatedDoubleValue += _generalRecorder.LastValueAsDouble;
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        public double GetAverageDoubleValue()
        {
            if(!_everyFrame)
            {
                throw new NotImplementedException("_everyFrame is false while getting average value");
            }
            double value = (_frameCount>0)? (_accumulatedDoubleValue / _frameCount) : 0;
            _frameCount = 0;
            _accumulatedDoubleValue = 0;
            return value;
        }

        public double GetLastDoubleValue()
        {
            return _generalRecorder.LastValueAsDouble;
        }

        public long GetAverageLongValue()
        {
            if (!_everyFrame)
            {
                throw new NotImplementedException("_everyFrame is false while getting average value");
            }
            long value = (_frameCount > 0) ? (_accumulatedLongValue / _frameCount) : 0;
            _frameCount = 0;
            _accumulatedLongValue = 0;
            //Debug.Log($"GetAverageLongValue _statName:{_statName}, value:{value}");
            return value;
        }

        public long GetLastLongValue()
        {
            return _generalRecorder.LastValue;
        }
    }
}
#endif