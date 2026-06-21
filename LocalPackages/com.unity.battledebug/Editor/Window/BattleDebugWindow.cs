///--------------------------------------------------------------------
/// 文件名   :   BattleDebugWindow.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/02 09:49:28
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using UnityEngine;
using System.Collections.Generic;
using BattleDebug;

public class BattleDebugWindow : EditorWindow
{

    private  BattleRecord battleRecord;

    int SkillID = 1001;

    int ClipIndex = 1;

    [MenuItem("Window/BattleWindow")]
    public static void Open()
    {
        BattleDebugWindow window = GetWindow<BattleDebugWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }


    public void OnGUI()
    {

        if(battleRecord==null)
        {
            battleRecord = new BattleRecord();
        }

        GUILayout.BeginHorizontal("技能");
        if (GUILayout.Button("技能开始"))
        {
            BattleDebugHelper.Debug(new SkillDebugData(1001,true));
        }

        if (GUILayout.Button("技能结束"))
        {
            BattleDebugHelper.Debug(new SkillDebugData(1001, false));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("动画");
        if (GUILayout.Button("动画开始"))
        {
            BattleDebugHelper.Debug(new AnimationDebugData(1001, (ClipIndex++).ToString(),true));
        }

        if (GUILayout.Button("动画结束"))
        {
            BattleDebugHelper.Debug(new AnimationDebugData(1001, (ClipIndex).ToString(), false));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("特效");
        if (GUILayout.Button("特效开始"))
        {
            BattleDebugHelper.Debug(new EffectDebugData(1001, (ClipIndex++).ToString(), true));
        }

        if (GUILayout.Button("特效结束"))
        {
            BattleDebugHelper.Debug(new EffectDebugData(1001, (ClipIndex).ToString(), false));
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal("音效");
        if (GUILayout.Button("音效开始"))
        {
            BattleDebugHelper.Debug(new AudioDebugData(1001, (ClipIndex++).ToString(), true));
        }

        if (GUILayout.Button("音效结束"))
        {
            BattleDebugHelper.Debug(new AudioDebugData(1001, (ClipIndex).ToString(), false));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("客户端效果");
        if (GUILayout.Button("接收客户端效果"))
        {
            BattleDebugHelper.Debug(new ClientEventDebugData(1001, (ClipIndex++).ToString()));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("服务器效果");
        if (GUILayout.Button("服务器效果"))
        {
            BattleDebugHelper.Debug(new ServerEventDebugData(1001, (ClipIndex++).ToString()));
        }
        GUILayout.EndHorizontal();
    }

    public void OnDestroy()
    {
        battleRecord = null;
    }
}
