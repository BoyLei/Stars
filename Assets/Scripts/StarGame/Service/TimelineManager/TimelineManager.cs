///--------------------------------------------------------------------
/// 文件名   :   TimelineManager.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/08 13:26:33
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using Cinemachine;
using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.OffLine;
using StarProject.Service.Cam;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProject.Service.UniversalRenderPipeline;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace StarProject.Service.Timeline
{
    public class QTEData
    {
        public float time = 0;//单位 ms
        public string iconPath;
        public float width = 0f;
        public float height = 0f;
        public float timeScale = 0f;
        public System.Action<float> cb = null;

    }
    public delegate void TimelineEventDelegate(string triiger, PlayableDirector director, TimelineEventData evtData,float time);

    public class TimelineManager : ServiceModule<TimelineManager>
    {
        private Dictionary<string, BaseTimeline> PlayingTimeline = null;
        private Dictionary<int, BaseTimeline> PlayingTimeline2 = null;

        private Dictionary<PlayableDirector,List<float>> mDialogueQueue = null;

        private Dictionary<string, System.Type> TimelineFactory = null;

        private Dictionary<TimelineEventDefine, TimelineEventDelegate> TimelineEventMap = null;
        public bool UseOld = false;

        //---- 当前是否正在播放timeline的标识
        // 这里就不计数了，timeline应该一个时刻只能播放一个
        // 目的是 播放timeline的时候，【禁止主角】自己【操作按钮】【放技能】和【移动】
        private bool m_IsPlayTimeLine = false;

        public bool IsPlayTimeLine
        {
            get { return m_IsPlayTimeLine; }
            set { m_IsPlayTimeLine = value; }
        }

        //---- 当前是否正在播放timeline

        public void Init()
        {
            PlayingTimeline = new Dictionary<string, BaseTimeline>();
            PlayingTimeline2 = new Dictionary<int, BaseTimeline>();
            TimelineFactory = new Dictionary<string, System.Type>();
            mDialogueQueue= new Dictionary<PlayableDirector,List<float>>();
            TimelineEventMap = new Dictionary<TimelineEventDefine, TimelineEventDelegate>();
            GlobalEvent.OnTimelineEvent.AddListener(OnTimelineEventHandler);
            TimelineEventMap.Add(TimelineEventDefine.Dialogue, OnDialogueHandler);
            TimelineEventMap.Add(TimelineEventDefine.PlaySound, OnPlaySoundHandler);
            TimelineEventMap.Add(TimelineEventDefine.RoleBindEffect, OnRoleBindEffectHandler);
            TimelineEventMap.Add(TimelineEventDefine.Blur, OnBlurHandler);
            TimelineEventMap.Add(TimelineEventDefine.ShakeScreen, OnShakeScreenHandler);
            TimelineEventMap.Add(TimelineEventDefine.BlackScreen, OnBlackScreenHandler);
            TimelineEventMap.Add(TimelineEventDefine.FlowScreen, OnFlowScreenHandler);
            TimelineEventMap.Add(TimelineEventDefine.SetScreenEffect, OnSetScreenEffectHandler);
            TimelineEventMap.Add(TimelineEventDefine.ModifySpeed, OnModifySpeedHandler);
            TimelineEventMap.Add(TimelineEventDefine.QuickTimeEvent, OnQTEHandler);
            TimelineEventMap.Add(TimelineEventDefine.ChangeModelShader, OnChangeModelShaderHandler);
            TimelineEventMap.Add(TimelineEventDefine.CtrMainLight, OnCtrMainLightHandler);
            TimelineEventMap.Add(TimelineEventDefine.PlayPlot, OnPlayPlotHandler);
            TimelineEventMap.Add(TimelineEventDefine.PlayWWiseBGM, OnPlayWWiseBGMHandler);

        }

        private void OnTimelineEventHandler(string trigger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            if (evtData != null)
            {
                if (TimelineEventMap.TryGetValue(evtData.EventType, out var callback) && callback != null)
                {
                    callback?.Invoke(trigger, director, evtData,time);
                }
            }
        }

        private void OnDialogueHandler(string trigger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            UnityEngine.Debug.Log("OnDialogueHandler");
            if (evtData.EventType != TimelineEventDefine.Dialogue)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }
            int id = 0;
            System.Int32.TryParse(evtData.EventArgs[0], out id);
            var cfg = LocalDataManager.Instance.GetTimelineDialogDataCell(id);

            if (cfg != null)
            {
                System.Action<float> GetNextTime = (cur) =>
                {
                    GetNextDialogueTime(director,cur);
                };
                    
                if (cfg.DialogType == 1)
                {
                    (string PlayerName, string Content, float DialogTime,float curTime,string eventName,System.Action<float> handler) arg = new(cfg.PlayerName, cfg.Content, cfg.GetDialogTime()*0.001f,time,cfg.Audio,GetNextTime);
                    UIManager.Instance.OpenWidgetAsync("avg/timelinesidebox", null, false,
                        arg, null, MainPageCommond.HideNone, false, true, false);
                }
                else if (cfg.DialogType == 2)
                {
                    var pos = GameManager.Instance.M_MainPlayerCtrlBase.entityBaseData.Pos;
                    if (!string.IsNullOrEmpty(trigger))
                    {
                        if (director != null &&  director.gameObject!=null)
                        {
                            var go = director.gameObject.transform.Find(trigger);
                            if (go != null)
                            {
                                pos = go.position;
                            }
                        }
                    }

                    (string PlayerName, string Content, float DialogTime,float curTime,string EventName,System.Action<float> handler, Vector3 pos) arg = new(cfg.PlayerName, cfg.Content, cfg.GetDialogTime()*0.001f,time,cfg.Audio,GetNextTime, pos);
                    UIManager.Instance.OpenWidgetAsync("avg/timelinepopbox", null, false,
                        arg, null, MainPageCommond.HideNone, false, false, false);
                }
            }
        }

        private void OnPlaySoundHandler(string triiger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            if (evtData.EventType != TimelineEventDefine.PlaySound)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            var soundEvent = evtData.EventArgs[0];
            UnityEngine.Debug.Log($"OnPlaySoundHandler soundEvent={soundEvent}");
            SoundManager.Instance.PlayWwiseAudio(soundEvent, false, E_SoundNTFtype.MyListener_SystemSound);
        }

        private void OnRoleBindEffectHandler(string triiger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            UnityEngine.Debug.Log("OnBindEffectHandler");
            if (evtData.EventType != TimelineEventDefine.RoleBindEffect)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }
            var key = evtData.EventArgs[0];
            var path = evtData.EventArgs[1];
            var player = GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup;
            if (player != null)
            {
                var model = player.Container.GetComponentInChildren<Animator>();
                if (model != null)
                {
                    var offilineData = model.transform.GetComponent<ModelOffLineData>();
                    if (offilineData != null)
                    {
                        var parent = offilineData.GetTransformByKey(key);
                        if (parent != null)
                        {
                            Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(path,
                                (obj) =>
                                {
                                    obj.transform.SetParent(parent);
                                    obj.transform.localPosition = Vector3.zero;
                                    obj.transform.localRotation = Quaternion.identity;
                                    obj.transform.localScale = Vector3.one;
                                    obj.SetActive(true);
                                });
                        }
                    }
                }
            }
        }

        private void OnBlurHandler(string triiger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            UnityEngine.Debug.Log("OnBlurHandler");
            if (evtData.EventType != TimelineEventDefine.Blur)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            // director.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }

        private void OnShakeScreenHandler(string triiger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            UnityEngine.Debug.Log("OnShakeScreenHandler");
            if (evtData.EventType != TimelineEventDefine.ShakeScreen)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }
            var path = evtData.EventArgs[0];
            Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(path, (go) =>
            {
                if (go != null)
                {
                    var impulse = go.GetComponent<Cinemachine.CinemachineImpulseSource>();
                    if (impulse != null)
                    {
                        impulse.GenerateImpulseWithForce(1);
                        DelayInvoker.DelayInvoke(this, impulse.m_ImpulseDefinition.m_ImpulseDuration, (args) =>
                        {
                            GameObject.Destroy(go);
                        });
                    }
                    else
                    {
                        StarDebug.Log($"Cinemachine.CinemachineImpulseSource is null  {path}");
                    }
                }
            });
        }

        /// <summary>
        /// 播放黑幕
        /// </summary>
        /// <param name="evtData"></param>
        private void OnBlackScreenHandler(string triiger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            UnityEngine.Debug.Log("OnBlackScreenHandler");
            if (evtData.EventType != TimelineEventDefine.BlackScreen)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            int id = 0;
            System.Int32.TryParse(evtData.EventArgs[0], out id);
            TaskHelper.PlayBlackMovie(id);
        }

        private void OnFlowScreenHandler(string triiger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            UnityEngine.Debug.Log("OnFlowScreenHandler");
            if (evtData.EventType != TimelineEventDefine.FlowScreen)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            string path = $"ScreenUIEffects/{evtData.EventArgs[0]}";
            bool isShow = evtData.EventArgs[1].Trim() == "1";
            UIManager.Instance.SetScreenUIEffect(path, isShow);
        }

        private void OnSetScreenEffectHandler(string triiger, PlayableDirector director, TimelineEventData evtData,float time)
        {

            if (evtData.EventType != TimelineEventDefine.SetScreenEffect)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            string path = $"ScreenUIEffects/{evtData.EventArgs[0]}";
            bool isShow = evtData.EventArgs[1].Trim() == "1";

            UnityEngine.Debug.Log($"OnSetScreenEffectHandler path={path} isShow={isShow}");
            UIManager.Instance.SetScreenUIEffect(path, isShow);
        }

        private void OnModifySpeedHandler(string triiger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            if (evtData.EventType != TimelineEventDefine.ModifySpeed)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            float speed = float.Parse(evtData.EventArgs[0]);
            director.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }

        private void OnQTEHandler(string triiger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            if (evtData.EventType != TimelineEventDefine.QuickTimeEvent)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }
            QTEData qTEData = new();
            qTEData.time = float.Parse(evtData.EventArgs[0]);
            qTEData.iconPath = evtData.EventArgs[1].Trim();
            string[] posArr = evtData.EventArgs[2].Split(',');
            qTEData.width = Convert.ToSingle(posArr[0]);
            qTEData.height = Convert.ToSingle(posArr[1]);
            qTEData.timeScale = Convert.ToSingle(evtData.EventArgs[3]);
            qTEData.cb = (speed) =>
            {
                director.playableGraph.GetRootPlayable(0).SetSpeed(speed);
            };

            UIManager.Instance.OpenWidgetAsync(UIDef.QTEWidget, null, false, qTEData,
             UIRoot.UiTopWidgetRoot.transform, MainPageCommond.HideNone, true);
        }

        private void OnChangeModelShaderHandler(string trigger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            if (evtData.EventType != TimelineEventDefine.ChangeModelShader)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            int type = int.Parse(evtData.EventArgs[0]);

            if (!string.IsNullOrEmpty(trigger))
            {
                if (director != null &&  director.gameObject!=null)
                {
                    var go = director.gameObject.transform.Find(trigger);
                    if (go != null)
                    {

                        var ctr = go.GetComponentInChildren<CharStateController>();
                        if (ctr == null)
                        {
                            ctr = go.gameObject.AddComponent<CharStateController>();
                            ctr.SetMeshRenderer();
                        }

                        if (ctr != null)
                        {
                            ctr.SetCharState((CharStateController.CharacterState)type);
                        }
                        //pos = go.position;
                    }
                }
            }
        }

        private void OnCtrMainLightHandler(string trigger, PlayableDirector director, TimelineEventData evtData,float time)
        {
            if (evtData.EventType != TimelineEventDefine.CtrMainLight)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            //1 开 0 关
            bool open = int.Parse(evtData.EventArgs[0])==1;

            var lights = UniRenderPipline.Instance.MainLights;
            foreach (var light in lights)
            {
                light.GetComponent<Light>().enabled = open;
            }

        }

        private void OnPlayPlotHandler(string trigger, PlayableDirector director, TimelineEventData evtData, float time)
        {
            if (evtData.EventType != TimelineEventDefine.PlayPlot)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            int id = int.Parse(evtData.EventArgs[0]);
            TaskHelper.PlayPlot(id,0,false);
        }

        private void OnPlayWWiseBGMHandler(string trigger, PlayableDirector director, TimelineEventData evtData,
            float time)
        {
            if (evtData.EventType!= TimelineEventDefine.PlayWWiseBGM)
            {
                return;
            }

            if (evtData.EventArgs.Count < 1)
            {
                return;
            }

            string bgm = evtData.EventArgs[0];
            
            if (string.IsNullOrEmpty(bgm))
            {
                return;
            }
            
            SoundManager.Instance.PlayWwiseAudio(bgm, false, E_SoundNTFtype.MyListener_SystemSound,director.gameObject);
        }
        
        public void OnTimerEnd()
        {

        }

        public override void Release()
        {
            GlobalEvent.OnTimelineEvent.RemoveListener(OnTimelineEventHandler);
            TimelineEventMap.Clear();
            PlayingTimeline.Clear();
            PlayingTimeline2.Clear();

            base.Release();
        }

        private void OnTimelinePlayEnd(string key, int effectID, int ID, bool NeedFadeOut)
        {
            if (PlayingTimeline.ContainsKey(key))
            {
                var director = PlayingTimeline[key].PlayableDirector;
                
                if (director!=null && mDialogueQueue.ContainsKey(director))
                {
                    mDialogueQueue[director].Clear();
                    mDialogueQueue.Remove(director);
                }
                //SGF.Debuger.LogWarning($"删除22222222222 主角移动 timeline key={key}");
                PlayingTimeline.Remove(key);
            }
            if (PlayingTimeline2.ContainsKey(effectID))
            {
                PlayingTimeline2.Remove(effectID);
            }

            if (NeedFadeOut)
            {
                //float time = ID == 1000401 ? 0.0f : 2.0f;
                float time = ID == 1000401 ? 3.0f : 2.0f;
                UIAPI.DoFade(2, false, time, () =>
                {
                    //RealPLay(key,timeline, director,  EffectID,  timelineConfigData,  isDestroy, cb);
                    UIManager.Instance.CloseWidget(UIDef.PlayTimelineBlackWidget, null, true);
                }, null);
            }
            GameManager.Instance.GetGameCameraComponent().M_CinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        }

        public void PlayTimeline(int timelineID, int EffectID, System.Action<object> cb)
        {
            TimelineConfigDataCell timelineConfigData = LocalData.LocalDataManager.Instance.GetTimelineConfigDataCell(timelineID);
            if (timelineConfigData != null)
            {
                string key = string.Format("{0}_{1}", timelineConfigData.TimelinePath, timelineConfigData.GetTimelineType());
                if (PlayingTimeline.ContainsKey(key))
                {
                    StarDebug.LogError($"timeline 正在播放不可重复调用 timelinePath{timelineConfigData.TimelinePath} timelineType{timelineConfigData.GetTimelineType()}");
                    return;
                }
                if (PlayingTimeline2.ContainsKey(EffectID))
                {
                    StarDebug.LogError($"timeline 正在播放不可重复调用 2 timelinePath{timelineConfigData.TimelinePath} timelineType{timelineConfigData.GetTimelineType()}");
                    return;
                }

                var type = GetTimelineType(timelineConfigData.TimelinePath);
                if (type == null)
                {
                    StarDebug.LogError($"timeline is null  timelinePath{timelineConfigData.TimelinePath} timelineType{timelineConfigData.GetTimelineType()}");
                    return;
                }

                PlayableDirector director = null;

                if (timelineConfigData.GetTimelineType() == 0)
                {
                    //场景读取
                    GameObject timelineRoot = GameObject.Find(timelineConfigData.TimelinePath);
                    if (timelineRoot != null)
                    {
                        //Debug.LogError("PlayTimeLine:" + timelineConfigData.TimelinePath);
                        timelineRoot.SetActive(true);
                        //  Debug.LogError("111111");
                        director = timelineRoot.GetComponentInChildren<PlayableDirector>();

                        //Debug.LogError("222222" + key + " " + type + " " + director + " " + EffectID + " " + timelineConfigData);
                        CreateTimelineRunniTime(key, type, director, EffectID, timelineConfigData, cb);
                    }
                    else
                    {
                        StarDebug.LogError($"timeline is null  timelinePath{timelineConfigData.TimelinePath} timelineType{timelineConfigData.GetTimelineType()}");
                    }
                }
                else if (timelineConfigData.GetTimelineType() == 1)
                {
                    if (!UseOld)
                    {
                        //资源加载
                        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(
                            timelineConfigData.TimelinePath,
                            (GameObject go) =>
                            {
                                if (go == null)
                                {
                                    return;
                                }

                                var gob = GameObject.Instantiate<GameObject>(go);
                                if (gob != null)
                                {
                                    director = gob.GetComponentInChildren<PlayableDirector>();
                                    director.gameObject.SetActive(false);
                                    CreateTimelineRunniTime(key, type, director, EffectID, timelineConfigData, cb);
                                }
                            });
                    }
                    else
                    {
                        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(
                            timelineConfigData.TimelinePath, (go) =>
                            {
                                if (go == null)
                                {
                                    return;
                                }

                                var gob = GameObject.Instantiate<GameObject>(go);
                                if (gob != null)
                                {
                                    director = gob.GetComponentInChildren<PlayableDirector>();
                                    director.gameObject.SetActive(false);
                                    CreateTimelineRunniTime(key, type, director, EffectID, timelineConfigData, cb);
                                }
                            });
                    }
                }
            }
        }

        private void CreateTimelineRunniTime(string key, System.Type type, PlayableDirector director, int EffectID, TimelineConfigDataCell timelineConfigData, System.Action<object> cb)
        {
            //director.gameObject.AddComponent<DontDestroy>();
            BaseTimeline timeline = System.Activator.CreateInstance(type) as BaseTimeline;
            //  Debug.LogError("3333");
            if (timeline == null)
            {
                StarDebug.LogError($"timeline is null  timelinePath{timelineConfigData.TimelinePath} timelineType{timelineConfigData.GetTimelineType()}");
                return;
            }

            if (director == null)
            {
                SGF.Debuger.LogError($"CreateTimelineRunniTime() director=null,EffectID={EffectID}");
                return;
            }
            //之前接黑幕融合 
            if (timelineConfigData.IsSkip || timelineConfigData.IsInBalack)
            {
                int fadeType = timelineConfigData.IsInBalack ? 1 : 0;
                UIAPI.DoFade(fadeType, timelineConfigData.IsSkip, 1.5f, () =>
                {
                    RealPLay(key, timeline, director, EffectID, timelineConfigData, cb);
                },
                () =>
                {
                    timeline.Stop(true);
                });
            }
            else
            {
                RealPLay(key, timeline, director, EffectID, timelineConfigData, cb);
            }
        }

        private void GetNextDialogueTime(PlayableDirector director,float time)
        {
            if (director != null)
            {
                if (mDialogueQueue.ContainsKey(director))
                {
                    var list=mDialogueQueue[director];
                    int index=list.IndexOf(time);

                    int nextIndex=index+1;
                    if (nextIndex > -1 && nextIndex < list.Count)
                    {
                       float next=  list[nextIndex]-0.001f;
                       director.time=next;
                    }
                }
            }

        }

        private void InitCustomEventMarker(IEnumerable<TrackAsset> trackAssets,ref List<float> Events)
        {

            if (trackAssets != null && trackAssets.Count()> 0)
            {
                foreach (var item in trackAssets)
                {
                    foreach (var clip in item.GetMarkers())
                    {
                        if (clip is CustomEventMarker marker)
                        {
                            foreach (var e in marker.Events)
                            {
                                if (e.EventType == TimelineEventDefine.Dialogue)
                                {
                                    Events.Add((float)System.Math.Round(clip.time, 3));
                                }
                            }
                        }

                    }
                    
                    var childs = item.GetChildTracks();
                    InitCustomEventMarker(childs, ref Events);
                }
            }

        }
        
        public void InitDialogueQueue(PlayableDirector director,string key)
        {
            var TimelineAsset = director.playableAsset as TimelineAsset;
            var tracks = TimelineAsset.GetRootTracks();
            List<float> makers = new List<float>();
            InitCustomEventMarker(tracks, ref makers);
            makers.Sort();
            if (mDialogueQueue.ContainsKey(director))
            {
                mDialogueQueue[director] = makers;
            }
            else
            {
                mDialogueQueue.Add(director, makers);
            }
        }
        
        private void RealPLay(string key, BaseTimeline timeline, PlayableDirector director, int EffectID, TimelineConfigDataCell timelineConfigData, System.Action<object> cb)
        {
            if (director == null)
            {
                SGF.Debuger.LogError($"CreateTimelineRunniTime() director=null,EffectID={EffectID}");
                return;
            }
            
            timeline.OnInit(timelineConfigData.GetID(), timelineConfigData.TimelinePath, timelineConfigData.GetTimelineType(), EffectID, OnTimelinePlayEnd, cb);
            InitDialogueQueue(director,key);
            //  Debug.LogError("555");
            SGF.Debuger.LogWarning($"禁止 主角移动 timeline ={key}, timelinePath={timelineConfigData.TimelinePath}");
            timeline.Play(director, timelineConfigData);

            UIManager.Instance.CloseWindow(UIDef.MedicineWindow);
            //  Debug.LogError("666");
            if (CameraManager.Instance.CurrentPlayCamera != null)
            {
                GameManager.Instance.GetGameCameraComponent().M_CinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            }
            PlayingTimeline.Add(key, timeline);
            PlayingTimeline2.Add(EffectID, timeline);
        }

        private System.Type GetTimelineType(string timelinePath)
        {
            if (TimelineFactory.ContainsKey(timelinePath))
            {
                return TimelineFactory[timelinePath];
            }

            return typeof(CommonTimeline);
        }
    }
}