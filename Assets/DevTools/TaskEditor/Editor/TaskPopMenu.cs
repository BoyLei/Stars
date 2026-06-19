///--------------------------------------------------------------------
/// 文件名   :   TaskPopMenu.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/16 10:42:56
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Task;
using UnityEditor;
using UnityEngine;

public enum WindowType
{
    Chapter,
    Task,
    Chain,
}
public class TaskPopMenu : EditorWindow
{
    /// <summary>
    /// 点击回调
    /// </summary>
    /// 
    public static WindowType WindowType;

    private static TaskClassifyType classifyType;
    private static int ChapterID; 
    private static string ChapterName;


    private static int TaskIndex;
    private static string TaskDesc;
    private static string TaskName;
    private static ushort ChainID;

    private static System.Action<TaskClassifyType/*任务类型*/, int/*章节ID*/, string /*章节名称*/, int /*索引*/ ,string/*任务名称*/, string/*任务描述*/,ushort> OnAddTask;
    private static System.Action<TaskClassifyType/*任务类型*/, int/*章节ID*/, string/*章节名称*/> OnAddChapter;
    private static System.Action<TaskClassifyType/*任务类型*/, int/*章节ID*/, string/*章节名称*/,ushort/*任务链ID*/> OnAddChain;

    private static TaskPopMenu Instance;


    /// <summary>
    /// 添加章节
    /// </summary>
    /// <param name="type"></param>
    /// <param name="taskID"></param>
    /// <param name="click"></param>
    public static void AddChapter( TaskClassifyType taskType, System.Action<TaskClassifyType/*任务类型*/, int/*章节ID*/, string/*章节名称*/> click)
    {
        if(Instance==null)
        {
            TaskPopMenu taskPop = GetWindow<TaskPopMenu>();
            taskPop.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 200);
            taskPop.ShowAuxWindow();
            Instance = taskPop;
        }
        OnAddChapter = click;
        WindowType = WindowType.Chapter;
        classifyType = taskType;
        ChapterID = 0;
        ChapterName = string.Empty;
    }

    public static void AddChain( TaskClassifyType taskType, int chapterID, string chapterName,  System.Action<TaskClassifyType/*任务类型*/, int/*章节ID*/, string/*章节名称*/,ushort> click)
    {
        if(Instance==null)
        {
            TaskPopMenu taskPop = GetWindow<TaskPopMenu>();
            taskPop.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 200);
            taskPop.ShowAuxWindow();
            Instance = taskPop;
        }
        OnAddChain = click;
        WindowType = WindowType.Chain;
        classifyType = taskType;
        ChapterID = chapterID;
        ChapterName = chapterName;
    }
    
    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="type"></param>
    /// <param name="taskType"></param>
    /// <param name="click"></param>
    public static void AddTask( TaskClassifyType taskType,int chapterID, string chapterName,ushort chainID, System.Action<TaskClassifyType/*任务类型*/, int/*章节ID*/, string /*章节名称*/, int /*索引*/ , string/*任务名称*/, string/*任务描述*/,ushort  > click)
    {
        if (Instance == null)
        {
            TaskPopMenu taskPop = GetWindow<TaskPopMenu>();
            taskPop.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 200);
            taskPop.ShowAuxWindow();
            Instance = taskPop;
        }
        OnAddTask = click;
        WindowType = WindowType.Task;
        classifyType = taskType;
        ChapterID = chapterID;
        ChainID = chainID;
        ChapterName = chapterName;
    }

    public string GetTaskTypeName(TaskClassifyType TaskType)
    {
        string typeName = "";
        switch (TaskType)
        {
            case TaskClassifyType.MainLine: typeName = "主线"; break;
            case TaskClassifyType.SubbranchLine: typeName = "支线"; break;
            case TaskClassifyType.Biography: typeName = "传记"; break;
            case TaskClassifyType.Play: typeName = "日常"; break;
            case TaskClassifyType.Guide: typeName = "引导"; break;
            case TaskClassifyType.Other: typeName = "其他"; break;
            case TaskClassifyType.Challenge: typeName = "挑战"; break;
        }
        return typeName;
    }

    public void OnGUI()
    {
        if(WindowType==WindowType.Chapter)
        {
            EditorGUILayout.TextField("任务类型", GetTaskTypeName(classifyType));
            ChapterID = EditorGUILayout.IntField("章节ID",ChapterID);
            ChapterName = EditorGUILayout.TextField("章节名称", ChapterName);
            if(GUILayout.Button("添加章节"))
            {
                if(string.IsNullOrEmpty(ChapterName))
                {
                    ShowNotification(new GUIContent("章节名称不能为空"));
                    return;
                }
                if (ChapterID<1)
                {
                    ShowNotification(new GUIContent("章节ID不能小于0"));
                    return;
                }
                OnAddChapter?.Invoke(classifyType,ChapterID,ChapterName);
                Close();
            }
            return;
        }

        if (WindowType == WindowType.Task)
        {
            EditorGUILayout.TextField("任务类型", GetTaskTypeName(classifyType));
            EditorGUILayout.IntField("章节ID", ChapterID);
            EditorGUILayout.TextField("章节名称", ChapterName);
            TaskIndex= EditorGUILayout.IntField("任务序号", TaskIndex);
            ChainID= (ushort)EditorGUILayout.IntField("任务链ID", ChainID);
            TaskName=EditorGUILayout.TextField("任务名称", TaskName);
            TaskDesc=EditorGUILayout.TextField("任务描述", TaskDesc);


            if (GUILayout.Button("添加任务"))
            {
                if (string.IsNullOrEmpty(TaskName))
                {
                    ShowNotification(new GUIContent("任务名称不能为空"));
                    return;
                }
                OnAddTask?.Invoke(classifyType, ChapterID, ChapterName, TaskIndex,TaskName,TaskDesc,ChainID);
                Close();
            }
            return;
        }
        
        
        if (WindowType == WindowType.Chain)
        {
            EditorGUILayout.TextField("任务类型", GetTaskTypeName(classifyType));
            EditorGUILayout.IntField("章节ID", ChapterID);
            EditorGUILayout.TextField("章节名称", ChapterName);
            ChainID= (ushort)EditorGUILayout.IntField("任务链ID", ChainID);
            if (GUILayout.Button("任务链"))
            {
                if (ChainID<1)
                {
                    ShowNotification(new GUIContent("任务链ID 不能小于1"));
                    return;
                }
                OnAddChain?.Invoke(classifyType, ChapterID, ChapterName,ChainID);
                Close();
            }
            return;
        }



    }
}
