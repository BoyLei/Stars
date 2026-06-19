///--------------------------------------------------------------------
/// 文件名   :   BattleDebugTrack.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/03 09:59:33
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace BattleDebug
{
    [DisplayName("效果轨道")]
    [TrackColor(0.8f, 0.8f, 0.2f)]
    [TrackClipType(typeof(BattleDebugAsset))]
    public class BattleDebugTrack : TrackAsset, INotification
    {
        /// <summary>
        /// 轨道名称
        /// </summary>
        public string trackName;
        public bool isRight;

        public PropertyName id { get; }

        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.duration = 1;
            clip.displayName = "效果";
            clip.UseCustom = true;
            BattleDebugAsset effectAsset = clip.asset as BattleDebugAsset;
            if (effectAsset != null)
            {
                clip.CustomColor = effectAsset.CustomCorlor;
                //effectAsset.Index = SkillEditorGlobal.Instance.GetEffectClipIndex();
            }
        }
    }
}
