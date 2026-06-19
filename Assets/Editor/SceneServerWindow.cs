using System;
using System.Collections;
using System.Collections.Generic;
using StarProject.Game;
using StarProject.Game.Data;
using StarProject.Game.Map;
using StarProject.Service.LocalData;
using StarProject.Service.ServerService;
using UnityEngine;
using UnityEditor;

public class SceneServerWindow : EditorWindow
{
    private bool isInit = false;
    private List<NpcInfo> Npcs = new List<NpcInfo>();
    private List<InterObjectInfo> Inters = new();

    private bool fouldNpc = false;
    private bool fouldInter = false;
    private Vector2 scrollview = Vector2.zero;
    private bool showService=true;
    private bool showTask=true;

    [MenuItem("Tools/【任务系统】服务效果显示工具")]
    static void OpenWindow()
    {
        var window = GetWindow<SceneServerWindow>("场景数据");
        window.Show();
    }


    private void Init()
    {
        Npcs.Clear();
        Inters.Clear();

        if (GameMap.sceneJsonData != null)
        {
            if (GameMap.sceneJsonData.Npcs != null && GameMap.sceneJsonData.Npcs.Count > 0)
            {
                foreach (var npc in GameMap.sceneJsonData.Npcs)
                {
                    NpcInfo info = new NpcInfo();
                    info.Index = npc.Key;
                    info.ConfigID = npc.Value.NpcID;
                    var cfg = LocalDataManager.Instance.GetNPCDataCell((uint)info.ConfigID);
                    if (cfg != null)
                    {
                        info.Name = cfg.Name;
                    }

                    info.Position = npc.Value.Position.Convert();
                    if (info.Services == null)
                    {
                        info.Services = new List<ServerService>();
                    }

                    info.Services.Clear();
                    var list = ServerServiceManager.Instance.GetRegisterServices(ServerServiceType.NPC, (uint)info.ConfigID);
                    if (list != null && list.Count > 0)
                    {
                        foreach (var service in list)
                        {
                            info.Services.Add(service);
                        }
                    }

                    if (info.Tasks == null)
                    {
                        info.Tasks = new List<TaskNpc>();
                    }

                    info.Tasks.Clear();
                    var tasks = TaskHelper.GetTaskNpcs((int)info.ConfigID);
                    if (tasks != null && tasks.Count > 0)
                    {
                        foreach (var task in tasks)
                        {
                            info.Tasks.Add(task);
                        }
                    }
                    Npcs.Add(info);
                }
            }

            if (GameMap.sceneJsonData.Mines != null && GameMap.sceneJsonData.Mines.Count > 0)
            {
                foreach (var inter in GameMap.sceneJsonData.Mines)
                {
                    InterObjectInfo info = new InterObjectInfo();
                    info.Index = inter.Key;
                    info.ConfigID = inter.Value.MineID;
                    var cfg = LocalDataManager.Instance.GetInteractDataCell((int)info.ConfigID);
                    if (cfg != null)
                    {
                        info.Name = cfg.ModelName;
                    }

                    info.Position = inter.Value.Position.Convert();
                    if (info.Services == null)
                    {
                        info.Services = new List<ServerService>();
                    }

                    info.Services.Clear();
                    var list = ServerServiceManager.Instance.GetRegisterServices(ServerServiceType.InterAction,
                        (uint)info.ConfigID);
                    if (list != null && list.Count > 0)
                    {
                        foreach (var service in list)
                        {
                            info.Services.Add(service);
                        }
                    }

                    if (info.Tasks == null)
                    {
                        info.Tasks = new List<TaskNpc>();
                    }

                    info.Tasks.Clear();
                    var tasks = TaskHelper.GetInterTasks((int)info.ConfigID);
                    if (tasks != null && tasks.Count > 0)
                    {
                        foreach (var task in tasks)
                        {
                            info.Tasks.Add(task);
                        }
                    }
                    Inters.Add(info);
                }
            }

            isInit = true;
        }
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (!isInit)
        {
            Init();
        }

        scrollview = GUILayout.BeginScrollView(scrollview, "box");
        if (GameMap.sceneJsonData != null)
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.IntField( "场景ID",GameMap.sceneJsonData.SceneID);
            EditorGUILayout.IntField("格子大小",GameMap.sceneJsonData.GridSize);
            showService=EditorGUILayout.Toggle("仅显示有服务的数据",showService);
            showTask=EditorGUILayout.Toggle("仅显示有任务的数据",showTask);
            GUILayout.EndVertical();


            GUILayout.BeginVertical();
            fouldNpc = EditorGUILayout.Foldout(fouldNpc, "Npc信息");
            if (fouldNpc)
            {
                if (Npcs != null && Npcs.Count > 0)
                {
                    foreach (var npc in Npcs)
                    {
                        var isShow = true;
                        if (showService)
                        {
                            isShow = npc.Services.Count>0;
                        }
                        
                        if (showTask)
                        {
                            isShow |= npc.Tasks.Count>0;
                        }

                        if (!isShow)
                        {
                            continue;
                        }
                        npc.fould = EditorGUILayout.Foldout(npc.fould, $"ConfigID={npc.ConfigID}   Index{npc.Index}");
                        if (npc.fould)
                        {
                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("名字",npc.Name );
                            EditorGUILayout.Vector3Field("坐标", npc.Position);
                            if (GUILayout.Button("前往"))
                            {
                                TaskHelper.FindPostion(GameMap.sceneJsonData.SceneID,npc.Position);
                            }
                            GUILayout.EndHorizontal();

                            npc.fouldService = EditorGUILayout.Foldout(npc.fouldService, "服务");
                            if (npc.fouldService)
                            {
                                if (npc.Services != null && npc.Services.Count > 0)
                                {
                                    foreach (var service in npc.Services)
                                    {
                                        GUILayout.BeginVertical("box");
                                        EditorGUILayout.LabelField("ServiceName",service.ServiceName);
                                        EditorGUILayout.IntField("条件组ID",service.ConditionGroupID);
                                        EditorGUILayout.IntField("效果ID",service.EffectID);
                                        EditorGUILayout.IntField("服务的来源ID",service.ServiceSourceID);
                                        EditorGUILayout.IntField("服务来源的子ID",service.ServiceSourceSubID);

                                        EditorGUILayout.IntField( "交互对象存在的配置ID",(int)service.ObjID);
                                        EditorGUILayout.LabelField( "注册的服务子类型",service.regServiceSubType.ToString());
                                        EditorGUILayout.LabelField( "服务的主key",service.MainKey);
                                        EditorGUILayout.LabelField("服务的状态",service.state.ToString());
                                        EditorGUILayout.Toggle( "是不是任务服务",service.IsTaskServer);
                                        if (GUILayout.Button("刷新状态"))
                                        {
                                            service.UpdateState();
                                        }

                                        GUILayout.EndVertical();
                                    }
                
                                }
                            }

                            npc.fouldTask = EditorGUILayout.Foldout(npc.fouldTask, "任务");
                            if (npc.fouldTask)
                            {
                                if (npc.Tasks != null && npc.Tasks.Count > 0)
                                {
                                    foreach (var task in npc.Tasks)
                                    {
                                        GUILayout.BeginVertical("box");

                                        EditorGUILayout.IntField( "任务ID",(int)task.TaskID);
                                        EditorGUILayout.IntField("事件ID",task.TaskEventID);
                                        EditorGUILayout.IntField( "任务类型",task.TaskType);
                                        EditorGUILayout.LabelField("来源",task.taskNPCSource.ToString());

                                        EditorGUILayout.IntField("条件ID",task.ConditionID);
                                        EditorGUILayout.LabelField("任务状态",task.StateE.ToString());

                                        if (GUILayout.Button("刷新"))
                                        {
                                            task.Update(task.TaskID);
                                        }

                                        GUILayout.EndVertical();
                                    }
                                }
                            }

                            
                            GUILayout.EndVertical();
                        }
                    }
                }
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            fouldInter = EditorGUILayout.Foldout(fouldInter, "交互物信息");
            if (fouldInter)
            {
                if (Inters != null && Inters.Count > 0)
                {
                    foreach (var inter in Inters)
                    {
                        var isShow = true;
                        if (showService)
                        {
                            isShow = inter.Services.Count >0;
                        }
                        
                        if (showTask)
                        {
                            isShow |= inter.Tasks.Count> 0;
                        }

                        if (!isShow)
                        {
                            continue;
                        }
                        
                        inter.fould = EditorGUILayout.Foldout(inter.fould,
                            $"ConfigID={inter.ConfigID}   Index{inter.Index}");
                        if (inter.fould)
                        {
                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(inter.Name, "名字");
                            EditorGUILayout.Vector3Field("坐标", inter.Position);
                            if (GUILayout.Button("前往"))
                            {
                                TaskHelper.FindPostion(GameMap.sceneJsonData.SceneID,inter.Position);
                            }
                            GUILayout.EndHorizontal();

                            inter.fouldService = EditorGUILayout.Foldout(inter.fouldService, "服务");
                            if (inter.fouldService)
                            {
                                if (inter.Services != null && inter.Services.Count > 0)
                                {
                                    foreach (var service in inter.Services)
                                    {
                                        GUILayout.BeginVertical("box");
                                        EditorGUILayout.LabelField("ServiceName",service.ServiceName);
                                        EditorGUILayout.IntField("条件组ID",service.ConditionGroupID);
                                        EditorGUILayout.IntField("效果ID",service.EffectID);
                                        EditorGUILayout.IntField("服务的来源ID",service.ServiceSourceID);
                                        EditorGUILayout.IntField("服务来源的子ID",service.ServiceSourceSubID);

                                        EditorGUILayout.IntField( "交互对象存在的配置ID",(int)service.ObjID);
                                        EditorGUILayout.LabelField( "注册的服务子类型",service.regServiceSubType.ToString());
                                        EditorGUILayout.LabelField( "服务的主key",service.MainKey);
                                        EditorGUILayout.LabelField("服务的状态",service.state.ToString());
                                        EditorGUILayout.Toggle( "是不是任务服务",service.IsTaskServer);
                                        if (GUILayout.Button("刷新状态"))
                                        {
                                            service.UpdateState();
                                        }

                                        GUILayout.EndVertical();
                                    }
                                }
                            }

                            inter.fouldTask = EditorGUILayout.Foldout(inter.fouldTask, "任务");
                            if (inter.fouldTask)
                            {
                                if (inter.Tasks != null && inter.Tasks.Count > 0)
                                {
                                    foreach (var task in inter.Tasks)
                                    {
                                        GUILayout.BeginVertical("box");

                                        EditorGUILayout.IntField( "任务ID",(int)task.TaskID);
                                        EditorGUILayout.IntField("事件ID",task.TaskEventID);
                                        EditorGUILayout.IntField( "任务类型",task.TaskType);
                                        EditorGUILayout.LabelField("来源",task.taskNPCSource.ToString());

                                        EditorGUILayout.IntField("条件ID",task.ConditionID);
                                        EditorGUILayout.LabelField("任务状态",task.StateE.ToString());

                                        if (GUILayout.Button("刷新"))
                                        {
                                            task.Update(task.TaskID);
                                        }

                                        GUILayout.EndVertical();
                                    }
                                }
                            }

                            GUILayout.EndVertical();
                        }
                    }
                }
            }

            GUILayout.EndVertical();
        }

        if (GUILayout.Button("刷新"))
        {
            Init();
        }

        GUILayout.EndScrollView();
    }

    private void OnDestroy()
    {
        isInit = false;
        fouldNpc = false;
        fouldInter = false;
        showService = true;
        showTask = true;
        scrollview = Vector2.zero;
        Npcs.Clear();
        Inters.Clear();
    }

    public class EntityInfo
    {
        public long EntityID;
        public long ConfigID;
        public long Index;
        public string Name;
        public Vector3 Position;
        public List<ServerService> Services;
        public List<TaskNpc> Tasks;

        public bool fould;
        public bool fouldService;
        public bool fouldTask;
    }

    public class NpcInfo : EntityInfo
    {
    }

    public class InterObjectInfo : EntityInfo
    {
    }
}