///--------------------------------------------------------------------
/// 文件名   :   WWiseEventMarker.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/15 10:57:51
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using UnityEngine;
using System.ComponentModel;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;

namespace SkillEditor
{
    [DisplayName("事件/Wwise事件")]
    [CustomStyle("LockMarker")]
    public class WWiseEventMarker : Marker, INotification, INotificationOptionProvider
    {
        [SerializeField] public bool emitOnce;
        [SerializeField] public bool emitInEditor;

        public PropertyName id { get; }

        public AK.Wwise.Event akEvent = new AK.Wwise.Event();

        public WWiseCustomData customData;

        //不过策划是可以知道声音是什么的
        [ContextMenu("测试听声音")]
        public void PlayWwiseSound()
        {
            GameObject sameNameGob;//同事件名的会互相替换,利用InstId
            if (SkillEditorGlobal.WwiseListenerTestRoot.transform.Find(akEvent.Name) != null)
            {
                sameNameGob = SkillEditorGlobal.WwiseListenerTestRoot.transform.Find(akEvent.Name).gameObject;
            }
            else
            {
                sameNameGob = new GameObject(akEvent.Name);
                sameNameGob.transform.SetParent(SkillEditorGlobal.WwiseListenerTestRoot.transform);
            }
            
            
            AkSoundEngine.PostEvent(akEvent.Name, sameNameGob
                );
            //UnityEngine.Debug.Log(akEvent.Name);
        }

        NotificationFlags INotificationOptionProvider.flags =>
           (emitOnce ? NotificationFlags.TriggerOnce : default) |
           (emitInEditor ? NotificationFlags.TriggerInEditMode : default);
    }
}