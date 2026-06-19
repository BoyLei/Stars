///--------------------------------------------------------------------
/// 文件名   :   SkillMenu.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/06 09:20:31
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class SkillMenu
{

    static private Transform Parent
    {
        get
        {
            return SkillEditorGlobal.Instance.transform;
        }
    }

    private static GameObject CreateEmptyGameObject(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(Parent);
        go.transform.position = Vector3.zero;
        go.transform.rotation = quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    [MenuItem("GameObject/技能编辑器/配置/生成枚举", false, 0)]

    static public void CreateEnum(MenuCommand menuCommand)
    {
        EnumDeineGenera.CreateEnum();
    }

    [MenuItem("GameObject/技能编辑器/配置/生成结构体", false, 0)]

    static public void CreateClass(MenuCommand menuCommand)
    {
        SkillConfigGenera.CreateConfig();
    }

    [MenuItem("GameObject/技能编辑器/配置/清理无引用代码", false, 0)]

    static public void GeneraClear(MenuCommand menuCommand)
    {
        // ClearGenera.CreateConfig();
        ClearGenera.ExecuteClear();
    }

    [MenuItem("GameObject/技能编辑器/资源浏览", false, 0)]
    static public void OpenExporerWindow()
    {
        ExplorerWindow.OpenWindow();
    }
    //ExecuteClear

    [InitializeOnLoadMethod]
    static void StartInitializeOnLoadMethod()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        if (Event.current != null && selectionRect.Contains(Event.current.mousePosition)
            && Event.current.button == 1 && Event.current.type <= EventType.MouseUp)
        {
            GameObject selectedGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (selectedGameObject && selectedGameObject.name == "SkillEditor")
            {
                Vector2 mousePosition = Event.current.mousePosition;

                EditorUtility.DisplayPopupMenu(new Rect(mousePosition.x, mousePosition.y, 0, 0), "GameObject/技能编辑器", null);
                Event.current.Use();
            }
        }
    }
}
