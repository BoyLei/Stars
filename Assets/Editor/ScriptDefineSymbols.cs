///--------------------------------------------------------------------
/// 文件名   :   ScriptDefineSymbols.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/16 18:33:27
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ScriptDefineSymbols : OdinEditorWindow
{
    [MenuItem("Tools/ScriptDefineSymbols")]
    public static void OpenWindow()
    {
        var window = GetWindow<ScriptDefineSymbols>();
        window.Show();
    }

    [LabelText("宏定义")]
    public List<string> Defines = new List<string>();

    [Button("应用")]
    public void Apply()
    {
        StringBuilder builder = new StringBuilder();
        if(Defines!=null && Defines.Count>0)
        {
            foreach (var item in Defines)
            {
                builder.Append(item);
                builder.Append(";");
            }
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,builder.ToString());
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Defines.Clear();
        string temps = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!string.IsNullOrEmpty(temps))
        {
            string[] des = temps.Split(';');
            if (des != null)
            {
                foreach (var item in des)
                {
                    Defines.Add(item);
                }
            }
        }
    }



}
