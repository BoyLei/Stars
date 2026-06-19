using CollectionUtils;
using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Service.Cam;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using XLua;

namespace StarProject.Service.Sound
{
    public class AudioSetting
    {
        public AudioPriority audioPriority;
        public AudioClip audioClip;
        public string method;//根据触发字段来配置
        public string audioName;//策划配置是万变的暂时不用
        public AudioUseType audioUseType = AudioUseType.Static;
    }
    public enum AudioPriority
    {
        UISystem1 = 1,
        RoleAudio2,
        EnvTri3,
        Bgm4,
        Max


    }

    public enum SoundType
    {
        Bus_volume, // Bus_volume 总音量																		
        Music_volume,   // Music_volume 背景音效-》音乐																		
        Amb_volume, // Amb_volume 环境音效-》3D 的																		
        UI_volume   // UI_volume 系统音效-》UI 2D的																		
    }

    public enum AudioMethodEvent
    {
        MAIN_MUSIC_BGM,
        BUTTON_CLICK = 1,//代码控制需要加入ExtendButton,位置不能变化
        LOADING_BGM,
        THREE_MATCH_BGM,
        SNAKE_BGM,
        PARKOUR_BGM,
        FOOT_STEP,//脚步声音
        ADD_NEW_FOLLOWER,//新增伙伴跟随
        LOGIN_BGM,
        MAIN_CITY_BGM,
        //-----攻击------------------
        ATTACK_1,
        ATTACK_2,
        ATTACK_3,
        ATTACK_4,
        ATTACK_5,
        ATTACK_6,
        ATTACK_7,
        ATTACK_8,
        ATTACK_9,
        ATTACK_10,
        //-----资源道具获取-----


        ADD_DIAMOND_COLLIDER = 1001,//钻石获取
        ADD_ACTIONCARD_COLLIDER = 1002,//超人卡片
        ADD_CHEST_COLLIDER = 1003,//宝箱
        ADD_ITEMCOMMON_COLLIDER = 1004,//通用物品
        ADD_GRASS_COLLIDER = 1005,//GRASS物品
        ADD_WOOD_COLLIDER = 1006,//树木物品
        ADD_STONE_COLLIDER = 1007,//石头物品
        ADD_METAL_COLLIDER = 1008,//金属
        ADD_WORM_COLLIDER = 1009,//虫子
                                 //-----资源道具获取-----
    }
    public enum AudioUseType
    {
        Static,//DoNotDestroy_Life
        Scene,//场景级需卸载
    }

    public class SettingSave
    {
        public bool isMute = false;    // 是否静音
        public float volume = 10;  // 音量
    }

    [LuaCallCSharp]
    public class SoundManager : ServiceModule<SoundManager>
    {
        #region WWISE_TYPE
        private GameObject MainPlayer;
        [Header("Wwise")]
        public AK.Wwise.RTPC OnMenu = new();
        public AK.Wwise.Event MusicEvent = new();
        public AK.Wwise.State MusicStart_Region = new();
        public AK.Wwise.Trigger EnemyMusicTrigger = new();
        public AK.Wwise.Event EnemyMusicEvent = new();


        public List<AK.Wwise.Event> AkPlayingEvents = new();
        /*        public List<AkTriggerEnter> AkTriggerEnters = new List<AkTriggerEnter>();
                public List<AkTriggerExit> AkTriggerExits = new List<AkTriggerExit>();*/
        /*   public List<ZoneTrigger> CurrentZones = new List<ZoneTrigger>();*/

        public void RegMainPlayer(GameObject gob)
        {
            MainPlayer = gob;
        }
        public void OnPlayerDie(bool isDie)
        {
            if (isDie)
            {
                //wwise是设计不同事件，对应不同的人。
                //现在要所有事件移除主角
                //TODO，但是要排除其他不需要排除的（碰撞音效）


                for (int i = 0; i < AkPlayingEvents.Count; i++)
                {
                    AkPlayingEvents[i].Stop(MainPlayer);
                }
                AkPlayingEvents.Clear();
                //播放再接再厉音效
            }


        }
        /* public void LeaveZone(ZoneTrigger Z)
         {
             CurrentZones.Remove(Z);
             UpdateMusic();
         }

         public void EnterZone(ZoneTrigger Z)
         {
             CurrentZones.Insert(0, Z);
             UpdateMusic();
         }
         void UpdateMusic()
         {
             if (CurrentZones.Count > 0)
             {
                 CurrentZones[0].MusicState.SetValue();//目前策略是播放最新，也可以融合
             }
             else
             {
                 MusicStart_Region.SetValue();
             }
         }*/
        #endregion
        ////怪物仇恨server战斗状态，客户端放过技能6秒，现在呗混合状态通知Cs有一个为战斗则为战斗；
        /* public AK.Wwise.RTPC TimeOfDayRTPC;*/

        public ulong FocusPlayerId;
        //场景初始化,
        public List<AudioSetting> AcList = new();
        //磁带播放器初始化
        public Dictionary<AudioPriority, AudioSource> AudioSource = new();
        private Dictionary<string, SettingSave> musicSettingSaveDic = new();    // 音效设置缓存字典
        public Dictionary<string, SettingSave> MusicSettingSaveDic
        {
            get { return musicSettingSaveDic; }
        }

