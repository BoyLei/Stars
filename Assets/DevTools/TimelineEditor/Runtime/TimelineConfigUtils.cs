///--------------------------------------------------------------------
/// 文件名   :   TimelineConfigUtils.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/04/17 09:55:09
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Excel;
using Sirenix.OdinInspector;
using StarProjectDef;
using UnityEngine;
using OfficeOpenXml;

public static class TimelineConfigUtils
{
    /// <summary>
    /// 摄像机路径
    /// </summary>
    public static string Tl_CameraPath =
        "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/TL_Camera.prefab";

    public static string PlayerPath = "Assets/Res/Roles/World/Character/QiangP_F/cm/QiangP_F_Model.prefab";

    public static string PlayerIdlePath = "Assets/Res/Animation/Roles/Character/QiangP_F/cm/Idle_01.anim";

    public static GameObject PlayerTemplete;

    public static GameObject Tl_CameraTemplete;

    public static ValueDropdownList<string> _vcCameraTemplete = new ValueDropdownList<string>();

    public static ValueDropdownList<int> _maplist = new ValueDropdownList<int>();

    public static Dictionary<int, SceneTlCfg> _SceneList = new Dictionary<int, SceneTlCfg>();

    public static AvatarData _avatarData;

    public static AnimationClip mIdleClip;

    public static Dictionary<int, TimelineExcelInfo> TimelineExcels = new Dictionary<int, TimelineExcelInfo>();

    public static int ExcelMaxIndex;

    public static TimelineConfigs Configs;

    public static ValueDropdownList<string> _groupTypes = new ValueDropdownList<string>();
    
    public  static  ValueDropdownList<string> _groupNames = new ValueDropdownList<string>();
    
    public  static  ValueDropdownList<int> _avatars = new ValueDropdownList<int>();

    
    //白名单
    public static List<string> _whiteList = new List<string>()
    {
        "CameraRoot",
        "Directional Light",
        "WwiseGlobal"
    };


    public static void ClearScene()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

