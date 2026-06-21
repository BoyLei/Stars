#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////
using StarProject.Service.Sound;
using StarProjectDef;

/// <summary>
/// RTPC 可以用ambient + triggerEnter 联动，不用别的了
/// 其他继承 AkTriggerBase 我都没改，没啥用，用到再改
/// </summary>
public class AkTriggerEnter : AkTriggerBase
{
	public UnityEngine.GameObject triggerObject = null;
    public AkEvent akevt;
    public AkState akstate;
    /* private bool isTriByMPlayer;*/
    bool isTri = false;
    public E_MusicTriggerType e_MusicTriggerType = E_MusicTriggerType.Event;
    private void Awake()
    {
        /*isTriByMPlayer = true;
        SoundManager.Instance.AkTriggerEnters.Add(this);*/
        triggerObject  = SoundManager.Instance.GlobalListener;
        if (GetComponent<AkEvent>() != null)
        {
            akevt = GetComponent<AkEvent>();
            e_MusicTriggerType = E_MusicTriggerType.Event;
        }
        else if (GetComponent<AkState>() != null)
        {
            akstate = GetComponent<AkState>();
            e_MusicTriggerType = E_MusicTriggerType.State;
        }
        
    }
    private void OnTriggerEnter(UnityEngine.Collider in_other)
    {
        //if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
        //    triggerDelegate(in_other.gameObject);
        /*if (triggerDelegate != null && isTriByMPlayer && in_other.gameObject.tag == E_TagType.MainPlayer.ToString())
        { triggerDelegate(in_other.gameObject); }*/

        /* if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
         {
             triggerDelegate(in_other.gameObject);
         }
 */
        //有代理，碰到耳朵（只有主角有）
        switch (e_MusicTriggerType)
        {
            case E_MusicTriggerType.State:
                if (triggerDelegate != null && in_other.gameObject.tag == "GlobalListener" && isTri == false)
                {
                    triggerDelegate(in_other.gameObject);
                    isTri = true;
                }
                break;
            case E_MusicTriggerType.Event:
                if (triggerDelegate != null && in_other.gameObject.tag == "GlobalListener" && isTri == false)
                {
                    triggerDelegate(in_other.gameObject);
                    isTri = true;
                }
                break;
            default:
                break;
        }
       
    }

    private void OnTriggerExit(UnityEngine.Collider other)
    {
        switch (e_MusicTriggerType)
        {
            case E_MusicTriggerType.State:

                if (triggerDelegate != null && other.gameObject.tag == "GlobalListener" && isTri == true)
                {
                    triggerDelegate(null);
                    isTri = false;
                }
                break;
            case E_MusicTriggerType.Event:
                //有代理，碰到耳朵（只有主角有）
                if (triggerDelegate != null && other.gameObject.tag == "GlobalListener" && isTri == true)
                {

                    // 将开关状态设置为 "Off"，关闭声音
                    //AkSoundEngine.SetSwitch("SoundSwitch", "Off", other.gameObject);
                    akevt.Stop(1);
                    // 停止相关的音频事件（如果需要）
                    /*if (akevt != null)
                    {
                       AkSoundEngine.StopPlayingID(akevt.playingId,1, AkCurveInterpolation.AkCurveInterpolation_Exp1) ;
                    }*/

                    triggerDelegate(null);
                    isTri = false;
                }
                break;
            default:
                break;
        }


     

 
    }
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.