        //方案1【不行】，引用一个mono单粒放在外面，然后可以随时调用到，然后他选择放在节点下方，调用有问题脱离soundmgr了，节点独立要自己注册有问题
        private GameObject globalListener;
        //可以获取让你引用，但是你不能修改。但是别人很有可能有给你瞎设置位置的问题你就人肉踹他就完事
        //用途：1，挪动位置有声音远近还原位置，2，交给特殊触发器直接触发（用处不大），3，全局事件获取
        //Gmodule - SoundMgr
        public GameObject GlobalListener
        {
            get
            {
                if (globalListener == null)
                {
                    //DoNotDestory,和放在destory下效果都一样；先有Module;出生在相机下面
                    if (GlobalModules.Instance.GlobalListener != null)
                    {
                        globalListener = GlobalModules.Instance.GlobalListener.gameObject;
                        globalListener.gameObject.layer = LayerMask.NameToLayer(E_LayerType.Entity.ToString());
                    }
                }
                return globalListener;
            }
        }

        private Transform oriGlobalListenerRoot;//原始根节点，开始会识别；放在原始，放在新地方，还原原始
        public void SetGListener(Transform pRoot)
        {
            //确保
            GlobalListener.transform.SetParent(pRoot);
            GlobalListener.transform.localPosition = Vector3.zero;
        }
        public void RevertListener()
        {
            //确保
            if (oriGlobalListenerRoot == null)
            {
                oriGlobalListenerRoot = GlobalListener.transform.parent;
            }

            GlobalListener.transform.SetParent(oriGlobalListenerRoot);
            GlobalListener.transform.localPosition = Vector3.zero;
        }
        //还有一个脚本可以用来控制这个，再相机，主角，中间的节点的：设置节点，设置距离，动画控制
        public void SetListenerLerp(Vector3 vector3)
        {
            //DG;
        }

        //------不必放在MusicManager中
        //public AK.Wwise.Event BgmState = new AK.Wwise.Event();  // 游戏背景音乐[状态]
        //---

        //动态声音大小声音大小 。动态变更TODO？，读表名字和id对应
        public void SetVolDynamic()
        {
            int s_Layer = 0;
            for (int i = 1; i < (int)AudioPriority.Max; i++)
            {
                if (AudioSource[(AudioPriority)i].isPlaying)
                {
                    AudioSource[(AudioPriority)i].volume = Mathf.Pow(0.5f, s_Layer);
                    s_Layer++;
                }
            }
        }

        public void WWiseBgmStar()
        {
            //BgmState.Post(gameObject)
        }


        public void Init()
        {
            CheckSingleton();


            if (oriGlobalListenerRoot == null)
            {
                oriGlobalListenerRoot = GlobalListener.transform.parent;
            }


            if (AudioSource == null)
            {
                AudioSource = new Dictionary<AudioPriority, AudioSource>();
            }
            //DontDestroyOnLoad(gameObject);
            //Clear
            AudioSource.Clear();
            if (AudioSource.Count == 0)
            {
                AudioSource.Add(AudioPriority.UISystem1, EnvironmentRoot.Instance.SoundRoot_UISystem1);
                AudioSource.Add(AudioPriority.RoleAudio2, EnvironmentRoot.Instance.SoundRoot_RoleAudio2);
                AudioSource.Add(AudioPriority.EnvTri3, EnvironmentRoot.Instance.SoundRoot_EnvTri3);//现在足迹，采集，就在脚下TODO远处的鸟语花香暂时不考虑
                AudioSource.Add(AudioPriority.Bgm4, EnvironmentRoot.Instance.SoundRoot_Bgm4);

            }
            InitSoundCache();
            //InitOrRefPlayList();
            InitSoundBankInfos();
            AkSoundEngine.SetGameObjectOutputBusVolume(globalListener, globalListener, 1);
        }

        private void InitSoundCache()
        {
            //读取音效设置的缓存
            {
                bool isExists = SaveManager.Instance.KeyExists(GameConfig.SETTING_KEY, "Setting");
                if (isExists)
                {
                    musicSettingSaveDic = SaveManager.Instance.Load<Dictionary<string, SettingSave>>(GameConfig.SETTING_KEY, "Setting");
                }
                musicSettingSaveDic ??= new();
            }
            // 总音效
            {
                if (musicSettingSaveDic.TryGetValue(GameConfig.SETTING_ALL_MUSIC, out var SaveAllMusic))
                {
                    SetMusicVolume(SoundType.Bus_volume, SaveAllMusic.volume);
                    SetMusicState(SoundType.Bus_volume, SaveAllMusic.isMute);
                }
                else
                {
                    SetMusicVolume(SoundType.Bus_volume, 10);
                    SetMusicState(SoundType.Bus_volume, false);
                }
            }
            // 背景音效
            {
                if (musicSettingSaveDic.TryGetValue(GameConfig.SETTING_BACKGROUND_MUSIC, out var SaveBackgroundMusic))
                {
                    SetMusicVolume(SoundType.Music_volume, SaveBackgroundMusic.volume);
                    SetMusicState(SoundType.Music_volume, SaveBackgroundMusic.isMute);
                }
                else
                {
                    SetMusicVolume(SoundType.Music_volume, 10);
                    SetMusicState(SoundType.Music_volume, false);
                }
            }
            // 系统音效
            {
                if (musicSettingSaveDic.TryGetValue(GameConfig.SETTING_SYSTEAM_MUSIC, out var SaveSysteamMusic))
                {
                    SetMusicVolume(SoundType.UI_volume, SaveSysteamMusic.volume);
                    SetMusicState(SoundType.UI_volume, SaveSysteamMusic.isMute);
                }
                else
                {
                    SetMusicVolume(SoundType.UI_volume, 10);
                    SetMusicState(SoundType.UI_volume, false);
                }
            }
            // 环境音效
            {
                if (musicSettingSaveDic.TryGetValue(GameConfig.SETTING_SURROUNDINGS_MUSIC, out var SaveSurroundingsMusic))
                {
                    SetMusicVolume(SoundType.Amb_volume, SaveSurroundingsMusic.volume);
                    SetMusicState(SoundType.Amb_volume, SaveSurroundingsMusic.isMute);
                }
                else
                {
                    SetMusicVolume(SoundType.Amb_volume, 10);
                    SetMusicState(SoundType.Amb_volume, false);
                }
            }
        }

