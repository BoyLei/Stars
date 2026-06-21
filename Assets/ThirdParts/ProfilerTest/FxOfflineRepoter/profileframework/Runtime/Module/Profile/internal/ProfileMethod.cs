// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/21 9:21:35)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Text;

namespace Yoka.Galaxy.Profile
{
    internal class MethodCache
    {
        public Type classType;
        public MethodInfo method;
        public string name;
        public string group;
        public int priority;
        public string tips;
    }

    public class CommandResult
    {
        public bool success;
        public string message;
        public object returnValue;
    }

    public class ProfileMethod
    {

    }
}
#endif