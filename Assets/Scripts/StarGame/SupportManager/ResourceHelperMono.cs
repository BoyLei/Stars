using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace StarProject
{
    /// <summary>
    /// 不是规范的ServiceModule
    /// </summary>

    [XLua.LuaCallCSharp]
    public static class ResourceHelperMono
    {
        //AB
        //AddRessable
        //Resource.Load
        public static string GlobalModule = "Prefabs/GlobalModule/Modules";
        public static string SingletonModule = "Perfab/Module/";
        public static string Folder = "Perfab/";

        public static string Texture = "Texture/";
        public static string SkillIconPath = "Texture/Icon/Skill/Common/";

        public static string ChapterAnimClipPath = "Models/Anims/VitalSign/Characters/";
        public static string ChapterAnimDefaultPath = "Models/Anims/VitalSign/Characters/Default";

        public static string MonsterAnimClipPath = "Models/Anims/VitalSign/Monsters/boss/";
        public static string MonsterAnimDefaultPath = "Models/Anims/VitalSign/Monsters/boss/Default";

        public static string SpecialTips = "Prefabs/MessagePerfab/SpecialTips";
        public static string BattleTips = "Prefabs/MessagePerfab/BattleTips";
        public static string SystemTips = "Prefabs/MessagePerfab/SystemTips";
        public static string AchievementTips = "Prefabs/MessagePerfab/AchievementTips";
        public static string PartnerLevelTips = "Prefabs/MessagePerfab/PartnerLevelTips";



        public static string AudioSourcePath = "Audio/";


        public static string WwisePath = "Wwise/";

        public static string AvgFg = "AVGRes/Fg/";


        /// <summary>
        /// 加载UI的Prefab
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject LoadGPrefab()
        {
            GameObject asset = (GameObject)Resources.Load(GlobalModule);
            GameObject go = GameObject.Instantiate(asset);
            go.name = "Modules";
            return go;
        }



        /// <summary>
        /// 加载UI的Prefab
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject AddressablesLoadSpecialTips()
        {
            if (string.IsNullOrEmpty(SpecialTips))
            {
                StarDebug.LogError("path is IsNullOrEmpty");
                return null;
            }

            var path = SpecialTips.ToLower();
            AsyncOperationHandle<GameObject> asyncOperation = Addressables.InstantiateAsync(path);
            return asyncOperation.WaitForCompletion();
        }

        /// <summary>
        /// 加载UI的Prefab
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject AddressablesLoadBattleTips()
        {
            if (string.IsNullOrEmpty(BattleTips))
            {
                StarDebug.LogError("path is IsNullOrEmpty");
                return null;
            }

            var path = BattleTips.ToLower();
            AsyncOperationHandle<GameObject> asyncOperation = Addressables.InstantiateAsync(path);
            return asyncOperation.WaitForCompletion();
        }

        /// <summary>
        /// 加载UI的Prefab
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject AddressablesLoadSystemTips()
        {
            if (string.IsNullOrEmpty(SystemTips))
            {
                StarDebug.LogError("path is IsNullOrEmpty");
                return null;
            }

            var path = SystemTips.ToLower();
            AsyncOperationHandle<GameObject> asyncOperation = Addressables.InstantiateAsync(path);
            return asyncOperation.WaitForCompletion();
        }

        /// <summary>
        /// 加载UI的Prefab
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject AddressablesLoadAchievementTips()
        {
            if (string.IsNullOrEmpty(AchievementTips))
            {
                StarDebug.LogError("path is IsNullOrEmpty");
                return null;
            }

            var path = AchievementTips.ToLower();
            AsyncOperationHandle<GameObject> asyncOperation = Addressables.InstantiateAsync(path);
            return asyncOperation.WaitForCompletion();
        }

        public static GameObject AddressablesLoadPartnerLevelTips()
        {
            if (string.IsNullOrEmpty(PartnerLevelTips))
            {
                StarDebug.LogError("path is IsNullOrEmpty");
                return null;
            }

            var path = PartnerLevelTips.ToLower();
            AsyncOperationHandle<GameObject> asyncOperation = Addressables.InstantiateAsync(path);
            return asyncOperation.WaitForCompletion();
        }

        /*public static GameObject AddressablesLoadGPrefab()
        {
            if (string.IsNullOrEmpty(GlobalModule))
            {
                StarDebug.LogError("path is IsNullOrEmpty");
                return null;
            }

            var path = GlobalModule.ToLower();
            AsyncOperationHandle<GameObject> asyncOperation = Addressables.InstantiateAsync(path);
            return asyncOperation.WaitForCompletion();
        }*/

        /// <summary>
        /// 加载UI的Prefab
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static   void  AddressablesLoadGPrefab( System.Action<GameObject> cb)
        {
            if (string.IsNullOrEmpty(GlobalModule))
            {
                StarDebug.LogError("path is IsNullOrEmpty");
                return;
            }
            var path = GlobalModule.ToLower();
            Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(path,cb);
          //  AsyncOperationHandle<GameObject> asyncOperation = Addressables.InstantiateAsync(path);
            //await asyncOperation;
        }

        /// <summary>
        /// 初始化，且获取实例的Module
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject LoadModulePrefab(string name)
        {
            GameObject asset = (GameObject)Resources.Load(SingletonModule + name);
            GameObject go = GameObject.Instantiate(asset);
            go.name = name;
            return go;
        }
        /// <summary>
        /// 加载resources下的预制体
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public static GameObject LoadPrefab(string prefabName)
        {
            GameObject asset = Resources.Load<GameObject>(Folder + prefabName);
            if (asset == null)
            {
                SGF.Debuger.Log($"ResourceManager LoadPrefab() err prefabName : {Folder}{prefabName}");
                return null;
            }
            GameObject go = GameObject.Instantiate(asset);
            return go;
        }
        /// <summary>
        /// 加载resources下的预制体资源
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public static GameObject LoadAsset(string prefabName)
        {
            GameObject asset = Resources.Load<GameObject>(prefabName);
            //Instance
            if (asset == null)
            {
                SGF.Debuger.Log($"ResourceManager LoadAsset() err prefabName : {prefabName}");
                return null;
            }
            return asset;
        }

        private static StringBuilder path = new();
        //LoadRes就不必Instantiate

        /// <summary>
        /// 这个接口帮拼接了 Texture 根目录，后续肯定要做资源计数管理，先只做简单的加载
        /// </summary>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public static Sprite LoadSprite(string spriteName)
        {
            // path.Clear
            Sprite sprite = Resources.Load<Sprite>(Texture + spriteName);

            return sprite;
        }


        public static TextAsset LoadWwiseSoundBankInfo(string Name)
        {
            string path = $"{WwisePath}{Name}";

#if UNITY_IPHONE || UNITY_IOS
			path = $"{path}_Ios";
#elif UNITY_ANDROID
            path = $"{path}_Android";
#else
            path = $"{path}_Windows";
#endif
            /* SGF.Debuger.LogError($"[wwise] load soundBankInfo path: {path}");*/
            TextAsset proper = Resources.Load<TextAsset>(path);

            return proper;
        }

        /// <summary>
        /// 加载技能图标
        /// </summary>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public static Sprite LoadSkillIconSprite(string spriteName)
        {
            Sprite sprite = Resources.Load<Sprite>(SkillIconPath + spriteName);
            return sprite;
        }
        /// <summary>
        /// 这个接口帮拼接了 Texture 根目录，后续肯定要做资源计数管理，先只做简单的加载
        /// </summary>
        /// <param name="spriteName">考虑角色id，和角色变身n/f/s两个层次，目前写死0</param>
        /// 动作还是共用：外观，特效变化
        /// 数值上变化：解锁技能
        /// 所以放一起就行特殊技能程序上认定普通也能释放，只不过他没配置
        /// <returns></returns>
        public static AnimationClip LoadAnimClip(int Roleid/*, string roleType*/, string animName, string defaultPrefabName = "")
        {
            AnimationClip anim = Resources.Load<AnimationClip>(ChapterAnimClipPath + Roleid + "/" + animName);
            if (anim == null && defaultPrefabName != "")//不填写没有候补资源，填写则候补
            {
                anim = Resources.Load<AnimationClip>(ChapterAnimDefaultPath + "/" + defaultPrefabName);
            }
            return anim;
        }



        /// <summary>
        /// 这个接口帮拼接了 Texture 根目录，后续肯定要做资源计数管理，先只做简单的加载
        /// </summary>
        /// <param name="spriteName">考虑角色id，和角色变身n/f/s两个层次，目前写死0</param>
        /// 动作还是共用：外观，特效变化
        /// 数值上变化：解锁技能
        /// 所以放一起就行特殊技能程序上认定普通也能释放，只不过他没配置
        /// <returns></returns>
        public static AnimationClip LoadMonsterAnimClip(int Roleid/*, string roleType*/, string animName, string defaultPrefabName = "")
        {
            // path.Clear

            AnimationClip anim = Resources.Load<AnimationClip>(MonsterAnimClipPath + Roleid + "/" + animName);
            if (anim == null && defaultPrefabName != "")//不填写没有候补资源，填写则候补
            {
                anim = Resources.Load<AnimationClip>(MonsterAnimDefaultPath + "/" + defaultPrefabName);
            }
            return anim;
        }

        public static GameObject LoadSpecialTips()
        {
            //GameObject messageGob = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(SpecialTips));
            GameObject messageGob = AddressablesLoadSpecialTips();

            messageGob.GetComponent<RectTransform>().localScale = Vector3.one;

            return messageGob;



        }

        public static GameObject LoadSystemTips()
        {
            //GameObject messageGob = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(SystemTips));
            GameObject messageGob = AddressablesLoadSystemTips();

            messageGob.GetComponent<RectTransform>().localScale = Vector3.one;

            return messageGob;
        }

        public static GameObject LoadAchievementTips()
        {
            //GameObject messageGob = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(AchievementTips));
            GameObject messageGob = AddressablesLoadAchievementTips();

            messageGob.GetComponent<RectTransform>().localScale = Vector3.one;

            return messageGob;
        }

        public static GameObject LoadPartnerLevelTeamTips()
        {
            //GameObject messageGob = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(AchievementTips));
            GameObject messageGob = AddressablesLoadPartnerLevelTips();

            messageGob.GetComponent<RectTransform>().localScale = Vector3.one;

            return messageGob;
        }

        public static GameObject LoadBattleTips()
        {
            //GameObject messageGob = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(BattleTips));
            GameObject messageGob = AddressablesLoadBattleTips();

            messageGob.GetComponent<RectTransform>().localScale = Vector3.one;

            return messageGob;



        }
        //public AudioClip GetAudioClip(int stageId, int levelId, string clipName)
        //{
        //    //去掉.mp3
        //    string[] strArr = clipName.Split('.');
        //    AudioClip audioClip = Resources.Load<AudioClip>(
        //        AudioSource +
        //        "Stage" + stageId + "/" +
        //        "Level" + levelId + "/" +
        //        strArr[0]);

        //    return audioClip;
        //}

        /// <summary>
        /// 通用全局资源加载
        /// </summary>
        /// <param name="clipName"></param>
        /// <returns></returns>
        public static AudioClip GetAudioCommonClip(string clipName)
        {
            //去掉.mp3
            string[] strArr = clipName.Split('.');
            AudioClip audioClip = Resources.Load<AudioClip>(
                AudioSourcePath +
                "Common/" +
                strArr[0]);

            return audioClip;
        }

        /// <summary>
        /// 加载角色立绘（临时）
        /// </summary>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public static Sprite LoadAvgFgSprite(string spriteName)
        {
            Sprite sprite = Resources.Load<Sprite>(AvgFg + spriteName);
            return sprite;
        }


        //private string CommonPath = "UISprite/Common/";
        //private string sideTaskPath = "UISprite/SideTask/";//支线任务
        //private string loadingSubPath = "Image/LoadingBg/";
        //private string miniSubPath = "Image/MiniMap/";
        //private string achievementSubPath = "UISprite/achievement/";
        //private string ItemIconSubPath = "UISprite/Item/";
        //private string HeadIconSubPath = "UISprite/PlayerHeadIcon/";
        //private string BuildIconSubPath = "UISprite/BuildHeadIcon/";
        //private string CommonBtnSubPath = "UISprite/CommonButton/";
        //private string EffectPath = "Effect/FX_Particle/";
        //private string EmojiSubPath = "UISprite/Emoji/";//表情
        //private string GatherTaskTargetPath = "UISprite/SceneItem/";//采集任务
        //private string GatherTaskPath = "UISprite/Gather/";//viking:采集任务 old
        //                                                   //private string SubVideoPath = "StreamingAssets/Video/";

        //private string NameBgPath = "UISprite/Story/";
        //private string AudioSource = "Audio/";

        //private string materialPath = "Material/";



        //    // 加一个通用基础方法：各个Window，设置自己的图片路径，和 icon
        //    public Sprite GetSprite(string path, string icon)
        //    {
        //        Sprite sprite = Resources.Load<Sprite>(path + icon);
        //        return sprite;
        //    }

        //    public Material GetMaterialPath(string path)
        //    {
        //        Material mat = Resources.Load<Material>(materialPath + path);

        //        return mat;
        //    }

        //    // CommonPath    
        //    public Sprite GetCommonPath(string spriteName)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(CommonPath + spriteName);

        //        return ItemSprite;
        //    }

        //    //表情
        //    public Sprite GetEmojiSubPath(string spriteName)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(EmojiSubPath + spriteName);

        //        return ItemSprite;
        //    }

        //    // 支线任务NPC    
        //    public Sprite GetSideTaskNpcPath(string spriteName)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(sideTaskPath + spriteName);

        //        return ItemSprite;
        //    }

        //    //用于不同界面内获取图片 

        //    public Sprite GetUISpriteTabPath(string Path, string spriteName)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(Path + spriteName);

        //        return ItemSprite;
        //    }
        //    /// <summary>
        //    /// 世界地图，
        //    /// </summary>
        //    /// <param name="stageNum"></param>
        //    /// <param name="levelId"></param>
        //    /// <param name="videoFullName"></param>
        //    /// <returns></returns>
        //    public string GetVideoPath(int stageNum, int levelId, string videoFullName)
        //    {
        //        ;
        //        string path = string.Empty;
        //        path = Application.streamingAssetsPath + "Video/Stage" + stageNum + "/" + "Level" + levelId + "/" + videoFullName;
        //        return path;
        //    }

        //    public string GetVideoPath(string name)
        //    {
        //        return Application.streamingAssetsPath + "/Video/" + name;
        //    }

        //    public Sprite GetLoadingBgImage(string spriteName)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(loadingSubPath + spriteName);

        //        return ItemSprite;
        //    }

        //    public Sprite GetMiniMapImage(int stageId, int levelId)
        //    {
        //        int stage = (stageId - 1) * 3 + levelId;//2，1 = 1，1=3+1=4
        //        string aa = miniSubPath + "Stage" + stage.ToString() + "/MiniMap";
        //        Sprite ItemSprite = Resources.Load<Sprite>(aa);

        //        return ItemSprite;
        //    }

        //    public AudioClip GetAudioClip(int stageId, int levelId, string clipName)
        //    {
        //        //去掉.mp3
        //        string[] strArr = clipName.Split('.');
        //        AudioClip audioClip = Resources.Load<AudioClip>(
        //            AudioSource +
        //            "Stage" + stageId + "/" +
        //            "Level" + levelId + "/" +
        //            strArr[0]);

        //        return audioClip;
        //    }


        //    public AudioClip GetAudioCommonClip(string clipName)
        //    {
        //        //去掉.mp3
        //        string[] strArr = clipName.Split('.');
        //        AudioClip audioClip = Resources.Load<AudioClip>(
        //            AudioSource +
        //            "Common/" +
        //            strArr[0]);

        //        return audioClip;
        //    }

        //    public Sprite GetAchievementIconImage(string spriteName)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(achievementSubPath + spriteName);

        //        return ItemSprite;
        //    }

        //    //TODO，拓展，可以传入id
        //    public Sprite GetItemIconImage(Item M_Item)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(ItemIconSubPath + M_Item.GetItemIconPath());
        //        if (ItemSprite == null)
        //        {
        //            ItemSprite = Resources.Load<Sprite>(ItemIconSubPath + "0");
        //        }

        //        return ItemSprite;
        //    }

        //    public Sprite GetItemIconImage(int itemId)
        //    {
        //        ItemCell cell = new ItemCell(itemId, 1);

        //        return GetItemIconImage(cell.M_Item);
        //    }

        //    public Sprite GetBuildIconImage(string resName)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(BuildIconSubPath + resName);

        //        return ItemSprite;
        //    }
        //    public Sprite GetHeadIconImage(string npcResName)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(HeadIconSubPath + npcResName);

        //        return ItemSprite;
        //    }

        //    public Sprite GetCommonBtnIconImage(string CmBtnIndex)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(CommonBtnSubPath + CmBtnIndex);

        //        return ItemSprite;
        //    }
        //    /// <summary>
        //    /// 表情加载
        //    /// </summary>
        //    /// <param name="emojiName"></param>
        //    /// <returns></returns>
        //    public Sprite GetEmojiImage(string emojiName)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(HeadIconSubPath + emojiName);

        //        return ItemSprite;
        //    }

        //    public Sprite GetGatherImage(string path)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(GatherTaskPath + path);

        //        return ItemSprite;
        //    }
        //    public Sprite GetGatherTargetImage(string path)
        //    {
        //        Sprite ItemSprite = Resources.Load<Sprite>(GatherTaskTargetPath + path);

        //        return ItemSprite;
        //    }

        //    public Sprite GetStoryNameBg(int id)
        //    {
        //        Sprite image = Resources.Load<Sprite>(NameBgPath + id);
        //        if (image == null)
        //        {
        //            image = Resources.Load<Sprite>(NameBgPath + "9999");
        //        }
        //        return image;
        //    }

        //    public GameObject LoadEffect(string name)
        //    {
        //        GameObject Effect = Resources.Load<GameObject>(EffectPath + name);
        //        return Effect;
        //    }
        //}

    }

}