        private readonly string SoundBankInfosCfgPath = "Config/SoundBankInfos/SoundbanksInfo";
        private void DeserializeSoundBankInfos() 
        {
            string path;
#if UNITY_IPHONE || UNITY_IOS
			path = $"{SoundBankInfosCfgPath}_Ios";
#elif UNITY_ANDROID
            path = $"{SoundBankInfosCfgPath}_Android";
#else
            path = $"{SoundBankInfosCfgPath}_Windows";
#endif

            var textasset = Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
            if (textasset != null)
            {
                var data = MessagePack.MessagePackSerializer.Deserialize<SoundBankData>(textasset.bytes);
                m_BankInfoDict = data.BankInfoDict;
                m_ID_BankInfoDict = data.ID_BankInfoDict;
                m_settingBanksSet = data.SettingBanksSet;
            }
    }

        private void InitSoundBankInfos()
        {
            // 废弃Read XML方法，改用读取二进制方案 @caojie
            DeserializeSoundBankInfos();

            /*
            TextAsset textAsset = ResourceHelperMono.LoadWwiseSoundBankInfo("SoundbanksInfo");
            if (textAsset != null && textAsset.text != null)
            {
                XmlDocument document = new();
                document.LoadXml(textAsset.text);

                XmlNode soundbankInfos = document.SelectSingleNode("SoundBanksInfo");
                XmlNode soundbanks = soundbankInfos.SelectSingleNode("SoundBanks");
                if (soundbanks != null && soundbanks.ChildNodes.Count > 0)
                {
                    foreach (var soundbank in soundbanks.ChildNodes)//SoundBank
                    {

                        XmlElement _XsoundBank = soundbank as XmlElement;
                        if (_XsoundBank == null || _XsoundBank.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        //string name = node.GetAttribute("SoundBank");
                        //string desc = node.GetAttribute("desc");
                        //string super = node.GetAttribute("super");
                        //bool ignore = node.GetAttribute("ignore").Equals("true");

                        XmlNode IncludedEvents = _XsoundBank.SelectSingleNode("IncludedEvents");
                        if (IncludedEvents == null)
                        {
                            continue;
                        }
                        string bankName = _XsoundBank.SelectSingleNode("ShortName").InnerText;//节点.包含值
                        foreach (var @event in IncludedEvents.ChildNodes)//SoundBank配置
                        {
                            XmlElement _event = @event as XmlElement;
                            if (_event == null || _event.NodeType == XmlNodeType.Comment)
                            {
                                continue;
                            }
                            string eventName = _event.GetAttribute("Name");//节点 属性

                            if (string.IsNullOrEmpty(eventName))
                            {
                                continue;
                            }

                            AddEvent2Banks(m_BankInfoDict, eventName, bankName);

                            uint eventId = System.Convert.ToUInt32(_event.GetAttribute("Id"));

                            AddEvent2Banks(m_ID_BankInfoDict, eventId, bankName);

                        }
                        m_settingBanksSet.Add(bankName);
                    }

                }
            }

            //GeneraConfig(ConfigMap);
            //GeneraSerializeConfig(SerializeMap);
            */
        }

        //加载不同场景得时候需要调用本方法！！TODO：
        private void InitOrRefPlayList()
        {
            //如未加载就全加载一次common数据，如加载就卸载
            if (AcList.Count != 0)
            {
                for (int i = AcList.Count - 1; i >= 0; i--)
                {
                    if (AcList[i].audioUseType == AudioUseType.Scene)
                    {
                        AcList.RemoveAt(i);
                    }
                }
            }

            AudioData gameAudioData = LocalDataManager.Instance.M_AudioData;
            if (gameAudioData == null)
            {
                return;
            }
            Dictionary<int, AudioDataCell> gadc = gameAudioData.StaticAudioDatas;
            //先不区分这些情况，ui都默认加载
            //后续需在此处添加，分章节，关卡 ，区块，地区（养成/探索），小游戏
            //GameStageManager sgm = GameStageManager.Instance;
            //int stageId = sgm.GetCurrentFBStage();
            //int levelId = sgm.GetCurrentFBLevel();//当前关卡
            if (gadc != null && gadc.Count != 0)
            {
                foreach (var item in gadc)
                {
                    AudioSetting audioSet = new();
                    int pri;
                    int.TryParse(item.Value.Priority, out pri);
                    audioSet.audioPriority = (AudioPriority)pri;
                    //if (item.Value.GetStageId() == 0 && item.Value.GetLevelId() == 0)//通用-不卸载-全加载
                    {
                        audioSet.audioClip = ResourceHelperMono.GetAudioCommonClip(item.Value.ResName);
                        audioSet.audioUseType = AudioUseType.Static;
                    }
                    //else if (gadc[i].GetStageId() == stageId && gadc[i].GetLevelId() == levelId)//配置路径-根据目前关卡加载-配置关卡
                    //{
                    //    audioSet.audioClip = ResourceManager.Instance.GetAudioClip(stageId, levelId, gadc[i].ResName);
                    //    audioSet.audioUseType = AudioUseType.Scene;
                    //}
                    audioSet.method = item.Value.TriMethod;
                    audioSet.audioName = item.Value.ResName;
                    AcList.Add(audioSet);
                }
            }

            //PlayEvent(AudioMethodEvent.LOGIN_BGM);
        }

