using SGF.Module.Framework;
using SGF.UI.Framework;
using StarProject.Service.LocalData;
using StarProject.Service.SDK;
using StarProjectDef;
using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using XLua;
namespace StarProject.Service.Language
{
    [LuaCallCSharp]
    public class LanguageManager : ServiceModule<LanguageManager>
    {
        private LanguageType curLanguageType = LanguageType.Chinese;
        public LanguageType CurLanguageType { get { return curLanguageType; } }

        private Action ChanageLanguageAction = null;

        public void Init()
        {
            CheckSingleton();

            InitSettingCache();
        }

        public string GetLanguageStr()
        {
            switch (CurLanguageType)
            {
                case LanguageType.None:
                case LanguageType.Chinese:
                    return "zh";
                    break;
                case LanguageType.English:
                    return "en";
                    break;
                default:
                    return "zh";
                    break;
            }

            return "zh";
        }

        private void InitSettingCache()
        {
            bool isExists = SaveManager.Instance.KeyExists(GameConfig.SETTING_LANGUAGE, "Setting");
            if (isExists)
            {
                int languageType = SaveManager.Instance.Load<int>(GameConfig.SETTING_LANGUAGE, "Setting");
                this.curLanguageType = (LanguageType)languageType;
            }
            else
            {
                LanguageType sdkType = GetSDKDefaultLanguage();
                // 如果有sdk 的语言类型，采用sdk 语言类型.
                curLanguageType = sdkType == LanguageType.None ? GameApp.Instance.languageType : sdkType;
            }
        }

        public LanguageType GetSDKDefaultLanguage()
        {
            if (SDKManager.Instance.SdkLanguage == "" || SDKManager.Instance.SdkLanguage == null)
            {

                return LanguageType.None;
            }
            if (SDKManager.Instance.SdkLanguage.Contains("zh"))
            {
                return LanguageType.Chinese;
            }

            if (SDKManager.Instance.SdkLanguage.Contains("en"))
            {
                return LanguageType.English;
            }

            return LanguageType.None;
        }

        public void ChanageLanguage(LanguageType languageType)
        {
            if (this.curLanguageType == languageType)
            {
                return;
            }
            this.curLanguageType = languageType;
            ChanageLanguageAction?.Invoke();
        }

        public int GetLanguageType()
        {
            return (int)CurLanguageType;
        }

#if UNITY_EDITOR
        public class LanguagePostprocessor : UnityEditor.AssetPostprocessor
        {
            //This function will be called before script compilation and will save the picker's expantion 
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling)
                    return;

                if (importedAssets != null && importedAssets.Length > 0)
                {
                    for (int i = 0; i < importedAssets.Length; i++)
                    {
                        if (importedAssets[i].EndsWith(".bytes"))
                        {
                            LocalDataManager.Instance.ClearEditorLanguageCache();
                            break;
                        }
                    }
                }

            }
        }
#endif

#if UNITY_EDITOR
        //任务编辑器是否被打开
        [XLua.BlackList]
        public bool TaskEditorOpened;
        //技能编辑器是否被打开
        [XLua.BlackList]
        public bool SkillEditorOpened;
#endif


        #region 文本

        public string GetLanguageByKey(string key, LanguageType languageType = LanguageType.None)
        {
            if (key == null)
            {
                return string.Empty;
            }

            string val = string.Empty;
            if (languageType == LanguageType.None)
            {
                languageType = this.curLanguageType;
            }
            switch (languageType)
            {
                case LanguageType.Chinese:
                    val = LocalDataManager.Instance.GetCNLanguageVal(key);
                    break;
                case LanguageType.English:
                    val = LocalDataManager.Instance.GetENLanguageVal(key);
                    break;
                default:
                    break;
            }

#if STAR_DEV
            if (string.IsNullOrEmpty(val))
            {
                Debug.LogWarning($"多语言Key未找到或对应的多语言内容为空，Key：{key}");
                return key;
            }
#endif
            return val;
            //return Translate(val, languageType);
        }

        private const string pattern = "(?<!#)#{1}([^#]+)#";
        private string Translate(string source, LanguageType languageType)
        {
            var results = Regex.Matches(source, pattern);
            if (results.Count == 0)
            {
                return source;
            }
            StringBuilder sb = new();
            int pos = 0;
            foreach (Match match in results)
            {
                sb.Append(source, pos, match.Index - pos);
                sb.Append(GetLanguageByKey(match.Groups[1].Value, languageType));
                pos = match.Index + match.Length;
            }
            sb.Append(source, pos, source.Length - pos);

            return sb.ToString();
        }

        #endregion

        #region 图片

        public void GetLanguageImg(string atlasPath, string iconPath, Action<Sprite> cb, LanguageType languageType = LanguageType.None)
        {
            if (languageType == LanguageType.None)
            {
                languageType = this.curLanguageType;
            }
            string atlasPath1 = string.Empty;
            string iconPath1 = iconPath;
            switch (languageType)
            {
                case LanguageType.Chinese:
                    if (!string.IsNullOrEmpty(atlasPath) && !string.IsNullOrWhiteSpace(atlasPath))
                    {
                        atlasPath1 = $"UI/LanguageUI/CN/{atlasPath}";
                    }
                    else
                    {
                        iconPath1 = $"UI/LanguageUI/CN/{iconPath}";
                    }
                    break;
                case LanguageType.English:
                    if (!string.IsNullOrEmpty(atlasPath) && !string.IsNullOrWhiteSpace(atlasPath))
                    {
                        atlasPath1 = $"UI/LanguageUI/EN/{atlasPath}";
                    }
                    else
                    {
                        iconPath1 = $"UI/LanguageUI/EN/{iconPath}";
                    }
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(atlasPath1) && !string.IsNullOrWhiteSpace(atlasPath1))
            {
                // 图集的异步加载
                AtlasManager.AtlasManager.Instance.GetSpriteAsync(atlasPath1, iconPath1, cb);
            }
            else
            {
                Resource.ResourceFormalManager.Instance.LoadSpriteAsync(iconPath1, cb);
            }
        }

        public void GetLanguageRawimg(string iconPath, Action<Texture2D> cb, LanguageType languageType = LanguageType.None)
        {
            if (languageType == LanguageType.None)
            {
                languageType = this.curLanguageType;
            }
            string iconPath1 = iconPath;
            switch (languageType)
            {
                case LanguageType.Chinese:
                    iconPath1 = $"UI/LanguageUI/CN/{iconPath}";
                    break;
                case LanguageType.English:
                    iconPath1 = $"UI/LanguageUI/EN/{iconPath}";
                    break;
                default:
                    break;
            }

            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTextureAsync(iconPath1, cb);
        }



        #endregion

        public void RegisterAction(Action cb)
        {
            ChanageLanguageAction += cb;
        }

        public void UnRegisterAction(Action cb)
        {
            ChanageLanguageAction -= cb;
        }
    }
}
