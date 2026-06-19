///--------------------------------------------------------------------
/// 文件名   :   CreateLua
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/19 16:54:58
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using UnityEditor;
using System.Text;

public class CreateLua
{
    [MenuItem("Assets/Create/Lua Module Script", false, 80)]
    public static void CreateNewLua()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<CreateScriptAssetAction>(),
            GetSelectedPathOrFallback() + "/New LuaModule.lua.txt",
            null,
            "Assets/DevTools/UIEditor/LuaModuleTemplate.lua.txt");
    }

    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }
}

class CreateScriptAssetAction : EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        //创建资源
        UnityEngine.Object obj = CreateAssetFromTemplate(pathName, resourceFile);
        //高亮显示该资源
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }
    internal static UnityEngine.Object CreateAssetFromTemplate(string pahtName, string resourceFile)
    {
        //获取要创建的资源的绝对路径
        string fullName = Path.GetFullPath(pahtName);
        //读取本地模板文件
        StreamReader reader = new StreamReader(resourceFile);
        string content = reader.ReadToEnd();
        reader.Close();

        //获取资源的文件名
        string fileName = Path.GetFileNameWithoutExtension(pahtName);
        Debug.LogError(fileName);
        fileName= Path.GetFileNameWithoutExtension(fileName);
        Debug.LogError(fileName);
        content = content.Replace("#CLASSNAME#", fileName);
        content = content.Replace("#CREATETIME#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        content = content.Replace("#AUTHOR#", System.Environment.UserName);

        //无BOM头 UTF8
        UTF8Encoding utf8 = new UTF8Encoding(false);
        //
        //写入新文件
        StreamWriter writer = new StreamWriter(fullName, false, utf8);
        writer.Write(content);
        writer.Close();

        //刷新本地资源
        AssetDatabase.ImportAsset(pahtName);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath(pahtName, typeof(UnityEngine.Object));
    }
}
#endif