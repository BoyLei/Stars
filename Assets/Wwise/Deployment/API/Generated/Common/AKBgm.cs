using StarProject.Service.Sound;
using StarProjectDef;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//AK��������_�Զ����_Ĭ��Trigger
//һ�л����¼�������״̬�Ƕ�����֧����Ƶ��֮��Ҫ�����¼�����״̬
//һ������һ�����֡������ڳ����С���ֻҪ�滺��Ҫս����Main��
//1����Ƶ����Ҫ���¼���Ƶ�����ǵ���������
//2, Akgob�ᶯ̬���Ӳ�Ҫ������̬���ӣ��������������Լ����ԣ�
//3����ײ��ֻ�Ǵ���ʱ��Ҫ�Ĵ���������
//4, �ص㣬1һ�������¼�PlayEvent��Ȼ����ȥ�����л�״̬�������������¼�����״̬��2������Ҫ����AkState��������
public class AKBgm : MonoBehaviour
{

    public AK.Wwise.Event OnTriEvent;
    public List<AK.Wwise.Event> _AkPlayingEventsCache;
    private void Awake()
    {
        SoundManager.Instance.EnsureWwiseEventBank(OnTriEvent);
    }
    private void Start()
    {
        _AkPlayingEventsCache = SoundManager.Instance.AkPlayingEvents;
        if (!_AkPlayingEventsCache.Contains(OnTriEvent))
        {
            OnTriEvent.Post(gameObject);
            _AkPlayingEventsCache.Add(OnTriEvent);
        }
    }

    //a e s u f disAble des 

    private void OnDestroy()
    {
        SoundManager.Instance.UnLoadWwiseEventBank(OnTriEvent);

        if (_AkPlayingEventsCache.Contains(OnTriEvent))
        {
            OnTriEvent.Stop(gameObject, 1, AkCurveInterpolation.AkCurveInterpolation_Exp1);
            _AkPlayingEventsCache.Remove(OnTriEvent);
        }
    }

}