using System.Collections;
using System.Collections.Generic;
using SGF.Unity;
using StarProject.Service.Sound;
using UnityEngine;

public class VoiceInstance
{
    public string Key{get; private set;}
    private List<(int, string)> VoiceInfoDic=null;
    public List<string> PlayVoiceList = null;
    private  System.Action<string> CompleteCallBack=null;
    private System.Action FinishCallBack=null;
    private GameObject Maker=null;
    private int VoiceCnt = 0;
    public VoiceInstance(string key,string voiceinfo,GameObject maker, System.Action<string> callback)
    {
        Key = key;

        Maker = maker;
        CompleteCallBack = callback;
        if (VoiceInfoDic == null)
        {
            VoiceInfoDic = new  List<(int, string)>();
        }
        if(PlayVoiceList==null)
        {
            PlayVoiceList = new List<string>();
        }
        if (!string.IsNullOrEmpty(voiceinfo))
        {
            string[] _infos = voiceinfo.Split(';');
            if (_infos!=null && _infos.Length > 0)
            {
                for (int i = 0; i < _infos.Length; i++)
                {
                    var _info = _infos[i];
                    string[] _infoArr = _info.Split(':');
                    if (_infoArr!=null && _infoArr.Length == 2)
                    {
                        int t=0;
                        System.Int32.TryParse(_infoArr[0],out t);
                        
                        VoiceInfoDic.Add((t,_infoArr[1]));
                    }
                }
            }
        }
    }
    
    public void PlayVoice()
    {
        if (VoiceInfoDic!=null && VoiceInfoDic.Count > 0)
        {
            VoiceCnt = VoiceInfoDic.Count;
            for (int i = 0; i < VoiceInfoDic.Count; i++)
            {
                var _info = VoiceInfoDic[i];
                var groupKey = $"{Key}_{i}";
                if (_info.Item1 > 0)
                {
       
                    DelayInvoker.DelayInvoke(groupKey,_info.Item1*0.001f, (a) =>
                    { 
                        SoundManager.Instance.PlayEventName(_info.Item2, Maker, Maker,1,(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)=>{
                                VoiceCnt--;
                                PlayVoiceList.Remove(groupKey);
                                if (VoiceCnt <= 0)
                                {
                                    CompleteCallBack?.Invoke(Key);
                                    //CallbackManager.Instance.CallBack(CallbackType.OnVoiceComplete,Key);
                                }
                            }
                        );
                        
                    },_info);
                    PlayVoiceList.Add(groupKey);
                
                }
                else
                {               
                    PlayVoiceList.Add(groupKey);
                    SoundManager.Instance.PlayEventName(_info.Item2, Maker, Maker,1,(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)=>{
                            VoiceCnt--;
                            PlayVoiceList.Remove(groupKey);
                            if (VoiceCnt <= 0)
                            {
                                CompleteCallBack?.Invoke(Key);
                            }
                        }
                    );
                }
            }
        }

        if (VoiceCnt <= 0)
        {
            CompleteCallBack?.Invoke(Key);
        }
    }

    public void StopVoice()
    {
        if (PlayVoiceList != null && PlayVoiceList.Count > 0)
        {
            for (int i = 0; i < PlayVoiceList.Count; i++)
            {
                
                SoundManager.Instance.StopVoiceEvent(VoiceInfoDic[i].Item2,Maker);
                
                if (DelayInvoker.ContainInvoke(PlayVoiceList[i]))
                {
                    DelayInvoker.CancelInvoke(PlayVoiceList[i]);
                }

            }
            PlayVoiceList.Clear();
        }

        if (DelayInvoker.ContainInvoke(Key))
        {
            DelayInvoker.CancelInvoke(Key);
        }
    }
}
