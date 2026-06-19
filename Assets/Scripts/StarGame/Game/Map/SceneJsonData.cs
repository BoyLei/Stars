///--------------------------------------------------------------------
/// 文件名   :   SceneJsonData
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/11 11:28:47
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.AI;
using Task;
using StarProject.Service.Language;
#if UNITY_EDITOR
using MapEditor;
#endif

#region Json数据导出

public class SceneJsonData
{
    public int SceneID;
    public int GridSize = 2000;
    public string Tag;
    public bool IsFixedBlock;
    public bool IsHidePartner;
    public bool IsHideOtherPlayer;
    public MainPlayerJsonData MainPlayer = new MainPlayerJsonData();
    public Dictionary<long, MonsterJsonData> Monsters = new Dictionary<long, MonsterJsonData>();
    public Dictionary<long/*唯一ID*/, NPCJsonData> Npcs = new Dictionary<long, NPCJsonData>();
    public Dictionary<long/*唯一ID*/, MineJsonData> Mines = new Dictionary<long, MineJsonData>();
    public Dictionary<int, AreaJsonData> Areas = new Dictionary<int, AreaJsonData>();
    public Dictionary<int, PathJsonData> Paths = new Dictionary<int, PathJsonData>();
    public Dictionary<int, SpawnerJsonData> Spawners = new Dictionary<int, SpawnerJsonData>();
    public Dictionary<int, ObstacleGroupJsonData> Obstacles = new Dictionary<int, ObstacleGroupJsonData>();
    public Dictionary<int, TriggerJsonData> Triggers = new Dictionary<int, TriggerJsonData>();
    public Dictionary<int, RandomMonsterJsonData> RandomMonsters = new Dictionary<int, RandomMonsterJsonData>();
    public Dictionary<int, CustomVector3> GVE_Points = new Dictionary<int, CustomVector3>();
    public Dictionary<int, WantedTaskData> WantedTasks = new Dictionary<int, WantedTaskData>();

    public List<NPCJsonData> GetNpcsByConfigID(long configID)
    {
        if (Npcs != null && Npcs.Count > 0)
        {
            List<NPCJsonData> list = new List<NPCJsonData>();

            foreach (var item in Npcs)
            {
                if (item.Value.NpcID == configID)
                {
                    list.Add(item.Value);
                }
            }
            return list;
        }

        return null;
    }

    public List<MineJsonData> GetMinesByConfigID(long configID)
    {
        if (Mines != null && Mines.Count > 0)
        {
            List<MineJsonData> list = new List<MineJsonData>();

            foreach (var item in Mines)
            {
                if (item.Value.MineID == configID)
                {
                    list.Add(item.Value);
                }
            }
            return list;
        }
        return null;
    }

    public NPCJsonData GetNPCJsonData(long configID)
    {
        var list = GetNpcsByConfigID(configID);
        if (list != null && list.Count > 0)
        {
            return list[0];
        }
        return null;
    }

    public MineJsonData GetMineJsonData(long configID)
    {
        var list = GetMinesByConfigID(configID);
        if (list != null && list.Count > 0)
        {
            return list[0];
        }
        return null;
    }

    public WantedTaskData GetWantedTaskData(int pointID)
    {
        WantedTaskData wantedTaskData = null;
        if (WantedTasks != null && WantedTasks.Count > 0)
        {
            if (WantedTasks.TryGetValue(pointID, out wantedTaskData))
            {

            }
        }
        return wantedTaskData;
    }


#if UNITY_EDITOR

    public void AddSpawner(int spawnerID, int type, long configID)
    {
        var spawner = GetData(spawnerID);
        if (spawner == null)
        {
            Debug.LogError($"没有找到Spawner spawnerID={spawnerID} {GetTypeName(type)} configID={configID}");
            return;
        }

        switch (type)
        {
            case 1:
                MonsterToSpawner(spawner, configID);
                break;
            case 2:
                NpcToSpawner(spawner, configID);
                break;
            case 3:
                MineToSpawner(spawner, configID);
                break;
        }
    }
    private string GetTypeName(int type)
    {
        switch (type)
        {
            case 1:
                return "怪物";
            case 2:
                return "Npc";
            case 3:
                return "矿物";
            default:
                break;
        }
        return string.Empty;
    }
    private SpawnerJsonData GetData(int spawnerID)
    {
        if (Spawners.TryGetValue(spawnerID, out var data))
        {
            return data;
        }
        return null;
    }

