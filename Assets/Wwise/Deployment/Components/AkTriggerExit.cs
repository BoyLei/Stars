#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////
using StarProject.Service.Sound;
using StarProjectDef;

public class AkTriggerExit : AkTriggerBase
{
	public UnityEngine.GameObject triggerObject = null;
    //private bool isTriByMPlayer;
    private void Awake()
    {
        //isTriByMPlayer = true;
        /* SoundManager.Instance.AkTriggerExits.Add(this);*/
        triggerObject = SoundManager.Instance.GlobalListener;
    }
    private void OnTriggerExit(UnityEngine.Collider in_other)
	{
        /*       if (triggerDelegate != null && isTriByMPlayer && in_other.gameObject.tag == E_TagType.MainPlayer.ToString())
               { triggerDelegate(in_other.gameObject); }*/

        if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
        { 
            triggerDelegate(in_other.gameObject);
        }
    }
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.