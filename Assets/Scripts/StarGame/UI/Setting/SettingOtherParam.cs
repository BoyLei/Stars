using SGF.Module.Framework;
using SGF.UI.Framework;
using Sirenix.OdinInspector;
using StarProject.Service.Language;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;
namespace StarProject.UI.StarWorld
{
    public class SettingOtherParam : SettingBaseParam
    {
        [LabelText("注销账号")]
        public JButton JBtnLogout;
        [LabelText("用户中心")]
        public JButton JBtnUserCenter;
        [LabelText("联系客服")]
        public JButton JBtnContactService;
        [LabelText("隐私政策")]
        public JButton JBtnPrivacyPolicy;
        [LabelText("个人信息收集清单")]
        public JButton JBtnInformationCollectionList;

        [LabelText("语言复选组")]
        public GameObject LanguageSettingPanel;
        [LabelText("语言复选组")]
        public ToggleGroup ToggleLanguage;
        [LabelText("确认修改语言")]
        public JButton JBtnSureChangeLanguage;

        private Toggle[] toggles; //用来存放Toggle

        private bool isInit = false;
        private LanguageType CurLanguageType;
        private LanguageType ChangeLanguageType;

        protected override void Awake()
        {
            base.Awake();

            toggles = ToggleLanguage.GetComponentsInChildren<Toggle>();
            for (int i = 0; i < toggles.Length; i++)
            {
                Toggle toggle = toggles[i];//循环遍历添加
                toggle.onValueChanged.AddListener((bool value) => OnValueChange(toggle));
            }

            JBtnSureChangeLanguage.OnClick += OnJBtnSureChangeLanguage;

            bool isShowLanguageSettingPanel = true;
            //#if STAR_DEV
            //            isShowLanguageSettingPanel = true;
            //#endif
            LanguageSettingPanel.SetActive(isShowLanguageSettingPanel);

            isInit = true;
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override void Init()
        {
            base.Init();
            CurLanguageType = LanguageManager.Instance.CurLanguageType;

            toggles[(int)CurLanguageType - 1].isOn = true;
            JBtnSureChangeLanguage.gameObject.SetActive(false);
        }

        private void OnValueChange(Toggle t)
        {
            if (isInit == false)
            {
                return;
            }
            if (t.isOn)
            {
                switch (t.name)
                {
                    case "ToggleCN":
                        {
                            ChangeLanguageType = LanguageType.Chinese;
                        }
                        break;
                    case "ToggleEN":
                        {
                            ChangeLanguageType = LanguageType.English;
                        }
                        break;
                    default:
                        {
                            bool isExists = SaveManager.Instance.KeyExists(GameConfig.SETTING_LANGUAGE, "Setting");
                            if (isExists)
                            {
                                int languageType = SaveManager.Instance.Load<int>(GameConfig.SETTING_LANGUAGE, "Setting");
                                ChangeLanguageType = (LanguageType)languageType;
                            }
                        }
                        break;
                }
                JBtnSureChangeLanguage.gameObject.SetActive(ChangeLanguageType != CurLanguageType);
            }
        }

        private void OnJBtnSureChangeLanguage(GameObject arg0)
        {
            if (ChangeLanguageType == CurLanguageType)
            {
                return;
            }
            UIAPI.ShowMsgBoxNewAsync(75, (string v) =>
            {
                CurLanguageType = ChangeLanguageType;
                SaveManager.Instance.Save<int>(GameConfig.SETTING_LANGUAGE, (int)CurLanguageType, "Setting");
                Application.Quit();

            });

            //ModuleManager.Instance.SendMessage(ModuleDef.Name.WorldMapModule, "ResetCacheData");
            //LanguageManager.Instance.ChanageLanguage(ChangeLanguageType);
            //UIManager.Instance.ReLogin(AgainLoginType.ChangeLanguage);
        }

    }
}