    private void MonsterToSpawner(SpawnerJsonData data, long configID)
    {
        data.AddMonster(configID);
    }
    private void NpcToSpawner(SpawnerJsonData data, long configID)
    {
        data.AddNpc(configID);
    }

    private void MineToSpawner(SpawnerJsonData data, long configID)
    {
        data.AddMine(configID);
    }
#endif
}

/**********************************怪物数据***********************************/

[System.Serializable]
public class MonsterJsonData
{
    public int Index;
    public long MonsterID;
    public int MonsterLevel;
    public int ChaseAreaId;
    public string Alias;
    public string Title;
    public int MonsterType;
    public bool IsMain;
    public int GroupID;
    public int WalkType;
    public int PathID;
    public int WalkRange;
    public bool IgnoreGravity;
    public CustomVector3 Position;
    public int Rotation;
    public int Num;
    public int Range;
    public int Count;
    public int FreshType;
    public int DeadFreshInterval; //  死亡间隔刷新时间
    public int OpenServerInterval; //   开服间隔刷新时间 
    public string FixTime; // 固定时刻刷新时间
    public int WaveID; // 波次ID
    public int ControlID; //    控制器ID
    public int RandomMin;
    public int RandomMax;
    public string Desc;
    public bool CanCreateUnreachableArea;
    public CustomVector3 ClientRot;
    public MonsterJsonData() { }

#if UNITY_EDITOR
    public MonsterJsonData(int index, Monster monster)
    {
        Index = index;
        Desc = monster.transform.name;
        MonsterID = monster.MonsterID;
        MonsterLevel = monster.MonsterLevel;
        if (monster.ChaseArea == null)
        {
            ChaseAreaId = 0;
        }
        else
        {
            ChaseAreaId = monster.ChaseAreaID;
        }
        Alias = monster.Alias;
        Title = monster.Title;
        MonsterType = (int)monster.monsterType;
        IsMain = monster.IsMain;
        GroupID = monster.GroupID;
        WalkType = (int)monster.mWalkType;
        PathID = monster.PathID;
        WalkRange = monster.WalkRange;
        Position = new CustomVector3(monster.transform.position);
        ClientRot = new CustomVector3(monster.transform.localRotation.eulerAngles);
        if (ClientRot.x != 0 || ClientRot.z != 0)
        {
            Rotation = 0;
        }
        else
        {
            Rotation = (int)monster.transform.rotation.eulerAngles.y;
        }
        Num = monster.Num;
        Range = monster.Range;
        Count = monster.Count;
        IgnoreGravity = monster.IgnoreGravity;
        FreshType = (int)monster.freshType;
        DeadFreshInterval = monster.DeadFreshInterval; //  死亡间隔刷新时间
        OpenServerInterval = monster.OpenServerInterval; //   开服间隔刷新时间 
        FixTime = monster.GetFixFreshTimes(); // 固定时刻刷新时间
        WaveID = monster.WaveID; // 波次ID
        ControlID = monster.ControlID; //    控制器ID
        RandomMin = monster.RandomMin;
        RandomMax = monster.RandomMax;
        CanCreateUnreachableArea = monster.CanCreateUnreachableArea;
    }
#endif
}
/******************************************************************************/

/**********************************怪物数据***********************************/

