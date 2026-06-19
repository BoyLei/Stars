///--------------------------------------------------------------------
/// 文件名   :   StageTrack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 18:35:47
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [DisplayName("阶段轨道")]
    [TrackColor(0.1f, 1, 0.5f)]
    [TrackClipType(typeof(ClipStageAsset))]
    [TrackClipType(typeof(TriggerStageAsset))]
    [TrackClipType(typeof(AddBuffStageAsset))]
    [TrackClipType(typeof(BuffEndStageAsset))]
    [TrackClipType(typeof(BulletStageAsset))]
    [TrackClipType(typeof(PassiveEndStageAsset))]
    public class StageTrack : TrackAsset, INotification
    {
        /// <summary>
        /// 轨道名称
        /// </summary>
        public string trackName;

        public PropertyName id { get; }


        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.UseCustom = true;
            clip.duration = 1;
            clip.CustomColor = TranslateColor(clip.displayName);
            clip.displayName = TranslateName(clip.displayName);
            if (clip.asset is ClipStageAsset stageAsset)
            {
                if (stageAsset.template.data==null)
                {
                    stageAsset.template.data = new StageNormal();
                }
                stageAsset.template.data.StageID = GetStageID(1001, "一般阶段");
            }
            else if (clip.asset is TriggerStageAsset triggerStage)
            {
                if (triggerStage.template.data == null)
                {
                    triggerStage.template.data = new StageTrigger();
                }
                triggerStage.template.data.StageID = GetStageID(2001, "触发阶段");
            }

            else if (clip.asset is AddBuffStageAsset addBuffStage)
            {
                if (addBuffStage.template.data == null)
                {
                    addBuffStage.template.data = new StageBUFFStart();
                }
                addBuffStage.template.data.StageID = GetStageID(3001, "Buff添加阶段");
            }

            else if (clip.asset is BuffEndStageAsset buffEndStage)
            {
                if (buffEndStage.template.data == null)
                {
                    buffEndStage.template.data = new StageBUFFEnd();
                }
                buffEndStage.template.data.StageID = GetStageID(4001, "Buff结束阶段");
            }
            else if (clip.asset is BulletStageAsset bulletStageAsset)
            {
                if (bulletStageAsset.template.data == null)
                {
                    bulletStageAsset.template.data = new StageBulletMotion();
                }
                bulletStageAsset.template.data.StageID = GetStageID(5001, "子弹移动阶段");
            }

            else if (clip.asset is PassiveEndStageAsset passiveEndStageAsset)
            {
                if (passiveEndStageAsset.template.data == null)
                {
                    passiveEndStageAsset.template.data = new StagePassiveEnd();
                }
                passiveEndStageAsset.template.data.StageID = GetStageID(6001, "被动结束阶段");
            }
        }

        [System.Obsolete]
        private int GetStageID(int defaultID, string displayName)
        {
            List<int> stageIDs = new();
            foreach (var track in UnityEditor.Timeline.TimelineEditor.timelineAsset.GetOutputTracks())
            {
                foreach (var clip in track.GetClips())
                {
                    if (clip.displayName == displayName)
                    {
                        switch (defaultID)
                        {
                            case 1001:
                                {
                                    ClipStageAsset clipAsset = (ClipStageAsset)clip.asset;
                                    stageIDs.Add(clipAsset.template.data.StageID);
                                    break;
                                }
                            case 2001:
                                {
                                    TriggerStageAsset clipAsset = (TriggerStageAsset)clip.asset;
                                    stageIDs.Add(clipAsset.template.data.StageID);
                                    break;
                                }
                            case 3001:
                                {
                                    AddBuffStageAsset clipAsset = (AddBuffStageAsset)clip.asset;
                                    stageIDs.Add(clipAsset.template.data.StageID);
                                    break;
                                }
                            case 4001:
                                {
                                    BuffEndStageAsset clipAsset = (BuffEndStageAsset)clip.asset;
                                    stageIDs.Add(clipAsset.template.data.StageID);
                                    break;
                                }
                            case 5001:
                                {
                                    BulletStageAsset clipAsset = (BulletStageAsset)clip.asset;
                                    stageIDs.Add(clipAsset.template.data.StageID);
                                    break;
                                }
                            case 6001:
                                {
                                    PassiveEndStageAsset clipAsset = (PassiveEndStageAsset)clip.asset;
                                    stageIDs.Add(clipAsset.template.data.StageID);
                                    break;
                                }
                        }
                    }
                }
            }
            if (stageIDs.Count == 1)
            {
                return defaultID;
            }
            else
            {
                return stageIDs.Max() + 1;
            }
        }

        private string TranslateName(string displayName)
        {
            switch (displayName)
            {
                case "ClipStageAsset": return "一般阶段";
                case "TriggerStageAsset": return "触发阶段";
                case "AddBuffStageAsset": return "Buff添加阶段";
                case "BuffEndStageAsset": return "Buff结束阶段";
                case "BulletStageAsset": return "子弹移动阶段";
                case "PassiveEndStageAsset": return "被动结束阶段";
            }
            return displayName;
        }

        private Color TranslateColor(string displayName)
        {
            switch (displayName)
            {
                case "ClipStageAsset": return Color.blue;
                case "TriggerStageAsset": return Color.green;
                case "AddBuffStageAsset": return Color.cyan;
                case "BuffEndStageAsset": return Color.red;
                case "BulletStageAsset": return Color.yellow;
            }
            return Color.white;
        }
    }
}
#endif
