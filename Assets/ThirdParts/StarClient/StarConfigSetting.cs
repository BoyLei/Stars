#if UNITY_EDITOR
///--------------------------------------------------------------------
/// 文件名   :   StarConfigSetting.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/18 10:57:16
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MapEditor;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName ="StarConfig",menuName ="Assets/StarConfig")]
public class StarConfigSetting : ScriptableObject
{
    [FoldoutGroup("ExcelSetting")]
    [FilePath(AbsolutePath =true)]
    [LabelText("Excel转表批处理")]
    public string ExcelBatPath;

    [FoldoutGroup("ExcelSetting")]
    [FolderPath(AbsolutePath = true)]
    [LabelText("Excel转表Warp文件路径")]
    public string ExcelCodePath;

    [FoldoutGroup("ExcelSetting")]
    [FolderPath(AbsolutePath = true)]
    [LabelText("Excel转表Json文件路径")]
    public string ExcelJsonPath;

    [FoldoutGroup("ExcelSetting")]
    [FolderPath(AbsolutePath = true)]
    [LabelText("Excel转表Lua文件路径")]
    public string ExcelLuaPath;



    [FoldoutGroup("ExcelSetting")]
    [Button("应用配置")]
    public void ApplyExcel()
    {
        if (!string.IsNullOrEmpty(ExcelBatPath))
        {
            if (File.Exists(ExcelBatPath))
            {

                string[] files = File.ReadAllLines(ExcelBatPath, System.Text.Encoding.UTF8);
                if (files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].StartsWith("@set CCODE="))
                        {
                            files[i] = $"@set CCODE={ExcelCodePath}";
                        }

                        if (files[i].StartsWith("@set LUACODE="))
                        {
                            files[i] = $"@set LUACODE={ExcelLuaPath}";
                        }

                        if (files[i].StartsWith("@set CJSON="))
                        {
                            files[i] = $"@set CJSON={ExcelJsonPath}";
                        }


                    }

                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var item in files)
                    {
                        stringBuilder.Append(item + "\n");
                    }

                    File.Delete(ExcelBatPath);

                    FileStream fs = new FileStream(ExcelBatPath, FileMode.CreateNew);
                    byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                }
            }
        }
        Debug.Log("应用配置");
    }

    [FoldoutGroup("ExcelSetting")]
    [Button("更新配置文件")]
    public void UpdateExcel()
    {
        if(!string.IsNullOrEmpty(ExcelBatPath))
        {
            string Directory = System.IO.Path.GetDirectoryName(ExcelBatPath);

            if (!string.IsNullOrEmpty(Directory))
            {
                MapEditor.MapEditorUtils.RunBat(EditorConfigUtils.TortoiseProc, string.Format($"/command:update /path:{Directory}"));

            }
        }
        else
        {
            Debug.LogError("请配置Assets/ThirdParts/StarClient/StarConfig.asset");
        }
        Debug.Log("更新配置文件");
    }

    [FoldoutGroup("ExcelSetting")]
    [Button("转表")]
    public void ExecuteExcel()
    {
        if(!string.IsNullOrEmpty(ExcelBatPath))
        {
            MapEditor.MapEditorUtils.RunBat(ExcelBatPath, string.Empty);
        }
        else
        {
            Debug.LogError("请配置Assets/ThirdParts/StarClient/StarConfig.asset");
        }
        Debug.Log("转表");
    }


    [FoldoutGroup("ProtoBuffSetting")]
    [LabelText("ProtoBuff配置文件")]
    [FilePath(AbsolutePath = true)]
    public string ProtoBuffConfigPath;

    [FoldoutGroup("ProtoBuffSetting")]
    [LabelText("ProtoBuff批处理")]
    [FilePath(AbsolutePath = true)]
    public string ProtoBuffBatPath;

    [FoldoutGroup("ProtoBuffSetting")]
    [FolderPath(AbsolutePath = true)]
    [LabelText("ProtoBuff协议文件路径")]
    public string ProtoBuffCodePath;



    [FoldoutGroup("ProtoBuffSetting")]
    [Button("应用配置")]
    public void ApplyProtoBuff()
    {
        if (!string.IsNullOrEmpty(ProtoBuffConfigPath))
        {
            if (File.Exists(ProtoBuffConfigPath))
            {

                string[] files = File.ReadAllLines(ProtoBuffConfigPath, System.Text.Encoding.UTF8);
                if (files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].StartsWith("csharp_dir="))
                        {
                            files[i] = $"csharp_dir={ProtoBuffCodePath}";
                        }
                    }

                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var item in files)
                    {
                        stringBuilder.Append(item + "\n");
                    }

                    File.Delete(ProtoBuffConfigPath);

                    FileStream fs = new FileStream(ProtoBuffConfigPath, FileMode.CreateNew);
                    byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                }
            }
        }
        Debug.Log("应用配置");
    }

    [FoldoutGroup("ProtoBuffSetting")]
    [Button("更新协议")]
    public void UpdateProtoBuff()
    {
        if(!string.IsNullOrEmpty(ProtoBuffBatPath))
        {
            string Directory = System.IO.Path.GetDirectoryName(ProtoBuffBatPath);
            if (!string.IsNullOrEmpty(Directory))
            {
                MapEditor.MapEditorUtils.RunBat(EditorConfigUtils.TortoiseProc, string.Format($"/command:update /path:{Directory}"));
            }
        }
        else
        {
            Debug.LogError("请配置Assets/ThirdParts/StarClient/StarConfig.asset");
        }
        Debug.Log("更新协议");
    }


    [FoldoutGroup("ProtoBuffSetting")]
    [Button("转协议")]
    public void ExecuteProtoBuff()
    {
        if(!string.IsNullOrEmpty(ProtoBuffBatPath))
        {
            MapEditor.MapEditorUtils.RunBat(ProtoBuffBatPath, string.Empty);
        }
        else
        {
            Debug.LogError("请配置Assets/ThirdParts/StarClient/StarConfig.asset");
        }
        Debug.Log("转协议");
    }


}
#endif