[System.Serializable]
public class RandomMonsterJsonData
{
    public int Index;
    public int IndexID;
    public int MonsterType;
    public int ChaseAreaId;
    public bool IsMain;
    public int GroupID;
    public int WalkType;
    public int PathID;
    public int WalkRange;
    public bool IgnoreGravity;
    public CustomVector3 Position;
    public int Rotation;
    public int Num;
    public int Range;
    public int Count;
    public int FreshType;
    public int DeadFreshInterval; //  死亡间隔刷新时间
    public int OpenServerInterval; //   开服间隔刷新时间 
    public string FixTime; // 固定时刻刷新时间
    public int WaveID; // 波次ID
    public int ControlID; //    控制器ID
    public int RandomMin;
    public int RandomMax;
    public string Desc;
    public bool CanCreateUnreachableArea;
    public RandomMonsterJsonData() { }

#if UNITY_EDITOR
    public RandomMonsterJsonData(int index, RandomMonster monster)
    {
        Index = index;
        IndexID = monster.IndexID;
        Desc = monster.transform.name;
        MonsterType = (int)monster.monsterType;
        if (monster.ChaseArea == null)
        {
            ChaseAreaId = 0;
        }
        else
        {
            ChaseAreaId = monster.ChaseAreaID;
        }
        IsMain = monster.IsMain;
        GroupID = monster.GroupID;
        WalkType = (int)monster.mWalkType;
        PathID = monster.PathID;
        IgnoreGravity = monster.IgnoreGravity;
        WalkRange = monster.WalkRange;
        Position = new CustomVector3(monster.transform.position);
        Rotation = (int)monster.transform.rotation.eulerAngles.y;
        Num = monster.Num;
        Range = monster.Range;
        Count = monster.Count;
        FreshType = (int)monster.freshType;
        DeadFreshInterval = monster.DeadFreshInterval; //  死亡间隔刷新时间
        OpenServerInterval = monster.OpenServerInterval; //   开服间隔刷新时间 
        FixTime = monster.GetFixFreshTimes(); // 固定时刻刷新时间
        WaveID = monster.WaveID; // 波次ID
        ControlID = monster.ControlID; //    控制器ID
        RandomMin = monster.RandomMin;
        RandomMax = monster.RandomMax;
        CanCreateUnreachableArea = monster.CanCreateUnreachableArea;
    }
#endif
}
/******************************************************************************/
/**********************************主角触发器***********************************/
public class MainPlayerJsonData
{
    public List<TriggerGroupJsonData> TriggerGroups;
    public string Desc;
    public MainPlayerJsonData() { }

#if UNITY_EDITOR

    public void Save(MainPlayer player)
    {
        Desc = player.transform.name;
        if (TriggerGroups == null)
        {
            TriggerGroups = new List<TriggerGroupJsonData>();
        }
        TriggerGroups.Clear();
        if (player.TriggerGroups != null && player.TriggerGroups.Count > 0)
        {
            foreach (var item in player.TriggerGroups)
            {
                TriggerGroups.Add(new TriggerGroupJsonData(item));
            }
        }
    }
#endif
}
/******************************************************************************/
/**********************************怪物数据***********************************/
[System.Serializable]
public class WantedTaskData
{
    public int Index;
    public int Type;
    public CustomVector3 Position;
    public int Rotation;
    public string Desc;
    public WantedTaskData() { }
#if UNITY_EDITOR
    public WantedTaskData(int index, WantedTask task)
    {
        Index = task.Index;
        Type = task.Type;
        Desc = task.transform.name;
        Position = new CustomVector3(task.transform.position);
        Rotation = (int)task.transform.rotation.eulerAngles.y;
    }
#endif
}
/******************************************************************************/
/**********************************怪物数据***********************************/

[System.Serializable]
public class NPCJsonData
{
    public int Index;
    public long NpcID;
    public int NpcType;
    public bool IsMain;
    public bool DefaultVisible;
    public int GroupID;
    public int WalkType;
    public int PathID;
    public int WalkRange;
    public CustomVector3 Position;
    public int Rotation;
    public int Num;
    public bool IgnoreGravity;
    public int Range;
    public int Count;
    public int FreshType;
    public int DeadFreshInterval; //  死亡间隔刷新时间
    public int OpenServerInterval; //   开服间隔刷新时间 
    public string FixTime; // 固定时刻刷新时间
    public int WaveID; // 波次ID
    public int ControlID; //    控制器ID
    public int RandomMin;
    public int RandomMax;
    public CustomVector3 ClientRot;
    // public EventEffectData EnterEventEffect;
    //public EventEffectData ExitEventEffect;
    public List<TriggerGroupJsonData> TriggerGroups;
    public bool CanCreateUnreachableArea;

