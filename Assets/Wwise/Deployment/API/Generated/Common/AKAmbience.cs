using StarProject.Service.Sound;
using StarProjectDef;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//AK����_�Զ����_Ĭ��Trigger
public class AKAmbience : MonoBehaviour
{
    public AK.Wwise.Event OnTriEvent;
    private bool isTriByMPlayer;
    public List<AK.Wwise.Event> _AkPlayingEventsCache;
    private void Awake()
    {
        isTriByMPlayer = true;
        SoundManager.Instance.EnsureWwiseEventBank(OnTriEvent);
    }
    private void Start()
    {
        _AkPlayingEventsCache = SoundManager.Instance.AkPlayingEvents;
    }

    private void OnDestroy()
    {
        SoundManager.Instance.UnLoadWwiseEventBank(OnTriEvent);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }
        if (isTriByMPlayer && other.gameObject.tag == E_TagType.MainPlayer.ToString())
        {
            if (!_AkPlayingEventsCache.Contains(OnTriEvent))
            {
                OnTriEvent.Post(gameObject);
                _AkPlayingEventsCache.Add(OnTriEvent);
            }

        }
        else
        {

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == null)
        {
            return;
        }
        if (isTriByMPlayer && other.gameObject.tag == E_TagType.MainPlayer.ToString())
        {
            if (_AkPlayingEventsCache.Contains(OnTriEvent))
            {
                OnTriEvent.Stop(gameObject, 1, AkCurveInterpolation.AkCurveInterpolation_Exp1);
                _AkPlayingEventsCache.Remove(OnTriEvent);
            }
        }
        else
        {

        }
    }
}