        //根据name都加载好了，事件中心


        #region Wwise 参数对应设置

        public void SetApplicationPauseMusic(bool pause)
        {
            if (pause)
            {
                SetMusicState(SoundType.Bus_volume, true);
            }
            else
            {
                if (musicSettingSaveDic != null && musicSettingSaveDic.TryGetValue(GameConfig.SETTING_ALL_MUSIC, out var SaveAllMusic))
                {
                    SetMusicState(SoundType.Bus_volume, SaveAllMusic.isMute);
                }
                else
                {
                    SetMusicState(SoundType.Bus_volume, false);
                }
            }
        }

        // 设置音频通道的音量
        public void SetMusicVolume(SoundType soundType, float val)
        {
            string group = soundType.ToString();
            AkSoundEngine.SetRTPCValue(group, val * 10);
        }

        // 设置音频通道的静音状态
        public void SetMusicState(SoundType soundType, bool isMute)
        {
            string group = soundType.ToString();
            string state = isMute ? "OFF" : "ON";
            AkSoundEngine.GetState(group, out var cStateId);
            if (AkSoundEngine.GetIDFromString(state) == cStateId)
            {
                return;
            }

            AkSoundEngine.SetState(group, state);
        }

        #endregion




        #region WWISE_TYPE 播放
        private bool m_PlaySound = true;
        private float m_SoundVolume = 1f;
        /// <summary>
        /// 对于 大部分的 音频来说, 直接根据 event 的名字 映射 bank即可 (此处依旧保留 eventName--->bank 的映射,是为了兼容之前 根据soundName 播放音频的接口).
        /// 其实更加推荐 使用 eventId 播放音频的接口
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<string>> m_BankInfoDict = new();

        /// <summary>
        /// event的 id----> bankList 的 信息.
        /// 与 李瑞峰 的约定是 :  
        ///     一个 event 不会存在于 多个bank 里面.
        /// 
        /// 在解析 SoundbanksInfo.xml的时候, 程序上 支持一个event对应多个 bank的情况.
        /// 同时,  由于 不同的音频 可以 有相同的 event名字. 所以 此处 采用 event.Id 作为 映射的key
        /// </summary>
        /// <typeparam name="uint">eventID</typeparam>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        private Dictionary<uint, List<string>> m_ID_BankInfoDict = new();

        /// <summary>
        /// 配置 表中的 bank 信息
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        private HashSet<string> m_settingBanksSet = new();


        /// <summary>
        /// 已经加载的 banks 记录
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        private HashSet<string> m_LoadedBanksSet = new();


        private AkCallbackManager.EventCallback m_SoundFinishCallback;
        private uint m_CallbackType = (uint)AkCallbackType.AK_EndOfEvent;

        private GameObject m_battleCameraGod = null;

        public GameObject BattleCameraGod
        {
            get
            {
                if (m_battleCameraGod == null)
                {
                    m_battleCameraGod = CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).gameObject;
                }
                return m_battleCameraGod;
            }
        }