    public string Desc;
    public NPCJsonData() { }
#if UNITY_EDITOR
    public NPCJsonData(int index, NPC npc)
    {
        Index = npc.Index;
        Desc = npc.transform.name;
        NpcID = npc.NpcID;
        NpcType = (int)npc.npcType;
        IsMain = npc.IsMain;
        GroupID = npc.GroupID;
        DefaultVisible = npc.DefaultVisible;
        WalkType = (int)npc.mWalkType;
        PathID = npc.PathID;
        WalkRange = npc.WalkRange;
        Position = new CustomVector3(npc.transform.position);
        ClientRot = new CustomVector3(npc.transform.localRotation.eulerAngles);
        if (ClientRot.x != 0 || ClientRot.z != 0)
        {
            Rotation = 0;
        }
        else
        {
            Rotation = (int)npc.transform.rotation.eulerAngles.y;
        }
        Num = npc.Num;
        Range = npc.Range;
        Count = npc.Count;
        IgnoreGravity = npc.IgnoreGravity;
        FreshType = (int)npc.freshType;
        DeadFreshInterval = npc.DeadFreshInterval; //  死亡间隔刷新时间
        CanCreateUnreachableArea = npc.CanCreateUnreachableArea;
        OpenServerInterval = npc.OpenServerInterval; //   开服间隔刷新时间 
        FixTime = npc.GetFixFreshTimes(); // 固定时刻刷新时间
        WaveID = npc.WaveID; // 波次ID
        ControlID = npc.ControlID; //    控制器ID
        RandomMin = npc.RandomMin;
        RandomMax = npc.RandomMax;

        if (npc.TriggerGroups != null && npc.TriggerGroups.Count > 0)
        {
            if (TriggerGroups == null)
            {
                TriggerGroups = new List<TriggerGroupJsonData>();
            }
            TriggerGroups.Clear();
            foreach (var item in npc.TriggerGroups)
            {
                TriggerGroups.Add(new TriggerGroupJsonData(item));
            }
        }
    }



#endif
}

[System.Serializable]
public class TriggerGroupJsonData
{
    public int TriggerID;

    public List<TrrigerEffectJsonData> Effects;
    public TriggerGroupJsonData() { }

#if UNITY_EDITOR
    public TriggerGroupJsonData(TriggerGroup triggerGroup)
    {
        TriggerID = triggerGroup.TriggerID;
        if (triggerGroup.TrrigerEffects != null && triggerGroup.TrrigerEffects.Count > 0)
        {
            if (Effects == null)
            {
                Effects = new List<TrrigerEffectJsonData>();
            }
            Effects.Clear();
            foreach (var item in triggerGroup.TrrigerEffects)
            {
                TrrigerEffectJsonData effectJsonData = new TrrigerEffectJsonData(item);
                Effects.Add(effectJsonData);
            }
        }
    }
#endif
}

[System.Serializable]
public class TrrigerEffectJsonData
{
    public List<List<JsonConditon>> Conditions;
    public List<EffectJsonData> Effects;
    public TrrigerEffectJsonData() { }

#if UNITY_EDITOR
    public TrrigerEffectJsonData(TrrigerEffect trrigerEffect)
    {
        if (trrigerEffect.Conditions != null && trrigerEffect.Conditions.Count > 0)
        {
            List<JsonConditon> conditons = new List<JsonConditon>();

            foreach (var item in trrigerEffect.Conditions)
            {
                foreach (var it in item.SubEditorConditions)
                {
                    JsonConditon conditon = new JsonConditon(it);
                    conditons.Add(conditon);
                }
            }
            if (Conditions == null)
            {
                Conditions = new List<List<JsonConditon>>();
            }
            Conditions.Add(conditons);
        }

        if (trrigerEffect.EditorEffects != null && trrigerEffect.EditorEffects!.Count > 0)
        {
            if (Effects == null)
            {
                Effects = new List<EffectJsonData>();
            }
            Effects.Clear();
            foreach (var item in trrigerEffect.EditorEffects)
            {
                EffectJsonData effectJson = new EffectJsonData(item);
                Effects.Add(effectJson);
            }
        }
    }

