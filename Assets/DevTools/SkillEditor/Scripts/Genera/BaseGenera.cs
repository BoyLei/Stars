using Newtonsoft.Json;
///--------------------------------------------------------------------
/// 文件名   :   BaseGenera.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/20 17:27:43
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    public delegate void OnFreshDelegate(int Index, int ID);
    public class BaseGenera : MonoBehaviour
    {
        public static OnFreshDelegate freshDelegate;

        public PlayableDirector director;

        [ShowInInspector]
        [HideLabel]
        public InputKeySerialize keySerialize;

        [HideInInspector]
        public int StageIndex;

        protected int Index { get; }

        public int GetStageIndex()
        {
            return ++StageIndex;
        }


        public void OnInputKey(string guid, string key)
        {
            keySerialize.OnInput(guid, key);
        }

        public List<string> GetOutputKey()
        {
            return keySerialize.GetOutputKey();
        }

        public void ExportJson(SkillConfig config)
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (string.IsNullOrEmpty(SkillEditorGlobal.Instance.SkillJsonExportPath1))
            {
                Debug.LogError("导出目录不存在，请先配置");
                return;
            }

            if (Directory.Exists(SkillEditorGlobal.Instance.SkillJsonExportPath1))
            {
                Directory.CreateDirectory(SkillEditorGlobal.Instance.SkillJsonExportPath1);
            }
            string ExportPath = SkillEditorGlobal.Instance.SkillJsonExportPath1 + "/Skill_{0}.json";
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            SkillJson skillJson = new SkillJson();
            skillJson.config = config;
            if (director != null)
            {
                TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
                if (timelineAsset != null)
                {
                    var outputs = timelineAsset.GetRootTracks();
                    int effectIndex = 1;
                    foreach (var item in outputs)
                    {
                        if (item.name == "NormalGroup")
                        {
                            skillJson.Normals = SkillEditorUtils.ExportStageJson(item, $"SkillConfig{config.ID} NormalGroup",ref effectIndex);
                        }
                        else if (item.name == "BulletGroup")
                        {
                            skillJson.Bullets = SkillEditorUtils.ExportStageJson(item, $"SkillConfig{config.ID} BulletGroup",ref effectIndex);
                        }
                        else if (item.name == "OtherGroup")
                        {
                            skillJson.Others = SkillEditorUtils.ExportStageJson(item, $"SkillConfig{config.ID} OtherGroup", ref effectIndex);
                        }
                    }
                }
                else
                {
                    Debug.LogError("TimelineAsset is Missing");
                }

            }

            string content = Newtonsoft.Json.JsonConvert.SerializeObject(skillJson, setting);
            string path = string.Format(ExportPath, config.ID);
            WriteJson(content, path);
            Debug.Log($"导出Json成功 Path={path}");
            OnFresh(config.ID);
#endif
        }

        public void ExportJson(BuffConfig config)
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            if (string.IsNullOrEmpty(SkillEditorGlobal.Instance.BuffJsonExportPath1))
            {
                Debug.LogError("导出目录不存在，请先配置");
                return;
            }

            if (Directory.Exists(SkillEditorGlobal.Instance.BuffJsonExportPath1))
            {
                Directory.CreateDirectory(SkillEditorGlobal.Instance.BuffJsonExportPath1);
            }

            string ExportPath = SkillEditorGlobal.Instance.BuffJsonExportPath1 + "/Buff_{0}.json";
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            BuffJson buffJson = new BuffJson();
            buffJson.config = config;
            PlayableDirector director = transform.gameObject.GetComponent<PlayableDirector>();
            if (director != null)
            {
                TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
                if (timelineAsset != null)
                {
                    var outputs = timelineAsset.GetRootTracks();
                    int effectIndex = 1;
                    foreach (var item in outputs)
                    {
                        if (item.name == "NormalGroup")
                        {
                            buffJson.Normals = SkillEditorUtils.ExportStageJson(item, $"Buff{config.ID} NormalGroup",ref effectIndex);
                        }
                        else if (item.name == "BulletGroup")
                        {
                            buffJson.Bullets = SkillEditorUtils.ExportStageJson(item, $"Buff{config.ID} BulletGroup", ref effectIndex);
                        }
                        else if (item.name == "OtherGroup")
                        {
                            buffJson.Others = SkillEditorUtils.ExportStageJson(item, $"Buff{config.ID} OtherGroup", ref effectIndex);
                        }
                    }
                }
                else
                {
                    Debug.LogError("TimelineAsset is Missing");
                }
            }
            string content = Newtonsoft.Json.JsonConvert.SerializeObject(buffJson, setting);
            string path = string.Format(ExportPath, config.ID);
            WriteJson(content, path);

            Debug.Log($"导出Json成功 Path={path}");
            OnFresh(config.ID);
