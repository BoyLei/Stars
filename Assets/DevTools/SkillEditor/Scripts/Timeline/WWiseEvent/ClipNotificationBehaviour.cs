///--------------------------------------------------------------------
/// 文件名   :   ClipNotificationBehaviour.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 10:57:51
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using UnityEngine.Playables;

namespace SkillEditor
{
    public class ClipNotificationBehaviour : PlayableBehaviour
    {
        double m_PreviousTime;

        public override void OnGraphStart(Playable playable)
        {
            m_PreviousTime = 0;
        }
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if ((int)m_PreviousTime < (int)playable.GetTime())
            {
                info.output.PushNotification(playable, new CustomNotification());
            }

            m_PreviousTime = playable.GetTime();
        }
    }
}
