/*
 * @Description: UI网格辅助工具
 */
using System;
using System.IO;
using System.Linq;
using GameTechTools.CommonLibs.CommonExtends;
using GameTechTools.CommonLibs.UnityToolbarExtender;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace GameTechTools.UIHelpsTool
{
    #region 网格设置相关的数据
    [System.Serializable]
    internal class UIHelpsGridToolSettingData
    {
        [BoxGroup("基础设置", false)]
        [PropertySpace(SpaceBefore = 5)]
        [HorizontalGroup("基础设置/closeDefaultGrid", Width = 20, PaddingLeft = 10)]
        [HideLabel, LabelWidth(20)]
        [OnValueChanged("OnSettingDataChange")]
        public bool closeDefaultSceneGrid = true;

        [PropertySpace(SpaceBefore = 5)]
        [HorizontalGroup("基础设置/closeDefaultGrid")]
        [HideLabel, DisplayAsString, NonSerialized, ShowInInspector]
        public string closeDefaultSceneGridTip = "关闭Unity默认网格(避免干扰)";


        // [PropertySpace(SpaceBefore = 5)]
        // [HorizontalGroup("基础设置/fixedGrid", Width = 20, PaddingLeft = 10)]
        // [HideLabel, LabelWidth(20)]
        // [OnValueChanged("OnSettingDataChange")]
        // public bool fixedGridDistance = true;

        // [PropertySpace(SpaceBefore = 5)]
        // [HorizontalGroup("基础设置/fixedGrid")]
        // [HideLabel, DisplayAsString, NonSerialized, ShowInInspector]
        // public string fixedGridDistanceTip = "固定网格分辨率，不会随鼠标滚轮缩放";

        [PropertySpace(SpaceBefore = 10)]
        [HorizontalGroup("基础设置/gridSet1", Width = 20, PaddingLeft = 10)]
        [HideLabel, LabelWidth(20), PropertyTooltip("点击切换显示与否")]
        [OnValueChanged("OnSettingDataChange")]
        public bool showGridRef1 = true;


        [PropertySpace(SpaceBefore = 10)]
        [HorizontalGroup("基础设置/gridSet1", Width = 0.65f)]
        [LabelText("参考网格1"), LabelWidth(60)]
        [PropertyRange(1, 2048)]
        [OnValueChanged("OnSettingDataChange")]
        public int gridRefSize1 = 10;

        [PropertySpace(SpaceBefore = 10)]
        [HorizontalGroup("基础设置/gridSet1", PaddingRight = 10)]
        [HideLabel]
        [OnValueChanged("OnSettingDataChange")]
        public Color gridColor1 = new Color(0, 1, 0, 0.2f);


        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [HorizontalGroup("基础设置/gridSet2", Width = 20, PaddingLeft = 10)]
        [HideLabel, LabelWidth(20), PropertyTooltip("点击切换显示与否")]
        [OnValueChanged("OnSettingDataChange")]
        public bool showGridRef2 = true;

        [PropertySpace(SpaceBefore = 10)]
        [HorizontalGroup("基础设置/gridSet2", Width = 0.65f)]
        [LabelText("参考网格2"), LabelWidth(60)]
        [PropertyRange(2, 2048)]
        [OnValueChanged("OnSettingDataChange")]
        [PropertyTooltip("网格2需要是网格1的整数倍，否则会出现交叉错乱")]
        public int gridRefSize2 = 100;

        [PropertySpace(SpaceBefore = 10)]
        [HorizontalGroup("基础设置/gridSet2", PaddingRight = 10)]
        [HideLabel]
        [OnValueChanged("OnSettingDataChange")]
        public Color gridColor2 = new Color(1, 0, 0, 0.78f);

        [PropertySpace(SpaceBefore = 5, SpaceAfter = 10)]
        [HorizontalGroup("基础设置/reset", PaddingLeft = 10, Width = 100)]
        [Button("重置数据", ButtonSizes.Large)]
        private void ResetData()
        {
            closeDefaultSceneGrid = true;
            showGridRef1 = true;
            gridRefSize1 = 10;
            gridColor1 = new Color(0, 1, 0, 0.2f);
            showGridRef2 = true;
            gridRefSize2 = 100;
            gridColor2 = new Color(1, 0, 0, 0.78f);
            OnSettingDataChange();
        }


        //是否启用网格辅助
        [HideInInspector]
        public bool useGridHelps = false;

        [HideInInspector, NonSerialized]
        public string useGridHelpsTip = "开启功能";

        [PropertySpace(SpaceBefore = 20)]
        [HorizontalGroup("enableTool", PaddingLeft = 50, PaddingRight = 50)]
        [GUIColor("GetEnableColor")]
        [Button("$useGridHelpsTip", ButtonHeight = 40)]
        private void EnableGridTool()
        {
            useGridHelps = !useGridHelps;
            useGridHelpsTip = useGridHelps ? "关闭功能" : "开启功能";
#if UNITY_2019_1_OR_NEWER
            if (useGridHelps == false && closeDefaultSceneGrid)
            {
                if (EditorUtility.DisplayDialog("提示", "Unity默认网格已被关闭，是否重新开启？", "是"))
                {
                    if (SceneView.lastActiveSceneView != null)
                    {
                        SceneView.lastActiveSceneView.showGrid = true;
                        SceneView.lastActiveSceneView.Repaint();
                    }
                }
            }
#endif
            OnSettingDataChange();
        }


        private Color GetEnableColor()
        {
            return useGridHelps ? Color.red : Color.green;
        }

        private void OnSettingDataChange()
        {
            UIHelpsGridTool.NotifySettingDataChange();
        }
    }
    #endregion

    #region 网格设置窗口
    internal class UIHelpsGridToolSettingWindow : OdinEditorWindow
    {
        [HideLabel]
        public UIHelpsGridToolSettingData settingData = new UIHelpsGridToolSettingData();

        private static UIHelpsGridToolSettingWindow mainWindow;

        [MenuItem("Tools/UI辅助工具 (v1.0.0)/UI对齐网格辅助", priority = 202)]
        public static void Open()
        {
            UIHelpsGridToolSettingWindow window = GetWindow<UIHelpsGridToolSettingWindow>("UI网格设置");
            window.Show();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(340f, 220f);
            window.maxSize = new Vector2(340f, 300f);
            window.minSize = new Vector2(260f, 220f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mainWindow = this;
            mainWindow.settingData = UIHelpsGridTool.settingDataCache;
            if (mainWindow.settingData == null)
            {
                mainWindow.settingData = new UIHelpsGridToolSettingData();
            }
            mainWindow.settingData.useGridHelpsTip = mainWindow.settingData.useGridHelps ? "关闭功能" : "开启功能";
        }

        protected override void OnDestroy()
        {
            mainWindow = null;
            base.OnDestroy();
        }
    }
    #endregion

    #region 网格设置绘制
    [InitializeOnLoad]
    internal class UIHelpsGridTool
    {
        public static UIHelpsGridToolSettingData settingDataCache;
        private static string cacheDir = Directory.GetParent(Application.dataPath).FullName + "/Library/UIHelpsGridToolCache";
        private static string cacheFildName = "gridToolSettingData.json";
        static UIHelpsGridTool()
        {
            if (GTHelper.LoadWithBatchmode)
            {
                return;
            }
            LoadCache();
            CheckEnbleState();
        }

        internal static void NotifySettingDataChange()
        {
            UIHelpsGridTool.SaveCache();
            CheckEnbleState();
        }

        internal static void CheckEnbleState()
        {
            if (UIHelpsGridTool.settingDataCache.useGridHelps)
            {
                EnableGridTool();
            }
            else
            {
                CloseGridTool();
            }

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        internal static void EnableGridTool()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnUpdateSceneView;
            SceneView.duringSceneGui += OnUpdateSceneView;
#else
                SceneView.onSceneGUIDelegate -= OnUpdateSceneView;
                SceneView.onSceneGUIDelegate += OnUpdateSceneView;
#endif
        }

        internal static void CloseGridTool()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnUpdateSceneView;
#else
                SceneView.onSceneGUIDelegate -= OnUpdateSceneView;
#endif
        }


        //网格设置的数据由于每个人的操作习惯不一样，所以不序列化存储，而是放在cache json里面
        internal static void LoadCache()
        {
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }
            string jsonData = "";
            if (File.Exists(cacheDir + "/" + cacheFildName))
            {
                jsonData = File.ReadAllText(cacheDir + "/" + cacheFildName);
            }

            if (string.IsNullOrEmpty(jsonData))
            {
                settingDataCache = new UIHelpsGridToolSettingData();
            }
            else
            {
                try
                {
                    settingDataCache = JsonUtility.FromJson(jsonData, typeof(UIHelpsGridToolSettingData)) as UIHelpsGridToolSettingData;
                }
                catch (Exception e)
                {
                    settingDataCache = new UIHelpsGridToolSettingData();
                }
            }
        }

        //每次设置修改数据都保存下cache
        internal static void SaveCache()
        {
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }
            if (settingDataCache == null)
            {
                settingDataCache = new UIHelpsGridToolSettingData();
            }
            File.WriteAllText(cacheDir + "/" + cacheFildName, JsonUtility.ToJson(settingDataCache));
        }

        //使用Handles的绘制线条方法，可以使得网格线跟随handles
        //https://github.com/mdechatech/Sonic-Realms/blob/7b7693e243f1461f1bf935d8b42f3321caa1e993/Assets/Scripts/SonicRealms/Core/Utils/Editor/SceneViewGrid.cs
        internal static void OnUpdateSceneView(SceneView sceneView)
        {
            if (settingDataCache == null || !settingDataCache.useGridHelps || sceneView.camera == null || sceneView.in2DMode == false)
            {
                return;
            }
            //隐藏默认的网格，避免干扰
#if UNITY_2019_1_OR_NEWER
            sceneView.showGrid = !settingDataCache.closeDefaultSceneGrid;
#endif
            var bounds = GetCameraBounds(sceneView.camera);

            //handes绘制1比10，且是否是矩形是跟随外部canvas来适配的
            float gridSize1 = 1.0f * settingDataCache.gridRefSize1 / 1000;
            float gridSize2 = 1.0f * settingDataCache.gridRefSize2 / 1000;

            //grid1
            if (settingDataCache.showGridRef1)
            {
                var divisions = bounds.size.x / gridSize1 + bounds.size.y / gridSize1;
                //避免太小了看不清了
                if (divisions > 0.0f && divisions < 5000.0f)
                {
                    DrawNewGrid1(bounds, gridSize1, settingDataCache.gridColor1);
                }
            }

            //grid2
            if (settingDataCache.showGridRef2)
            {
                var divisions = bounds.size.x / gridSize2 + bounds.size.y / gridSize2;
                if (divisions > 0.0f && divisions < 5000.0f)
                {
                    DrawNewGrid1(bounds, gridSize2, settingDataCache.gridColor2);
                }
            }

        }

        //绘制网格
        internal static void DrawNewGrid1(Bounds bounds, float pandding, Color color)
        {
            for (var x = Mathf.Floor(bounds.min.x / pandding) * pandding; x < bounds.max.x; x += pandding)
            {
                Handles.color = color;

                Handles.DrawLine(new Vector3(x, bounds.max.y), new Vector3(x, bounds.min.y));
            }

            for (var y = Mathf.Floor(bounds.min.y / pandding) * pandding; y < bounds.max.y; y += pandding)
            {
                Handles.color = color;

                Handles.DrawLine(new Vector3(bounds.min.x, y), new Vector3(bounds.max.x, y));
            }
        }

        public static Bounds GetCameraBounds(Camera camera)
        {
            var min = camera.ViewportToWorldPoint(Vector3.zero);
            min = new Vector3(min.x, min.y, 0.0f);

            var max = camera.ViewportToWorldPoint(new Vector3(1.0f, 1.0f));
            max = new Vector3(max.x, max.y, 0.0f);

            return new Bounds((min + max) / 2.0f, max - min);
        }

        //实时绘制网格方法2，采用sceneView上层中心点绘制，当前方法会出现无法跟随handler，舍弃
        internal static void OnUpdateSceneView2(SceneView sceneView)
        {
            if (settingDataCache == null || !settingDataCache.useGridHelps || sceneView.in2DMode == false)
            {
                return;
            }
            //隐藏默认的网格，避免干扰
#if UNITY_2019_1_OR_NEWER
            sceneView.showGrid = !settingDataCache.closeDefaultSceneGrid;
#endif
            //开始绘制GUI，一定要添加手柄的开始绘制，否则会丢失掉手柄的GUI操作
            Handles.BeginGUI();
            //规定GUI显示区域
            GUILayout.BeginArea(new Rect(0, 0, sceneView.position.width, sceneView.position.height));

            //离相机的距离，随着鼠标滚轮不同分辨率不同
            float distance = 1;
            // if (!settingDataCache.fixedGridDistance)
            // {
            //     distance = sceneView.cameraDistance;
            // }
            // if (distance == 0)
            // {
            //     distance = 1;
            // }

            //grid1
            if (settingDataCache.showGridRef1)
            {
                DrawNewGrid2(sceneView, (float)Math.Round(settingDataCache.gridRefSize1 / distance, 1), settingDataCache.gridColor1);
            }

            //grid2
            if (settingDataCache.showGridRef2)
            {
                DrawNewGrid2(sceneView, (float)Math.Round(settingDataCache.gridRefSize2 / distance, 1), settingDataCache.gridColor2);
            }

            GUILayout.EndArea();
            Handles.EndGUI();
        }

        //绘制网格
        internal static void DrawNewGrid2(SceneView sceneView, float pandding, Color color)
        {
            pandding = Math.Max(pandding, 1);
            int iThickness = 1; //线条粗细
            //场景中心往两边扩散
            float centerX = (float)Math.Round(sceneView.position.width / 2 - iThickness / 2, 4); //减去线条的一半，这样避免0.5像数误差
            float centerY = (float)Math.Round(sceneView.position.height / 2 - iThickness / 2, 4);

            //纵
            float tmpX = centerX;
            while (tmpX > 0)
            {
                EditorGUI.DrawRect(new Rect(tmpX, 0, iThickness, sceneView.position.height), color);
                tmpX -= pandding;
            }
            tmpX = centerX;
            while (tmpX < sceneView.position.width)
            {
                EditorGUI.DrawRect(new Rect(tmpX, 0, iThickness, sceneView.position.height), color);
                tmpX += pandding;
            }

            //横
            float tmpY = centerY;
            while (tmpY > 0)
            {
                EditorGUI.DrawRect(new Rect(0, tmpY, sceneView.position.width, iThickness), color);
                tmpY -= pandding;
            }

            tmpY = centerY;
            while (tmpY < sceneView.position.height)
            {
                EditorGUI.DrawRect(new Rect(0, tmpY, sceneView.position.width, iThickness), color);
                tmpY += pandding;
            }
        }
    }
    #endregion
}