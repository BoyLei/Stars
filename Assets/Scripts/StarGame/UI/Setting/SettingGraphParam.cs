using SGF.UI.Framework;
using Sirenix.OdinInspector;
using StarProject.Game;
using StarProject.Service.LocalDynamic;
using StarProjectDef;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace StarProject.UI.StarWorld
{
    public enum GraphLevelEnum
    {
        None = 0,
        Lowest = 1,
        Low = 2,
        Middle = 3,
        High = 4,
        Custom = 5,
    }
    public class GameGraphSetting
    {
        public GraphLevelEnum level = GraphLevelEnum.Lowest;
        public GraphLevelEnum TrueLevel = GraphLevelEnum.Lowest;
        public bool msaa = false;
        public ModelQualityLevel modelType = ModelQualityLevel.Low;
        public bool outline = false;
    }
    public class SettingGraphParam : SettingBaseParam
    {
        [LabelText("图像复选组")]
        public ToggleGroup ToggleGroupGraph;

        [LabelText("抗锯齿复选组")]
        public ToggleGroup SMAAGroupGraph;

        [LabelText("模型质量复选组")]
        public ToggleGroup ModelGroupGraph;

        [LabelText("描边复选组")]
        public ToggleGroup OutlineGroupGraph;

        private Toggle[] toggles; //用来存放Toggle

        private Toggle[] SMAAToggles; //

        private Toggle[] ModelToggles; //

        private Toggle[] OutlineToggles; //

        private bool isChanage = false;
        private bool isInit = false;

        public Toggle toggleSGSR;
        public Toggle toggleFx;
        public bool onlyShowSelfFx = false;
        public Slider sliderSGSR;
        public Text texSGSR;
        public GameGraphSetting setting = new GameGraphSetting();
        private GraphLevelEnum curLevel = GraphLevelEnum.None;
        public GameGraphSetting customSetting;//缓存的自定义配置
        protected override void Awake()
        {
            base.Awake();

            toggles = ToggleGroupGraph.GetComponentsInChildren<Toggle>();
            for (int i = 0; i < toggles.Length; i++)
            {
                Toggle toggle = toggles[i];
                toggle.onValueChanged.AddListener((bool value) => OnValueChange(toggle));
            }

            SMAAToggles = SMAAGroupGraph.GetComponentsInChildren<Toggle>();
            for (int i = 0; i < SMAAToggles.Length; i++)
            {
                Toggle toggle = SMAAToggles[i];
                toggle.onValueChanged.AddListener((bool value) => OnMSAAValueChange(toggle));
            }

            ModelToggles = ModelGroupGraph.GetComponentsInChildren<Toggle>();
            for (int i = 0; i < ModelToggles.Length; i++)
            {
                Toggle toggle = ModelToggles[i];
                toggle.onValueChanged.AddListener((bool value) => OnModelValueChange(toggle));
            }

            OutlineToggles = OutlineGroupGraph.GetComponentsInChildren<Toggle>();
            for (int i = 0; i < OutlineToggles.Length; i++)
            {
                Toggle toggle = OutlineToggles[i];
                toggle.onValueChanged.AddListener((bool value) => OnOutlineValueChange(toggle));
            }

            toggleSGSR.onValueChanged.AddListener((bool value) => OnSGSRToggleValueChange(toggleSGSR));
            toggleFx.onValueChanged.AddListener((bool value) => OnFxToggleValueChange(toggleFx));
            sliderSGSR.onValueChanged.AddListener((float value) => OnSGSRSliderValueChange(value));
        }
        public override void Reset()
        {
            base.Reset();
            isChanage = false;
        }

        public override void Init()
        {
            base.Init();
            if (SaveManager.Instance.KeyExists(GameConfig.SETTING_GRAPHKEY_LEVEL, "Setting"))
            {
                curLevel = SaveManager.Instance.Load<GraphLevelEnum>(GameConfig.SETTING_GRAPHKEY_LEVEL, "Setting");
            }
            if (SaveManager.Instance.KeyExists(GameConfig.SETTING_GRAPHKEY, "Setting"))
            {
                customSetting = SaveManager.Instance.Load<GameGraphSetting>(GameConfig.SETTING_GRAPHKEY, "Setting");
            }
            //根据机型设置默认值
            if (curLevel == GraphLevelEnum.None)
            {
                if (GameConfig.MachineQualityLevel == MachineQualityLevel.LowestLevel)
                {
                    InitLevelSetting(GraphLevelEnum.Lowest);
                }
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.LowerLevel)
                {
                    InitLevelSetting(GraphLevelEnum.Low);
                }
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.MiddleLevel)
                {
                    InitLevelSetting(GraphLevelEnum.Middle);
                }
                else if (GameConfig.MachineQualityLevel == MachineQualityLevel.TopLevel || GameConfig.MachineQualityLevel == MachineQualityLevel.TopestLevel)
                {
                    InitLevelSetting(GraphLevelEnum.High);
                }
            }
            else
            {
                InitLevelSetting(curLevel, false);//如果有缓存就读缓存
            }

            if (SaveManager.Instance.KeyExists(GameConfig.SETTING_SGSR_Fx, "Setting"))//
            {
                onlyShowSelfFx = SaveManager.Instance.Load<bool>(GameConfig.SETTING_SGSR_Fx, "Setting");
            }
            toggleFx.isOn = onlyShowSelfFx;
            toggleSGSR.isOn = GameConfig.UseSGSR;
            sliderSGSR.value = GameConfig.SGSR_EdgeSharpness;
            texSGSR.text = GameConfig.SGSR_EdgeSharpness.ToString("F1");
            sliderSGSR.gameObject.SetActive(GameConfig.UseSGSR);

            isInit = true;
        }
        private void InitLevelSetting(GraphLevelEnum level, bool init = false)
        {
            isInit = false;
            bool isRefreshFlag = init;
            if (setting == null) { return; }
            curLevel = level;
            switch (curLevel)
            {
                case GraphLevelEnum.Lowest:
                    setting.level = GraphLevelEnum.Lowest;
                    setting.TrueLevel = GraphLevelEnum.Lowest;
                    setting.msaa = false;
                    setting.modelType = ModelQualityLevel.Low;
                    setting.outline = false;
                    break;
                case GraphLevelEnum.Low:
                    setting.level = GraphLevelEnum.Low;
                    setting.TrueLevel = GraphLevelEnum.Low;
                    setting.msaa = false;
                    setting.modelType = ModelQualityLevel.Low;
                    setting.outline = false;
                    break;
                case GraphLevelEnum.Middle:
                    setting.level = GraphLevelEnum.Middle;
                    setting.TrueLevel = GraphLevelEnum.Middle;
                    setting.msaa = true;
                    setting.modelType = ModelQualityLevel.Middle;
                    setting.outline = true;
                    break;
                case GraphLevelEnum.High:
                    setting.level = GraphLevelEnum.High;
                    setting.TrueLevel = GraphLevelEnum.High;
                    setting.msaa = true;
                    setting.modelType = ModelQualityLevel.High;
                    setting.outline = true;
                    break;
                case GraphLevelEnum.Custom:
                    if (customSetting != null)
                    {

                    }
                    else
                    {
                        SaveCustomSettingData();
                    }
                    setting.level = GraphLevelEnum.Custom;
                    setting.msaa = customSetting.msaa;
                    setting.TrueLevel = customSetting.TrueLevel;
                    setting.modelType = customSetting.modelType;
                    setting.outline = customSetting.outline;
                    break;
                default:
                    setting.level = GraphLevelEnum.Lowest;
                    setting.TrueLevel = GraphLevelEnum.Lowest;
                    setting.msaa = false;
                    setting.modelType = ModelQualityLevel.Low;
                    setting.outline = false;
                    break;
            }
            switch (setting.TrueLevel)
            {
                case GraphLevelEnum.Lowest:
                    GameConfig.MachineQualityLevel = MachineQualityLevel.LowestLevel;
                    AppMain.Instance.UseLowestLevelSetting();
                    break;
                case GraphLevelEnum.Low:
                    GameConfig.MachineQualityLevel = MachineQualityLevel.LowerLevel;
                    AppMain.Instance.UseLowLevelSetting();
                    break;
                case GraphLevelEnum.Middle:
                    GameConfig.MachineQualityLevel = MachineQualityLevel.MiddleLevel;
                    AppMain.Instance.UseMiddleLevelSetting();
                    break;
                case GraphLevelEnum.High:
                    GameConfig.MachineQualityLevel = MachineQualityLevel.TopLevel;//save & evt Noti
                    AppMain.Instance.UseHighLevelSetting();//globalSetting
                    break;
                default:
                    GameConfig.MachineQualityLevel = MachineQualityLevel.LowestLevel;
                    AppMain.Instance.UseLowestLevelSetting();
                    break;
            }

            toggles[(int)setting.level - 1].isOn = true;

            for (int i = 0; i < SMAAToggles.Length; i++)
            {
                Toggle toggle = SMAAToggles[i];
                if (setting.msaa)
                {
                    if (toggle.name == "Open")
                    {
                        toggle.isOn = true;
                        break;
                    }
                }
                else
                {
                    if (toggle.name == "Close")
                    {
                        toggle.isOn = true;
                        break;
                    }
                }
            }
            for (int i = 0; i < ModelToggles.Length; i++)
            {
                Toggle toggle = ModelToggles[i];
                switch (setting.modelType)
                {
                    case ModelQualityLevel.Low:
                        if (toggle.name == "ToggleLowestGraph")
                        {
                            toggle.isOn = true;
                            break;
                        }
                        continue;
                    case ModelQualityLevel.Middle:
                        if (toggle.name == "ToggleLowGraph")
                        {
                            toggle.isOn = true;
                            break;
                        }
                        continue;
                    case ModelQualityLevel.High:
                        if (toggle.name == "ToggleHignGraph")
                        {
                            toggle.isOn = true;
                            break;
                        }
                        continue;
                    default:
                        break;
                }
            }
            for (int i = 0; i < OutlineToggles.Length; i++)
            {
                Toggle toggle = OutlineToggles[i];
                if (setting.outline)
                {
                    if (toggle.name == "Open")
                    {
                        toggle.isOn = true;
                        break;
                    }
                }
                else
                {
                    if (toggle.name == "Close")
                    {
                        toggle.isOn = true;
                        break;
                    }
                }
            }
            OnPreFreshData(isRefreshFlag);
        }

        private void OnPreFreshData(bool init = false)
        {
            if (setting != null)
            {
                SettingCameraAAByType(setting.msaa, setting.TrueLevel);
                SetModelSetting(setting.modelType);
                SetOutlineSetting(setting.outline);
                SetFxSetting(onlyShowSelfFx);
            }
            if (init)
            {
                isInit = init;
            }
        }
        private void OnSGSRSliderValueChange(float t)
        {
            if (isInit == false)
            {
                return;
            }
            isChanage = true;

            GameConfig.SGSR_EdgeSharpness = t;
            texSGSR.text = t.ToString("F1");

        }



        private void OnSGSRToggleValueChange(Toggle t)
        {
            if (isInit == false)
            {
                return;
            }
            isChanage = true;



            GameConfig.UseSGSR = t.isOn;
            //开关GRSR
            GameManager.Instance.SetSGSR(GameConfig.UseSGSR);

            sliderSGSR.gameObject.SetActive(GameConfig.UseSGSR);
        }
        /// <summary>
        /// 屏蔽他人特效
        /// </summary>
        /// <param name="t"></param>
        private void OnFxToggleValueChange(Toggle t)
        {
            if (isInit == false)
            {
                return;
            }
            isChanage = true;
            SetFxSetting(t.isOn);
        }
        private void SetFxSetting(bool onlyShow)
        {
            LocalFxManager.Instance.ForbidOtherPlayerFX = onlyShow;
        }
        public void SettingCameraAAByType(bool isOpen, GraphLevelEnum level)
        {
            if (isOpen)
            {
                AppMain.Instance.SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
            }
            else
            {
                switch (level)
                {
                    case GraphLevelEnum.None:
                        AppMain.Instance.SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
                        break;
                    case GraphLevelEnum.Lowest:
                        AppMain.Instance.SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
                        break;
                    case GraphLevelEnum.Low:
                        AppMain.Instance.SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
                        break;
                    case GraphLevelEnum.Middle:
                        AppMain.Instance.SettingCameraAA(false, AntialiasingMode.FastApproximateAntialiasing, AntialiasingQuality.Medium);
                        break;
                    case GraphLevelEnum.High:
                        AppMain.Instance.SettingCameraAA(false, AntialiasingMode.FastApproximateAntialiasing, AntialiasingQuality.High);
                        break;
                    case GraphLevelEnum.Custom:
                        AppMain.Instance.SettingCameraAA(false, AntialiasingMode.None, AntialiasingQuality.Low);
                        break;
                    default:
                        break;
                }
            }
        }
        private void OnValueChange(Toggle t)
        {
            if (isInit == false)
            {
                return;
            }
            if (t.isOn)
            {
                isChanage = true;
                switch (t.name)
                {
                    case "ToggleHighGraph":
                        {
                            InitLevelSetting(GraphLevelEnum.High, true);
                        }
                        break;
                    case "ToggleMidGraph":
                        {
                            InitLevelSetting(GraphLevelEnum.Middle, true);
                        }
                        break;
                    case "ToggleLowGraph":
                        {
                            InitLevelSetting(GraphLevelEnum.Low, true);
                        }
                        break;
                    case "ToggleLowestGraph":
                        {
                            InitLevelSetting(GraphLevelEnum.Lowest, true);
                        }
                        break;
                    case "TogglCustomGraph":
                        {
                            InitLevelSetting(GraphLevelEnum.Custom, true);
                        }
                        break;
                    default:
                        {
                            isChanage = false;
                        }
                        break;
                }
            }
        }

        private void OnMSAAValueChange(Toggle t)
        {
            if (isInit == false)
            {
                return;
            }
            if (t.isOn)
            {
                isChanage = true;
                switch (t.name)
                {
                    case "Open":
                        {
                            if (!setting.msaa)
                            {
                                setting.msaa = true;

                                SaveCustomSettingData();
                                InitLevelSetting(GraphLevelEnum.Custom, true);
                            }
                            SettingCameraAAByType(true, setting.level);
                        }
                        break;
                    case "Close":
                        {
                            if (setting.msaa)
                            {
                                setting.msaa = false;
                                SaveCustomSettingData();
                                InitLevelSetting(GraphLevelEnum.Custom, true);
                            }
                            SettingCameraAAByType(false, setting.level);
                        }
                        break;
                    default:
                        {
                            isChanage = false;
                        }
                        break;
                }
            }
        }
        private void OnModelValueChange(Toggle t)
        {
            if (isInit == false)
            {
                return;
            }
            if (t.isOn)
            {
                isChanage = true;
                switch (t.name)
                {
                    case "ToggleLowestGraph":
                        {
                            if (setting.modelType != ModelQualityLevel.Low)
                            {
                                setting.modelType = ModelQualityLevel.Low;
                                SaveCustomSettingData();
                                InitLevelSetting(GraphLevelEnum.Custom, true);
                            }
                            SetModelSetting(ModelQualityLevel.Low);
                        }
                        break;
                    case "ToggleLowGraph":
                        {
                            if (setting.modelType != ModelQualityLevel.Middle)
                            {
                                setting.modelType = ModelQualityLevel.Middle;
                                SaveCustomSettingData();
                                InitLevelSetting(GraphLevelEnum.Custom, true);
                            }
                            SetModelSetting(ModelQualityLevel.Middle);
                        }
                        break;

                    case "ToggleHignGraph":
                        {
                            if (setting.modelType != ModelQualityLevel.High)
                            {
                                setting.modelType = ModelQualityLevel.High;
                                SaveCustomSettingData();
                                InitLevelSetting(GraphLevelEnum.Custom, true);
                            }
                            SetModelSetting(ModelQualityLevel.High);
                        }
                        break;
                    default:
                        {
                            isChanage = false;
                        }
                        break;
                }
            }
        }
        private void SetModelSetting(ModelQualityLevel level)
        {
            setting.modelType = level;
            GameConfig.modelType = level;
        }
        private void OnOutlineValueChange(Toggle t)
        {
            if (isInit == false)
            {
                return;
            }
            if (t.isOn)
            {
                isChanage = true;
                switch (t.name)
                {
                    case "Open":
                        {
                            if (!setting.outline)
                            {
                                setting.outline = true;
                                SaveCustomSettingData();
                                InitLevelSetting(GraphLevelEnum.Custom, true);
                            }
                            SetOutlineSetting(true);
                        }
                        break;
                    case "Close":
                        {
                            if (setting.outline)
                            {
                                setting.outline = false;
                                SaveCustomSettingData();
                                InitLevelSetting(GraphLevelEnum.Custom, true);
                            }
                            SetOutlineSetting(false);
                        }
                        break;
                    default:
                        {
                            isChanage = false;
                        }
                        break;
                }
            }
        }
        private void SetOutlineSetting(bool show)
        {
            setting.outline = show;
            DynamicRenderQueueManager.Instance.MAT_OVER_SCORE = show ? 80 : 0;
        }

        private void SaveCustomSettingData()
        {
            if (customSetting == null)
            {
                customSetting = new();
            }
            customSetting.level = setting.level;
            customSetting.TrueLevel = setting.TrueLevel;
            customSetting.msaa = setting.msaa;
            customSetting.modelType = setting.modelType;
            customSetting.outline = setting.outline;
        }
        #region 本地缓存

        public override void SaveLocalData()
        {
            base.SaveLocalData();

            if (isChanage == false)
            {
                return;
            }
            //数据库，内存用的是AppMain的，
            SaveManager.Instance.Save<int>(GameConfig.SETTING_GRAPH, (int)GameConfig.MachineQualityLevel, "Setting");

            SaveManager.Instance.Save<bool>(GameConfig.SETTING_SGSR, GameConfig.UseSGSR, "Setting");
            SaveManager.Instance.Save<float>(GameConfig.SETTING_SGSR_EdgeSharpness, GameConfig.SGSR_EdgeSharpness, "Setting");

            SaveManager.Instance.Save<bool>(GameConfig.SETTING_SGSR_Fx, LocalFxManager.Instance.ForbidOtherPlayerFX, "Setting");

            SaveManager.Instance.Save<GameGraphSetting>(GameConfig.SETTING_GRAPHKEY, customSetting, "Setting");
            SaveManager.Instance.Save<GraphLevelEnum>(GameConfig.SETTING_GRAPHKEY_LEVEL, curLevel, "Setting");
        }

        #endregion

    }
}
