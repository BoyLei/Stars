// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/20 13:9:0)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoka.Galaxy.Profile
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProfilerAttribute: Attribute
    {
        private string _profilerGroup;
        private int _priority;

        public ProfilerAttribute(string profilerGroup, int priority)
        {
            this._profilerGroup = profilerGroup;
            this._priority = priority;
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class ProfilerMethodAttribute: Attribute
    {
        private string _profilerGroup;
        public string ProfilerGroup
        {
            get
            {
                return this._profilerGroup;
            }
        }

        private int _priority;

        public int Priority
        {
            get
            {
                return this._priority;
            }
        }

        private string _tips;

        public string Tips
        {
            get
            {
                return this._tips;
            }
        }

        public ProfilerMethodAttribute(string profilerGroup, int priority, string tips)
        {
            this._profilerGroup = profilerGroup;
            this._priority = priority;
            this._tips = tips;
        }
    }

}
#endif