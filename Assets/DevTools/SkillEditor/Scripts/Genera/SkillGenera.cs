#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SkillEditor;
using System.IO;
using System.Text;
using Cinemachine;
using Newtonsoft.Json;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;
using UnityEditor;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Service.LocalData;

public class SkillGenera : BaseGenera
{

    protected int Index { get { return 1; } }

#if UNITY_EDITOR
    //[Button("保存到本地服务器")]
    public void ExportServerSkill()
    {
        //string ExportPath = SkillEditorGlobal.Instance.SkillJsonExportPath + "/Skill_{0}.json";
        //string locPath = string.Format(ExportPath, config.ID);
        //ExportPath = Path.Combine(Application.dataPath + "/../", SkillEditorGlobal.Instance.ServerJsonExportPath + "/Skill/Skill_{0}.json");
        //string srvPath = string.Format(ExportPath, config.ID);
        //File.Copy(locPath, srvPath, true);
        //Debug.Log($"拷贝Json成功 Path={srvPath}");
    }

    //[Button("技能及时生效")]
    public void RefreshSkill()
    {
        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();
        //if (config.ID < 1000)
        //{
        //    var pcg = (GameManager.Instance.mainPlayerCtrlBase as PlayerCtrlGroup);
        //    pcg.GetSkillDispather()?.SkillUnitController?.RefreshSkillInfo(config.ID);
        //}
        //else
        //{
        //    LocalDataManager.Instance.GetSkillJson(config.ID, null, true, true);
        //}
        //Debug.Log("刷新内存数据");
    }
#endif
}
#endif