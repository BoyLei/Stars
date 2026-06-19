///--------------------------------------------------------------------
/// 文件名   :   ClipNotificationAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 10:57:51
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor
{
    public class ClipNotificationAsset : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<ClipNotificationBehaviour>.Create(graph);
        }
    }
}
