using System.Collections;
using System.Collections.Generic;
using SGF.Time;
using UnityEditor;
using UnityEngine;

public class VoiceLogic 
{
    public Dictionary<string,VoiceInstance> m_VoiceInstanceDic = new Dictionary<string, VoiceInstance>();

    public (bool result,string key) PlayVoice(string voiceinfo,GameObject maker,System.Action<string> completeCallBack=null)
    {
        if (string.IsNullOrEmpty(voiceinfo))
        {
            return (false,null);
        }
        
        var _key =$"{voiceinfo}_{System.DateTime.Now.ToString("yyyy-mm-dd-hh:mm:ss:fff")}";
        
        void OnPlayComplete(string key)
        {
            StopVoice(key);
            if (completeCallBack!=null)
            {
                completeCallBack(key);
            }
        }
        
        var instance = new VoiceInstance(_key,voiceinfo,maker,OnPlayComplete);
        m_VoiceInstanceDic.Add(_key,instance);
        instance.PlayVoice();
        return (true,_key);
    }
    
    public void StopVoice(string key)
    {
        
        
        if (m_VoiceInstanceDic.ContainsKey(key))
        {
            m_VoiceInstanceDic[key].StopVoice();
            m_VoiceInstanceDic.Remove(key);
        }
    }





    
    
}
