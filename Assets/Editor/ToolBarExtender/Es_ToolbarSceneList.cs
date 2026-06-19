using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Es_ToolbarSceneList
{
    //public class Es_SceneAssetNameAndReference
    //{
    //    public string Name;
    //    public string Path;
    //    public SceneAsset
    //}

    private static string[] searchFolders = { "Assets/Res/Map", "Assets/ArtWorkSpace/Scenes/CommonObstacle/", "Assets/ArtWorkSpace/Scenes/Scene" };

    public static Dictionary<string, string> ScenesCache = new Dictionary<string, string>();

    static Es_ToolbarSceneList()
    {
        FetchAllScenes();
    }
    public static void OnToolbarGUI()
    {
        //设置按钮底色
        var backgroundoldColor = GUI.backgroundColor;
        GUI.backgroundColor = Es_ToolbarStyle.EstoolbarSceneListButtonBGColor;

        var rect = GUILayoutUtility.GetRect(new GUIContent("场景导览"), Es_ToolbarStyle.EstoolbarSceneListButton);
        if (GUI.Button(rect, new GUIContent("场景导览"), Es_ToolbarStyle.EstoolbarSceneListButton))
        {
            var dropdown = new SceneListDropdown(new AdvancedDropdownState());
            dropdown.SetSize(new Vector2(200, 300));
            dropdown.Show(rect);
        }

        GUI.backgroundColor = backgroundoldColor;
    }

    public static void FetchAllScenes()
    {
        ScenesCache.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Scene", searchFolders);
        var allScenesNameList = new List<string>();

        foreach (string guid in guids)
        {
            var secnePath = AssetDatabase.GUIDToAssetPath(guid);
            //string path = secnePath.Split('.')[0];
            //var temp = path.Split('/');
            //string name = string.Format("[{0}]  {1}", temp[temp.Length - 2], temp[temp.Length - 1]);
            var name = System.IO.Path.GetFileNameWithoutExtension(secnePath);
            ScenesCache[name] = secnePath;
        }
    }
}

class SceneListDropdown : AdvancedDropdown
{
    public SceneListDropdown(AdvancedDropdownState state) : base(state)
    {
        BuildRoot();
    }
    public void SetSize(Vector2 size)
    {
        minimumSize = size;
    }

    const string DropDownItem_ReloadSceneList = "【Reload Scene List】";

    private Dictionary<string, string> sceneDic;

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Scene List");
        var refrash = new AdvancedDropdownItem(DropDownItem_ReloadSceneList);
        sceneDic = Es_ToolbarSceneList.ScenesCache;
        if (sceneDic == null)
        {
            root.AddChild(refrash);
            return root;
        }

        var levelScenes = new AdvancedDropdownItem("Levels");
        //AdvancedDropdownItem levelSubItemNode_ld = null;
        foreach (var sceneData in sceneDic)
        {
            var path = sceneData.Value;
            var name = sceneData.Key;
            if (path.StartsWith("Assets/Res/Map/"))
            {
                //if (levelSubItemNode_ld == null)
                //{
                //    levelSubItemNode_ld = new AdvancedDropdownItem("Levels");
                //    levelScenes.AddChild(levelSubItemNode_ld);
                //}
                levelScenes.AddChild(new AdvancedDropdownItem(name));
            }
        }

        var commonObstacleScenes = new AdvancedDropdownItem("Common Obstacles");
        //AdvancedDropdownItem levelSubItemNode_ld = null;
        foreach (var sceneData in sceneDic)
        {
            var path = sceneData.Value;
            var name = sceneData.Key;
            if (path.StartsWith("Assets/ArtWorkSpace/Scenes/CommonObstacle/"))
                commonObstacleScenes.AddChild(new AdvancedDropdownItem(name));
        }

        var artScenes = new AdvancedDropdownItem("ArtScenes");
        //AdvancedDropdownItem levelSubItemNode_ld = null;
        foreach (var sceneData in sceneDic)
        {
            var path = sceneData.Value;
            var name = sceneData.Key;
            if (path.StartsWith("Assets/ArtWorkSpace/Scenes/Scene/"))
                artScenes.AddChild(new AdvancedDropdownItem(name));
        }

        root.AddChild(levelScenes);
        root.AddChild(commonObstacleScenes);
        root.AddChild(artScenes);
        root.AddChild(refrash);
        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        if (item.name == DropDownItem_ReloadSceneList)
        {
            Es_ToolbarSceneList.FetchAllScenes();
            return;
        }
        if (sceneDic.ContainsKey(item.name))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(sceneDic[item.name]);
        }
    }
}