        if (scene != null)
        {
            var objects = scene.GetRootGameObjects();
            foreach (var obj in objects)
            {
                if (_whiteList.Contains(obj.name))
                {
                    continue;
                }

                GameObject.DestroyImmediate(obj);
            }
        }
    }

    public static DataSet ReadBook(string path)
    {
        DataSet dataSet = null;
        try
        {
            FileStream mStream = File.Open(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path),
                FileMode.Open, FileAccess.Read, FileShare.Read);

            byte[] bytes = new byte[mStream.Length];

            mStream.Read(bytes);


            Debug.Log($"mStream{Convert.ToString(bytes)}");
            IExcelDataReader mExcelReader = ExcelReaderFactory.CreateOpenXmlReader(mStream);
            dataSet = mExcelReader.AsDataSet();
        }
        catch (Exception e)
        {
            Debug.LogError("文件读取失败，" + e.ToString());
        }

        return dataSet;
    }

    public static void ReadExcel()
    {
        TimelineExcels.Clear();
        DataSet data = MapEditor.EditorConfigUtils.ReadBook(Configs.TimelineExcelPath);
        if (data == null)
        {
            return;
        }

        DataTable sheet = data.Tables["TimelineConfig"];
        var rowCount = sheet.Rows.Count;
        for (int i = 6; i < rowCount; i++)
        {
            DataRow dataRow = sheet.Rows[i];
            if (dataRow.IsNull(0))
            {
                continue;
            }

            TimelineExcelInfo effectData = new TimelineExcelInfo();
            effectData.ReadExcel(dataRow);
            if (effectData.ID > ExcelMaxIndex)
            {
                ExcelMaxIndex = effectData.ID;
            }

            TimelineExcels.Add(effectData.ID, effectData);
        }
    }

    public static void WriteExcel()
    {
        string path = Configs.TimelineExcelPath;
        FileInfo newFile = new FileInfo(path);
        if (newFile.Exists)
        {
            //创建一个新的excel文件
            newFile.Delete();
            newFile = new FileInfo(path);
        }

        //通过ExcelPackage打开文件
        using (ExcelPackage package = new ExcelPackage(newFile))
        {
            //在excel空文件添加新sheet
            ExcelWorksheet config = package.Workbook.Worksheets.Add("config");

            config.Cells[1, 1].Value = "server";
            config.Cells[2, 1].Value = "client";
            config.Cells[1, 2].Value = "TimelineConfig";


            //在excel空文件添加新sheet
            ExcelWorksheet timelineconfig = package.Workbook.Worksheets.Add("TimelineConfig");
            int index = 1;
            timelineconfig.Cells[1, index].Value = "ID";
            timelineconfig.Cells[2, index].Value = "ID";
            timelineconfig.Cells[3, index++].Value = "int";

            timelineconfig.Cells[1, index].Value = "描述";
            timelineconfig.Cells[2, index].Value = "desc";
            timelineconfig.Cells[3, index++].Value = "";

            timelineconfig.Cells[1, index].Value = "Timeline路径";
            timelineconfig.Cells[2, index].Value = "TimelinePath";
            timelineconfig.Cells[3, index++].Value = "string";


            timelineconfig.Cells[1, index].Value = "类型";
            timelineconfig.Cells[2, index].Value = "TimelineType";
            timelineconfig.Cells[3, index++].Value = "int";

            timelineconfig.Cells[1, index].Value = "结束隐藏Timeline";
            timelineconfig.Cells[2, index].Value = "FinishHide";
            timelineconfig.Cells[3, index++].Value = "bool";

            timelineconfig.Cells[1, index].Value = "播放时隐藏UI";
            timelineconfig.Cells[2, index].Value = "HideUI";
            timelineconfig.Cells[3, index++].Value = "bool";

            timelineconfig.Cells[1, index].Value = "播放时隐藏主角";
            timelineconfig.Cells[2, index].Value = "HideRole";
            timelineconfig.Cells[3, index++].Value = "bool";

            timelineconfig.Cells[1, index].Value = "播放时隐藏伙伴";
            timelineconfig.Cells[2, index].Value = "HidePartner";
            timelineconfig.Cells[3, index++].Value = "bool";

            timelineconfig.Cells[1, index].Value = "是否使用主角模型";
            timelineconfig.Cells[2, index].Value = "UseRoleModel";
            timelineconfig.Cells[3, index++].Value = "bool";

            timelineconfig.Cells[1, index].Value = "是否可移动";
            timelineconfig.Cells[2, index].Value = "IsCanMove";
            timelineconfig.Cells[3, index++].Value = "bool";

            timelineconfig.Cells[1, index].Value = "是否隐藏地图NPC";
            timelineconfig.Cells[2, index].Value = "HideMapNPC";
            timelineconfig.Cells[3, index++].Value = "bool";

            timelineconfig.Cells[1, index].Value = "是否隐藏其他玩家";
            timelineconfig.Cells[2, index].Value = "HideOtherPlayer";
            timelineconfig.Cells[3, index++].Value = "bool";

            if (TimelineExcels != null && TimelineExcels.Count > 0)
            {
                int row = 6;
                foreach (var item in TimelineExcels)
                {
                    item.Value.ExportExcel(row, timelineconfig.Cells);
                    row++;
                }
            }

            timelineconfig.Cells.AutoFitColumns();
            //保存excel
            package.Save();
        }
    }

    public static void AddTimelineExcel(TimelineExcelInfo info)
    {
        ExcelMaxIndex++;
        info.ID = ExcelMaxIndex;
        TimelineExcels.Add(ExcelMaxIndex, info);
        WriteExcel();
    }

    public static void Init()
    {
#if UNITY_EDITOR
        Configs = UnityEditor.AssetDatabase.LoadAssetAtPath<TimelineConfigs>(
            "Assets/DevTools/TimelineEditor/TimelineConfigs.asset");
        ReadExcel();
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (scene.path ==
            "Assets/DevTools/TimelineEditor/timeline.unity")
        {
            ClearScene();
        }
        else
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/DevTools/TimelineEditor/timeline.unity");
        }

        if (Tl_CameraTemplete == null)
        {
            Tl_CameraTemplete = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(Tl_CameraPath);
        }

        if (PlayerTemplete == null)
        {
            PlayerTemplete = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
        }

        if (mIdleClip == null)
        {
            mIdleClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(PlayerIdlePath);
        }

        _vcCameraTemplete.Clear();
        _vcCameraTemplete.Add("游戏位置向前推",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_01.anim");
        _vcCameraTemplete.Add("向后拉到游戏位置",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_02.anim");
        _vcCameraTemplete.Add("游戏位置拉向俯视角度向下推",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_03.anim");
        _vcCameraTemplete.Add("近景对话展示一位角色对话",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_04.anim");
        _vcCameraTemplete.Add("近景对话展示两位角色对话",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_05.anim");
        _vcCameraTemplete.Add("近景对话展示多位角色对话",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_06.anim");
        _vcCameraTemplete.Add("中景推近",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_07.anim");
        _vcCameraTemplete.Add("中景拉远",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_08.anim");
        _vcCameraTemplete.Add("第一视角上下查看",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_09.anim");
        _vcCameraTemplete.Add("第一视角左右查看",
            "Assets/ArtWorkSpace/TimelineGroups/GroupDisplay/Common/VCamera/Clips/Pb_general_shot_10.anim");

        
        InitGroupType();


        InitGroupName();

        
        var serverJsonDir = PlayerPrefs.GetString("serverJsonDir", Application.dataPath);
        //加载外部配置
        MapEditor.EditorConfigUtils.Init();
        
        
        
        
        _SceneList.Clear();
        _maplist.Clear();

        var mapli = MapEditor.EditorConfigUtils.SceneCfgs.Scenes;
        {
            foreach (var mapmd in mapli)
            {
                _maplist.Add($"{mapmd.Key} {mapmd.Value.SceneName}", mapmd.Key);
                var scenecfg = new SceneTlCfg(mapmd.Value);
                scenecfg.Init(serverJsonDir);
                _SceneList.Add(mapmd.Key, scenecfg);
            }
        }

        var maplevel = MapEditor.EditorConfigUtils.LevelCfgs.Scenes;
        {
            foreach (var mapmd in maplevel)
            {
                _maplist.Add($"{mapmd.Key} {mapmd.Value.SceneName}", mapmd.Key);
                var scenecfg = new SceneTlCfg(mapmd.Value);
                scenecfg.Init(serverJsonDir);
                _SceneList.Add(mapmd.Key, scenecfg);
            }
        }
        //_avatarData = MapEditor.EditorConfigUtils.ReadJson<AvatarData>("Assets/Res/Config/Excel/Avatar.json");
        _avatarData = MapEditor.EditorConfigUtils.ReadBytes<AvatarData>("Assets/Res/Config/ExcelBytes/Avatar.bytes");
        _avatars.Clear();
        if (_avatarData != null && _avatarData.StaticAvatarDatas != null && _avatarData.StaticAvatarDatas.Count > 0)
        {
            foreach (var avatar in _avatarData.StaticAvatarDatas)
            {
                _avatars.Add(avatar.Value.Desc,avatar.Value.GetModelId());
            }
        }
#endif
    }

    public static void InitGroupType()
    {
        _groupTypes.Clear();
        if (Configs.TimelineDicGroupTypeDic != null && Configs.TimelineDicGroupTypeDic.Count>0)
        {
            foreach (var dir in Configs.TimelineDicGroupTypeDic)
            {
                _groupTypes.Add($"{dir.Desc}({System.IO.Path.GetFileNameWithoutExtension(dir.FolderPath)})",dir.FolderPath);
            }
        }
    }

    public static void InitGroupName()
    {
        _groupNames.Clear();
        if (Configs.TimelineDicNameDic != null && Configs.TimelineDicNameDic.Count > 0)
        {
            foreach (var dir in Configs.TimelineDicNameDic)
            {
                _groupNames.Add($"{dir.Desc}({dir.DicName})",dir.DicName);
            }
        }
    }
    public static SceneJsonData CreateMapObject(string path)
    {
        if (File.Exists(path))
        {
            var text = File.ReadAllText(path);
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(text);
            return json;
        }

        return null;
    }

    public static void Destroy()
    {
        _maplist.Clear();
        _SceneList.Clear();
    }
}


#if UNITY_EDITOR
/// <summary>
/// 地图配置
/// </summary>
public class SceneTlCfg
{
    public SceneTlCfg(MapEditor.SceneData data)
    {
        this.MapID = data.MapID;
        this.MapName = data.MapName;
        this.SceneName = data.SceneName;
    }

    public SceneTlCfg(MapEditor.LevelData data)
    {
        this.MapID = data.MapID;
        this.MapName = data.MapName;
        this.SceneName = data.SceneName;
    }


    public int MapID;
    public string MapName;
    public string SceneName;

    public ValueDropdownList<int> _npclist = new ValueDropdownList<int>();
    public Dictionary<long /*唯一ID*/, NPCJsonData> Npcs = new Dictionary<long, NPCJsonData>();
    private static string SaveDir = "Assets/DevTools/MapEditor/Prefabs/";


    public void Init(string serverJsonDir)
    {
        string path = $"{serverJsonDir}/{MapID}/data.json";
        var mapJson = TimelineConfigUtils.CreateMapObject(path);
        if (mapJson != null)
        {
            _npclist.Clear();
            foreach (var item in mapJson.Npcs)
            {
                if (MapEditor.EditorConfigUtils.NpcCfgs.Npcs.TryGetValue(item.Value.NpcID, out var nPCData))
                {
                    _npclist.Add($"{item.Value.Index}_{item.Value.NpcID}_{nPCData.Name}", item.Value.Index);

                    Npcs.Add(item.Value.Index, item.Value);
                }
            }

            /*_interlist.Clear();
            foreach (var item in mapJson.Mines)
            {
                if (MapEditor.EditorConfigUtils.MineCfgs.Mines.TryGetValue(item.Value.MineID, out var cfgv))
                {
                    _interlist.Add($"{item.Value.MineID} {cfgv.ObjectName}", item.Value.MineID);
                }
            }*/
        }
    }
}
#endif


[System.Serializable]
public class TimelineExcelInfo
{
    /// <summary>
    /// ID 
    /// </summary>
    public int ID;

    /// <summary>
    /// 描述
    /// </summary>
    public string desc;

    /// <summary>
    /// Timeline路径
    /// </summary>
    public string TimelinePath;

    /// <summary>
    /// 类型
    /// </summary>
    public int TimelineType;

    /// <summary>
    /// 结束隐藏Timeline
    /// </summary>
    public bool FinishHide;


    /// <summary>
    /// 播放时隐藏UI
    /// </summary>
    public bool HideUI;


    /// <summary>
    /// 播放时隐藏主角
    /// </summary>
    public bool HideRole;


    /// <summary>
    /// 播放时隐藏伙伴
    /// </summary>
    public bool HidePartner;

    /// <summary>
    /// 是否使用主角模型
    /// </summary>
    public bool UseRoleModel;


    /// <summary>
    /// 是否可移动
    /// </summary>
    public bool IsCanMove;


    /// <summary>
    /// 是否隐藏地图NPC
    /// </summary>
    public bool HideMapNPC;

    /// <summary>
    /// 是否隐藏其他玩家
    /// </summary>
    public bool HideOtherPlayer;


    public void ReadExcel(DataRow dataRow)
    {
        int index = 0;
        this.ID = System.Int32.Parse(dataRow[index++].ToString());
        this.desc = dataRow[index++].ToString();
        this.TimelinePath = dataRow[index++].ToString();
        this.TimelineType = System.Int32.Parse(dataRow[index++].ToString());
        this.FinishHide = System.Boolean.Parse(dataRow[index++].ToString());
        this.HideUI = System.Boolean.Parse(dataRow[index++].ToString());
        this.HideRole = System.Boolean.Parse(dataRow[index++].ToString());
        this.HidePartner = System.Boolean.Parse(dataRow[index++].ToString());
        this.UseRoleModel = System.Boolean.Parse(dataRow[index++].ToString());
        this.IsCanMove = System.Boolean.Parse(dataRow[index++].ToString());
        this.HideMapNPC = System.Boolean.Parse(dataRow[index++].ToString());
        this.HideOtherPlayer = System.Boolean.Parse(dataRow[index++].ToString());
    }

    public void ExportExcel(int row, ExcelRange excel)
    {
        int index = 1;
        excel[row, index++].Value = ID;
        excel[row, index++].Value = desc;
        excel[row, index++].Value = TimelinePath;
        excel[row, index++].Value = TimelineType;
        excel[row, index++].Value = FinishHide;
        excel[row, index++].Value = HideUI;
        excel[row, index++].Value = HideRole;
        excel[row, index++].Value = HidePartner;
        excel[row, index++].Value = UseRoleModel;
        excel[row, index++].Value = IsCanMove;
        excel[row, index++].Value = HideMapNPC;
        excel[row, index++].Value = HideOtherPlayer;
    }
}
#endif