///--------------------------------------------------------------------
/// 文件名   :   ExplorerManager.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/09 10:43:47
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarProject.OffLine;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    [System.Serializable]
    public class ExplorerManager
    {
        [LabelText("$name")]
        public List<ExplorerItem> Items = new List<ExplorerItem>();
    }

    [System.Serializable]

    /// <summary>
    /// 每个ExplorerItem存有这个ID下各种文件（prefab、timeline、json）的路径
    /// 这四个文件相互绑定，四个文件缺一不可，如果某一个缺了，说明在建立ID的时候出现了问题，或者在外部删除了某个文件
    /// </summary>
    /// <returns></returns>
    public class ExplorerItem
    {
        /// <summary>
        /// 通知技能编辑器刷新
        /// </summary>
        [HideInInspector]
        [JsonIgnore]
        public System.Action OnSkillEditorReInit;

        /// <summary>
        /// 创建技能的委托
        /// </summary>
        [HideInInspector]
        [JsonIgnore]
        public System.Action<BattleRuntimeTypeEnum, int, ExplorerItem> CreateSkill;

        /// <summary>
        /// 对应实例物体
        /// </summary>
        [HideInInspector]
        [JsonIgnore]
        public GameObject ExplorerItemObj;

        [HideInInspector]
        [JsonIgnore]
        public GameObject ExplorereItemModel;

        /// <summary>
        /// 当前运行时的类型
        /// </summary>
        [HideInInspector]
        public BattleRuntimeTypeEnum RunTimeType;

        /// <summary>
        /// 当前运行时是否被选择
        /// </summary>
        [HideInInspector]
        public bool Select;

        [HideInInspector]
        public string PrefabPath;

        [HideInInspector]
        public string TimelinePath;

        [HideInInspector]
        public string JsonPath;

        [PropertyOrder(1)]
        [OnValueChanged("OnChangeID"), DelayedProperty]
        [LabelText("修改ID")]
        public int ID;

        [PropertyOrder(2)]
        [HideIf("@GetTypeEnum()!=1")]
        [LabelText("技能配置")]
        public SkillConfig skillConfig = new SkillConfig();

        [PropertyOrder(2)]
        [HideIf("@GetTypeEnum()!=2")]
        [LabelText("BUFF配置")]
        public BuffConfig buffConfig = new BuffConfig();

        [PropertyOrder(2)]
        [HideIf("@GetTypeEnum()!=3")]
        [LabelText("子弹配置")]
        public BulletConfig bulletConfig = new BulletConfig();

        [PropertyOrder(2)]
        [HideIf("@GetTypeEnum()!=4")]
        [LabelText("被动配置")]
        public PassiveSkillConfig passiveSkillConfig = new PassiveSkillConfig();

#if UNITY_EDITOR
        [ButtonGroup("Export")]
        [PropertyOrder(2.5f)]
        [Button("导出技能")]
        public void ExportJson()
        {
#if UNITY_EDITOR
            string Name = System.IO.Path.GetFileNameWithoutExtension(PrefabPath);

            var old = SkillEditorGlobal.Instance.transform.Find(Name);
            if (old == null)
            {
                LoadPrefab(false);
                old = SkillEditorGlobal.Instance.transform.Find(Name);
            }

            UnityEditor.EditorGUIUtility.PingObject(old.gameObject);
            UnityEditor.Selection.activeGameObject = old.gameObject;
            ExplorerItemObj = old.gameObject;

            SaveAsPrefab();
            switch (RunTimeType)
            {
                case BattleRuntimeTypeEnum.Skill:
                    ExplorerItemObj.GetComponent<BaseGenera>().ExportJson(skillConfig);
                    break;
                case BattleRuntimeTypeEnum.Buff:
                    ExplorerItemObj.GetComponent<BaseGenera>().ExportJson(buffConfig);
                    break;
                case BattleRuntimeTypeEnum.Bullet:
                    ExplorerItemObj.GetComponent<BaseGenera>().ExportJson(bulletConfig);
                    break;
                case BattleRuntimeTypeEnum.Passive:
                    ExplorerItemObj.GetComponent<BaseGenera>().ExportJson(passiveSkillConfig);
                    break;
            }

            if (!string.IsNullOrEmpty(skillConfig.SkillIconDesc) || (buffConfig.AddEffectFlys != null && buffConfig.AddEffectFlys.Count > 0) )
            {
                ExportLanguageStrBySkillEditorSave();
            }

            UnityEditor.AssetDatabase.Refresh();
#endif
        }

#if UNITY_EDITOR
        private static List<SkillExport> m_taskTextStr = new();
        public class TaskExportArr
        {
            public int ID;
            public string Key;
            public string Val;
            public string Des;

            public void SetData(int id, string key, string val, string des)
            {
                ID = id;
                Key = key;
                Val = val;
                Des = des;
            }

            public void ExportExcel(int row, OfficeOpenXml.ExcelRange excel)
            {
                int index = 1;
                excel[row, index++].Value = Key;
                excel[row, index++].Value = Val;
                excel[row, index++].Value = Des;
                excel[row, index++].Value = ID;
            }
        }

        public class SkillExport
        {
            public int ID;
            // 技能Icon描述
            public TaskExportArr SkillIconDesc = new();
            public void SetSkillIconDesc(string key, string val, string des)
            {
                SkillIconDesc.SetData(ID, key, val, des);
            }
        }

        /// <summary>
        /// 技能编辑器保存时自动导出多语言xlsm文件，及生成对应的策划目录目录下的json明文对应的key
        /// </summary>
        public static void ExportLanguageStrBySkillEditorSave()
        {
            Release();
            //将源数据写入languageKey
            string clientConfigDir = Application.dataPath + "/DevTools/SkillEditor/Export/Json/";
            ExportTaskText(clientConfigDir);
            ////多语言excel生成
            string designDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, @"..\..\..\..\StarsProject_Design\trunk\配置文件"));
            //string languageExcelDir = designDir + "\\编辑器文本";
            WriteSkillExcel(designDir);
            Release();

            //再保存策划目录
            //string designConfigDir = designDir + "\\battle";
            //ExportTaskText(designConfigDir);
            //Release();
        }

        private static void WriteSkillExcel(string designDir)
        {
            string sourceFolderPath = designDir + "\\多语言工具\\source";
            // 检查文件夹是否存在
            if (!Directory.Exists(sourceFolderPath))
            {
                Debug.LogError($"停止生成多语言表，未找到\"\\多语言工具\\source\"文件夹");
                return;
            }

            var path = sourceFolderPath + "\\技能编辑器文本_SkillEditorText.xlsx";
            if (path != "")
            {
                FileInfo newFile = new(path);
                if (newFile.Exists)
                {
                    //创建一个新的excel文件
                    newFile.Delete();
                    newFile = new FileInfo(path);
                }

                //通过ExcelPackage打开文件
                using (OfficeOpenXml.ExcelPackage package = new(newFile))
                {
                    //在excel空文件添加新sheet
                    OfficeOpenXml.ExcelWorksheet taskConfig = package.Workbook.Worksheets.Add("SkillEditorText");
                    int index = 1;
                    taskConfig.Cells[1, index].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    taskConfig.Cells[1, index++].Value = "多语言key";
                    taskConfig.Cells[1, index++].Value = "文本";
                    taskConfig.Cells[1, index++].Value = "类型";
                    taskConfig.Cells[1, index++].Value = "ID";

                    int row = 6;
                    foreach (var item in m_taskTextStr)
                    {
                        item.SkillIconDesc.ExportExcel(row++, taskConfig.Cells);
                    }

                    taskConfig.Cells.AutoFitColumns();
                    //保存excel
                    package.Save();

                    Debug.Log($"一共:{m_taskTextStr.Count}条\n\n\t导出成功");
                    //EditorUtility.DisplayDialog("消息", $"一共:{m_taskTextStr.Count}条\n\n\t导出成功", "您辛苦了");
                }
            }
        }

        private static void Release()
        {
            m_taskTextStr.Clear();
        }

        private static void ExportTaskText(string sourceDir)
        {
            m_taskTextStr.Clear();

            string[] skillFiles = Directory.GetFiles(sourceDir+"/Skill", "*.json", SearchOption.AllDirectories);
            string[] buffFiles = Directory.GetFiles(sourceDir + "/Buff", "*.json", SearchOption.AllDirectories);

            foreach (var item in skillFiles)
            {
                var textAsset = File.ReadAllText(item);
                var skillConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<SkillJson>(textAsset);

                if (skillConfig != null && !string.IsNullOrEmpty(skillConfig.config.SkillIconDesc)  && !string.IsNullOrWhiteSpace(skillConfig.config.SkillIconDesc))
                {
                    SkillExport taskExport = new();
                    // 基础信息
                    int id = skillConfig.config.ID; // 技能ID
                    taskExport.ID = id;
                    taskExport.SetSkillIconDesc($"SkillIconDesc_{skillConfig.config.ID}", skillConfig.config.SkillIconDesc, "Skill");
                    //skillConfig.config.SkillIconDesc_Key = $"SkillIconDesc_{skillConfig.config.ID}";
                    m_taskTextStr.Add(taskExport);

                    JsonSerializerSettings setting = new JsonSerializerSettings();
                    setting.NullValueHandling = NullValueHandling.Ignore;
                    string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(skillConfig, setting);
                    jsonstr = ConvertJsonString(jsonstr);

                    //string newJson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonAsset, Newtonsoft.Json.Formatting.Indented,);
                    //if (File.Exists(item))
                    //{
                    //    File.Delete(item);
                    //}
                    //File.WriteAllText(item, jsonstr);
                }
            }

            foreach (var item in buffFiles)
            {
                var textAsset = File.ReadAllText(item);
                var buffConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<BuffJson>(textAsset);

                if (buffConfig != null && buffConfig.config.AddEffectFlys != null && buffConfig.config.AddEffectFlys.Count > 0)
                {
                    int i = 1;
                    foreach (var fly in buffConfig.config.AddEffectFlys)
                    {
                        SkillExport taskExport = new();
                        // 基础信息
                        int id = buffConfig.config.ID; // 技能ID
                        taskExport.ID = id;
                        taskExport.SetSkillIconDesc($"AddEffectFly_{id}_{i}", fly.Value, "Buff");
                        //fly.Value_Key = $"AddEffectFly_{id}_{i}";
                        m_taskTextStr.Add(taskExport);
                        i++;
                    }

                    JsonSerializerSettings setting = new JsonSerializerSettings();
                    setting.NullValueHandling = NullValueHandling.Ignore;
                    string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(buffConfig, setting);
                    jsonstr = ConvertJsonString(jsonstr);

                    //if (File.Exists(item))
                    //{
                    //    File.Delete(item);
                    //}
                    //File.WriteAllText(item, jsonstr);
                }
            }
        }

        static public string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

