using Sirenix.OdinInspector;
using StarProject.Service.Language;
using StarProjectDef;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.LanguageUI
{
    public class LocalizationImage : MonoBehaviour
    {
        public void OnSetImage()
        {
#if UNITY_EDITOR
            if (Image != null)
            {
                string iconName = Image.name;
                iconName = iconName.Replace("Assets/Res/", "").Replace(".png", "");

                string imagePath = AssetDatabase.GetAssetPath(Image);
                imagePath = imagePath.Replace("Assets/Res/", "").Replace(".png", "");
                var split = imagePath.Split("/");
                if (split.Length > 0)
                {
                    imagePath = split[split.Length - 1];
                }

                if (iconName != imagePath)
                {
                    AtlasPath = imagePath;
                }
                else
                {
                    AtlasPath = string.Empty;
                }

                ImagePath = iconName;

                Image = null;
            }
#endif
        }
        [LabelText("图片")]
        [OnValueChanged("OnSetImage")]
        public Sprite Image;
        [LabelText("是否使用图片正式尺寸")]
        public bool IsNativeSize = true;
        [ReadOnly]
        public string AtlasPath;
        [ReadOnly]
        public string ImagePath;

        public void OnClear()
        {
#if UNITY_EDITOR
            Image = null;
            AtlasPath = string.Empty;
            ImagePath = string.Empty;
#endif
        }
        [LabelText("清除")]
        [OnValueChanged("OnClear")]
        public bool Clear = false;



        private IEnumerable _languageType = new ValueDropdownList<LanguageType>()
        {
            { "中文", LanguageType.Chinese},
            { "英文", LanguageType.English},
        };

        [LabelText("默认语言")]
        [ValueDropdown("_languageType")]
        [OnValueChanged("OnLanguageTypeChange")]
        public LanguageType languageType;

        public void OnLanguageTypeChange()
        {
            if (languageType != LanguageManager.Instance.CurLanguageType)
            {
                Refresh();
            }
        }

        private Image image;
        private RawImage rawImage;

        private Action<Sprite> loadSprite;
        private Action<Texture2D> loadTexture2D;


        private void Awake()
        {
            image = GetComponent<Image>();
            rawImage = GetComponent<RawImage>();

            LanguageManager.Instance.RegisterAction(Refresh);

            languageType = LanguageManager.Instance.CurLanguageType;
        }

        private void OnEnable()
        {
            var _languageType = LanguageManager.Instance.CurLanguageType;
            if (languageType != _languageType)
            {
                languageType = _languageType;
                Refresh();
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (string.IsNullOrEmpty(ImagePath) || string.IsNullOrWhiteSpace(ImagePath))
            {
                return;
            }

            if (image != null)
            {
                LoadSprite();
            }
            else if (rawImage != null)
            {
                LoadTexture2D();
            }
        }

        private void LoadSprite()
        {
            loadSprite = (Sprite sp) =>
            {
                if (image != null && sp != null)
                {
                    image.sprite = sp;
                    if (IsNativeSize)
                    {
                        image.SetNativeSize();
                    }
                }
                if (sp == null)
                {
                    SGF.Debuger.LogWarning($"LocalizationImage LoadSprite() AtlasPath={AtlasPath},ImagePath={ImagePath},sp=null");
                }
                loadSprite = null;
            };
            LanguageManager.Instance.GetLanguageImg(AtlasPath, ImagePath, loadSprite, languageType);
        }

        private void LoadTexture2D()
        {
            loadTexture2D = (Texture2D sp) =>
            {
                if (rawImage != null && sp != null)
                {
                    rawImage.texture = sp;
                    if (IsNativeSize)
                    {
                        rawImage.SetNativeSize();
                    }
                }
                if (sp == null)
                {
                    SGF.Debuger.LogWarning($"LocalizationImage LoadTexture2D() AtlasPath={AtlasPath},ImagePath={ImagePath},sp=null");
                }
                loadTexture2D = null;
            };
            LanguageManager.Instance.GetLanguageRawimg(ImagePath, loadTexture2D, languageType);
        }

        private void OnDisable()
        {
            LanguageManager.Instance.UnRegisterAction(Refresh);
        }

        private void OnDestroy()
        {
            LanguageManager.Instance.UnRegisterAction(Refresh);
            loadSprite = null;
            loadTexture2D = null;
        }

    }
}
