///--------------------------------------------------------------------
/// 文件名   :   BulletGenera.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/09 11:20:47
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarProject.Service.LocalData;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [HideMonoScript]
    public class BulletGenera : BaseGenera
    {
        protected int Index { get { return 3; } }

#if UNITY_EDITOR
        //[Button("保存到本地服务器")]
        public void ExportServer()
        {
            //string ExportPath = SkillEditorGlobal.Instance.BulletJsonExportPath + "/Bullet_{0}.json";
            //string locPath = string.Format(ExportPath, config.ID);
            //ExportPath = Path.Combine(Application.dataPath + "/../", SkillEditorGlobal.Instance.ServerJsonExportPath + "/Bullet/Bullet_{0}.json");
            //string srvPath = string.Format(ExportPath, config.ID);
            //File.Copy(locPath, srvPath, true);
        }

        //[Button("子弹及时生效")]
        public void RefreshBullet()
        {
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
            //LocalDataManager.Instance.GetBulletJson(config.ID, null, true);
            //Debug.Log("刷新内存数据");
        }
#endif
    }
}
#endif