#endif

        [ButtonGroup("OpenAsset")]
        [PropertyOrder(3)]
        [Button("打开Json路径")]
        public void OpenJsonFile()
        {
#if UNITY_EDITOR
            System.Diagnostics.Process.Start("explorer.exe", "/select," + JsonPath.Replace("/", "\\"));
#endif
        }

        [ButtonGroup("OpenAsset")]
        [PropertyOrder(3)]
        [Button("打开资源路径")]
        public void OpenPrefabFile()
        {
#if UNITY_EDITOR
            System.Diagnostics.Process.Start("explorer.exe", "/select," + PrefabPath.Replace("/", "\\"));
#endif
        }

        [ButtonGroup("3")]
        [PropertyOrder(4)]
        [Button("导入当前动作")]
        public void ImportAnim()
        {
            if (string.IsNullOrEmpty(SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].AnimsPath))
            {
                UnityEngine.Debug.Log("路径为空，请配置路径");
                return;
            }
            string animFile = $"Assets/Res/{SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].AnimsPath}";
            ImportAnimWithPath(animFile);
        }

        [ButtonGroup("3")]
        [PropertyOrder(4)]
        [Button("导入当前特效")]
        public void ImportEffect()
        {
            if (string.IsNullOrEmpty(SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].EffectsPath))
            {
                UnityEngine.Debug.Log("路径为空，请配置路径");
                return;
            }
            string effectFile = $"Assets/Res/{SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].EffectsPath}";
            ImportEffectWithPath(effectFile);
        }

        [ButtonGroup("4")]
        [PropertyOrder(4)]
        [Button("进入动作文件夹")]
        public void OpenAnimFile()
        {
            if (string.IsNullOrEmpty(SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].AnimsPath))
            {
                UnityEngine.Debug.Log("路径为空，请配置路径");
                return;
            }
            string animFile = $"Assets/Res/{SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].AnimsPath}";
            UnityEngine.Debug.Log(SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].AnimsPath);
            UnityEngine.Debug.Log(animFile);
            if (Directory.Exists(animFile))
            {
                UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(animFile));
            }
            else
            {
                UnityEngine.Debug.Log("请先在Res下建立资源文件夹");
            }
        }

        [ButtonGroup("4")]
        [PropertyOrder(4)]
        [Button("进入特效文件夹")]
        public void OpenEffectFile()
        {
            if (string.IsNullOrEmpty(SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].EffectsPath))
            {
                UnityEngine.Debug.Log("路径为空，请配置路径");
                return;
            }
            string effectFile = $"Assets/Res/{SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].EffectsPath}";
            if (Directory.Exists(effectFile))
            {
                UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(effectFile));
            }
            else
            {
                UnityEngine.Debug.Log("请先在Res下建立资源文件夹");
            }
        }

        [ButtonGroup("5")]
        [PropertyOrder(5)]
        [Button("进入美术动作文件夹")]
        public void OpenArtAnimFile()
        {
            if (string.IsNullOrEmpty(SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].AnimsPath))
            {
                UnityEngine.Debug.Log("路径为空，请配置路径");
                return;
            }
            string animFile = $"Assets/ArtWorkSpace/{SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].AnimsPath.Replace("Roles/", "Roles/World/").Replace("Animation/", "")}/Animation";
            if (Directory.Exists(animFile))
            {
                UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(animFile));
            }
            else
            {
                UnityEngine.Debug.Log("请先在美术资源下建立资源文件夹");
            }
        }

        [ButtonGroup("5")]
        [PropertyOrder(5)]
        [Button("进入美术特效文件夹")]
        public void OpenArtEffectFile()
        {
            if (string.IsNullOrEmpty(SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].EffectsPath))
            {
                UnityEngine.Debug.Log("路径为空，请配置路径");
                return;
            }
            //Assets/Res/Effects/Roles/Monster/Partner/DunN/cm
            //Assets/ArtWorkSpace/Effects/Prefabs/Roles/Monster/Partner/DunN/cm
            string effectFile = $"Assets/ArtWorkSpace/{SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()].EffectsPath.Replace("Effects/", "Effects/Prefabs/")}";
            if (Directory.Exists(effectFile))
            {
                UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(effectFile));
            }
            else
            {
                UnityEngine.Debug.Log("请先在Res下建立资源文件夹");
            }
        }

        /// <summary>
        /// 根据路径导入特效
        /// </summary>
        /// <param name="effectFile"></param>
        public void ImportEffectWithPath(string effectFile)
        {
            UnityEngine.Debug.Log(effectFile);
            if (!Directory.Exists(effectFile))
            {
                UnityEngine.Debug.Log("请先在Res下建立资源文件夹");
                if (GetPathToRole(effectFile, "Roles") != null)
                {
                    ImportEffectWithPath(GetPathToRole(effectFile, "Roles"));
                }
                return;
            }
            if (GetPathToRole(effectFile, "Roles") == null)
            {
                UnityEngine.Debug.Log("已到根目录，导入结束");
                return;
            }
            var artEffectFile = effectFile.Replace("Res/Effects", "ArtWorkSpace/Effects/Prefabs");
            UnityEngine.Debug.Log(artEffectFile);
            if (Directory.Exists(artEffectFile))
            {
                List<string> effectPaths = new List<string>();
                var artEffects = GetPrefabWithParentFile(artEffectFile);
                if (artEffects != null && Directory.Exists(artEffectFile))
                {
                    foreach (var item in artEffects)
                    {
                        Directory.CreateDirectory(effectFile);
                        GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(item) as GameObject;
                        UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, effectFile + "/" + item.name + ".prefab");
                        GameObject.DestroyImmediate(obj);
                        effectPaths.Add($"{effectFile}/{item.name}.prefab");
                    }
                    var effects = Directory.GetFiles(effectFile);
                    if (effects.Length > effectPaths.Count() * 2)
                    {
                        foreach (var item in effects)
                        {
                            if (!effectPaths.Any((a) => a == item.Replace("\\", "/")))
                            {
                                UnityEditor.AssetDatabase.DeleteAsset(item.Replace("\\", "/"));
                            }
                        }
                    }
                    else if (effects.Length < effectPaths.Count() * 2)
                    {
                        UnityEngine.Debug.LogError("快查查吧，导出的文件都少了");
                    }
                    UnityEngine.Debug.Log("已导入美术特效文件");
                }
                else
                { UnityEngine.Debug.LogError("未新建对应文件夹，且未找到美术特效文件"); }
            }
            if (GetPathToRole(effectFile, "Roles") != null)
            {
                ImportEffectWithPath(GetPathToRole(effectFile, "Roles"));
            }
            UnityEditor.AssetDatabase.Refresh();
        }

        /// <summary>
        /// 根据路径导入动作
        /// </summary>
        /// <param name="animFile"></param>
        public void ImportAnimWithPath(string animFile)
        {
            UnityEngine.Debug.Log(animFile);
            if (!Directory.Exists(animFile))
            {
                UnityEngine.Debug.Log("请先在Res下建立资源文件夹");
                if (GetPathToRole(animFile, "Roles") != null)
                {
                    ImportAnimWithPath(GetPathToRole(animFile, "Roles"));
                }
                return;
            }
            var artAnimFile = animFile.Replace("Res/Animation/Roles", "ArtWorkSpace/Roles/World") + "/Animation";
            if (GetPathToRole(animFile, "Roles") == null)
            {
                UnityEngine.Debug.Log("已到根目录，导入结束");
                return;
            }
            List<string> clipPaths = new List<string>();
            if (Directory.Exists(artAnimFile))
            {
                var artClips = GetClipsWithParentFile(artAnimFile);
                if (artClips != null)
                {
                    Directory.CreateDirectory(animFile);
                    foreach (var item in artClips)
                    {
                        //UnityEngine.Debug.Log(animFile + "/" + item.name + ".anim");
                        AnimationClip clip = new AnimationClip();
                        UnityEditor.EditorUtility.CopySerialized(item, clip);
                        UnityEditor.AssetDatabase.CreateAsset(clip, $"{artAnimFile}/{item.name}.anim");
                        FileInfo go = new FileInfo($"{artAnimFile}/{item.name}.anim");
                        go.CopyTo($"{animFile}/{item.name}.anim", true);
                        UnityEditor.AssetDatabase.DeleteAsset($"{artAnimFile}/{item.name}.anim");
                        clipPaths.Add($"{animFile}/{item.name}.anim");
                    }
                    var clips = Directory.GetFiles(animFile);
                    if (clips.Length > clipPaths.Count * 2)
                    {
                        foreach (var item in clips)
                        {
                            if (!clipPaths.Any((a) => a == item.Replace("\\", "/")))
                            {
                                UnityEditor.AssetDatabase.DeleteAsset(item.Replace("\\", "/"));
                            }
                        }
                    }
                    else if (clips.Length < clipPaths.Count * 2)
                    {
                        //UnityEngine.Debug.LogError("快查查吧，导出的文件都少了");
                    }

                    UnityEngine.Debug.Log("已导入美术动作文件");
                }
                else
                { UnityEngine.Debug.LogError("未新建对应文件夹，且未找到美术动作文件"); }
            }
            else
            {
                UnityEngine.Debug.LogError("未新建对应文件夹，且未找到美术动作文件");
                UnityEngine.Debug.LogError(animFile);
                UnityEngine.Debug.LogError(artAnimFile);
            }
            if (GetPathToRole(animFile, "Roles") != null)
            {
                ImportAnimWithPath(GetPathToRole(animFile, "Roles"));
            }
            UnityEditor.AssetDatabase.Refresh();
        }

        /// <summary>
        /// 把传入的路径去掉最后一层，返回到上一层直到最后一层文件夹和传入参数名相同
        /// </summary>
        public string GetPathToRole(string path, string endKey)
        {
            var pathSplit = path.Split("/");
            if (pathSplit[pathSplit.Length - 1] == endKey)
            {
                return null;
            }
            if (pathSplit[pathSplit.Length - 1].Equals("Cm", StringComparison.OrdinalIgnoreCase))
            {
                string curPath = path.Substring(0, path.LastIndexOf("/"));
                var curPtahSplit = curPath.Split("/");
                if (curPtahSplit[curPtahSplit.Length - 1] == endKey)
                {
                    return curPath;
                }
                else
                {
                    return $"{curPath.Substring(0, curPath.LastIndexOf("/"))}/Cm";
                }
            }
            return $"{path.Substring(0, path.LastIndexOf("/"))}/Cm";
        }

        public List<GameObject> GetPrefabWithParentFile(string path)
        {
#if UNITY_EDITOR
            List<GameObject> gameObjects = new List<GameObject>();
            var prefabConfig = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
            if (prefabConfig != null)
            {
                foreach (var item in prefabConfig)
                {
                    gameObjects.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(item.ToString()));
                }
                return gameObjects;
            }