    public TrrigerEffect GetTrrigerEffect()
    {
        TrrigerEffect trrigerEffect = new TrrigerEffect();

        if (trrigerEffect.Conditions == null)
        {
            trrigerEffect.Conditions = new List<TaskSubContionGroup>();
        }
        trrigerEffect.Conditions.Clear();
        if (Conditions != null && Conditions.Count > 0)
        {
            foreach (var conditions in Conditions)
            {
                TaskSubContionGroup subContionGroup = new TaskSubContionGroup();

                foreach (var condition in conditions)
                {
                    subContionGroup.SubConditions.Add(condition);

                    ConditionSerialize conditionSerialize = new ConditionSerialize(condition);
                    conditionSerialize.ConditionType = condition.ConditionType;

                    subContionGroup.SubEditorConditions.Add(conditionSerialize);
                }

                trrigerEffect.Conditions.Add(subContionGroup);
            }
        }

        if (trrigerEffect.EditorEffects == null)
        {
            trrigerEffect.EditorEffects = new List<EffectSerialize>();
        }
        trrigerEffect.EditorEffects.Clear();
        if (trrigerEffect.Effects == null)
        {
            trrigerEffect.Effects = new List<EffectJsonData>();

        }
        trrigerEffect.Effects.Clear();

        if (Effects != null && Effects!.Count > 0)
        {

            foreach (var item in Effects)
            {
                trrigerEffect.Effects.Add(item);
                EffectSerialize effectSerialize = new EffectSerialize(item);
                effectSerialize.EffectType = item.EffectType;
                trrigerEffect.EditorEffects.Add(effectSerialize);
            }
        }

        return trrigerEffect;
    }
#endif
}
/******************************************************************************/


/**********************************矿物数据***********************************/
[System.Serializable]
public class MineJsonData
{
    public int Index;
    public long MineID;
    public int MineGroup;
    public bool DefaultVisible;
    public int MineType;
    public bool IsShowRange;
    public bool IgnoreGravity;
    public CustomVector3 Position;
    public int Rotation;
    public int Num;
    public int Range;
    public int Count;
    public int FreshType;
    public int DeadFreshInterval; //  死亡间隔刷新时间
    public int OpenServerInterval; //   开服间隔刷新时间 
    public string FixTime; // 固定时刻刷新时间
    public int WaveID; // 波次ID
    public int ControlID; //    控制器ID
    public string Desc;
    public bool CanCreateUnreachableArea;
    public CustomVector3 ClientRot;
    public MineJsonData() { }

#if UNITY_EDITOR
    public MineJsonData(int index, Mine mine)
    {
        Index = mine.Index;
        Desc = mine.transform.name;
        MineID = mine.MineID;
        MineGroup = mine.MineGroup;
        MineType = (int)mine.mineType;
        IsShowRange = mine.IsShowRange;
        DefaultVisible = mine.DefaultVisible;
        Position = new CustomVector3(mine.transform.position);
        ClientRot = new CustomVector3(mine.transform.localRotation.eulerAngles);
        if (ClientRot.x != 0 || ClientRot.z != 0)
        {
            Rotation = 0;
        }
        else
        {
            Rotation = (int)mine.transform.rotation.eulerAngles.y;
        }
        Num = mine.Num;
        IgnoreGravity = mine.IgnoreGravity;
        Range = mine.Range;
        Count = mine.Count;
        FreshType = (int)mine.freshType;
        DeadFreshInterval = mine.DeadFreshInterval; //  死亡间隔刷新时间
        OpenServerInterval = mine.OpenServerInterval; //   开服间隔刷新时间 
        CanCreateUnreachableArea = mine.CanCreateUnreachableArea;
        FixTime = mine.GetFixFreshTimes(); // 固定时刻刷新时间
        WaveID = mine.WaveID; // 波次ID
        ControlID = mine.ControlID; //    控制器ID
    }
#endif
}
/******************************************************************************/

/**********************************区域数据***********************************/
[System.Serializable]
public class AreaJsonData
{
    public int Index;
    public int AreaID;
    public int Faction;
    public int TriggerCount;
    public CustomVector3 Position;
    public int Rotation;
    public int areaType;
    public int shapType;
    public int Length;
    public int Width;
    public int Radius;
    public List<CustomVector3> Polygons;

    private string areaName;
    public string AreaName
    {
        get
        {
#if UNITY_EDITOR
            //编辑器模式直接返回中文
            if (!Application.isPlaying)
            {
                return areaName;
            }
#endif
            //非编辑器模式返回key对应的语言文本
            return LanguageManager.Instance.GetLanguageByKey(AreaName_Key);
        }
        set{ areaName = value; }
    }
    public string AreaName_Key ;

    //public int colliderTypes;
    public int blockEffect;
    public int showType;
    public int EffectNums;

