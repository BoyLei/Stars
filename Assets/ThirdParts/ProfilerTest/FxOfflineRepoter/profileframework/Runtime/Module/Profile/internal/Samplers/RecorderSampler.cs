// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/3/8 16:38:31)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using System;


namespace Yoka.Galaxy.Profile
{
    public abstract class RecorderSampler : IProfilerSampler
    {
        private UnityRecorderWrapper _wrapper = null;
        private bool _average = false;
        private ProfilerCategory _profilerCategory;
        private string _statName;
        private string _lable;
        private RecorderValueType _valueType;
        private RecorderValueUnit _valueUnit;

        public RecorderSampler()
        {
            DoInit();
        }

        protected abstract void DoInit();

        protected void InitImp(bool average, ProfilerCategory profilerCategory, string statName, string lable, RecorderValueType valueType, RecorderValueUnit valueUnit)
        {
            _average = average;
            _profilerCategory = profilerCategory;
            _statName = statName;
            _lable = lable;
            _valueType = valueType;
            _valueUnit = valueUnit;
        }

        public void Begin()
        {
            _wrapper = new UnityRecorderWrapper(_average, _profilerCategory, _statName, _valueType);
        }

        public void End()
        {
            _wrapper.Dispose();
            _wrapper = null;
        }

        public void Frame(float frameTime)
        {
            if(_average )
            {
                _wrapper.FrameUpdate();
            }
        }

        public string Lable()
        {
            return _lable;
        }

        private string LongToString(long value )
        {
            switch(_valueUnit)
            {
                case RecorderValueUnit.One:
                    return value.ToString();
                case RecorderValueUnit.K:
                    return $"{Math.Round((double)value / 1024, 2)}";
                case RecorderValueUnit.M:
                    return $"{Math.Round((double)value / (1024 * 1024), 4)}";
            }
            throw new System.NotImplementedException();
        }

        private string DoubleToString(double value)
        {
            switch (_valueUnit)
            {
                case RecorderValueUnit.One:
                    return value.ToString();
                case RecorderValueUnit.K:
                    return $"{Math.Round(value / 1024, 2)}";
                case RecorderValueUnit.M:
                    return $"{Math.Round(value / (1024 * 1024), 2)}";
            }
            throw new System.NotImplementedException();
        }

        public string TakeSample()
        {
            //GLog.LogFormat("TakeSample {0}", _statName);
            switch(_valueType)
            {
                case RecorderValueType.Long:
                    if(_average )
                    {
                        return LongToString(_wrapper.GetAverageLongValue());
                    }
                    else
                    {
                        return LongToString(_wrapper.GetLastLongValue());
                    }
                case RecorderValueType.Double:
                    if(_average)
                    {
                        return DoubleToString(_wrapper.GetAverageDoubleValue());
                    }
                    else
                    {
                        return DoubleToString(_wrapper.GetLastDoubleValue());
                    }
            }
            throw new System.NotImplementedException();
        }
    }

}
#endif