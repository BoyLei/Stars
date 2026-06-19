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

    [MenuItem("Tools/BattleWindow")]
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

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("技能开始"))
        {
            BattleDebugHelper.Debug(new SkillDebugData(1001, 1001.ToString(), true));
        }

        if (GUILayout.Button("技能结束"))
        {
            BattleDebugHelper.Debug(new SkillDebugData(1001, 1001.ToString(), false));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("客户端开始"))
        {
            BattleDebugHelper.Debug(new SkillStateDebugData(1001.ToString(), (++ClipIndex).ToString(),true, true));
        }

        if (GUILayout.Button("客户端结束"))
        {
            BattleDebugHelper.Debug(new SkillStateDebugData(1001.ToString(), (ClipIndex).ToString(), true, false));
        }
        if (GUILayout.Button("服务器开始"))
        {
            BattleDebugHelper.Debug(new SkillStateDebugData(1001.ToString(), (++ClipIndex).ToString(), false, true));
        }

        if (GUILayout.Button("服务器结束"))
        {
            BattleDebugHelper.Debug(new SkillStateDebugData(1001.ToString(), (ClipIndex).ToString(), false, false));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("动画开始"))
        {
            //第一个参数 技能的UID  第二个参数是动画的UID
            BattleDebugHelper.Debug(new AnimationDebugData(1001.ToString(), (++ClipIndex).ToString(),true));
        }

        if (GUILayout.Button("动画结束"))
        {
            BattleDebugHelper.Debug(new AnimationDebugData(1001.ToString(), (ClipIndex).ToString(), false));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("特效开始"))
        {
            BattleDebugHelper.Debug(new EffectDebugData(1001.ToString(), (++ClipIndex).ToString(), true));
        }

        if (GUILayout.Button("特效结束"))
        {
            BattleDebugHelper.Debug(new EffectDebugData(1001.ToString(), (ClipIndex).ToString(), false));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("音效开始"))
        {
            BattleDebugHelper.Debug(new AudioDebugData(1001.ToString(), (++ClipIndex).ToString(), true));
        }

        if (GUILayout.Button("音效结束"))
        {
            BattleDebugHelper.Debug(new AudioDebugData(1001.ToString(), (ClipIndex).ToString(), false));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("接收客户端效果"))
        {
            BattleDebugHelper.Debug(new ClientEventDebugData(1001.ToString(), (++ClipIndex).ToString()));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("服务器效果"))
        {
            BattleDebugHelper.Debug(new ServerEventDebugData(1001.ToString(), (++ClipIndex).ToString()));
        }
        GUILayout.EndHorizontal();
    }

    public void OnDestroy()
    {
        if(battleRecord!=null)
        {
            battleRecord.OnDestroy();
        }
        battleRecord = null;
    }
}
