///--------------------------------------------------------------------
/// 文件名   :   TaskMenuWindow.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/04/03 14:18:20
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TaskMenuWindow : EditorWindow
{

    //[MenuItem("Tools/任务/菜单")]
    public static void Open()
    {
        TaskMenuWindow taskPop = GetWindow<TaskMenuWindow>("功能");
        taskPop.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 200);
        taskPop.ShowPopup();
    }

    public void OnGUI()
    {
      
        if (GUILayout.Button("导出"))
        {
            TaskWindow.TaskMenuWindowEvent?.Invoke(1);
        }

        if (GUILayout.Button("提交"))
        {
            TaskWindow.TaskMenuWindowEvent?.Invoke(2);
        }

        if (GUILayout.Button("删除选中"))
        {
            TaskWindow.TaskMenuWindowEvent?.Invoke(3);
        }

        if (GUILayout.Button("复制到客户端"))
        {
            TaskWindow.TaskMenuWindowEvent?.Invoke(4);
        }

        if (GUILayout.Button("复制到服务器"))
        {
            TaskWindow.TaskMenuWindowEvent?.Invoke(5);
        }
    }
}
