using SGF.Module.Framework;
using SGF.UI.Framework;
using StarProject.Game;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Module
{
    public class MapShowEntity
    {
        public MapShowTabType TabType;    // 显示功能分类类型
        public E_EntityType EntityType; // 实体类型
        public int SortPriority;    // 排序优先级
        public long ConfID;  // 配置表ID
        public Vector3 Pos;    // 坐标
        public string Name; // 名字
        public string Title;    // 称谓
        public string IconPath;

        public int Time;    // 次数
    }

    public class MapShowArea
    {
        public MapNationType TabType;    // 显示页签分类 国度下标
        public int SortPriority;    // 排序优先级
        public int MapID;  // 配置表ID
        public string MapName; // 名字
    }
    public class WorldMapModule : BusinessModule
    {
        private const string LOG_TAG = "WorldMapModule";

        public bool IsSecretBossAppear = false;     // 秘境boss是否出现

        public int EctypeStage = -1;    // 单人副本进度

        // 地图显示实体字典
        // {地图ID，{页签类型，List[实体信息]}}
        private Dictionary<int, Dictionary<MapShowTabType, List<MapShowEntity>>> MapShowEntityDic = new();

        // 世界地图显示区域字典
        // {国度ID，地区[]}
        private Dictionary<MapNationType, List<MapShowArea>> MapShowAreaDic = new();

        public Action<SceneJsonData, int> OnChanageMapAction;

        public override void Create(object args = null)
        {
            base.Create(args);
            try
            {
                GlobalEvent.OnPersonSecretBossAppearRet.AddListener(OnPersonSecretBossAppearRet);
                GlobalEvent.OnMiniMapShowEctypeTask.AddListener(OnMiniMapShowEctypeTaskHandler);

                //InitMapShowEntity();
                InitMapArea();
            }
            catch (System.Exception)
            {


            }

        }

        public void GetCurrentMiniMap()
        {
            //获取当前副本id
            //切换当前场景地图
        }

        /// <summary>
        /// 既然是一一对应的Show那肯定是对应打开的Window，至于主界面的功能隶属于主界面
        /// </summary>
        /// <param name="arg"></param>
        protected override void Show(object arg)
        {
            base.Show(arg);

            //UIManager.Instance.OpenWindow(UIDef.MiniMapWindow, null, MainPageCommond.HideBoth);
            if (GameManager.Instance.GetBaseMapId() == 27)
            {
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("CannotOpenMiniMapWindowTips"));
                return;
            }
            UIManager.Instance.OpenWindowAsync(UIDef.MiniMapWindow, null, null, MainPageCommond.HideBoth);
        }

        public override void Release()
        {
            base.Release();

            GlobalEvent.OnPersonSecretBossAppearRet.RemoveListener(OnPersonSecretBossAppearRet);
            GlobalEvent.OnMiniMapShowEctypeTask.RemoveListener(OnMiniMapShowEctypeTaskHandler);

            MapShowEntityDic.Clear();
            MapShowAreaDic.Clear();
        }

        private void OnPersonSecretBossAppearRet(bool isAppear)
        {
            IsSecretBossAppear = isAppear;
        }

        private void OnMiniMapShowEctypeTaskHandler(int arg0)
        {
            EctypeStage = arg0;
        }

        public void ResetCacheData()
        {
            MapShowAreaDic.Clear();
            MapShowEntityDic.Clear();
        }

        #region 地图显示实体

        #region 小地图显示

        private void InitMapShowEntity()
        {
            MapShowEntityDic.Clear();
            foreach (var sceneCfg in LocalDataManager.Instance.M_SceneMapDataData.StaticSceneMapDatas)
            {
                int mapID = sceneCfg.Value.GetID();
                string path = $"MapData/{mapID}/data";
                var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
                if (jsonAsset != null)
                {
                    SceneJsonData sceneJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(jsonAsset.text);
                    // 遍历NPC
                    foreach (var item in sceneJsonData.Npcs)
                    {
                        long confID = item.Value.NpcID;
                        NpcDataCell npcDataCell = LocalDataManager.Instance.GetNPCDataCell(confID);
                        if (npcDataCell == null)
                        {
                            continue;
                        }
                        NpcMapListDataCell npcMapListDataCell = LocalDataManager.Instance.GetNpcMapListDataCell(confID);
                        if (npcMapListDataCell == null)
                        {
                            continue;
                        }

                        MapShowEntity data = new();
                        data.TabType = (MapShowTabType)npcMapListDataCell.GetTabType();
                        data.EntityType = E_EntityType.Npc;
                        data.SortPriority = npcMapListDataCell.GetSortPriority();
                        data.ConfID = confID;
                        data.Pos = item.Value.Position.Convert();
                        data.Name = npcDataCell.Name;
                        data.Title = npcDataCell.Title;
                        data.Time = 0;

                        if (MapShowEntityDic.TryGetValue(mapID, out var mapShowEntityTabDic))
                        {
                            if (mapShowEntityTabDic.TryGetValue(data.TabType, out var mapShowEntities))
                            {
                                long ConfID = npcMapListDataCell.GetNPC();
                                int index = mapShowEntities.FindIndex(t => t.ConfID == ConfID);
                                if (index == -1)
                                {
                                    mapShowEntities.Add(data);
                                }
                                else
                                {
                                    SGF.Debuger.LogWarning($"{LOG_TAG} InitMapShowEntity() mapID={confID},tab={data.TabType},entityType={data.EntityType},ConfID={ConfID} 已经存在");
                                }
                            }
                            else
                            {
                                List<MapShowEntity> list = new() { data };
                                mapShowEntityTabDic.Add(data.TabType, list);
                            }
                        }
                        else
                        {
                            Dictionary<MapShowTabType, List<MapShowEntity>> tabDic = new();
                            List<MapShowEntity> list = new() { data };
                            tabDic.Add(data.TabType, list);
                            MapShowEntityDic.Add(mapID, tabDic);
                        }
                    }
                    // 遍历交互物件
                    foreach (var item in sceneJsonData.Mines)
                    {
                        long confID = item.Value.MineID;
                        InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(confID);
                        if (interactDataCell == null)
                        {
                            continue;
                        }

                        InteractMapListDataCell interactMapListDataCell = LocalDataManager.Instance.GetInteractMapListDataCell(confID);
                        if (interactMapListDataCell == null)
                        {
                            continue;
                        }

                        MapShowEntity data = new();
                        data.TabType = (MapShowTabType)interactMapListDataCell.GetTabType();
                        data.EntityType = E_EntityType.Interact;
                        data.SortPriority = interactMapListDataCell.GetSortPriority();
                        data.ConfID = confID;
                        data.Pos = item.Value.Position.Convert();
                        data.Name = interactDataCell.ModelName;
                        data.Title = interactDataCell.Title;
                        data.Time = 0;

                        if (MapShowEntityDic.TryGetValue(mapID, out var mapShowEntityTabDic))
                        {
                            if (mapShowEntityTabDic.TryGetValue(data.TabType, out var mapShowEntities))
                            {
                                long ConfID = interactMapListDataCell.GetInteract();
                                int index = mapShowEntities.FindIndex(t => t.ConfID == ConfID);
                                if (index == -1)
                                {
                                    mapShowEntities.Add(data);
                                }
                                else
                                {
                                    SGF.Debuger.LogWarning($"{LOG_TAG} InitMapShowEntity() mapID={mapID},tab={data.TabType},entityType={data.EntityType},ConfID={ConfID} 已经存在");
                                }
                            }
                            else
                            {
                                List<MapShowEntity> list = new() { data };
                                mapShowEntityTabDic.Add(data.TabType, list);
                            }
                        }
                        else
                        {
                            Dictionary<MapShowTabType, List<MapShowEntity>> tabDic = new();
                            List<MapShowEntity> list = new() { data };
                            tabDic.Add(data.TabType, list);
                            MapShowEntityDic.Add(mapID, tabDic);
                        }
                    }
                    // 遍历怪物
                    foreach (var item in sceneJsonData.Monsters)
                    {
                        long monsterID = item.Value.MonsterID;
                        MonsterDataCell monsterDataCell = LocalDataManager.Instance.GetMonsterDataCell(monsterID);
                        if (monsterDataCell == null)
                        {
                            continue;
                        }

                        //if (item.Value.MonsterType != 3 /*(int)MapEditor.MonsterType.Boss*/)
                        //{
                        //    continue;
                        //}

                        MonsetrMapListDataCell monsetrMapListDataCell = LocalDataManager.Instance.GetMonsetrMapListDataCell(monsterID);
                        if (monsetrMapListDataCell == null)
                        {
                            continue;
                        }

                        MapShowEntity data = new();
                        data.TabType = MapShowTabType.Monster;
                        data.EntityType = E_EntityType.Monster;
                        data.SortPriority = 1;
                        data.ConfID = mapID;
                        data.Pos = item.Value.Position.Convert();
                        data.Name = monsterDataCell.Name;
                        data.Title = monsterDataCell.Appellation;
                        data.Time = 0;

                        if (MapShowEntityDic.TryGetValue(mapID, out var mapShowEntityTabDic))
                        {
                            if (mapShowEntityTabDic.TryGetValue(data.TabType, out var mapShowEntities))
                            {
                                int ConfID = (int)monsterDataCell.GetID();
                                int index = mapShowEntities.FindIndex(t => t.ConfID == ConfID);
                                if (index == -1)
                                {
                                    mapShowEntities.Add(data);
                                }
                                else
                                {
                                    SGF.Debuger.LogWarning($"{LOG_TAG} InitMapShowEntity() mapID={mapID},tab={data.TabType},entityType={data.EntityType},ConfID={ConfID} 已经存在");
                                }
                            }
                            else
                            {
                                List<MapShowEntity> list = new() { data };
                                mapShowEntityTabDic.Add(data.TabType, list);
                            }
                        }
                        else
                        {
                            Dictionary<MapShowTabType, List<MapShowEntity>> tabDic = new();
                            List<MapShowEntity> list = new() { data };
                            tabDic.Add(data.TabType, list);
                            MapShowEntityDic.Add(mapID, tabDic);
                        }
                    }

                    // 补全
                    {
                        if (!MapShowEntityDic.ContainsKey(mapID))
                        {
                            Dictionary<MapShowTabType, List<MapShowEntity>> tabDic = new();
                            MapShowEntityDic.Add(mapID, tabDic);
                        }
                        if (MapShowEntityDic.TryGetValue(mapID, out var mapShowEntityTabDic))
                        {
                            // 玩法
                            if (!mapShowEntityTabDic.ContainsKey(MapShowTabType.GamePlay))
                            {
                                List<MapShowEntity> list = new();
                                mapShowEntityTabDic.Add(MapShowTabType.GamePlay, list);
                            }
                            // 功能
                            if (!mapShowEntityTabDic.ContainsKey(MapShowTabType.Function))
                            {
                                List<MapShowEntity> list = new();
                                mapShowEntityTabDic.Add(MapShowTabType.Function, list);
                            }
                            // 传送
                            if (!mapShowEntityTabDic.ContainsKey(MapShowTabType.Transfer))
                            {
                                List<MapShowEntity> list = new();
                                mapShowEntityTabDic.Add(MapShowTabType.Transfer, list);
                            }
                            // 怪物
                            if (!mapShowEntityTabDic.ContainsKey(MapShowTabType.Monster))
                            {
                                List<MapShowEntity> list = new();
                                mapShowEntityTabDic.Add(MapShowTabType.Monster, list);
                            }
                        }
                    }
                }
            }
        }

        private Dictionary<MapShowTabType, List<MapShowEntity>> GetLoadCurMapShowList(int mapID)
        {
            string path = $"MapData/{mapID}/data";
            var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
            if (jsonAsset != null)
            {
                SceneJsonData sceneJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(jsonAsset.text);
                // 遍历NPC
                foreach (var item in sceneJsonData.Npcs)
                {
                    long confID = item.Value.NpcID;
                    NpcDataCell npcDataCell = LocalDataManager.Instance.GetNPCDataCell(confID);
                    if (npcDataCell == null)
                    {
                        continue;
                    }
                    NpcMapListDataCell npcMapListDataCell = LocalDataManager.Instance.GetNpcMapListDataCell(confID);
                    if (npcMapListDataCell == null)
                    {
                        continue;
                    }

                    MapShowEntity data = new();
                    data.TabType = (MapShowTabType)npcMapListDataCell.GetTabType();
                    data.EntityType = E_EntityType.Npc;
                    data.SortPriority = npcMapListDataCell.GetSortPriority();
                    data.ConfID = confID;
                    data.Pos = item.Value.Position.Convert();
                    data.Name = npcDataCell.Name;
                    data.Title = npcDataCell.Title;
                    data.Time = 0;

                    if (MapShowEntityDic.TryGetValue(mapID, out var mapShowEntityTabDic))
                    {
                        if (mapShowEntityTabDic.TryGetValue(data.TabType, out var mapShowEntities))
                        {
                            long ConfID = npcMapListDataCell.GetNPC();
                            int index = mapShowEntities.FindIndex(t => t.ConfID == ConfID);
                            if (index == -1)
                            {
                                mapShowEntities.Add(data);
                            }
                            else
                            {
                                SGF.Debuger.LogWarning($"{LOG_TAG} InitMapShowEntity() mapID={confID},tab={data.TabType},entityType={data.EntityType},ConfID={ConfID} 已经存在");
                            }
                        }
                        else
                        {
                            List<MapShowEntity> list = new() { data };
                            mapShowEntityTabDic.Add(data.TabType, list);
                        }
                    }
                    else
                    {
                        Dictionary<MapShowTabType, List<MapShowEntity>> tabDic = new();
                        List<MapShowEntity> list = new() { data };
                        tabDic.Add(data.TabType, list);
                        MapShowEntityDic.Add(mapID, tabDic);
                    }
                }
                // 遍历交互物件
                foreach (var item in sceneJsonData.Mines)
                {
                    long confID = item.Value.MineID;
                    InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(confID);
                    if (interactDataCell == null)
                    {
                        continue;
                    }

                    InteractMapListDataCell interactMapListDataCell = LocalDataManager.Instance.GetInteractMapListDataCell(confID);
                    if (interactMapListDataCell == null)
                    {
                        continue;
                    }

                    MapShowEntity data = new();
                    data.TabType = (MapShowTabType)interactMapListDataCell.GetTabType();
                    data.EntityType = E_EntityType.Interact;
                    data.SortPriority = interactMapListDataCell.GetSortPriority();
                    data.ConfID = confID;
                    data.Pos = item.Value.Position.Convert();
                    data.Name = interactDataCell.ModelName;
                    data.Title = interactDataCell.Title;
                    data.Time = 0;

                    if (MapShowEntityDic.TryGetValue(mapID, out var mapShowEntityTabDic))
                    {
                        if (mapShowEntityTabDic.TryGetValue(data.TabType, out var mapShowEntities))
                        {
                            long ConfID = interactMapListDataCell.GetInteract();
                            int index = mapShowEntities.FindIndex(t => t.ConfID == ConfID);
                            if (index == -1)
                            {
                                mapShowEntities.Add(data);
                            }
                            else
                            {
                                SGF.Debuger.LogWarning($"{LOG_TAG} InitMapShowEntity() mapID={mapID},tab={data.TabType},entityType={data.EntityType},ConfID={ConfID} 已经存在");
                            }
                        }
                        else
                        {
                            List<MapShowEntity> list = new() { data };
                            mapShowEntityTabDic.Add(data.TabType, list);
                        }
                    }
                    else
                    {
                        Dictionary<MapShowTabType, List<MapShowEntity>> tabDic = new();
                        List<MapShowEntity> list = new() { data };
                        tabDic.Add(data.TabType, list);
                        MapShowEntityDic.Add(mapID, tabDic);
                    }
                }
                // 遍历怪物
                foreach (var item in sceneJsonData.Monsters)
                {
                    long monsterID = item.Value.MonsterID;
                    MonsterDataCell monsterDataCell = LocalDataManager.Instance.GetMonsterDataCell(monsterID);
                    if (monsterDataCell == null)
                    {
                        continue;
                    }

                    //if (item.Value.MonsterType != 3 /*(int)MapEditor.MonsterType.Boss*/)
                    //{
                    //    continue;
                    //}

                    MonsetrMapListDataCell monsetrMapListDataCell = LocalDataManager.Instance.GetMonsetrMapListDataCell(monsterID);
                    if (monsetrMapListDataCell == null)
                    {
                        continue;
                    }

                    MapShowEntity data = new();
                    data.TabType = MapShowTabType.Monster;
                    data.EntityType = E_EntityType.Monster;
                    data.SortPriority = 1;
                    data.ConfID = mapID;
                    data.Pos = item.Value.Position.Convert();
                    data.Name = monsterDataCell.Name;
                    data.Title = monsterDataCell.Appellation;
                    data.Time = 0;

                    if (MapShowEntityDic.TryGetValue(mapID, out var mapShowEntityTabDic))
                    {
                        if (mapShowEntityTabDic.TryGetValue(data.TabType, out var mapShowEntities))
                        {
                            int ConfID = (int)monsterDataCell.GetID();
                            int index = mapShowEntities.FindIndex(t => t.ConfID == ConfID);
                            if (index == -1)
                            {
                                mapShowEntities.Add(data);
                            }
                            else
                            {
                                SGF.Debuger.LogWarning($"{LOG_TAG} InitMapShowEntity() mapID={mapID},tab={data.TabType},entityType={data.EntityType},ConfID={ConfID} 已经存在");
                            }
                        }
                        else
                        {
                            List<MapShowEntity> list = new() { data };
                            mapShowEntityTabDic.Add(data.TabType, list);
                        }
                    }
                    else
                    {
                        Dictionary<MapShowTabType, List<MapShowEntity>> tabDic = new();
                        List<MapShowEntity> list = new() { data };
                        tabDic.Add(data.TabType, list);
                        MapShowEntityDic.Add(mapID, tabDic);
                    }
                }

                // 补全
                {
                    if (!MapShowEntityDic.ContainsKey(mapID))
                    {
                        Dictionary<MapShowTabType, List<MapShowEntity>> tabDic = new();
                        MapShowEntityDic.Add(mapID, tabDic);
                    }
                    if (MapShowEntityDic.TryGetValue(mapID, out var mapShowEntityTabDic))
                    {
                        // 玩法
                        if (!mapShowEntityTabDic.ContainsKey(MapShowTabType.GamePlay))
                        {
                            List<MapShowEntity> list = new();
                            mapShowEntityTabDic.Add(MapShowTabType.GamePlay, list);
                        }
                        // 功能
                        if (!mapShowEntityTabDic.ContainsKey(MapShowTabType.Function))
                        {
                            List<MapShowEntity> list = new();
                            mapShowEntityTabDic.Add(MapShowTabType.Function, list);
                        }
                        // 传送
                        if (!mapShowEntityTabDic.ContainsKey(MapShowTabType.Transfer))
                        {
                            List<MapShowEntity> list = new();
                            mapShowEntityTabDic.Add(MapShowTabType.Transfer, list);
                        }
                        // 怪物
                        if (!mapShowEntityTabDic.ContainsKey(MapShowTabType.Monster))
                        {
                            List<MapShowEntity> list = new();
                            mapShowEntityTabDic.Add(MapShowTabType.Monster, list);
                        }
                    }
                }
            }
            Dictionary<MapShowTabType, List<MapShowEntity>> dic = new();
            if (MapShowEntityDic.TryGetValue(mapID, out dic))
            {

            }
            return dic;
        }

        public Dictionary<MapShowTabType, List<MapShowEntity>> GetCurMapShowList(int mapID)
        {
            Dictionary<MapShowTabType, List<MapShowEntity>> dic = new();
            if (MapShowEntityDic.TryGetValue(mapID, out dic))
            {
                return dic;
            }
            return GetLoadCurMapShowList(mapID);
        }
        #endregion

        #region 世界地图国度地区

        private void InitMapArea()
        {
            MapShowAreaDic.Clear();

            foreach (MapNationType type in Enum.GetValues(typeof(MapNationType)))
            {
                var list = LocalDataManager.Instance.GetWorldMapListDataCellList((int)type);
                for (int i = 0; i < list.Count; i++)
                {
                    var child = list[i];
                    MapShowArea data = new();
                    data.TabType = type;    // 显示页签分类 国度下标
                    data.SortPriority = child.GetSortPriority();    // 排序优先级
                    data.MapID = child.GetMapID();  // 配置表ID
                    string mapName = $"{data.MapID}";
                    var mapConf = LocalDataManager.Instance.GetMapCfgData(data.MapID);
                    if (mapConf != null)
                    {
                        mapName = mapConf.MapName; // 名字
                    }
                    data.MapName = mapName; // 名字
                    if (MapShowAreaDic.TryGetValue(type, out var item))
                    {
                        item.Add(data);
                    }
                    else
                    {
                        List<MapShowArea> mapShowAreas = new() { data };
                        MapShowAreaDic.Add(type, mapShowAreas);
                    }
                    if (i + 1 == list.Count && list.Count > 1)
                    {
                        // 排序
                        MapShowAreaDic[type].Sort(SortNationPriority);
                    }
                }
            }
        }

        private int SortNationPriority(MapShowArea a, MapShowArea b)
        {
            if (a.SortPriority > b.SortPriority)
            {
                return -1;
            }
            return 1;
        }

        public Dictionary<MapNationType, List<MapShowArea>> GetMapShowAreaDic()
        {
            return MapShowAreaDic;
        }

        #endregion

        #endregion


    }
}
