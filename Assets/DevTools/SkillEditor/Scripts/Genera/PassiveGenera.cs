///--------------------------------------------------------------------
/// 文件名   :   PassiveGenera.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/21 11:05:55
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

#if UNITY_EDITOR
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Service.LocalData;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillEditor
{
    public class PassiveGenera : BaseGenera
    {
        protected int Index { get { return 4; } }

#if UNITY_EDITOR
        //[Button("保存到本地服务器")]
        public void ExportServer()
        {
            //string ExportPath = SkillEditorGlobal.Instance.PassiveJsonExportPath + "/Passive_{0}.json";
            //string locPath = string.Format(ExportPath, config.ID);
            //ExportPath = Path.Combine(Application.dataPath + "/../", SkillEditorGlobal.Instance.ServerJsonExportPath + "/Passive/Passive_{0}.json");
            //string srvPath = string.Format(ExportPath, config.ID);
            //File.Copy(locPath, srvPath, true);
        }

        [Button("被动技能及时生效")]
        public void RefreshBullet()
        {
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
            //LocalDataManager.Instance.GetPassiveJson(config.ID, null, true);

            //Debug.Log("刷新内存数据");
        }
#endif
    }

}
#endif