        public void PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                return;
            }
            if (!m_PlaySound)
            {
                return;
            }
            string eventName = "Play_" + soundName;
            PlayEventName(eventName, null, null, volume: m_SoundVolume, null);
        }



        /// <summary>
        /// 通过 eventName 播放event. 兼容的是旧的接口(新接口采用 eventID 播放name)
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventOwner">事件拥有者并不一定是自己发声</param>
        /// <param name="volume"></param>
        /// <param name="finishCallback"></param>
        public void PlayEventName(string eventName, GameObject eventOwner = null, GameObject soundMaker = null, float volume = 1, AkCallbackManager.EventCallback playSoundEndCb = null, uint eventid = 0)
        {
            /*      if (globalListener == null)
                  {
                      return;
                  }*/

            //EventName我使用默认的“操作_资源名”,如：Play_Close。用户输入直接输入Close即可。
            //发生位置如果没填写那一定就相当于对着玩家耳朵发声
            if (soundMaker == null)
            {
                //加载一个预制体，预制体是空的
                soundMaker = SoundManager.Instance.GlobalListener;//CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).gameObject;//AddSoundGameObject(eventName);
            }
            //播放
            //MMo通常就绑定在身上
            PostWwiseEvent(eventid, eventName, /*谁身上的事件*/eventOwner,/*谁就是发声者*/ soundMaker, playSoundEndCb, volume);
        }


        private string LastBGMName = string.Empty;

        public void PlayEventBGMName(string eventName, GameObject eventOwner, GameObject soundMaker, float volume = 1, AkCallbackManager.EventCallback playSoundEndCb = null, uint eventid = 0)
        {
            if (eventName == LastBGMName && LastBGMName != string.Empty)
            {
                return;
            }
            LastBGMName = eventName;
            //EventName我使用默认的“操作_资源名”,如：Play_Close。用户输入直接输入Close即可。
            //发生位置如果没填写那一定就相当于对着玩家耳朵发声
            if (soundMaker == null)
            {
                //加载一个预制体，预制体是空的
                soundMaker = SoundManager.Instance.GlobalListener;//CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).gameObject;//AddSoundGameObject(eventName);
            }
            //播放
            //MMo通常就绑定在身上
            PostWwiseEvent(eventid, eventName, /*谁身上的事件*/eventOwner,/*谁就是发声者*/ soundMaker, playSoundEndCb, volume);
        }


        /// <summary>
        /// 我，只接受我，或者别人
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="finishCallback"></param>
        /// <param name="e_SoundNTFtype"></param>
        /// <param name="soundMaker">发声者</param>
        public void PlayWwiseAudio(string eventName, bool finishCallback, E_SoundNTFtype e_SoundNTFtype, GameObject soundMaker = null)
        {
            //eventid 和 EventName 和 Event挂载的Gob 三个确定一个就可以
            switch (e_SoundNTFtype)
            {
                case E_SoundNTFtype.MySelf:
                    //这里不知道你是谁，我代码也没写你（实体）身上
                    break;
                case E_SoundNTFtype.Target:
                    PlayEventName(eventName, null, soundMaker, 1, null);
                    break;
                case E_SoundNTFtype.Global:
                    break;
                case E_SoundNTFtype.MyListener_SystemSound://这里是可以的
                    PlayEventName(eventName, null, SoundManager.Instance.GlobalListener, 1, null);//距离会根据相机进行一次衰减
                    break;
                default:
                    break;
            }

        }

        //==================================停止=============================================

        //第一种方法
        //  AkSoundEngine.StopAll(targetGameObject);
        //   //第二种
        //   AkSoundEngine.PostEvent("Stop_" + soundName,targetGameObject);

        ////==================================清理=============================================
        //     AkSoundEngine.ClearBanks();
        //==================================热更=============================================
        //                  #if !UNITY_EDITOR
        //            string newPath = Application.persistentDataPath + "/core/" + settings.UserSettings.m_BasePath;
        //#if UNITY_ANDROID
        //            newPath += "Android";
        //#elif UNITY_IOS
        //            newPath += "iOSResource";
        //#else
        //            newPath += "Windows";
        //#endif
        //            SGF.Debuger.Log(string.Format("添加搜索路径：{0}",newPath));
        //            AkSoundEngine.AddBasePath(newPath);
        //#endif

        #endregion
        /// <summary>
        /// 正在播放 音频 id 的记录, 在 提前终止 音频的时候 有用
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="uint"></typeparam>
        /// <returns></returns>
        private Dictionary<string, uint> playingAudioID = new();

        /// 2023/3/22
        /// 目前 技能 音效的播放 并没有 走 统一 SoundManager管理逻辑, 上面的接口目前看 并没有在工程中使用
        /// 此处 新加一个 通用 音效 播放 接口, 先不做 音效的 内存管理, 只做一个简单的播放 接口
        #region 

        /// <summary>
        /// 使用 audioParam 播放 音频
        /// </summary>
        /// <param name="audioParam"></param>
        /// <param name="gameObject"></param>
        public void PlayAudioParam(I_AudioParam audioParam, GameObject gameObject, bool finishCallback = false)
        {
            uint eventID = audioParam.SoundEventID;
            string eventName = audioParam.SoundName;

            //1，只有特殊情况采用主角或则发声实体的节点身上，比如隐身效果的声音根据主角距离相关的才这样
            //ui声音gob给的是ui，给相机吧，ui相机1000米高，现在要给battleCamera这个是listener

            // 目前发声的节点都挂在battlecamera，后续有需求了在判断：场景中会有很多发声实体，很多摄像机：之后全局的要特殊写，别人相机不必关心，[自己相机目前放弃了]，
            //GameObject postGameObject = BattleCameraGod;

            // TODO: DL
            // 从指定时间播放 音频
            {
                // 使用 AkSoundEngine.Seek 方法将播放头移动到一半位置
                // AkSoundEngine.Seek(sourceID, Mathf.FloorToInt(halfDuration * 1000), AkSeekType.AkSeekType_FromStart);
            }

            PostWwiseEvent(eventID, eventName, gameObject, gameObject, null, 1);
        }

        public void StopAudio(I_AudioParam audioParam, GameObject gameObject)
        {
            uint eventID = audioParam.SoundEventID;
            string eventName = audioParam.SoundName;

            string key = ForamtPlayingAudioKey(eventName, gameObject);
            if (!playingAudioID.ContainsKey(key))
            {
                return;
            }

            uint pID = playingAudioID[key];
            AkSoundEngine.StopPlayingID(pID, 100, AkCurveInterpolation.AkCurveInterpolation_Constant);

            DeleteAudioRecord(eventName, gameObject, pID);
        }






        private void OnPlaySoundEventCb(AkCallbackType in_type, uint pID, string path, GameObject gameObject)
        {
            //   SGF.Debuger.LogError($"[wwis] , endCb ===> in_type: {in_type} --> pID: {pID}, path: {path}");
            switch (in_type)
            {
                case AkCallbackType.AK_EndOfEvent:
                    {
                        DeleteAudioRecord(path, gameObject, pID);
                    }
                    break;
                case AkCallbackType.AK_EndOfDynamicSequenceItem:
                    {

                    }
                    break;
                case AkCallbackType.AK_MusicSyncExit:
                    {

                    }
                    break;
                case AkCallbackType.AK_AudioInterruption:
                    {

                    }
                    break;
                case AkCallbackType.AK_AudioSourceChange:
                    {

                    }
                    break;
                default: break;
            }
        }

        /// <summary>
        /// 验证 postEvent 是否成功的 接口, wwise post 返回的是 uint32的整数,如果 返回的是 AkSoundEngine.AK_INVALID_PLAYING_ID 即表明post 失败
        /// </summary>
        /// <param name="playingId"></param>
        /// <param name="eventName"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        private bool VerifyPlayingID(uint playingId, string eventName, uint eventID)
        {
            if (playingId == AkSoundEngine.AK_INVALID_PLAYING_ID && AkSoundEngine.IsInitialized())
            {
                SGF.Debuger.LogWarning("[wwise] WwiseUnity: Could not post event (name: " + eventName + ", ID: " + eventID + "). Please make sure to load or rebuild the appropriate SoundBank.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 发送 wwise event的 通用接口
        /// 应该是uint eventID, string eventName, GameObject gob, 事件拥有者，可以是发声者也可以指定给别人
        /// postGob确定位置被指定的发声者（如果指定耳朵那就固定最大声音比如ui，还可以指定具体其他位置和gob）
        /// 听者一定是引擎根据距离的（引擎都帮你算好了距离衰减）
        /// </summary>
        /// <param name="eventID">eventID 和 eventName 只需要一个存在即可</param>
        /// <param name="eventName"></param>
        /// <param name="eventOwner">事件发生者</param>
        /// <param name="soundMaker">交给别人发出声音</param>
        /// <param name="playSoundEndCb"></param>
        /// <param name="volume"></param>
        private void PostWwiseEvent(uint eventID, string eventName, GameObject eventOwner, GameObject soundMaker, AkCallbackManager.EventCallback playSoundEndCb = null, float volume = 1)
        {
            uint pID = 0;
            bool isEventID = eventID != 0;

            eventOwner = eventOwner == null ? soundMaker : eventOwner;
            // 如果 是 post eventID, 那就按 eventID 的方式 postEvent
            if (isEventID)
            {
                if (!EnsureBank<uint>(eventID, m_ID_BankInfoDict))
                {
                    SGF.Debuger.Log(string.Format("[wwise] 加载event({0})失败,没有找到所属的SoundBank", eventName));
                    return;
                }

                pID = AkSoundEngine.PostEvent(eventID, soundMaker, m_CallbackType,
                  (object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info) =>
                  {
                      OnPlaySoundEventCb(in_type, pID, eventName, eventOwner);

                      playSoundEndCb?.Invoke(in_cookie, in_type, in_info);
                  }, null);
            }
            else
            {
                // 如果 是 post eventName, 那就按 eventName 的方式 postEvent
                if (!EnsureBank<string>(eventName, m_BankInfoDict))
                {
                    // SGF.Debuger.LogError(string.Format("[wwise] 加载event({0})失败,没有找到所属的SoundBank", eventName));
                    return;
                }
                pID = AkSoundEngine.PostEvent(eventName, soundMaker, m_CallbackType,
                (object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info) =>
                {
                    OnPlaySoundEventCb(in_type, pID, eventName, eventOwner);

                    playSoundEndCb?.Invoke(in_cookie, in_type, in_info);
                }, null);
            }

            // post 如果 失败, 那就 打个日志结束.成功的话,就去 做个 record 记录.
            if (!VerifyPlayingID(pID, eventName, eventID))
            {
                return;
            }
            RecordAudioPID(eventName, eventOwner, pID);
            ////设置音量
            //AkSoundEngine.SetGameObjectOutputBusVolume(eventOwner, GlobalListener, volume);
        }

        public bool PostSoundBankEvent(string bankName, string eventName, GameObject eventOwner, GameObject soundMaker, AkCallbackManager.EventCallback playSoundEndCb = null, float volume = 1)
        {
            if (!EnsureBank(bankName))
            {
                return false;
            }
            eventOwner = eventOwner == null ? soundMaker : eventOwner;

            uint pID = 0;
            pID = AkSoundEngine.PostEvent(eventName, soundMaker, m_CallbackType,
                 (object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info) =>
                 {
                     OnPlaySoundEventCb(in_type, pID, eventName, eventOwner);

                     playSoundEndCb?.Invoke(in_cookie, in_type, in_info);
                 }, null);

            // post 如果 失败, 那就 打个日志结束.成功的话,就去 做个 record 记录.
            if (!VerifyPlayingID(pID, eventName, 0))
            {
                return false;
            }
            RecordAudioPID(eventName, eventOwner, pID);
            ////设置音量
            //AkSoundEngine.SetGameObjectOutputBusVolume(eventOwner, GlobalListener, volume);

            return true;
        }

        public bool UnLoadBank(string bankName)
        {
            if (string.IsNullOrEmpty(bankName))
            {
                return false;
            }
            // 直接 unload, soundBank 内部做了 引用计数
            AkBankManager.UnloadBank(bankName);

            int refCount = AkBankManager.BanksRefCount(bankName);

            if (refCount == 0 && m_LoadedBanksSet.Contains(bankName))
            {
                m_LoadedBanksSet.Remove(bankName);
            }

            //   SGF.Debuger.LogError($"[wwise] unLoad bank: {bankName} , count: {AkBankManager.BanksRefCount(bankName)}");

            return false;
        }

        private bool UnLoadBank(AK.Wwise.Event events)
        {
            // 首先 查找 eventKey 是否存在 对应的 banks, 如果没有，没必要卸载bank
            if (!m_ID_BankInfoDict.TryGetValue(events.Id, out List<string> bankListName))
            {
                return false;
            }

            // 有对应的 banks,那就 首先看 这些banks 中是否有曾经加载过,如果加载过,那就直接卸载
            foreach (var bankName in bankListName)
            {
                if (m_LoadedBanksSet.Contains(bankName))
                {
                    return UnLoadBank(bankName);
                }
            }

            // 如果 从来没加载过这个 event 对应的bank, 就直接返回 false
            return false;
        }

        /// <summary>
        /// 加载 bankName 的soundBank. 当 bankName 为null或"" 返回false
        /// </summary>
        /// <param name="bankName"></param>
        /// <returns></returns>
        public bool LoadBank(string bankName)
        {
            if (string.IsNullOrEmpty(bankName))
            {
                return false;
            }
            if (!m_settingBanksSet.Contains(bankName))
            {
                //   SGF.Debuger.LogError($"[wwise] load bank: {bankName} , not find!!!");
                return false;
            }

            AkBankManager.LoadBank(bankName, false, false);

            int refCount = AkBankManager.BanksRefCount(bankName);

            if (refCount > 0)
            {
                m_LoadedBanksSet.Add(bankName);
            }

            //   SGF.Debuger.LogError($"[wwise] load bank: {bankName} , refCount: {refCount}");
            return true;
        }

        private bool EnsureBank(string bankName)
        {
            if (m_LoadedBanksSet.Contains(bankName))
            {
                return true;
            }

            return LoadBank(bankName);
        }

        /// <summary>
        /// 加载 eventKey 对应的 soundBank . 如果 一个 eventKey 对应 多个soundBank, 那么 只需要 加载第一个 bank即可.
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="bankInfos">所有的 banks 信息</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private bool EnsureBank<T>(T eventKey, Dictionary<T, List<string>> bankInfos)
        {
            // 首先 查找 eventKey 是否存在 对应的 banks, 如果没有，就不加载
            if (!bankInfos.TryGetValue(eventKey, out List<string> bankListName))
            {
                return false;
            }

            // 有对应的 banks,那就 首先看 这些banks 中是否有曾经加载过,如果加载过,那就不加载
            foreach (var bankName in bankListName)
            {
                if (m_LoadedBanksSet.Contains(bankName))
                {
                    return true;
                }
            }

            // 如果一个都没加载,那就去加载 soundbank. 一旦成功 就结束
            foreach (var bankName in bankListName)
            {
                if (LoadBank(bankName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 通过 event 来 加载bank, 用来 处理 挂在在场景上的 event 的soundBank的 加载
        /// </summary>
        /// <param name="events"></param>
        public void EnsureWwiseEventBank(AK.Wwise.Event events)
        {
            EnsureBank<uint>(events.Id, m_ID_BankInfoDict);
        }

        /// <summary>
        /// 通过 event 来 释放 bank
        /// </summary>
        /// <param name="events"></param>
        public void UnLoadWwiseEventBank(AK.Wwise.Event events)
        {
            UnLoadBank(events);

        }


        #endregion

        #region 几个 公用的方法
        private void AddEvent2Banks<T>(Dictionary<T, List<string>> banksDic, T eventKey, string bankName)
        {
            DictionaryUtils.AddListItem(banksDic, eventKey, bankName);
        }

        private string ForamtPlayingAudioKey(string path, GameObject gameObject)
        {
            // 使用 GetAkGameObjectID 防止 gameObject 为null 的情况
            ulong in_gameObjectID_id = AkSoundEngine.GetAkGameObjectID(gameObject);
            return $"{path}_{in_gameObjectID_id}";
        }

        private void RecordAudioPID(string path, GameObject gameObject, uint pID)
        {
            string key = ForamtPlayingAudioKey(path, gameObject);
            playingAudioID[key] = pID;
            //   SGF.Debuger.LogError($"[wwise] , record --> pID: {pID}, path: {path}");
        }


        private void DeleteAudioRecord(string path, GameObject gameObject, uint pID)
        {
            string key = ForamtPlayingAudioKey(path, gameObject);
            if (playingAudioID.ContainsKey(key))
            {
                playingAudioID.Remove(key);
                //   SGF.Debuger.LogError($"[wwise] , delete --> pID: {pID}, path: {path}");
            }
        }
        #endregion

        // =====================================AudioSource==============================================
        #region  采用 AudioSource 播放音频的 接口部分, 目前 已经采用了 wwise, 不过接口 暂时保留
        /// <summary>
        ///当前播放的音乐，回调,声音动态变化
        /// </summary>
        /// <param name="audioPriority"></param>
        /// <param name="ac"></param>
        /// <param name="forceUpdate">默认强更【按钮】，【bgm】，不强更就是播放忽略指令</param>
        public void PlayAudioSource(AudioPriority audioPriority, AudioClip ac, bool forceUpdate = true, bool needLoop = false)
        {

            //脚步声不需要loop
            bool isLoop = audioPriority == AudioPriority.Bgm4 || needLoop;
            if (ac == null)
            {
                AudioSource[audioPriority].Stop();
                AudioSource[audioPriority].clip = null;
            }
            else
            {
                AudioSource[audioPriority].clip = ac;
                AudioSource[audioPriority].loop = isLoop;//结束回调

                if (forceUpdate)
                {
                    if (isLoop)//bgm
                    {
                        AudioSource[audioPriority].Play();
                    }
                    else
                    {
                        //Shot后的回调
                        AudioSource[audioPriority].PlayOneShot(ac);

                        float delayTime = ac.length + 0.2f;
                        DelayInvoker.DelayInvoke(delayTime, (object[] args) =>
                        {
                            SetVolDynamic();
                        },
                        null);

                    }

                }
                else
                {
                    if (!AudioSource[audioPriority].isPlaying)
                    {
                        //重播是否有其他逻辑
                        if (isLoop)//bgm
                        {
                            AudioSource[audioPriority].Play();
                        }
                        else
                        {
                            //Shot后的回调
                            AudioSource[audioPriority].PlayOneShot(ac);
                            float delayTime = ac.length + 0.2f;
                            DelayInvoker.DelayInvoke(delayTime, (object[] args) =>
                            {
                                SetVolDynamic();
                            },
                            null);

                        }
                    }//不必延迟播放
                }
            }
            SetVolDynamic();
        }

        /// <summary>
        /// 增加一个WWise
        /// 外层驱动控制如不确定时，如对话聊天，
        /// 但如Aoi已驱动更新则不需要此处调用
        ///当前播放的音乐，回调,声音动态变化
        /// </summary>
        /// <param name="audioPriority"></param>
        /// <param name="ac"></param>
        /// <param name="forceUpdate">默认强更【按钮】，【bgm】</param>
        public void PlayAudioSourceMuti(AudioPriority audioPriority, AudioClip ac, bool forceUpdate, ulong entityId, bool needLoop)
        {
            if (ac == null)
            {
                return;
            }
            //Ac是空还玩啥？
            //if (forceUpdate)
            {

                GameManager.Instance.GetEntityCtr(entityId).M_Curr.ActionOnPlayAudio?.Invoke(ac, needLoop);
                //if (AudioSource[audioPriority].clip != null)
                //{
                //    if (AudioSource[audioPriority].isPlaying)
                //    {
                //        AudioSource[audioPriority].Stop();
                //    }
                //    AudioSource[audioPriority].clip = null;
                //}

            }

            //多人本质上就不用延迟
            //AudioSource[audioPriority].loop = isLoop;//结束回调,没有人的声音一直嘟嘟囔囔的loop吧，需要再加
            //if (isLoop)
            //{

            //}
            // else
            //{
            //    GameManager.Instance.GetEntityCtr(entityId).M_Curr.ActionOnPlayAudio?.Invoke(ac);
            //    //AudioSource[audioPriority].clip = ac;

            //    ////Shot后的回调
            //    //AudioSource[audioPriority].PlayOneShot(ac);

            //}
        }


        public void PlayAudioSourceMuti(AudioPriority audioPriority, AudioClip ac, bool forceUpdate, Action<AudioClip, bool> action, bool needLoop)
        {
            if (ac == null)
            {
                return;
            }
            action?.Invoke(ac, needLoop);
        }

        /// <summary>
        /// 不强制更新就是声音没有反复第一声音可以避免走路的第一帧重复。
        /// 另外触发环境也可以用只不过等第一个声音流程完事后播放第二个声音也不影响。
        /// </summary>
        /// <param name="audioMethodEvent"></param>
        /// <param name="forceUpdate"></param>
        public void PlayAudioSourceEvent(AudioMethodEvent audioMethodEvent, bool forceUpdate = true, ulong entityId = 0, bool needLoop = false)
        {
            for (int i = 0; i < AcList.Count; i++)
            {
                if (Enum.GetName(typeof(AudioMethodEvent), audioMethodEvent) == AcList[i].method)
                {
                    if (AcList[i].audioPriority == AudioPriority.RoleAudio2 && entityId != 0)
                    {
                        PlayAudioSourceMuti(AcList[i].audioPriority, AcList[i].audioClip, forceUpdate, entityId, needLoop);
                        break;
                    }
                    else
                    {
                        PlayAudioSource(AcList[i].audioPriority, AcList[i].audioClip, forceUpdate, needLoop);
                        break;
                    }

                }
            }
        }
        /// <summary>
        ///这个方法应对非逻辑层到显示层的
        ///只针对于多人音效，即第二层
        /// </summary>
        /// <param name="audioMethodEvent"></param>
        /// <param name="forceUpdate"></param>
        public void PlayAudioSourceEvent(AudioMethodEvent audioMethodEvent, Action<AudioClip, bool> action, bool forceUpdate = true, bool needLoop = false)
        {
            for (int i = 0; i < AcList.Count; i++)
            {
                if (Enum.GetName(typeof(AudioMethodEvent), audioMethodEvent) == AcList[i].method)
                {
                    if (AcList[i].audioPriority == AudioPriority.RoleAudio2 && action != null)
                    {
                        PlayAudioSourceMuti(AcList[i].audioPriority, AcList[i].audioClip, forceUpdate, action, needLoop);
                        break;
                    }
                    //else
                    //{
                    //    PlayAudioSource(AcList[i].audioPriority, AcList[i].audioClip, forceUpdate, needLoop);
                    //    break;
                    //}

                }
            }
        }
        #endregion

        #region AVG Timeline 播放音效
        
        private VoiceLogic m_VoiceLogic=new VoiceLogic();

        public (bool result,string key) PlayVoice(string voiceinfo,GameObject maker,System.Action<string> completeCallBack=null)
        {
           return m_VoiceLogic.PlayVoice(voiceinfo,maker,completeCallBack);
        }

        public void StopVoice(string key)
        {
            m_VoiceLogic.StopVoice(key);
        }
        
        
        public void StopVoiceEvent(string path,GameObject gameObject)
        {
            string key = ForamtPlayingAudioKey(path, gameObject);
            
            if (playingAudioID!=null && playingAudioID.ContainsKey(key))
            {    uint pID = playingAudioID[key];
                AkSoundEngine.StopPlayingID(pID, 100, AkCurveInterpolation.AkCurveInterpolation_Constant);
                
            }
            
        }
        
        #endregion
    }


}


