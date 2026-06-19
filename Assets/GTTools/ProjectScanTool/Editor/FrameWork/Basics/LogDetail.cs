/*
 * @Description: 检查后生成的详细日志展示
 */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{
    [Serializable, HideLabel, HideReferenceObjectPicker]
    public class LogDetail
    {
        [VerticalGroup("详情")]
        [NonSerialized, ShowInInspector, ReadOnly, HideLabel]
        public UnityEngine.Object warnObj;

        [VerticalGroup("详情")]
        [NonSerialized, ShowInInspector, ReadOnly, HideLabel]
        public string warnPath = "";

        [VerticalGroup("详情")]
        [NonSerialized, ShowInInspector, ReadOnly, HideLabel, DisplayAsString(Overflow = false)]
        public string warnInfo = "";

    }

    [Serializable, HideLabel, HideReferenceObjectPicker]
    public class LogStatistics
    {
        [HideInInspector]
        public CustomRule customRule;

        [HideLabel, DisplayAsString(false)]
        [VerticalGroup("统计结果")]
        [HorizontalGroup("统计结果/a", Width= 0.43f)]
        public string description;

        //扫描总数
        [HideLabel, DisplayAsString(false)]
        [VerticalGroup("统计结果")]
        [HorizontalGroup("统计结果/a", Width= 0.23f)]
        public string scanTotalDsc = "";

        //异常数量
        [HideLabel, DisplayAsString(false)]
        [VerticalGroup("统计结果")]
        [HorizontalGroup("统计结果/a", Width= 0.23f)]
        public string abnormalDsc = "";

        [ShowIf("LogIsNotEmpty")]
        [HorizontalGroup("统计结果/a", Width = 0.11f), LabelText("展开详情"), LabelWidth(52)]
        public bool viewDetail = false;
        private bool LogIsNotEmpty()
        {
            return logDetails.Count > 0;
        }


        //统计异常对象个数
        // [HideInInspector]
        // public List<UnityEngine.Object> abnormalObjs = new List<UnityEngine.Object>();

        [VerticalGroup("统计结果")]
        [TableList(IsReadOnly = true, HideToolbar = true, AlwaysExpanded = true, ShowIndexLabels = true, DrawScrollView = true, MinScrollViewHeight = 200, MaxScrollViewHeight = 200)]
        [ShowIf("CheckShowLogs")]
        public List<LogDetail> logDetails = new List<LogDetail>();
        private bool CheckShowLogs()
        {
            return viewDetail && logDetails.Count > 0;
        }

        public void InsertLog(UnityEngine.Object context, string logDetail, string path)
        {

            foreach (var item in logDetails)
            {
                if (item.warnPath == path)
                {
                    item.warnInfo += "\n" + logDetail;
                    return;
                }
            }

            logDetails.Add(new LogDetail()
            {
                warnObj = context,
                warnInfo = logDetail,
                warnPath = path
            });
        }

        public void Statistics()
        {
            if (logDetails.Count == 0)
            {
                description = string.Format("<color=#00ff00>{0}</color>", customRule.ruleTitle);
                scanTotalDsc = string.Format("<color=#00ff00>共扫描资源数量 {0}</color>", customRule.scanAssetsCnt);
                abnormalDsc = string.Format("<color=#00ff00>无异常</color>");
            }
            else
            {
                description = string.Format("<color=#ff0000>{0}</color>", customRule.ruleTitle);
                scanTotalDsc = string.Format("<color=#ff0000>共扫描资源数量 {0}</color>", customRule.scanAssetsCnt);
                // abnormalDsc = string.Format("<color=#ff0000>异常资源数 {0} ({1}%)</color>", abnormalObjs.Count, Math.Round(100f * abnormalObjs.Count / customRule.scanAssetsCnt, 2));
                abnormalDsc = string.Format("<color=#ff0000>异常资源数 {0} ({1}%)</color>", logDetails.Count, Math.Round(100f * logDetails.Count / customRule.scanAssetsCnt, 2));
            }
        }

    }
}
