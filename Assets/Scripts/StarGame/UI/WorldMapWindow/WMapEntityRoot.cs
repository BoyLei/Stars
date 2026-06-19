using SGF.Module.Framework;
using SGF.UI.Framework;
using StarProject.Game;
using StarProject.Module;
using StarProject.Service.LocalData;
using StarProject.Service.User;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Linq;
using Task;
using UnityEngine;
using UnityEngine.UI;

public class WMapEntityRoot : MonoBehaviour
{
    // 常用、玩法、功能、任务、怪物
    private Transform Content;
    private VerticalLayoutGroup VerticalLayoutGroup;
    private ContentSizeFitter ContentSizeFitter;

    private GameObject WMapEntityTabPrefab;
    private GameObject WMapEntityPrefab;

    private int mapID = -1;
    private List<WMapEntityTabItem> wMapEntityTabItems = new();
    private WMapEntityTabItem curCheckTabItem = null;

    private Action<int, MapShowEntity> m_ClickMapShowEntity = null;  // 小地图点击回调
    private Action<MapShowArea> m_ClickMapShowArea = null;      // 世界地图点击回调

    private WorldMapModule WorldMapModule;

    private void Awake()
    {
        Content = transform.Find("Viewport/Content").transform;
        VerticalLayoutGroup = transform.Find("Viewport/Content").GetComponent<VerticalLayoutGroup>();
        ContentSizeFitter = transform.Find("Viewport/Content").GetComponent<ContentSizeFitter>();

        WMapEntityTabPrefab = transform.Find("WMapEntityTabItem").gameObject;
        WMapEntityPrefab = transform.Find("WMapEntityItem").gameObject;


        WorldMapModule = ModuleManager.Instance.GetModule(ModuleDef.Name.WorldMapModule) as WorldMapModule;
    }

    private void Reset()
    {
        mapID = -1;
        m_ClickMapShowEntity = null;
        m_ClickMapShowArea = null;
    }

    public void Release()
    {
        Reset();
        ReleaseWMapEntityTabItems();
    }

    public void HideItem()
    {
        ClearWMapEntityTabItems();
    }


    #region 小地图列表

    public void SetMapShowData(int mapid, Action<int, MapShowEntity> click)
    {
        gameObject.SetActive(true);
        mapID = mapid;
        m_ClickMapShowEntity = click;
        GetMapJsonData(mapID, InitMapShowData);
    }

