using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

public class AddFileHeadComment : UnityEditor.AssetModificationProcessor
{


    /// <summary>
    /// 此函数在asset被创建完，文件已经生成到磁盘上，但是没有生成.meta文件和import之前被调用
    /// </summary>
    /// <param name="newFileMeta">newfilemeta 是由创建文件的path加上.meta组成的</param>
    public static void OnWillCreateAsset(string newFileMeta)
    {
        // 只修改C#脚本
        string newFilePath = newFileMeta.Replace(".meta", "");
        if (newFilePath.EndsWith(".cs"))
        {
            string scriptContent = File.ReadAllText(newFilePath);
            // 替换字符串为系统时间
            //这里实现自定义的一些规则
            scriptContent = scriptContent.Replace("#SCRIPTFULLNAME#", Path.GetFileName(newFilePath));
            scriptContent = scriptContent.Replace("#COMPANY#", PlayerSettings.companyName);
            scriptContent = scriptContent.Replace("#PRODUCTNAME#", PlayerSettings.productName);
            scriptContent = scriptContent.Replace("#AUTHOR#", System.Environment.UserName);
            scriptContent = scriptContent.Replace("#CREATETIME#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            scriptContent = scriptContent.Replace("#VERSION#", "1.0");
            scriptContent = scriptContent.Replace("#UNITYVERSION#", Application.unityVersion);


            //无BOM头 UTF8
            UTF8Encoding utf8 = new UTF8Encoding(false);
            File.WriteAllText(newFilePath, scriptContent, utf8);
        }
    }
}
