///--------------------------------------------------------------------
/// 文件名   :   BattleDebugAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/03 09:49:28
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace BattleDebug
{
    [DisplayName("效果")]
    [HideMonoScript]
    public class BattleDebugAsset : PlayableAsset, ITimelineClipAsset
    {

        [HideLabel] public BattleDebugBehaviour template = new BattleDebugBehaviour();

        [HideInInspector]
        public Color CustomCorlor;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<BattleDebugBehaviour>.Create(graph, template);
            BattleDebugBehaviour behaviour = playable.GetBehaviour();
            behaviour.owner = owner;


            return playable;
        }
    }
}