    public bool DeadValid = true;

    public List<EventEffectData> EnterEventEffect;
    public List<EventEffectData> ExitEventEffect;

    public int WaveID; // 波次ID
    public int ControlID; //    控制器ID
    public string Desc;
    public List<TaskSubContionGroup> SpawnPointActiveConditions;
    public AreaJsonData() { }
    /// <summary>
    /// 数据导出
    /// </summary>
    /// <param name="index"></param>
    /// <param name="area"></param>
#if UNITY_EDITOR
    public AreaJsonData(int index, Area area)
    {
        Polygons = new List<CustomVector3>();
        Index = index;
        Desc = area.transform.name;
        AreaID = area.AreaID;
        Faction = area.Faction;
        TriggerCount = area.TriggerCount;
        Position = new CustomVector3(area.transform.position);
        Rotation = (int)area.transform.rotation.eulerAngles.y;
        areaType = (int)area.areaType;
        shapType = (int)area.shapType;
        Length = area.Length;
        Width = area.Width;
        Radius = area.Radius;
        var list = area.PolygonsToString();
        if (list != null && list.Count > 0)
        {
            foreach (var item in list)
            {
                Polygons.Add(new CustomVector3(item));
            }
        }
        AreaName = area.AreaName;
        AreaName_Key = area.AreaName_Key;
        //colliderTypes = (int)area.colliderTypes;
        blockEffect = area.blockEffect;
        showType = (int)area.showType;
        EffectNums = area.EffectNums;
        DeadValid = area.DeadValid;
        EnterEventEffect = area.EnterEventEffect;
        ExitEventEffect = area.ExitEventEffect;
        WaveID = area.WaveID; // 波次ID
        ControlID = area.ControlID; //    控制器ID
        if (area.SpawnPointActiveConditions != null)
        {
            SpawnPointActiveConditions = area.SpawnPointActiveConditions;
        }
    }
#endif

}

/******************************************************************************/

/**********************************区域效果数据********************************/
[System.Serializable]
public class AreaEffectJsonData
{
    public int EffectType;
    public string EffectArg;
    public AreaEffectJsonData()
    {

    }

#if UNITY_EDITOR
    public AreaEffectJsonData(AreaEffect areaEffect)
    {
        EffectType = (int)areaEffect.effectType;
        EffectArg = areaEffect.GetArgs();
    }
#endif
}

/******************************************************************************/

/**********************************路径数据***********************************/

[Serializable]
public class CustomWaypoint
{
    public CustomVector3 position;
    public CustomVector3 tangent;
    public float roll;
}


[System.Serializable]
public class PathJsonData
{
    public int Index;

    public int PathID;
    public int Resolution;
    public bool IsLoop;
    public string Desc;
    public PathJsonData() { }

    public CustomVector3 Position;
    public CustomVector3 Rotation;
    public List<CustomVector3> WayPoints;

    public List<CustomWaypoint> KeyPoints;
#if UNITY_EDITOR
    public PathJsonData(int index, Path path)
    {
        WayPoints = new List<CustomVector3>();
        Index = index;
        Desc = path.transform.name;
        PathID = path.PathID;
        IsLoop = path.IsLoop;
        Resolution = path.path.m_Resolution;

        KeyPoints = new List<CustomWaypoint>();
        Position = new CustomVector3(path.transform.position);
        Rotation = new CustomVector3(path.transform.rotation.eulerAngles);

        foreach (var p in path.path.m_Waypoints)
        {
            KeyPoints.Add(new CustomWaypoint()
            {
                position = new CustomVector3(p.position),
                tangent = new CustomVector3(p.tangent),
                roll = p.roll
            });
        }
        var list = path.GetPaths();
        if (list != null && list.Count > 0)
        {
            foreach (var item in list)
            {
                WayPoints.Add(new CustomVector3(item));
            }
        }
    }
#endif
}

/******************************************************************************/


/**********************************Spawner数据***********************************/
[System.Serializable]
public class SpawnerJsonData
{
    public int Index;

    public int SpawnerID;

    public int Range;

    public CustomVector3 Position;

    public List<long> Monsters;

    public List<long> Npcs;

    public List<long> Mines;

