using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDelay : MonoBehaviour {

    public float Delay;

    public float Duration;

    private bool isPlay;
         
	// Use this for initialization

    public void Play( )
    {
        isPlay = true;
        XLua.LuaTimer.Add(this, Mathf.CeilToInt(Delay * 1000), Mathf.CeilToInt(Duration * 1000), (sn) =>
        {
            gameObject.SetActive(isPlay);
            bool ret = isPlay;
            if (isPlay)
            {
                isPlay = false;
            }
            return ret;
        });
    }
}