#endif
            return null;
        }

        public List<AnimationClip> GetClipsWithParentFile(string path)
        {
#if UNITY_EDITOR
            List<AnimationClip> clips = new List<AnimationClip>();
            var clipConfig = Directory.GetFiles(path, "*.FBX", SearchOption.AllDirectories);
            if (clipConfig != null)
            {
                foreach (var item in clipConfig)
                {
                    var objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(item.ToString());
                    foreach (var obj in objs)
                    {
                        if (obj is AnimationClip && obj.name != "__preview__Take 001")
                        {
                            clips.Add((AnimationClip)obj);
                        }
                    }
                }
                return clips;
            }
#endif
            return null;
        }

        /// <summary>
        /// 现在会在左键点击的时候自动创建并指向预制体，所以取消了加载预制体按钮
        /// </summary>
        public void LoadPrefab(bool isFirst)
        {
            if (string.IsNullOrEmpty(PrefabPath))
            {
                return;
            }
            //检查能不能拿到对应资源
            if (UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Res/{GetModelData().ModelsPath}.prefab"))
            {
                //删除原先的Model下所有物体
                foreach (var item in SkillEditorGlobal.Model.transform.GetChildren())
                {
                    GameObject.DestroyImmediate(item.gameObject);
                }
                var curModel = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Res/{GetModelData().ModelsPath}.prefab");
                ExplorereItemModel = GameObject.Instantiate(curModel, SkillEditorGlobal.Model.transform);
                //设置模型大小
                ExplorereItemModel.transform.localScale = new Vector3(GetAvatarData().ModelScaling, GetAvatarData().ModelScaling, GetAvatarData().ModelScaling) / 100f;
            }
            else
            {
                UnityEngine.Debug.Log($"模型路径下没找到对应模型，请检查一下模型路径吧-----Assets/Res/{GetModelData().ModelsPath}.prefab");
            }

            //删除原先的SkillEditor下所有物体
            foreach (var item in SkillEditorGlobal.Instance.transform.GetChildren())
            {
                GameObject.DestroyImmediate(item.gameObject);
            }
            //删除原先的SkillEditor下所有物体
            foreach (var item in SkillEditorGlobal.Instance.Effect.transform.GetChildren())
            {
                GameObject.DestroyImmediate(item.gameObject);
            }
            //检查能不能拿到预制体路径
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                return;
            }
            prefab.GetComponent<PlayableDirector>().playableAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            var go = isFirst ? GameObject.Instantiate(prefab) as GameObject : UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (go == null)
            {
                return;
            }
            go.transform.SetParent(SkillEditorGlobal.Instance.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.name = prefab.name;

            UnityEditor.EditorGUIUtility.PingObject(go);
            UnityEditor.Selection.activeGameObject = go;
            ExplorerItemObj = go;
            PlayableDirector playableDirector = go.GetComponent<PlayableDirector>();
            SkillEditorGlobal.PlayableDirector = playableDirector;
            if (ExplorereItemModel == null)
            {
                return;
            }
            TimelineAsset timelineAsset = (TimelineAsset)playableDirector.playableAsset;

            foreach (var track in timelineAsset.GetOutputTracks())
            {
                //设置动作轴指定模型
                if (track.GetType() == typeof(StarAnimatorTrack))
                {
                    playableDirector.SetGenericBinding(track, ExplorereItemModel.GetComponent<Animator>());
                }
                SetRffectRoot(track, ExplorereItemModel);
            }

            SkillEditorData.curExplorerItem = this;
        }

        public void SetRffectRoot(TrackAsset track, GameObject model)
        {
            //设置所有特效节点
            if (track.GetType() == typeof(StarControlTrack))
            {
                foreach (TimelineClip clip in track.GetClips())
                {

                    ClipStarControlAsset starControlAsset = (ClipStarControlAsset)clip.asset;
                    ModelOffLineData modelOffLineData = model.GetComponent<ModelOffLineData>();

                    if (!modelOffLineData.BindDummyPos.ContainsKey(starControlAsset.template.configEffect.HangPoint.ToString()))
                    {
                        return;
                    }
                    GameObject root = new GameObject("EffectRoot");
                    root.transform.parent = SkillEditorGlobal.Instance.Effect.transform;
                    starControlAsset.template.HangPoint = modelOffLineData.BindDummyPos[starControlAsset.template.configEffect.HangPoint.ToString()].gameObject;
                    starControlAsset.template.EffectRoot = root;
                    ExposedReference<GameObject> reference = new ExposedReference<GameObject>();
                    reference.defaultValue = root;
                    starControlAsset.sourceGameObject = reference;
                }
            }
        }

        public void ResetRffectRoot()
        {
            PlayableDirector playableDirector = ExplorerItemObj.GetComponent<PlayableDirector>();
            SkillEditorGlobal.PlayableDirector = playableDirector;
            TimelineAsset timelineAsset = (TimelineAsset)playableDirector.playableAsset;

            foreach (var track in timelineAsset.GetOutputTracks())
            {
                //设置所有特效节点
                if (track.GetType() == typeof(StarControlTrack))
                {
                    foreach (TimelineClip clip in track.GetClips())
                    {

                        ClipStarControlAsset starControlAsset = (ClipStarControlAsset)clip.asset;
                        ModelOffLineData modelOffLineData = ExplorereItemModel.GetComponent<ModelOffLineData>();

                        if (!modelOffLineData.BindDummyPos.ContainsKey(starControlAsset.template.configEffect.HangPoint.ToString()))
                        {
                            return;
                        }
                        GameObject root = new GameObject("EffectRoot");
                        root.transform.parent = SkillEditorGlobal.Instance.Effect.transform;
                        if (starControlAsset.template.EffectRoot != null)
                        {
                            GameObject.DestroyImmediate(starControlAsset.template.EffectRoot);
                        }
                        starControlAsset.template.HangPoint = modelOffLineData.BindDummyPos[starControlAsset.template.configEffect.HangPoint.ToString()].gameObject;
                        starControlAsset.template.EffectRoot = root;
                        ExposedReference<GameObject> reference = new ExposedReference<GameObject>();
                        reference.defaultValue = root;
                        starControlAsset.sourceGameObject = reference;
                    }
                }
            }
        }

        [PropertyOrder(9)]
        [Title("复制")]
        [OnValueChanged("OnCopy"), DelayedProperty]
        [LabelText("复制")]
        public int CopyID;

        /// <summary>
        /// 修改当前ExplorerItem中的ID
        /// </summary>
        public void OnChangeID()
        {
            string Name = System.IO.Path.GetFileNameWithoutExtension(PrefabPath);

            //删除场景中的该物体
            var old = SkillEditorGlobal.Instance.transform.Find(Name);
            if (old != null)
            {
                GameObject.DestroyImmediate(old.gameObject);
            }

            //拿到要修改的ID
            int oldID = int.Parse(Name.Split("_")[1]);
            int newID = ID;
            ID = oldID;

            //移动文件路径，并创建新实例
            RenameFile(oldID, newID);
            ExplorerItem explorerItem = new ExplorerItem(newID, RunTimeType);
            explorerItem.OnSkillEditorReInit = OnSkillEditorReInit;
            switch (RunTimeType)
            {
                case BattleRuntimeTypeEnum.Skill:
                    explorerItem.skillConfig.ID = newID;
                    break;
                case BattleRuntimeTypeEnum.Buff:
                    explorerItem.buffConfig.ID = newID;
                    break;
                case BattleRuntimeTypeEnum.Bullet:
                    explorerItem.bulletConfig.ID = newID;
                    break;
                case BattleRuntimeTypeEnum.Passive:
                    explorerItem.passiveSkillConfig.ID = newID;
                    break;
            }

            //刷新编辑器
            explorerItem.ExportJson();
            OnSkillEditorReInit?.Invoke();
        }
        /// <summary>
        /// 如果需要将旧的数据导入到新的数据之中，就把代码逻辑写在这个位置
        /// </summary>
        public void ImportOld()
        {
        }

        [PropertyOrder(100)]
        [PropertySpace(30)]
        [OnValueChanged("Delete"), DelayedProperty]
        [LabelText("输入技能ID删除")]
        public int DeleteID;
        public void Delete()
        {
            if (DeleteID == ID)
            {
                string Name = System.IO.Path.GetFileNameWithoutExtension(PrefabPath);

                //删除场景中的该物体
                var old = SkillEditorGlobal.Instance.transform.Find(Name);
                if (old != null)
                {
                    GameObject.DestroyImmediate(old.gameObject);
                }

                //删除资源
                if (System.IO.File.Exists(PrefabPath))
                {
                    UnityEditor.AssetDatabase.DeleteAsset(PrefabPath);
                }
                if (System.IO.File.Exists(TimelinePath))
                {
                    UnityEditor.AssetDatabase.DeleteAsset(TimelinePath);
                }

                //删除Json
                if (System.IO.File.Exists(JsonPath))
                {
                    UnityEditor.AssetDatabase.DeleteAsset(JsonPath);
                }

                OnSkillEditorReInit?.Invoke();
            }
            else
            {
                DeleteID = 0;
            }
        }

        /// <summary>
        /// 返回一个ID+备注的字符串
        /// </summary>
        /// <returns></returns>
        public string GetGroupName()
        {
            if (!File.Exists(PrefabPath))
            {
                return ID.ToString() + "预制体缺失";
            }
            switch (RunTimeType)
            {
                case BattleRuntimeTypeEnum.Skill:
                    return skillConfig.ID + ":" + skillConfig.SkillDesc;
                case BattleRuntimeTypeEnum.Buff:
                    return buffConfig.ID + ":" + buffConfig.BuffDesc;
                case BattleRuntimeTypeEnum.Bullet:
                    return bulletConfig.ID + ":" + bulletConfig.BulletDesc;
                case BattleRuntimeTypeEnum.Passive:
                    return passiveSkillConfig.ID + ":" + passiveSkillConfig.PassiveSkillDesc;
            }
            return ID.ToString();
        }

        /// <summary>
        /// 返回一个与当前ExplorerItem除了ID外完全相同的ExplorerItem
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ExplorerItem DeepCopy(int ID)
        {
            ExplorerItem explorerItem = new ExplorerItem(ID, RunTimeType);
            explorerItem.ID = ID;
            explorerItem.RunTimeType = RunTimeType;
            explorerItem.OnSkillEditorReInit = OnSkillEditorReInit;
            explorerItem.skillConfig = skillConfig;
            explorerItem.buffConfig = buffConfig;
            explorerItem.bulletConfig = bulletConfig;
            explorerItem.passiveSkillConfig = passiveSkillConfig;
            return explorerItem;
        }

        /// <summary>
        /// 将当前ExplorerItem管理的文件全部修改为新的命名，调用后请销毁当前ExplorerItem
        /// </summary>
        /// <param name="ID"></param>
        public void RenameFile(int oldID, int newID)
        {
            UnityEditor.AssetDatabase.RenameAsset(JsonPath, System.IO.Path.GetFileNameWithoutExtension(JsonPath).Replace(oldID.ToString(), newID.ToString()));
            UnityEditor.AssetDatabase.RenameAsset(PrefabPath, System.IO.Path.GetFileNameWithoutExtension(PrefabPath).Replace(oldID.ToString(), newID.ToString()));
            UnityEditor.AssetDatabase.RenameAsset(TimelinePath, System.IO.Path.GetFileNameWithoutExtension(TimelinePath).Replace(oldID.ToString(), newID.ToString()));
        }

        /// <summary>
        /// 将当前ExplorerItem管理的文件全部复制到新ID下，保留当前文件
        /// </summary>
        /// <param name="ID"></param>
        public void CopyFile(int newID)
        {
            string Name = System.IO.Path.GetFileNameWithoutExtension(PrefabPath);

            UnityEditor.AssetDatabase.CopyAsset(TimelinePath, TimelinePath.Replace(ID.ToString(), newID.ToString()));
            UnityEditor.AssetDatabase.CopyAsset(JsonPath, JsonPath.Replace(ID.ToString(), newID.ToString()));
        }

        /// <summary>
        /// 根据当前文件的类型复制一个出来，不能复制到已有ID上
        /// </summary>
        public void OnCopy()
        {
            if (CopyID == 0)
            {
                return;
            }
            int copyID = CopyID;
            CopyID = 0;
            switch (RunTimeType)
            {
                case BattleRuntimeTypeEnum.Skill:
                    if (SkillEditorData.Skills.ContainsKey(copyID))
                    {
                        UnityEngine.Debug.Log("ID重复了，请先删除再创建");
                        copyID = 0;
                        return;
                    }
                    break;
                case BattleRuntimeTypeEnum.Buff:
                    if (SkillEditorData.Buffs.ContainsKey(copyID))
                    {
                        UnityEngine.Debug.Log("ID重复了，请先删除再创建");
                        copyID = 0;
                        return;
                    }
                    break;
                case BattleRuntimeTypeEnum.Bullet:
                    if (SkillEditorData.Bullets.ContainsKey(copyID))
                    {
                        UnityEngine.Debug.Log("ID重复了，请先删除再创建");
                        copyID = 0;
                        return;
                    }
                    break;
                case BattleRuntimeTypeEnum.Passive:
                    if (SkillEditorData.Passives.ContainsKey(copyID))
                    {
                        UnityEngine.Debug.Log("ID重复了，请先删除再创建");
                        copyID = 0;
                        return;
                    }
                    break;
            }
            CopyFile(copyID);
            ExplorerItem explorerItem = new ExplorerItem(copyID, RunTimeType);
            CreateSkill?.Invoke(RunTimeType, copyID, this);

            OnSkillEditorReInit?.Invoke();
        }

        public void SaveAsPrefab()
        {
            UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(ExplorerItemObj, PrefabPath, UnityEditor.InteractionMode.AutomatedAction, out bool saveResult);

            UnityEditor.AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 调用该方法后会删除文件夹下除了技能本身的所有文件
        /// </summary>
        public void DeleteUselessFile()
        {
            string Name = System.IO.Path.GetFileNameWithoutExtension(PrefabPath);
            string foldName = PrefabPath.Replace("/" + Name + ".prefab", string.Empty);
            var files = Directory.GetFiles(foldName);
            foreach (var item in files)
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(item);
                var fileID = System.Text.RegularExpressions.Regex.Replace(name, @"[^0-9]+", "");

                if (int.Parse(fileID) != ID)
                {
                    UnityEditor.AssetDatabase.DeleteAsset(item);
                }
            }
        }

        private int GetTypeEnum()
        {
            return (int)RunTimeType;
        }

        /// <summary>
        /// 构造函数需要传入一个ID来创建该文件夹
        /// </summary>
        public ExplorerItem(int ID, BattleRuntimeTypeEnum type)
        {
            string jsonText;
            this.ID = ID;
            switch (type)
            {
                case BattleRuntimeTypeEnum.Skill:
                    RunTimeType = type;
                    PrefabPath = $"Assets/DevTools/SkillEditor/Export/Timeline/Skill_{ID}_prefab.prefab";
                    TimelinePath = $"Assets/DevTools/SkillEditor/Export/Timeline/Skill_{ID}_timeline.playable";
                    JsonPath = $"Assets/DevTools/SkillEditor/Export/Json/Skill/Skill_{ID}.json";
                    jsonText = System.IO.File.ReadAllText(JsonPath);
                    skillConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<SkillConfigImport>(jsonText).config;
                    break;
                case BattleRuntimeTypeEnum.Buff:
                    RunTimeType = type;
                    PrefabPath = $"Assets/DevTools/SkillEditor/Export/Timeline/Buff_{ID}_prefab.prefab";
                    TimelinePath = $"Assets/DevTools/SkillEditor/Export/Timeline/Buff_{ID}_timeline.playable";
                    JsonPath = $"Assets/DevTools/SkillEditor/Export/Json/Buff/Buff_{ID}.json";
                    jsonText = System.IO.File.ReadAllText(JsonPath);
                    buffConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<BuffConfigImport>(jsonText).config;
                    break;
                case BattleRuntimeTypeEnum.Bullet:
                    RunTimeType = type;
                    PrefabPath = $"Assets/DevTools/SkillEditor/Export/Timeline/Bullet_{ID}_prefab.prefab";
                    TimelinePath = $"Assets/DevTools/SkillEditor/Export/Timeline/Bullet_{ID}_timeline.playable";
                    JsonPath = $"Assets/DevTools/SkillEditor/Export/Json/Bullet/Bullet_{ID}.json";
                    jsonText = System.IO.File.ReadAllText(JsonPath);
                    bulletConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<BulletConfigImport>(jsonText).config;
                    break;
                case BattleRuntimeTypeEnum.Passive:
                    RunTimeType = type;
                    PrefabPath = $"Assets/DevTools/SkillEditor/Export/Timeline/Passive_{ID}_prefab.prefab";
                    TimelinePath = $"Assets/DevTools/SkillEditor/Export/Timeline/Passive_{ID}_timeline.playable";
                    JsonPath = $"Assets/DevTools/SkillEditor/Export/Json/Passive/Passive_{ID}.json";
                    jsonText = System.IO.File.ReadAllText(JsonPath);
                    passiveSkillConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<PassiveConfigImport>(jsonText).config;
                    break;
            }
        }

        /// <summary>
        /// 根据ID和类型返回化身ID
        /// </summary>
        /// <returns></returns>
        public int GetAvatarID()
        {
            switch (RunTimeType)
            {
                case BattleRuntimeTypeEnum.Skill:
                    if (ID.ToString().Length == 4)
                    {
                        return ID / 100 * 100 + 1;
                    }
                    if (ID.ToString().Length == 5)
                    {
                        return ID / 100 * 100 + 1;
                    }
                    if (ID.ToString().Length == 6)
                    {
                        return ID / 10000 * 100 + 1;
                    }
                    break;
                case BattleRuntimeTypeEnum.Buff:
                    if (ID.ToString().Length == 6)
                    {
                        return ID / 1000 * 100 + 1;
                    }
                    if (ID.ToString().Length == 7)
                    {
                        return ID / 100000 * 100 + 1;
                    }
                    break;
                case BattleRuntimeTypeEnum.Bullet:
                    if (ID.ToString().Length == 6)
                    {
                        return ID / 1000 * 100 + 1;
                    }
                    if (ID.ToString().Length == 7)
                    {
                        return ID / 100000 * 100 + 1;
                    }
                    break;
                case BattleRuntimeTypeEnum.Passive:
                    if (ID.ToString().Length == 5)
                    {
                        return ID / 100 * 100 + 1;
                    }
                    if (ID.ToString().Length == 6)
                    {
                        return ID / 1000 * 100 + 1;
                    }
                    break;
                default:
                    return 0;
            }
            return 0;
        }

        /// <summary>
        /// 拿到当前运行时的模型数据
        /// </summary>
        /// <returns></returns>
        public ModelDataCell GetModelData()
        {
            if (SkillEditorData.modelData.StaticModelDatas.ContainsKey(GetAvatarID()))
            {
                return SkillEditorData.modelData.StaticModelDatas[GetAvatarID()];
            }
            return SkillEditorData.modelData.StaticModelDatas[10001];
        }

        /// <summary>
        /// 拿到当前运行时的化身数据
        /// </summary>
        /// <returns></returns>
        public AvatarDataCell GetAvatarData()
        {
            if (SkillEditorData.avatarData.StaticAvatarDatas.ContainsKey(GetAvatarID()))
            {
                return SkillEditorData.avatarData.StaticAvatarDatas[GetAvatarID()];
            }
            return SkillEditorData.avatarData.StaticAvatarDatas[10001];
        }
#endif
    }
}