    public string Desc;
    public SpawnerJsonData() { }

#if UNITY_EDITOR
    public SpawnerJsonData(int index, Spawner spawner)
    {
        this.Index = index;
        this.Desc = spawner.transform.name;
        this.Range = spawner.Range;
        this.SpawnerID = spawner.SpawnerID;
        this.Position = new CustomVector3(spawner.transform.position);

        this.Monsters = new List<long>();
        this.Npcs = new List<long>();
        this.Mines = new List<long>();
    }

    public void AddMonster(long configID)
    {
        Monsters.Add(configID);
    }

    public void AddNpc(long configID)
    {
        Npcs.Add(configID);
    }

    public void AddMine(long configID)
    {
        Mines.Add(configID);
    }
#endif
}

/******************************************************************************/

/*************************************触发器数据***************************************/
[System.Serializable]
public class TriggerJsonData
{

    public int TrrigerType;
    public int ID;
    public int Count;
    public string Desc;
    public CustomVector3 Position;
    public TriggerPositionJsonData PositionJsonData;
    public TriggerTimerJsonData TimerJsonData;
    public TriggerPropertyJsonData PropertyJsonData;

    public TriggerJsonData()
    {
        TrrigerType = 0;
        ID = 0;
        Count = 0;
    }
#if UNITY_EDITOR
    public TriggerJsonData(TrrigerBase trrigerBase)
    {
        this.Position = new CustomVector3(trrigerBase.transform.position);
        TrrigerType = (int)trrigerBase.TrrigerType;
        ID = trrigerBase.ID;
        Count = trrigerBase.Count;
        Desc = trrigerBase.transform.name;
        switch (TrrigerType)
        {
            case (int)Task.TrrigerType.PositionTrriger:
                {
                    if (PositionJsonData == null)
                    {
                        PositionJsonData = new TriggerPositionJsonData();
                    }
                    PositionJsonData.Position = new CustomVector3(trrigerBase.transform.position); ;
                    PositionJsonData.Radius = trrigerBase.Radius;
                    PositionJsonData.IsEnterTrigger = trrigerBase.IsEnterTrigger;
                    PositionJsonData.shapType = (int)trrigerBase.shapType;
                    PositionJsonData.Length = trrigerBase.Length;
                    PositionJsonData.Width = trrigerBase.Width;
                    if (PositionJsonData.Polygons == null)
                    {
                        PositionJsonData.Polygons = new List<CustomVector3>();
                    }
                    PositionJsonData.Polygons.Clear();
                    var list = trrigerBase.PolygonsToString();
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            PositionJsonData.Polygons.Add(new CustomVector3(item));
                        }
                    }

                }
                break;
            case (int)Task.TrrigerType.TimerTrriger:
                {
                    if (TimerJsonData == null)
                    {
                        TimerJsonData = new TriggerTimerJsonData();
                    }
                    TimerJsonData.Delay = trrigerBase.Delay;
                    TimerJsonData.Interval = trrigerBase.Interval;

                }
                break;
            case (int)Task.TrrigerType.PropertyTrriger:
                {
                    if (PropertyJsonData == null)
                    {
                        PropertyJsonData = new TriggerPropertyJsonData();
                    }
                    PropertyJsonData.IsSelf = trrigerBase.IsSelf;
                    PropertyJsonData.PropName = trrigerBase.PropName;
                    PropertyJsonData.PropValue = trrigerBase.PropValue;
                    PropertyJsonData.Compare = (int)trrigerBase.Compare;
                }
                break;
        }
    }
#endif

    public bool ShouldSerializePositionJsonData()
    {
        return TrrigerType == (int)Task.TrrigerType.PositionTrriger;
    }

    public bool ShouldSerializeTimerJsonData()
    {
        return TrrigerType == (int)Task.TrrigerType.TimerTrriger;
    }

    public bool ShouldSerializePropertyJsonData()
    {
        return TrrigerType == (int)Task.TrrigerType.PropertyTrriger;
    }
}

[System.Serializable]
public class TriggerPositionJsonData
{
    public CustomVector3 Position;
    public int Radius;
    public bool IsEnterTrigger;
    public int Width;
    public int Length;
    public int shapType;
    public List<CustomVector3> Polygons;
}

[System.Serializable]
public class TriggerTimerJsonData
{
    public int Delay;
    public int Interval;
}