#endif
        }

        public void ExportJson(BulletConfig config)
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (string.IsNullOrEmpty(SkillEditorGlobal.Instance.BulletJsonExportPath1))
            {
                Debug.LogError("导出目录不存在，请先配置");
                return;
            }

            if (Directory.Exists(SkillEditorGlobal.Instance.BulletJsonExportPath1))
            {
                Directory.CreateDirectory(SkillEditorGlobal.Instance.BulletJsonExportPath1);
            }

            string ExportPath = SkillEditorGlobal.Instance.BulletJsonExportPath1 + "/Bullet_{0}.json";
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore; //设置全局的Null值处理，JsonSerializerSettings竟然没有构造函数，一点都不OOP
            //setting.ContractResolver = new NullToEmptyStringResolver();
            BulletJson bulletJson = new BulletJson();
            bulletJson.config = config;
            if (director != null)
            {
                TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
                if (timelineAsset != null)
                {
                    var outputs = timelineAsset.GetRootTracks();
                    int effectIndex = 1;
                    foreach (var item in outputs)
                    {
                        if (item.name == "NormalGroup")
                        {
                            bulletJson.Normals = SkillEditorUtils.ExportStageJson(item, $"Bullet{config.ID} NormalGroup", ref effectIndex);
                        }
                        else if (item.name == "BulletGroup")
                        {
                            bulletJson.Bullets = SkillEditorUtils.ExportStageJson(item, $"Bullet{config.ID} BulletGroup", ref effectIndex);
                        }
                        else if (item.name == "OtherGroup")
                        {
                            bulletJson.Others = SkillEditorUtils.ExportStageJson(item, $"Bullet{config.ID} OtherGroup", ref effectIndex);
                        }
                    }
                }
                else
                {
                    Debug.LogError("TimelineAsset is Missing");
                }
            }
            // bulletJson.Motions = SkillEditorUtils.ExportMotionJson(director);
            string content = Newtonsoft.Json.JsonConvert.SerializeObject(bulletJson, setting);
            string path = string.Format(ExportPath, config.ID);
            WriteJson(content, path);
            Debug.Log($"导出Json成功 Path={path}");
            OnFresh(config.ID);

#endif
        }

        public void ExportJson(PassiveSkillConfig config)
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (string.IsNullOrEmpty(SkillEditorGlobal.Instance.PassiveJsonExportPath1))
            {
                Debug.LogError("导出目录不存在，请先配置");
                return;
            }

            if (Directory.Exists(SkillEditorGlobal.Instance.PassiveJsonExportPath1))
            {
                Directory.CreateDirectory(SkillEditorGlobal.Instance.PassiveJsonExportPath1);
            }

            string ExportPath = SkillEditorGlobal.Instance.PassiveJsonExportPath1 + "/Passive_{0}.json";
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            PassiveJson passiveJson = new PassiveJson();
            passiveJson.config = config;
            if (director != null)
            {
                TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
                if (timelineAsset != null)
                {
                    var outputs = timelineAsset.GetRootTracks();
                    int effectIndex = 1;
                    foreach (var item in outputs)
                    {
                        if (item.name == "NormalGroup")
                        {
                            passiveJson.Normals = SkillEditorUtils.ExportStageJson(item, $"Passive{config.ID} NormalGroup", ref effectIndex);
                        }
                        else if (item.name == "BulletGroup")
                        {
                            passiveJson.Bullets = SkillEditorUtils.ExportStageJson(item, $"Passive{config.ID} BulletGroup", ref effectIndex);
                        }
                        else if (item.name == "OtherGroup")
                        {
                            passiveJson.Others = SkillEditorUtils.ExportStageJson(item, $"Passive{config.ID} OtherGroup", ref effectIndex);
                        }
                    }
                }
                else
                {
                    Debug.LogError("TimelineAsset is Missing");
                }
            }
            string content = Newtonsoft.Json.JsonConvert.SerializeObject(passiveJson, setting);
            string path = string.Format(ExportPath, config.ID);
            WriteJson(content, path);
            Debug.Log($"导出Json成功 Path={path}");
            OnFresh(config.ID);
#endif
        }

        public void WriteJson(string content, string path)
        {
#if UNITY_EDITOR

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            FileStream fs = new FileStream(path, FileMode.CreateNew);
            byte[] bytes = Encoding.UTF8.GetBytes(SkillEditorUtils.ConvertJsonString(content));
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        protected void OnFresh(int ID)
        {
            freshDelegate?.Invoke(Index, ID);
        }
    }

}