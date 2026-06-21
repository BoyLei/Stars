#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////
using StarProject.Service.Sound;
using StarProjectDef;

public class AkTriggerCollisionEnter : AkTriggerBase
{
	public UnityEngine.GameObject triggerObject = null;
    //private bool isTriByMPlayer;
    //所有环境触发音效都是基于主角的
    private void Awake()
    {
        /* isTriByMPlayer = true;
         SoundManager.Instance.akTriggerCollisionEnters.Add(this);*/
        triggerObject = SoundManager.Instance.GlobalListener;
    }
    private void OnCollisionEnter(UnityEngine.Collision in_other)
	{
        ////要么主角，要么绑定，都会触发
        //if (triggerDelegate != null && isTriByMPlayer && in_other.gameObject.tag == E_TagType.MainPlayer.ToString())
        //{ triggerDelegate(in_other.gameObject); }
        ////else if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
        ////{ triggerDelegate(in_other.gameObject); }
        if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
        { triggerDelegate(in_other.gameObject); }

    }

    private void OnTriggerEnter(UnityEngine.Collider in_other)
	{
       /* //要么主角，要么绑定，都会触发
        if (triggerDelegate != null && isTriByMPlayer && in_other.gameObject.tag == E_TagType.MainPlayer.ToString())
        { triggerDelegate(in_other.gameObject); }
        //else if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
        //{ triggerDelegate(in_other.gameObject); }*/

        if (triggerDelegate != null && (triggerObject == null || triggerObject == in_other.gameObject))
        { triggerDelegate(in_other.gameObject); }
    }
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.