[System.Serializable]
public class TriggerPropertyJsonData
{
    public bool IsSelf;
    public string PropName;
    public string PropValue;
    public int Compare;
}


/****************************************************************************/

/**********************************动态阻挡数据***********************************/

[System.Serializable]
public class ObstacleGroupJsonData
{
    public int GroupID;
    public int Index;
    public bool IsOpen;
    public string Desc;
    public List<ObstacleJsonData> Obstacles = new List<ObstacleJsonData>();
    public List<ObstacleEffectJsonData> Effects = new List<ObstacleEffectJsonData>();
    public ObstacleGroupJsonData()
    {

    }

#if UNITY_EDITOR
    public ObstacleGroupJsonData(int index, ObstacleGroup areaEffect)
    {
        Index = index;
        GroupID = areaEffect.ID;
        IsOpen = areaEffect.IsOpen;
        Desc = areaEffect.transform.name;
        Obstacles.Clear();

        var list = areaEffect.GetComponentsInChildren<NavMeshObstacle>();
        if (list != null && list.Length > 0)
        {
            foreach (var item in list)
            {
                Obstacles.Add(new ObstacleJsonData(item, item.transform.localScale));
            }
        }

        if (areaEffect.Effects != null && areaEffect.Effects.Count > 0)
        {
            foreach (var item in areaEffect.Effects)
            {
                ObstacleEffectJsonData obstacleEffectJson = new ObstacleEffectJsonData();
                if (item.EffectTransfom != null)
                {
                    obstacleEffectJson.Position = new CustomVector3(item.EffectTransfom.position);
                    obstacleEffectJson.Rotation = (int)item.EffectTransfom.rotation.eulerAngles.y;
                    obstacleEffectJson.Scale = new CustomVector3(item.EffectTransfom.localScale);
                }
                else
                {
                    obstacleEffectJson.Position = new CustomVector3(areaEffect.transform.position);
                    obstacleEffectJson.Rotation = (int)areaEffect.transform.rotation.eulerAngles.y;
                    obstacleEffectJson.Scale = new CustomVector3(areaEffect.transform.localScale);
                }
                obstacleEffectJson.Path = item.EffectPath;

                Effects.Add(obstacleEffectJson);
            }
        }
    }
#endif

    [System.Serializable]
    public class ObstacleEffectJsonData
    {
        public int Rotation;
        public CustomVector3 Position;
        public CustomVector3 Scale;
        public string Path;

        public ObstacleEffectJsonData() { }
    }

    [System.Serializable]
    public class ObstacleJsonData
    {
        public int Shape;
        public int Rotation;
        public CustomVector3 Center;
        public CustomVector3 Size;
        public CustomVector3 Position;
        public ObstacleJsonData() { }
        public ObstacleJsonData(NavMeshObstacle obstacle, Vector3 Scale)
        {
            Position = new CustomVector3(obstacle.transform.position);
            //所有碰撞体的角度限制在  0-180以内   超出部分取余
            Rotation = (int)obstacle.transform.rotation.eulerAngles.y % 180;
            Shape = (int)obstacle.shape;
            Center = new CustomVector3(obstacle.center);
            Size = new CustomVector3(Scale);
        }
    }

}
/******************************************************************************/

[System.Serializable]
[Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptOut)]
public class EventEffectData
{

    [LabelText("条件组ID")]
    public int ConditionID;

    [LabelText("事件ID")]
    public int EventID;

    [LabelText("备注")]
    [Newtonsoft.Json.JsonIgnore]
    public string Desc;

    public EventEffectData() { }
}
#endregion


[System.Serializable]
public class CustomVector3
{
    public float x;
    public float y;
    public float z;

    public CustomVector3() { }
    public CustomVector3(float _x, float _y, float _z)
    {
        this.x = (float)Math.Round(_x, 2);
        this.y = (float)Math.Round(_y, 2);
        this.z = (float)Math.Round(_z, 2);
    }

    public CustomVector3(UnityEngine.Vector3 vector)
    {
        this.x = (float)Math.Round(vector.x, 2);
        this.y = (float)Math.Round(vector.y, 2);
        this.z = (float)Math.Round(vector.z, 2);
    }

    public UnityEngine.Vector3 Convert()
    {
        return new UnityEngine.Vector3(x, y, this.z);
    }
}