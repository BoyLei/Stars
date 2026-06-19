///--------------------------------------------------------------------
/// �ļ���   :   ClipNotificationBehaviour.cs
/// ��  ��   :   
/// ˵  ��   :  
/// �������� :   2022/09/15 10:57:51
/// ������   :   �Զ���
/// ��Ȩ���� :   �ο�����Ƽ��������޹�˾ 
///--------------------------------------------------------------------
using UnityEngine.Playables;


    public class CustomEventClipNotificationBehaviour : PlayableBehaviour
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