    private void InitMapShowData(SceneJsonData sceneJsonData)
    {
        // 获取当前地图的实体显示列表
        Dictionary<MapShowTabType, List<MapShowEntity>> dic = WorldMapModule.GetCurMapShowList(mapID);
        // 获取当前地图常用的信息
        {
            List<MapShowEntity> commonList = GetCurMapCommonSort();
            if (dic.ContainsKey(MapShowTabType.Common))
            {
                dic[MapShowTabType.Common] = commonList;
            }
            else
            {
                dic.Add(MapShowTabType.Common, commonList);
            }
        }
        // 添加当前地图的任务NPC和交互物件实体
        {
            List<MapShowEntity> curMapTasks = GetCurMapTask(sceneJsonData);

            if (dic.ContainsKey(MapShowTabType.Task))
            {
                dic[MapShowTabType.Task] = curMapTasks;
            }
            else
            {
                dic.Add(MapShowTabType.Task, curMapTasks);
            }
        }
        // 根据Key从小到大排序
        Dictionary<MapShowTabType, List<MapShowEntity>> dic1Asc = dic.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
        // 根据Key从大到小排序
        //Dictionary<MapShowTabType, List<MapShowEntity>> dic1desc = dic.OrderByDescending(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
        // 遍历创建添加吧
        int index = 0;
        foreach (var item in dic1Asc)
        {
            WMapEntityTabItem childRoot = GetWMapEntityTabItem(index);
            // 根据优先级从大到小排序
            item.Value.Sort(SortPriority);
            childRoot.InitMiniMap(item.Key, WMapEntityPrefab, ClickTab, ClickEntityItem);
            childRoot.SetMapShowEntitys(item.Value);
            index++;
        }
        if (curCheckTabItem == null)
        {
            curCheckTabItem = wMapEntityTabItems[0];
            curCheckTabItem.CheckTab(true);
        }
        VerticalLayoutGroup.CalculateLayoutInputVertical();
        ContentSizeFitter.SetLayoutVertical();
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.transform.GetComponent<RectTransform>());
    }

    private List<MapShowEntity> GetCurMapCommonSort()
    {
        List<MapShowEntity> commonList = GetCurMapCommon();
        commonList.Sort(SortTime);
        if (commonList.Count < 5)
        {
            return commonList;
        }
        else
        {
            List<MapShowEntity> sortList = new();
            for (int i = 0; i < 5; i++)
            {
                sortList.Add(commonList[i]);
            }
            return sortList;
        }
    }

    private List<MapShowEntity> GetCurMapCommon()
    {
        List<MapShowEntity> commonList = new();
        string key = $"{GameConfig.MAP_COMMON_MAP_CACHE}{UserManager.Instance.MainUserData.playerRoleId}_{mapID}";
        bool isExists = SaveManager.Instance.KeyExists(key, GameConfig.MAP_COMMON_CACHE);
        if (isExists)
        {
            commonList = SaveManager.Instance.Load<List<MapShowEntity>>(key, GameConfig.MAP_COMMON_CACHE);
            if (commonList == null)
            {
                commonList = new List<MapShowEntity>();
            }
        }
        return commonList;
    }

    // 获得当前地图未完成的任务NPC和交互物件实体
    private List<MapShowEntity> GetCurMapTask(SceneJsonData sceneJsonData)
    {
        List<MapShowEntity> list = new();
        //当前运行的任务数据
        var Tasks = TaskHelper.GetAllTask();
        if (Tasks != null && Tasks.Count > 0)
        {
            foreach (var item in Tasks)
            {
                bool delete = item.TaskState == ProtoMsg.TaskStateEnum.TaskStateBeDeleted || item.TaskState == ProtoMsg.TaskStateEnum.TaskStateGetReward;
                if (delete)
                {
                    continue;
                }

                if (item.TaskTarget != null && item.TaskTarget.Count > 0)
                {
                    int taskType = item.TaskType;
                    ProtoMsg.TaskStateEnum TaskState = item.TaskState;
                    string iconPath = TaskManager.GetUITaskIconNameByType((TaskClassifyType)taskType, TaskState);
                    string taskName = item.TaskTitle;

                    foreach (var target in item.TaskTarget)
                    {
                        if (target.FindPath.FindPathType == Task.E_FindPath.None)
                        {
                            continue;
                        }

                        if (mapID != target.MapID)
                        {
                            continue;
                        }
                        long confID = target.FindPath.ID;
                        bool isShow = false;

                        MapShowEntity data = new();
                        data.TabType = MapShowTabType.Task;
                        data.EntityType = E_EntityType.None;
                        data.SortPriority = 1;
                        data.ConfID = confID;
                        data.Pos = Vector3.zero;
                        data.Name = "";
                        data.Title = "";
                        data.IconPath = iconPath;
                        data.Time = 0;


                        switch (target.FindPath.FindPathType)
                        {
                            case Task.E_FindPath.NPC:
                                {
                                    var npc = sceneJsonData.GetNPCJsonData(confID);
                                    NpcDataCell npcDataCell = LocalDataManager.Instance.GetNPCDataCell(confID);
                                    if (npc != null && npcDataCell != null)
                                    {
                                        data.EntityType = E_EntityType.Npc;
                                        data.Pos = npc.Position.Convert();
                                        //data.Name = npcDataCell.Name;
                                        //data.Title = npcDataCell.Title;
                                        data.Name = taskName;
                                        isShow = true;
                                    }
                                }
                                break;
                            case Task.E_FindPath.InterAction:
                                {
                                    var mine = sceneJsonData.GetMineJsonData(confID);
                                    InteractDataCell interactDataCell = LocalDataManager.Instance.GetInteractDataCell(confID);
                                    if (mine != null && interactDataCell != null)
                                    {
                                        data.EntityType = E_EntityType.Interact;
                                        data.Pos = mine.Position.Convert();
                                        //data.Name = interactDataCell.ModelName;
                                        //data.Title = interactDataCell.Title;
                                        data.Name = taskName;

                                        isShow = true;
                                    }
                                }
                                break;
                            case Task.E_FindPath.Area:
                                {
                                    //if (GameMap.sceneJsonData.Areas.ContainsKey(target.FindPath.ID))
                                    //{
                                    //    position = GameMap.sceneJsonData.Areas[target.FindPath.ID].Position.Convert();
                                    //}
                                }
                                break;
                            case Task.E_FindPath.Spawer:
                                {
                                    //if (GameMap.sceneJsonData.Spawners.ContainsKey(target.FindPath.ID))
                                    //{
                                    //    position = GameMap.sceneJsonData.Spawners[target.FindPath.ID].Position.Convert();
                                    //}
                                }
                                break;
                        }

                        if (isShow)
                        {
                            list.Add(data);
                        }
                    }
                }
            }
        }
        return list;
    }

    private void GetMapJsonData(int mapID, Action<SceneJsonData> cb)
    {
        string path = $"MapData/{mapID}/data";
        var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
        if (jsonAsset != null)
        {
            var sceneJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(jsonAsset.text);
            cb?.Invoke(sceneJsonData);
        }
        else
        {
            cb?.Invoke(null);
        }
    }

    private int SortPriority(MapShowEntity a, MapShowEntity b)
    {
        if (a.SortPriority > b.SortPriority)
        {
            return -1;
        }
        return 1;
    }

    private int SortTime(MapShowEntity a, MapShowEntity b)
    {
        if (a.Time > b.Time)
        {
            return -1;
        }
        return 1;
    }

    private void ClickEntityItem(MapShowEntity mapShowEntity)
    {
        if (mapShowEntity == null)
        {
            return;
        }
        // 缓存该常用的显示实体信息
        if (mapShowEntity.TabType != MapShowTabType.None && mapShowEntity.TabType != MapShowTabType.Task)
        {
            List<MapShowEntity> commonList = GetCurMapCommon();
            bool isFind = false;
            foreach (var item in commonList)
            {
                if (item.TabType == mapShowEntity.TabType && item.EntityType == mapShowEntity.EntityType && item.ConfID == mapShowEntity.ConfID)
                {
                    item.Time++;
                    isFind = true;
                    break;
                }
            }
            if (!isFind)
            {
                mapShowEntity.Time++;
                commonList.Add(mapShowEntity);
            }
            string key = $"{GameConfig.MAP_COMMON_MAP_CACHE}{UserManager.Instance.MainUserData.playerRoleId}_{mapID}";
            SaveManager.Instance.Save(key, commonList, GameConfig.MAP_COMMON_CACHE);
        }

        m_ClickMapShowEntity?.Invoke(mapID, mapShowEntity);
    }

    #endregion

    #region 世界地图列表

    public void SetMapNationData(Action<MapShowArea> click, int defualtOpenFromeNation = 1)
    {
        m_ClickMapShowArea = click;
        Dictionary<MapNationType, List<MapShowArea>> MapShowAreaDic = WorldMapModule.GetMapShowAreaDic();
        int index = 0;
        foreach (var item in MapShowAreaDic)
        {
            if (GetHaveUnlockMap(item.Value))
            {
                WMapEntityTabItem childRoot = GetWMapEntityTabItem(index);
                childRoot.InitWorldMap(item.Key, WMapEntityPrefab, ClickTab, ClickEntityItem);
                childRoot.SetMapShowEntitys(item.Value);
                childRoot.CheckTab(defualtOpenFromeNation == (int)item.Key);
                if (defualtOpenFromeNation == (int)item.Key)
                {
                    curCheckTabItem = childRoot;
                }
                index++;
            }
        }

        if (curCheckTabItem == null)
        {
            curCheckTabItem = wMapEntityTabItems[0];
            curCheckTabItem.CheckTab(true);
        }
        VerticalLayoutGroup.CalculateLayoutInputVertical();
        ContentSizeFitter.SetLayoutVertical();
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.transform.GetComponent<RectTransform>());
    }

    private bool GetHaveUnlockMap(List<MapShowArea> maps)
    {
        foreach (var item in maps)
        {
            bool isOpen = GameManager.Instance.CheckMapIsOpen(item.MapID);
            if (isOpen)
            {
                return isOpen;
            }
        }
        return false;
    }

    private void ClickEntityItem(MapShowArea mapShowArea)
    {
        if (mapShowArea == null)
        {
            return;
        }
        // 跳转小地图信息
        m_ClickMapShowArea?.Invoke(mapShowArea);
    }

    #endregion

    #region 页签

    private WMapEntityTabItem GetWMapEntityTabItem(int index)
    {
        WMapEntityTabItem item = GetIdleWMapEntityTabItemByIndex(index);
        if (item == null)
        {
            item = CreateWMapEntityTabItem();
            wMapEntityTabItems.Add(item);
        }

        item.gameObject.SetActive(true);
        return item;
    }

    private WMapEntityTabItem GetIdleWMapEntityTabItemByIndex(int index)
    {
        if (wMapEntityTabItems.Count > index)
        {
            return wMapEntityTabItems[index];
        }

        return null;
    }

    private WMapEntityTabItem CreateWMapEntityTabItem()
    {
        var gob = GameObject.Instantiate<GameObject>(WMapEntityTabPrefab);
        gob.transform.SetParent(Content);
        gob.transform.localPosition = Vector3.zero;
        gob.transform.localScale = Vector3.one;
        WMapEntityTabItem item = gob.GetComponent<WMapEntityTabItem>();
        return item;
    }

    private void ClearWMapEntityTabItems()
    {
        for (int i = 0; i < wMapEntityTabItems.Count; i++)
        {
            var child = wMapEntityTabItems[i];
            if (child != null)
            {
                child.HideItem();
            }
        }
        curCheckTabItem = null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.transform.GetComponent<RectTransform>());
    }

    private void ReleaseWMapEntityTabItems()
    {
        for (int i = 0; i < wMapEntityTabItems.Count; i++)
        {
            var child = wMapEntityTabItems[i];
            if (child != null)
            {
                child.Release();
            }
        }
    }

    private void ClickTab(WMapEntityTabItem wMapEntityTabItem)
    {
        // �����ҳǩ
        if (wMapEntityTabItem == null)
        {
            return;
        }
        //if (curCheckTabItem != null)
        //{
        //    curCheckTabItem.CheckTab(false);
        //}
        if (curCheckTabItem != wMapEntityTabItem)
        {
            curCheckTabItem = wMapEntityTabItem;
            //curCheckTabItem.CheckTab(true);
        }
        else
        {
            curCheckTabItem = null;
        }
    }

    #endregion

}
