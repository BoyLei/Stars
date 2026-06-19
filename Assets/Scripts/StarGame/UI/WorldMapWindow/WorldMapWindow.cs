using Frame;
using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using Sirenix.OdinInspector;
using StarProject.Game;
using StarProject.Module;
using StarProject.Service.AtlasManager;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.StarFramework.UI.Extend;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.UI.WorldMap
{
    [System.Serializable]
    public class WorldMapTypeSerialize
    {
        [LabelText("国度ID")]
        public int NationID = -1;
        [LabelText("国度")]
        public RectTransform Nation = null;
        [LabelText("国度b包含的地区")]
        public List<WorldMapAreaItem> MapList = new();
    }


    public class WorldMapWindow : UIWindow
    {
        private string LOG_TAG = "[WorldMapWindow]";

        public ScrollViewMapExtend ScrollRect;
        public ScrollViewNevigation M_SelectRoleScrollView;
        public RectTransform M_RectTransform; // 滑动区域
        public Image WorldMapIcon;  // 世界地图
        public Text MapName; // 地图名字
        public Image Headimg;   // 主角职业头像
        public RectTransform MainRole; // 主角节点
        public WMapEntityRoot WMapEntityRoot; // 地图列表显示
        public Slider ScaleSlider;    // 地图缩放滑动框
        public Text ScaleMin; // 地图缩放滑动最小值
        public Text ScaleMax; // 地图缩放滑动最大值
        public List<WorldMapTypeSerialize> NationList;

        private WorldMapModule WorldMapModule;

        private float CurMapScale = 1.0f;   // 当前地图缩放值
        private bool IsClickArea = true;


        // 世界地图缩放的最大最小值
        private float minScale = 1.0f;
        private float maxScale = 1.0f;
        private float middleScale = 1.0f;

        protected override void Awake()
        {
            base.Awake();

            minScale = (float)SystemConstConfigs.WMapMinZoomRatio / 1000;
            ScaleMin.text = $"x{minScale}";
            ScaleSlider.minValue = minScale;
            maxScale = (float)SystemConstConfigs.WMapMaxZoomRatio / 1000;
            ScaleMax.text = $"x{maxScale}";
            ScaleSlider.maxValue = maxScale;
            middleScale = minScale + ((maxScale - minScale) / 2);

            ScaleSlider.onValueChanged.AddListener(OnValueChanged);

            ScrollRect.SetDragDistance(10.0f);
            ScrollRect.SetScale(minScale, maxScale);
            ScrollRect.SetScaleTarget(WorldMapIcon.rectTransform);
            ScrollRect.FingerScaleAction = OnFingerScaleAction;

            WorldMapModule = ModuleManager.Instance.GetModule(ModuleDef.Name.WorldMapModule) as WorldMapModule;

            InitWorldMapAreaItems();
        }

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);

            SetMainPlayer();
            SetMapLock();
            bool isFind = false;
            int mapID = GameManager.Instance.M_Map.GetMapId();
            int fromeNation = LocalDataManager.Instance.GetMapIDFromeNation(mapID);
            // 公会领地中默认选中王城的所属国度
            if (fromeNation == -1 && GameManager.Instance.GetCurMapType() == ProtoMsg.SpaceType.SpaceGuildTerritory)
            {
                mapID = 12;
                fromeNation = 1;
            }
            if (fromeNation != -1)
            {
                // 定位并放大
                RectTransform rectTransform = GetNationRect(fromeNation);
                if (rectTransform != null)
                {
                    OnValueChanged(ScaleSlider.maxValue);
                    ScaleSlider.value = CurMapScale;
                    M_SelectRoleScrollView.Nevigate(rectTransform, false);
                    RectTransform areaRect = GetAreaRect(mapID);
                    SetMainPlayerPos(areaRect.position);
                    MainRole.gameObject.SetActive(true);
                    isFind = true;
                }
            }

            if (!isFind)
            {
                // 每次打开小地图，都回归00默认位置
                Rect rect = M_RectTransform.rect;
                M_RectTransform.transform.localPosition = Vector3.zero;
                rect.xMax = 0;
                rect.yMax = 0;
                // 缩放还原
                OnValueChanged(ScaleSlider.minValue);
                ScaleSlider.value = CurMapScale;

                MainRole.gameObject.SetActive(false);
            }

            InitWMapEntityRoot(fromeNation);
            ScrollRect.SetState(true);
        }

        protected override void OnClose(object arg = null)
        {
            base.OnClose(arg);

            HideEntityRoot();
            ScrollRect.SetState(false);
        }

        private void OpenMiniMap(int mapID)
        {
            string path = $"MapData/{mapID}/data";
            var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
            if (jsonAsset != null)
            {
                var sceneJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(jsonAsset.text);
                WorldMapModule.OnChanageMapAction?.Invoke(sceneJsonData, mapID);
                UIManager.Instance.CloseWindow(UIDef.WorldMapWindow);
            }
        }

        #region 主角设置

        private void SetMainPlayer()
        {
            // 头像设置
            uint mainPlayerJobID = GameManager.Instance.GetPlayerJob();
            var M_JobDataCell = LocalDataManager.Instance.GetJobDataCell((int)mainPlayerJobID);
            if (M_JobDataCell != null)
            {
                var M_AvatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(M_JobDataCell.GetAvatarID());
                if (M_AvatarDataCell != null)
                {
                    ModelHeadDataCell modelHeadDataCell = LocalDataManager.Instance.GetModelHeadDataCell(M_AvatarDataCell.GetHeadID());
                    if (modelHeadDataCell != null)
                    {
                        AtlasManager.Instance.GetSpriteAsync(modelHeadDataCell.HeadAtlasName, modelHeadDataCell.MinHead, (Sprite img) =>
                        {
                            if (img == null)
                            {
                                return;
                            }
                            if (Headimg != null)
                            {
                                Headimg.sprite = img;
                            }
                        });
                    }
                }
            }
        }

        private void SetMainPlayerPos(Vector3 worldPos)
        {
            MainRole.position = worldPos;
        }

        #endregion

        #region 国度地区

        private RectTransform GetNationRect(int nationID)
        {
            foreach (var item in NationList)
            {
                if (item.NationID == nationID)
                {
                    return item.Nation;
                }
            }
            return null;
        }

        private RectTransform GetAreaRect(int mapID)
        {
            foreach (var item in NationList)
            {
                foreach (var item1 in item.MapList)
                {
                    if (item1.MapID == mapID)
                    {
                        return item1.MapRect;
                    }
                }
            }
            return null;
        }

        private WorldMapAreaItem GetWorldMapAreaItem(int nationID, int mapID)
        {
            foreach (var item in NationList)
            {
                if (item.NationID == nationID)
                {
                    foreach (var item1 in item.MapList)
                    {
                        if (item1.MapID == mapID)
                        {
                            return item1;
                        }
                    }
                }
            }
            return null;
        }

        private void InitWorldMapAreaItems()
        {
            foreach (var item in NationList)
            {
                foreach (var item1 in item.MapList)
                {
                    item1.InitWorldMapAreaItem(OnClickAreaBtn);
                }
            }
        }

        private void SetMapLock()
        {
            foreach (var item in NationList)
            {
                foreach (var item1 in item.MapList)
                {
                    item1.CheckLock();
                }
            }
        }

        private void OnClickAreaBtn(WorldMapAreaItem worldMapAreaItem)
        {
            if (!IsClickArea)
            {
                // 放大后不能点击地区了
                SGF.Debuger.LogWarning($"放大后不能点击地区了");
                return;
            }

            if (ScrollRect.isDrag || ScrollRect.isScaleing)
            {
                // 触发滑动，过滤点击寻路
                SGF.Debuger.LogWarning($"触发滑动，过滤点击寻路");
                return;
            }

            if (worldMapAreaItem == null)
            {
                return;
            }

            if (!worldMapAreaItem.isUnlock)
            {
                //Util.ShowMessage($"{worldMapAreaItem.AreaText.text}未解锁");
                //Util.ShowMessage(string.Format(GameConfig.LocalStr["XUnlock"], worldMapAreaItem.AreaText.text));
                Util.ShowMessage(string.Format(LanguageManager.Instance.GetLanguageByKey("XUnlock"), worldMapAreaItem.AreaText.text));
                return;
            }

            int mapID = worldMapAreaItem.MapID;
            OpenMiniMap(mapID);
        }

        #endregion

        #region 地图缩放

        private void OnValueChanged(float value)
        {
            if (value == CurMapScale)
            {
                return;
            }
            float filterValue = value >= middleScale ? maxScale : minScale;
            ScaleMap(CurMapScale, filterValue);
        }

        private void OnFingerScaleAction(float value)
        {
            if (value == CurMapScale)
            {
                return;
            }
            float filterValue = value >= middleScale ? maxScale : minScale;
            float showValue = value >= middleScale ? 1 : 0;
            if (showValue == ScaleSlider.value)
            {
                return;
            }
            ScaleSlider.value = showValue;
            ScaleMap(CurMapScale, filterValue);
        }

        private void ScaleMap(float lastScale, float curScale)
        {
            CurMapScale = curScale;
            IsClickArea = CurMapScale > middleScale;
            Vector3 scale = Vector3.one * curScale;
            WorldMapIcon.transform.SetLocalScale(scale);
            //UpdateScaleContentPos(curScale, lastScale);
            UpdateScaleMainPlayerPos(curScale, lastScale);
        }

        private void UpdateScaleContentPos(float scale, float lastScale)
        {
            RectTransform rt = ScrollRect.content;
            Vector2 pos = rt.anchoredPosition / lastScale * scale;
            rt.anchoredPosition = pos;
        }

        private void UpdateScaleMainPlayerPos(float scale, float lastScale)
        {
            MainRole.transform.SetLocalScale(Vector3.one);
            //Vector2 pos = MainRole.anchoredPosition / lastScale * scale;
            //MainRole.anchoredPosition = pos;
        }


        #endregion

        #region 地图实体列表

        private void InitWMapEntityRoot(int defualtOpenFromeNation = 1)
        {
            WMapEntityRoot.SetMapNationData(OnClickEntityItem, defualtOpenFromeNation);
        }

        private void OnClickEntityItem(MapShowArea mapShowArea)
        {
            if (mapShowArea == null)
            {
                return;
            }
            // 定位 
            int fromeNation = LocalDataManager.Instance.GetMapIDFromeNation(mapShowArea.MapID);
            if (fromeNation != -1)
            {
                RectTransform rectTransform = GetNationRect(fromeNation);
                if (rectTransform != null)
                {
                    OnValueChanged(ScaleSlider.maxValue);
                    ScaleSlider.value = CurMapScale;
                    M_SelectRoleScrollView.Nevigate(rectTransform);
                    object[] Os = new object[] { fromeNation, mapShowArea.MapID };
                    DelayInvoker.DelayInvoke(0.9f, OnDelayOpenMiniMap, Os);
                }
            }
        }

        private void OnDelayOpenMiniMap(object[] args)
        {
            int nationID = (int)args[0];
            int mapID = (int)args[1];

            WorldMapAreaItem worldMapAreaItem = GetWorldMapAreaItem(nationID, mapID);

            if (worldMapAreaItem == null)
            {
                return;
            }

            if (!worldMapAreaItem.isUnlock)
            {
                //Util.ShowMessage($"{worldMapAreaItem.AreaText.text}未解锁");
                //Util.ShowMessage(string.Format(GameConfig.LocalStr["XUnlock"], worldMapAreaItem.AreaText.text));
                Util.ShowMessage(string.Format(LanguageManager.Instance.GetLanguageByKey("XUnlock"), worldMapAreaItem.AreaText.text));
                return;
            }

            OpenMiniMap(mapID);
        }

        private void HideEntityRoot()
        {
            if (WMapEntityRoot == null)
            {
                return;
            }
            WMapEntityRoot.HideItem();
        }

        #endregion

    }
}