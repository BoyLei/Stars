using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Diagnostics;

namespace GameDLL.Resource.Statistics
{
    /// <summary>
    /// by Rick 统计门面类，避免各个统计接口过于耦合
    /// </summary>
    public class StatisticsFacade
    {
        static bool logVerboseMode = false;
        static public bool LogVerboseMode
        {
            get { return logVerboseMode; }
            set { logVerboseMode = value; }
        }
    }
}
