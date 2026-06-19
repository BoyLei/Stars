using Animancer;
using Cinemachine;
using DG.DemiEditor;
using LuaInterface;
using MessagePack;
using Newtonsoft.Json;
using RenderHeads.Media.AVProVideo;
using SGF.Utlis;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using StarProject.OffLine;
using StarProject.Service.Language;
using System;
///--------------------------------------------------------------------
/// 文件名   :   ExplorerWindow.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/09 10:53:35
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SkillEditor
{
    public delegate void OnSelectAllDelegate(int Index, bool active);

    public class ExplorerWindow : OdinMenuEditorWindow
    {
#if UNITY_EDITOR
        private bool selectSkill = false;
        private bool selectBUFF = false;
        private bool selectBullet = false;
        private bool selectPassive = false;

        [UnityEditor.MenuItem("Tools/SkillEditor/运行时管理器")]
        public static void OpenWindow()
        {
            ExplorerWindow window = GetWindow<ExplorerWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        protected override void OnGUI()
        {
            //先把窗口缩小
            //position = new Rect(position.x, position.y, position.width, position.height - 84f);

            // 标题栏的矩形区域
            Rect titleBarRect = new(0f, 0f, position.width, 21);

            Dictionary<string, Action> normalAction = new();
            normalAction.Add("创建", Create);
            normalAction.Add("清理场景", ClearScene);
            normalAction.Add("导出所选", ExportSelectJson);
            normalAction.Add("删除所选", DeleteSelect);
            normalAction.Add("刷新编辑器", ReInit);
            normalAction.Add("Copy", CopyFile);
            CreateTitle(Color.white, normalAction, 0f);

            Dictionary<string, Action> serverAction = new();
            serverAction.Add("打开服务器", ServerOpen);
            serverAction.Add("更新服务器", ServerUpdate);
            serverAction.Add("更新编辑器", EditorUpdate);
            serverAction.Add("更新客户端", ClientUpdate);
            CreateTitle(Color.red, serverAction, 21f);

            Dictionary<string, Action> selectAction = new();
            selectAction.Add("全选技能", SelectAllSkill);
            selectAction.Add("全选BUFF", SelectAllBUFF);
            selectAction.Add("全选子弹", SelectAllBullet);
            selectAction.Add("全选被动", SelectAllPassive);
            selectAction.Add("导出文本", ExportString);
            selectAction.Add("导出伤害ID", ExportDamageParam);
            CreateTitle(Color.white, selectAction, 42f);

            Dictionary<string, Action> importAction = new();
            importAction.Add("占位", Test);
            importAction.Add($"视角转换:{SkillEditorData.EditorCameraType.ToString()}", ChangeCameraType);
            CreateTitle(Color.white, importAction, 63f);

            // 计算内容区域的矩形范围，该区域将跳过标题栏进行绘制
            //  Rect contentRect = new(0, 84f, position.width, position.height - 84f);

            // 在内容区域绘制原生的OdinMenuEditorWindow内容
            EditorGUILayout.Space(84);
            EditorGUILayout.BeginVertical();

            base.OnGUI();

            EditorGUILayout.EndVertical();

            //再把窗口变回去
            //position = new Rect(position.x, position.y, position.width, position.height + 84f);
        }

        /// <summary>
        /// 创建标题栏的按钮
        /// </summary>
        /// <param name="color"></param>
        /// <param name="buttons"></param>
        /// <param name="startHight"></param>
        private void CreateTitle(Color color, Dictionary<string, Action> buttons, float startHight)
        {
            // 标题栏的矩形区域
            Rect titleBarRect = new(0f, startHight, position.width, 21);

            GUILayout.BeginArea(titleBarRect);

            GUILayout.BeginHorizontal(UnityEditor.EditorStyles.toolbar);

            GUIStyle buttonStyle = new(GUI.skin.button);
            buttonStyle.normal.textColor = color;

            foreach (var button in buttons)
            {
                if (GUILayout.Button(button.Key, buttonStyle))
                {
                    button.Value?.Invoke();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        /// <summary>
        /// 创建实体的按钮
        /// </summary>
        private void Create()
        {
            Rect buttonRect = GUILayoutUtility.GetLastRect();
            GUIContent[] menuOptions = new GUIContent[]
            {
                new("技能"),
                new("BUFF"),
                new("子弹"),
                new("被动"),
                new("仅导出Avatar动作"),
            };

            GUIStyle menuStyle = UnityEditor.EditorStyles.toolbarButton;

            BattleRuntimeTypeEnum explorerItemType = 0;

            UnityEditor.EditorUtility.DisplayCustomMenu(buttonRect, menuOptions, -1, (userData, options, selected) =>
            {
                if (tree.Config.SearchTerm.All(char.IsDigit) && int.Parse(tree.Config.SearchTerm) == 0)
                {
                    return;
                }

                if (selected == 4)
                {
                    if (string.IsNullOrEmpty(SkillEditorData.avatarData.StaticAvatarDatas[int.Parse(tree.Config.SearchTerm)].AnimsPath))
                    {
                        UnityEngine.Debug.Log("路径为空，请配置路径");
                        return;
                    }
                    string animFile = $"Assets/Res/{SkillEditorData.avatarData.StaticAvatarDatas[int.Parse(tree.Config.SearchTerm)].AnimsPath}";
                    ImportAnimWithPath(animFile);
                    return;
                }

                explorerItemType = (BattleRuntimeTypeEnum)selected + 1;

                //记录要创建的ID
                int createID = int.Parse(tree.Config.SearchTerm);

                //判断是否重复
                if (SkillEditorData.IsRepeat(explorerItemType, createID))
                {
                    UnityEngine.Debug.Log($"ID{createID}在{explorerItemType}中已经存在了");
                    return;
                }

                CreateSkill(explorerItemType, createID);
            }, null);
        }


        public void ExportString()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanel("选择导出文本列表", "", "skillString.csv", "*.csv");
            if (path != "")
            {
                var data = new StringBuilder();

                //foreach (var skill in SkillEditorData.Skills)
                //{
                //    data.AppendFormat("{0},{1},{2}\n",
                //        skill.Value.skillConfig.ID,
                //        skill.Value.skillConfig.SkillDesc,
                //        skill.Value.skillConfig.CD
                //        );
                //}
                foreach (var skill in SkillEditorData.Skills)
                {
                    if (skill.Value.skillConfig.SkillIconDesc != "")
                    {
                        data.AppendFormat("{0},{1},{2},{3}\n",
                            "Skill",
                            skill.Value.skillConfig.ID,
                            $"SkillIconDesc_{skill.Value.skillConfig.ID}",
                            skill.Value.skillConfig.SkillIconDesc
                            );
                    }
                }
                foreach (var buff in SkillEditorData.Buffs)
                {
                    if (buff.Value.buffConfig.AddEffectFlys.Count != 0)
                    {
                        int i = 1;
                        foreach (var fly in buff.Value.buffConfig.AddEffectFlys)
                        {
                            data.AppendFormat("{0},{1},{2},{3}\n",
                                "Buff",
                                buff.Value.buffConfig.ID,
                                $"AddEffectFly_{buff.Value.buffConfig.ID}_{i}",
                                fly.Value
                                );
                            i++;
                        }
                    }
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
                writer.WriteLine(data.ToString());
                writer.Flush();
                writer.Close();
                writer.Dispose();
                UnityEditor.EditorUtility.DisplayDialog("消息", "导出成功", "ok");
            }
        }

        public void ExportDamageParam()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanel("选择导出位置", "", "skillParam.csv", "*.csv");
            if (path != "")
            {
                var data = new StringBuilder();

                //Key：运行时_伤害值组成的字符串
                Dictionary<string, ParamsInfo> damageParams = new Dictionary<string, ParamsInfo>();

                foreach (var skill in SkillEditorData.Skills)
                {
                    //检查所有效果轴，拿到伤害效果，并把数值组ID填入
                    TimelineAsset timelineAsset = AssetDatabase.LoadAssetAtPath<TimelineAsset>(skill.Value.TimelinePath);

                    GetParamsIDWithTimeline(skill.Key, skill.Value.skillConfig.SkillDesc, skill.Value, timelineAsset,ref damageParams);
                }
                foreach (var passive in SkillEditorData.Passives)
                {
                    //检查所有效果轴，拿到伤害效果，并把数值组ID填入
                    TimelineAsset timelineAsset = AssetDatabase.LoadAssetAtPath<TimelineAsset>(passive.Value.TimelinePath);

                    GetParamsIDWithTimeline(passive.Key, passive.Value.passiveSkillConfig.PassiveSkillDesc, passive.Value, timelineAsset, ref damageParams);
                }
                foreach (var bullet in SkillEditorData.Bullets)
                {
                    //检查所有效果轴，拿到伤害效果，并把数值组ID填入
                    TimelineAsset timelineAsset = AssetDatabase.LoadAssetAtPath<TimelineAsset>(bullet.Value.TimelinePath);

                    GetParamsIDWithTimeline(bullet.Key, bullet.Value.bulletConfig.BulletDesc, bullet.Value, timelineAsset, ref damageParams);
                }
                foreach (var buff in SkillEditorData.Passives)
                {
                    //检查所有效果轴，拿到伤害效果，并把数值组ID填入
                    TimelineAsset timelineAsset = AssetDatabase.LoadAssetAtPath<TimelineAsset>(buff.Value.TimelinePath);

                    GetParamsIDWithTimeline(buff.Key, buff.Value.buffConfig.BuffDesc, buff.Value, timelineAsset, ref damageParams);
                }

                data.AppendFormat("{0},{1},{2},{3},{4},{5},{6}\n",
                    "伤害ID",
                    "运行时ID",
                    "运行时类型",
                    "运行时描述",
                    "伤害次数",
                    "第几段",
                    "活跃时间"
                    );

                foreach (var damageParam in damageParams.OrderBy(i => i.Value.ParamsID))
                {
                    data.AppendFormat("{0},{1},{2},{3},{4},{5},{6}\n",
                                        damageParam.Value.ParamsID.ToString(),
                                        damageParam.Value.RuntimeID.ToString(),
                                        damageParam.Value.RuntimeType.ToString(),
                                        damageParam.Value.RuntimeDesc,
                                        damageParam.Value.Count.ToString(),
                                        damageParam.Value.Timing.ToString(),
                                        damageParam.Value.ActiveTime.ToString()
                                        );
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
                writer.WriteLine(data.ToString());
                writer.Flush();
                writer.Close();
                writer.Dispose();
                UnityEditor.EditorUtility.DisplayDialog("消息", "导出成功", "ok");
            }
        }

        public void GetParamsIDWithTimeline(int ID, string Desc,ExplorerItem runtime, TimelineAsset timelineAsset,ref Dictionary<string, ParamsInfo> damageParamsInfo)
        {
            foreach (var track in timelineAsset.GetOutputTracks())
            {
                if (track.GetType() == typeof(EffectTrack))
                {
                    foreach (var clip in track.GetClips())
                    {
                        ClipEffectAsset clipEffectAsset = (ClipEffectAsset)clip.asset;
                        foreach (var effect in clipEffectAsset.template.frameEffect)
                        {
                            if (effect.EffectArgs.BaseEffect == null)
                            {
                                UnityEngine.Debug.Log($"{runtime.RunTimeType.ToString()}ID({ID})有个效果是空");
                            }
                            if (effect.EffectArgs.BaseEffect != null && effect.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeDamage))
                            {
                                EffectTypeDamage effectTypeDamage = (EffectTypeDamage)effect.EffectArgs.BaseEffect;
                                //如果有过，就+1，否则放进去
                                if (damageParamsInfo.ContainsKey($"{ID}_{effectTypeDamage.GroupID}"))
                                {
                                    damageParamsInfo[$"{ID}_{effectTypeDamage.GroupID}"].Count++;
                                }
                                else
                                {
                                    //拿到当前段数，并拿到活跃时间
                                    EffectTrack effectTrack = (EffectTrack)track;
                                    var result = GetActiveTimeWithTimeline(timelineAsset, clip, effectTrack.isRight);

                                    ParamsInfo info = new ParamsInfo()
                                    {
                                        ParamsID = effectTypeDamage.GroupID,
                                        RuntimeID = ID,
                                        RuntimeDesc = Desc,
                                        RuntimeType = runtime.RunTimeType,
                                        Count = 1,
                                        Timing = result.Item1,
                                        ActiveTime = (int)(result.Item2 * 1000)
                                    };
                                    damageParamsInfo.Add($"{ID}_{effectTypeDamage.GroupID}", info);
                                }
                            }
                        }
                    }
                }
            }
        }

        public (int,double) GetActiveTimeWithTimeline(TimelineAsset timelineAsset, TimelineClip timelineClip,bool isRight)
        {
            int timing = 1;
            double activeTime = 0;

            foreach (var track in timelineAsset.GetOutputTracks())
            {
                if (track.GetGroup() == null)
                {
                    continue;
                }
                //先找到普通组中的阶段轴
                if (track.GetGroup().name == "NormalGroup" && track.GetType() == typeof(StageTrack))
                {
                    //拿到所有阶段，并依次判断
                    StageTrack stageTrack = (StageTrack)track;
                    List<TimelineClip> clipStageAssets = stageTrack.GetClips().ToList();
                    foreach (var clip in clipStageAssets.OrderBy(k => k.start))
                    {
                        //isRight需要分别判断
                        if (isRight)
                        {
                            //如果效果在阶段中，说明结束了
                            if (timelineClip.start >= clip.start && timelineClip.end < clip.end)
                            {
                                activeTime += clip.duration;
                                break;
                            }
                            //如果不在，就说明还没到
                            ClipStageAsset clipStageAsset = (ClipStageAsset)clip.asset;
                            //如果阶段是激活的，就要加时间
                            //如果没激活，就要刷新计时，并增加段数
                            if (clipStageAsset.template.data.Active)
                            {
                                activeTime += clip.duration;
                            }
                            else
                            {
                                timing++;
                                activeTime = 0;
                            }
                        }
                        else
                        {
                            //如果效果在阶段中，说明结束了
                            if (timelineClip.start > clip.start && timelineClip.end <= clip.end)
                            {
                                activeTime += clip.duration;
                                break;
                            }
                            //如果不在，就说明还没到
                            ClipStageAsset clipStageAsset = (ClipStageAsset)clip.asset;
                            //如果阶段是激活的，就要加时间
                            //如果没激活，就要刷新计时，并增加段数
                            if (clipStageAsset.template.data.Active)
                            {
                                activeTime += clip.duration;
                            }
                            else
                            {
                                timing++;
                                activeTime = 0;
                            }
                        }

                    }
                    break;
                }
            }
            return (timing, activeTime);
        }
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
            List<string> clipPaths = new();
            if (Directory.Exists(artAnimFile))
            {
                var artClips = GetClipsWithParentFile(artAnimFile);
                if (artClips != null)
                {
                    Directory.CreateDirectory(animFile);
                    foreach (var item in artClips)
                    {
                        //UnityEngine.Debug.Log(animFile + "/" + item.name + ".anim");
                        AnimationClip clip = new();
                        UnityEditor.EditorUtility.CopySerialized(item, clip);
                        UnityEditor.AssetDatabase.CreateAsset(clip, $"{artAnimFile}/{item.name}.anim");
                        FileInfo go = new($"{artAnimFile}/{item.name}.anim");
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

        public List<AnimationClip> GetClipsWithParentFile(string path)
        {
            List<AnimationClip> clips = new();
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
            return null;
        }

        /// <summary>
        /// 创建技能
        /// </summary>
        /// <param name="explorerItemType"></param>
        /// <param name="createID"></param>
        /// <param name="parent"></param>
        public void CreateSkill(BattleRuntimeTypeEnum explorerItemType, int createID, ExplorerItem parent = null)
        {
            bool isCopy = false;//通过判断有没有传入parent来确定是不是copy

            if (parent != null)
            {
                isCopy = true;
            }

            //创建物体
            string goPath = $"{SkillEditorGlobal.assteExportPath}/{explorerItemType.ToString()}_{createID}_prefab.prefab";
            GameObject go = new($"{explorerItemType.ToString()}_{createID}_prefab");
            go.transform.SetParent(SkillEditorGlobal.Instance.transform);
            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one;
            BaseGenera baseGenera = go.AddComponent<BaseGenera>();

            //创建Timeline
            string tlPath = $"{SkillEditorGlobal.assteExportPath}/{explorerItemType.ToString()}_{createID}_timeline.playable";
            PlayableDirector director = go.AddComponent<PlayableDirector>();
            baseGenera.director = director;
            TimelineAsset timelineAsset;
            if (isCopy)
            {
                isCopy = true;
                timelineAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TimelineAsset>(tlPath);
                director.playableAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TimelineAsset>(tlPath);
            }
            else
            {
                timelineAsset = TimelineAsset.CreateInstance<TimelineAsset>();
                timelineAsset.name = $"{explorerItemType}_{createID}_timeline";
                UnityEditor.AssetDatabase.CreateAsset(timelineAsset, tlPath);
                director.playableAsset = timelineAsset;
            }

            //保存预制体并创建关联
            var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(go, goPath, UnityEditor.InteractionMode.AutomatedAction, out bool saveResult);

            //拿到config，并初始化默认轴
            if (!isCopy)
            {
                var trackConfig = SkillEditorGlobal.DefaultTrack;
                if (trackConfig.NormalGroup != null)
                {
                    GroupTrack group = timelineAsset.CreateTrack<GroupTrack>(trackConfig.NormalGroup.DisplayName);

                    foreach (var item in trackConfig.NormalGroup.Tracks)
                    {
                        timelineAsset.CreateTrack(SkillEditorGlobal.GetStrType(item), group, trackConfig.NormalGroup.TrackDisplayName[item]);
                    }
                }
                if (trackConfig.BulletGroup != null)
                {
                    GroupTrack group = timelineAsset.CreateTrack<GroupTrack>(trackConfig.BulletGroup.DisplayName);

                    foreach (var item in trackConfig.BulletGroup.Tracks)
                    {
                        timelineAsset.CreateTrack(SkillEditorGlobal.GetStrType(item), group, trackConfig.BulletGroup.TrackDisplayName[item]);
                    }
                }
                if (trackConfig.OtherGroup != null)
                {
                    GroupTrack group = timelineAsset.CreateTrack<GroupTrack>(trackConfig.OtherGroup.DisplayName);

                    foreach (var item in trackConfig.OtherGroup.Tracks)
                    {
                        timelineAsset.CreateTrack(SkillEditorGlobal.GetStrType(item), group, trackConfig.OtherGroup.TrackDisplayName[item]);
                    }
                }
            }

            //导出Json
            if (!isCopy)
            {
                string jsonPath = $"{SkillEditorGlobal.GetJsonPath(explorerItemType)}/{explorerItemType.ToString()}_{createID}.json";
                JsonSerializerSettings setting = new();
                setting.NullValueHandling = NullValueHandling.Ignore;

                switch (explorerItemType)
                {
                    case BattleRuntimeTypeEnum.Skill:
                        baseGenera.ExportJson(new SkillConfig() { ID = createID });
                        break;
                    case BattleRuntimeTypeEnum.Buff:
                        baseGenera.ExportJson(new BuffConfig() { ID = createID });
                        break;
                    case BattleRuntimeTypeEnum.Bullet:
                        baseGenera.ExportJson(new BulletConfig() { ID = createID });
                        break;
                    case BattleRuntimeTypeEnum.Passive:
                        baseGenera.ExportJson(new PassiveSkillConfig() { ID = createID });
                        break;
                }
            }
            else
            {
                string jsonPath = $"{SkillEditorGlobal.GetJsonPath(explorerItemType)}/{explorerItemType.ToString()}_{createID}.json";
                JsonSerializerSettings setting = new();
                setting.NullValueHandling = NullValueHandling.Ignore;

                switch (explorerItemType)
                {
                    case BattleRuntimeTypeEnum.Skill:
                        SkillConfig skillConfig = parent.skillConfig;
                        skillConfig.ID = createID;
                        baseGenera.ExportJson(skillConfig);
                        break;
                    case BattleRuntimeTypeEnum.Buff:
                        BuffConfig buffConfig = parent.buffConfig;
                        buffConfig.ID = createID;
                        baseGenera.ExportJson(buffConfig);
                        break;
                    case BattleRuntimeTypeEnum.Bullet:
                        BulletConfig bulletConfig = parent.bulletConfig;
                        bulletConfig.ID = createID;
                        baseGenera.ExportJson(bulletConfig);
                        break;
                    case BattleRuntimeTypeEnum.Passive:
                        PassiveSkillConfig passiveConfig = parent.passiveSkillConfig;
                        passiveConfig.ID = createID;
                        baseGenera.ExportJson(passiveConfig);
                        break;
                }
            }

            //刷新一下
            UnityEditor.AssetDatabase.Refresh();
            ReInit();
        }

        /// <summary>
        /// 跳转到编辑器场景清理编辑器场景
        /// </summary>
        private void ClearScene()
        {
            var sceneFullName = "Assets/DevTools/SkillEditor/Scene/SkillEditor.unity";
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != "SkillEditor")
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sceneFullName);
            }
            GameObject model = GameObject.Find("Model");
            foreach (var item in model.transform.GetChildren())
            {
                GameObject.DestroyImmediate(item.gameObject);
            }
            GameObject skillEditor = GameObject.Find("SkillEditor");
            foreach (var item in skillEditor.transform.GetChildren())
            {
                GameObject.DestroyImmediate(item.gameObject);
            }
        }

        /// <summary>
        /// 导出所选技能
        /// </summary>
        public void ExportSelectJson()
        {
            int i = 0;
            foreach (var item in SkillEditorData.Skills)
            {
                if (item.Value.Select == true)
                {
                    item.Value.ExportJson();
                    i++;
                }
            }
            foreach (var item in SkillEditorData.Buffs)
            {
                if (item.Value.Select == true)
                {
                    item.Value.ExportJson();
                    i++;
                }
            }
            foreach (var item in SkillEditorData.Bullets)
            {
                if (item.Value.Select == true)
                {
                    item.Value.ExportJson();
                    i++;
                }
            }
            foreach (var item in SkillEditorData.Passives)
            {
                if (item.Value.Select == true)
                {
                    item.Value.ExportJson();
                    i++;
                }
            }
            if (i != 0)
            {
                ReInit();
                ClearScene();
                UnityEngine.Debug.Log($"导出了{i}个项目");
            }
        }

        private void DeleteSelect()
        {
            int i = 0;

            List<int> skillDel = new List<int>();
            List<int> buffDel = new List<int>();
            List<int> passiveDel = new List<int>();
            List<int> bulletDel = new List<int>();


            foreach (var item in SkillEditorData.Skills)
            {
                if (item.Value.Select == true)
                {
                    skillDel.Add(item.Key);
                }
            }
            foreach (var item in SkillEditorData.Buffs)
            {
                if (item.Value.Select == true)
                {
                    buffDel.Add(item.Key);
                }
            }
            foreach (var item in SkillEditorData.Passives)
            {
                if (item.Value.Select == true)
                {
                    passiveDel.Add(item.Key);
                }
            }
            foreach (var item in SkillEditorData.Bullets)
            {
                if (item.Value.Select == true)
                {
                    bulletDel.Add(item.Key);
                }
            }
            foreach (var del in skillDel)
            {
                SkillEditorData.Skills[del].DeleteID = SkillEditorData.Skills[del].ID;
                SkillEditorData.Skills[del].Delete();
                i++;
            }
            foreach (var del in buffDel)
            {
                SkillEditorData.Buffs[del].DeleteID = SkillEditorData.Buffs[del].ID;
                SkillEditorData.Buffs[del].Delete();
                i++;
            }
            foreach (var del in passiveDel)
            {
                SkillEditorData.Passives[del].DeleteID = SkillEditorData.Passives[del].ID;
                SkillEditorData.Passives[del].Delete();
                i++;
            }
            foreach (var del in bulletDel)
            {
                SkillEditorData.Bullets[del].DeleteID = SkillEditorData.Bullets[del].ID;
                SkillEditorData.Bullets[del].Delete();
                i++;
            }
            if (i != 0)
            {
                ReInit();
                ClearScene();
                UnityEngine.Debug.Log($"删除了{i}个项目");
            }
        }

        [UnityEditor.MenuItem("Tools/SkillEditor/导入模型")]
        /// <summary>
        /// 导入所有模型
        /// </summary>
        private static void ImportModel()
        {
            //美术文件目录
            string artRolePath = "Assets/ArtWorkSpace/Roles/World";
            //资源文件目录
            string resRolePath = "Assets/Res/Roles/World";

            //拿到所有模型
            List<GameObject> models = new();
            List<GameObject> createModels = new();
            var prefabConfig = Directory.GetFiles(artRolePath, "*.prefab", SearchOption.AllDirectories);
            if (prefabConfig != null)
            {
                foreach (var item in prefabConfig)
                {
                    //如果模型的路径不在prefab下面，就不用导了
                    if (item.Contains("\\prefab\\", StringComparison.OrdinalIgnoreCase) || item.Contains("\\prefap\\", StringComparison.OrdinalIgnoreCase))
                    {
                        models.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(item.ToString()));
                    }
                }
            }
            //遍历所有模型，如果该模型没有对应的变体，就创建一个变体
            foreach (var item in models)
            {
                try
                {
                    GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(item) as GameObject;
                    createModels.Add(obj);
                    string resPath = string.Empty;
                    //判断模型结尾名，如果是Ld0就是高模，Ld1就是低模。和美术对过，暂时不会有更详细的拆分
                    if (UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).EndsWith("Ld0.prefab"))
                    {
                        resPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath).Replace($"/Prefap", "").Replace($"/Prefab", "").Replace("P_WD_", "").Replace("Ld0", "Model");
                    }
                    else
                    {
                        resPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath).Replace($"/Prefap", "").Replace($"/Prefab", "").Replace("P_WD_", "").Replace("Ld1", "Model_LOD1");
                    }
                    if (File.Exists(resPath))
                    {
                        var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(resPath);
                        // 检查GameObject是否是预制体的实例
                        if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(go))
                        {
                            // 获取GameObject关联的预制体
                            GameObject prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(go);

                            // 检查预制体是否存在
                            if (prefab != null)
                            {
                                continue;
                            }
                            else
                            {
                                UnityEditor.AssetDatabase.DeleteAsset(resPath);
                            }
                        }
                    }
                    UnityEditor.EditorGUIUtility.PingObject(obj.gameObject);
                    Animator anim = obj.AddComponent<Animator>();
                    AnimancerExtend ae = obj.AddComponent<AnimancerExtend>();
                    ae.Animator = anim;
                    BindDummy(obj);

                    var str = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath).Replace($"/Prefap/{item.name}.prefab", "").Replace($"/Prefab/{item.name}.prefab", "");
                    if (!Directory.Exists(UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath).Replace($"/Prefap/{item.name}.prefab", "").Replace($"/Prefab/{item.name}.prefab", "")))
                    {
                        Directory.CreateDirectory(UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath).Replace($"/Prefap/{item.name}.prefab", "").Replace($"/Prefab/{item.name}.prefab", ""));
                    }
                    string pathpath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath).Replace($"/Prefap/{item.name}.prefab", "").Replace($"/Prefab/{item.name}.prefab", "");
                    UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, resPath);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }
            }

            //再帮关卡导一下交互物
            //美术路径1：Assets/ArtWorkSpace/Scenes/CommonObstacle/Collection/Prefab
            //美术路径2：Assets/ArtWorkSpace/Effects/Prefabs/Scene/Cm
            //美术路径3：Assets/ArtWorkSpace/Effects/Prefabs/Dynamicobstacle

            //资源路径：Assets/Res/Roles/World/DynamicObstacle/cm
            List<GameObject> levelModel = new();
            var levelPrefabConfig1 = Directory.GetFiles("Assets/ArtWorkSpace/Scenes/CommonObstacle/Collection/Prefab", "*.prefab", SearchOption.AllDirectories);
            var levelPrefabConfig2 = Directory.GetFiles("Assets/ArtWorkSpace/Effects/Prefabs/Scene/Cm", "*.prefab", SearchOption.AllDirectories);
            var levelPrefabConfig3 = Directory.GetFiles("Assets/ArtWorkSpace/Effects/Prefabs/Dynamicobstacle", "*.prefab", SearchOption.AllDirectories);
            if (levelPrefabConfig1 != null)
            {
                foreach (var item in levelPrefabConfig1)
                {
                    levelModel.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(item.ToString()));
                }
            }
            if (levelPrefabConfig2 != null)
            {
                foreach (var item in levelPrefabConfig2)
                {
                    levelModel.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(item.ToString()));
                }
            }
            if (levelPrefabConfig3 != null)
            {
                foreach (var item in levelPrefabConfig3)
                {
                    levelModel.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(item.ToString()));
                }
            }
            //遍历所有模型，如果该模型没有对应的变体，就创建一个变体
            foreach (var model in levelModel)
            {
                try
                {
                    GameObject curModel = UnityEditor.PrefabUtility.InstantiatePrefab(model) as GameObject;
                    createModels.Add(curModel);
                    if (!Directory.Exists("Assets/Res/Roles/World/DynamicObstacle/cm"))
                    {
                        Directory.CreateDirectory("Assets/Res/Roles/World/DynamicObstacle/cm");
                    }
                    if (File.Exists($"Assets/Res/Roles/World/DynamicObstacle/cm/{model.name}.prefab"))
                    {
                        var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Res/Roles/World/DynamicObstacle/cm/{model.name}.prefab");
                        // 检查GameObject是否是预制体的实例
                        if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(go))
                        {
                            // 获取GameObject关联的预制体
                            GameObject prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(go);

                            // 检查预制体是否存在
                            if (prefab != null)
                            {
                                continue;
                            }
                            else
                            {
                                UnityEditor.AssetDatabase.DeleteAsset($"Assets/Res/Roles/World/DynamicObstacle/cm/{model.name}.prefab");
                            }
                        }
                    }
                    UnityEditor.PrefabUtility.SaveAsPrefabAsset(curModel, $"Assets/Res/Roles/World/DynamicObstacle/cm/{model.name}.prefab");
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                    continue;
                }
            }
            for (int i = createModels.Count - 1; i >= 0; i--)
            {
                if (createModels[i] != null)
                {
                    DestroyImmediate(createModels[i]);
                }
            }
            UnityEditor.AssetDatabase.Refresh();
        }

        [UnityEditor.MenuItem("Tools/SkillEditor/导入动作")]
        /// <summary>
        /// 导入所有动作
        /// </summary>
        private static void ImportAnim()
        {
            //美术文件目录
            string artRolePath = "Assets/ArtWorkSpace/Roles/World";

            //资源文件目录
            string resRolePath = "Assets/Res/Animation/Roles";

            //拿到所有FBX和Anim
            List<string> fbxs = new();
            var fbxConfigs = Directory.GetFiles(artRolePath, "*.FBX", SearchOption.AllDirectories);
            var clipConfigs = Directory.GetFiles(artRolePath, "*.anim", SearchOption.AllDirectories);
            if (fbxConfigs != null)
            {
                foreach (var item in fbxConfigs)
                {
                    //如果FBX的路径不在Animation下面，就不用导了
                    if (item.Contains("\\Animation\\", StringComparison.OrdinalIgnoreCase))
                    {
                        fbxs.Add(item);
                    }
                }
            }
            //遍历所有FBX，不考虑目标文件，直接导出到目标文件里面，如果有同名的就覆盖一下
            foreach (var fbx in fbxs)
            {
                //读取FBX下所有动作Clip
                var objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(fbx.ToString()).OfType<AnimationClip>().ToArray();
                if (objs != null && objs.Length != 0)
                {
                    //先判断有没有路径，没有就要创建一个路径
                    string curTargetPath = fbx.Replace(artRolePath, resRolePath).Replace($"\\Animation\\{Path.GetFileName(fbx)}", "");
                    if (!File.Exists(curTargetPath))
                    {
                        var str = curTargetPath;
                        if (!Directory.Exists(curTargetPath))
                        {
                            Directory.CreateDirectory(curTargetPath);
                        }
                    }
                    //遍历所有obj
                    foreach (var obj in objs)
                    {
                        //如果是Anim就需要拷贝过去
                        if (obj.name != "__preview__Take 001")
                        {
                            try
                            {
                                AnimationClip clip = new();
                                UnityEditor.EditorUtility.CopySerialized(obj, clip);
                                UnityEditor.AssetDatabase.CreateAsset(clip, $"{artRolePath}/{obj.name}.anim");
                                FileInfo go = new($"{artRolePath}/{obj.name}.anim");
                                go.CopyTo($"{curTargetPath}/{obj.name}.anim", true);
                                UnityEditor.AssetDatabase.DeleteAsset($"{artRolePath}/{obj.name}.anim");
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            //遍历所有anim
            foreach (var clipconfig in clipConfigs)
            {
                //如果clip的路径不在Animation下面，就不用导了
                if (!clipconfig.Contains("\\Animation\\", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                //针对每个clip单独处理
                AnimationClip curClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(clipconfig);
                //先判断有没有路径，没有就要创建一个路径
                string curTargetPath = clipconfig.Replace(artRolePath, resRolePath).Replace($"\\Animation\\{Path.GetFileName(clipconfig)}", "");
                if (!File.Exists(curTargetPath))
                {
                    var str = curTargetPath;
                    if (!Directory.Exists(curTargetPath))
                    {
                        Directory.CreateDirectory(curTargetPath);
                    }
                }
                //如果是Anim就需要拷贝过去
                if (curClip.name != "__preview__Take 001")
                {
                    try
                    {
                        AnimationClip newClip = new();
                        UnityEditor.EditorUtility.CopySerialized(curClip, newClip);
                        UnityEditor.AssetDatabase.CreateAsset(newClip, $"{artRolePath}/{curClip.name}.anim");
                        FileInfo go = new($"{artRolePath}/{curClip.name}.anim");
                        go.CopyTo($"{curTargetPath}/{curClip.name}.anim", true);
                        UnityEditor.AssetDatabase.DeleteAsset($"{artRolePath}/{curClip.name}.anim");
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            UnityEditor.AssetDatabase.Refresh();
        }

        [UnityEditor.MenuItem("Tools/SkillEditor/导入特效")]
        /// <summary>
        /// 导入所有特效
        /// </summary>
        private static void ImportEffect()
        {
            //美术文件目录
            string artRolePath = "Assets/ArtWorkSpace/Effects/Prefabs/Roles";
            //资源文件目录
            string resRolePath = "Assets/Res/Effects/Roles";

            //拿到所有模型
            List<GameObject> prefabs = new();
            var prefabConfig = Directory.GetFiles(artRolePath, "*.prefab", SearchOption.AllDirectories);
            if (prefabConfig != null)
            {
                foreach (var item in prefabConfig)
                {
                    prefabs.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(item.ToString()));
                }
            }
            //遍历所有模型，如果该模型没有对应的变体，就创建一个变体
            foreach (var item in prefabs)
            {
                GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(item) as GameObject;
                if (!File.Exists(UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath)))
                {
                    UnityEditor.EditorGUIUtility.PingObject(obj.gameObject);
                    Animator anim = obj.AddComponent<Animator>();
                    AnimancerComponent ac = obj.AddComponent<AnimancerComponent>();
                    ac.Animator = anim;
                    BindDummy(obj);

                    //var str = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath).Replace($"/{item.name}.prefab", "");
                    if (!Directory.Exists(UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath).Replace($"/{item.name}.prefab", "")))
                    {
                        Directory.CreateDirectory(UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath).Replace($"/{item.name}.prefab", ""));
                    }
                    string pathpath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath);
                    UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj).Replace(artRolePath, resRolePath));
                }
                GameObject.DestroyImmediate(obj);
            }
            UnityEngine.Debug.Log("导完了");
            UnityEditor.AssetDatabase.Refresh();
        }

        [UnityEditor.MenuItem("Tools/SkillEditor/导出所有模型动作库")]
        private static void ExportAnimName()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanel("选择导出位置", "", "AnimName.csv", "*.csv");

            var data = new StringBuilder();

            //拿到所有动作路径
            //美术文件目录
            string artRolePath = "Assets/ArtWorkSpace/Roles/World";

            //资源文件目录
            string resRolePath = "Assets/Res/Animation/Roles";

            Dictionary<string,StringBuilder> modelWithAnim = new Dictionary<string, StringBuilder>();

            //拿到所有FBX和Anim
            List<string> fbxs = new();
            var fbxConfigs = Directory.GetFiles(artRolePath, "*.FBX", SearchOption.AllDirectories);
            var clipConfigs = Directory.GetFiles(artRolePath, "*.anim", SearchOption.AllDirectories);

            if (fbxConfigs != null)
            {
                foreach (var item in fbxConfigs)
                {
                    //如果FBX的路径不在Animation下面，就不管
                    if (item.Contains("\\Animation\\", StringComparison.OrdinalIgnoreCase) && item.Contains("@"))
                    {
                        string modelName = Path.GetFileName(item).Split("@")[0];
                        //处理模型名字
                        if (!modelName.Contains("A_"))
                        {
                            continue;
                        }
                        else
                        {
                            modelName = modelName.Replace("A_", "");
                        }
                        string animName = Path.GetFileName(item).Split("@")[1];
                        //处理动作名字
                        animName = animName.Replace(".FBX", "");
                        //判断是否存在
                        if (modelWithAnim.ContainsKey(modelName))
                        {
                            modelWithAnim[modelName].Append($",{animName}");
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(animName);
                            modelWithAnim.Add(modelName, sb);
                        }
                    }
                }
            }

            data.AppendFormat("{0},{1}\n",
                    "模型名",
                    "包含动作名"
                    );

            foreach (var item in modelWithAnim)
            {
                string anims = item.Value.ToString();
                data.AppendFormat("{0},{1}\n",
                                    item.Key,
                                    anims
                                    );
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine(data.ToString());
            writer.Flush();
            writer.Close();
            writer.Dispose();
            UnityEditor.EditorUtility.DisplayDialog("消息", "导出成功", "ok");
        }

        [UnityEditor.MenuItem("Tools/SkillEditor/导出所有特效路径")]
        private static void ExportEffectPath()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanel("选择导出位置", "", "EffectPath.csv", "*.csv");

            var data = new StringBuilder();

            //拿到所有特效
            //资源文件目录
            string resRolePath = "Assets/Res/Effects/Roles";

            Dictionary<string, string> prefabWithPath = new Dictionary<string, string>();

            //拿到所有prefab
            var clipConfigs = Directory.GetFiles(resRolePath, "*.prefab", SearchOption.AllDirectories);

            if (clipConfigs != null)
            {
                foreach (var item in clipConfigs)
                {
                    //拿到名字
                    string[] strings = item.Replace("\\Cm", "", StringComparison.OrdinalIgnoreCase).Split("\\");

                    string fileName = strings[strings.Length - 2];


                    //拿到路径
                    string filePath = item.Replace($"\\{Path.GetFileName(item)}","").Replace("\\","/");

                    if (prefabWithPath.ContainsKey(fileName))
                    {
                        continue;
                    }
                    else
                    {
                        prefabWithPath.Add(fileName, filePath);
                    }
                }
            }

            data.AppendFormat("{0},{1}\n",
                    "模型名",
                    "特效路径"
                    );

            foreach (var item in prefabWithPath)
            {
                data.AppendFormat("{0},{1}\n",
                                    item.Key,
                                    item.Value
                                    );
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine(data.ToString());
            writer.Flush();
            writer.Close();
            writer.Dispose();
            UnityEditor.EditorUtility.DisplayDialog("消息", "导出成功", "ok");
        }

        private static string Hurt_D = "Hurt_D";                        //胸部受击伤害挂点
        private static string BackWeapon_D = "BackWeapon_D";            //后背部武器挂点
        private static string Wing_D = "Wing_D";                        //后背部翅膀挂点
        private static string HandWeapon_D_L = "HandWeapon_D_L";        //左手武器手部挂点
        private static string HandWeapon_D_R = "HandWeapon_D_R";        //右手武器手部挂点
        private static string WeaponRoot_D_L = "WeaponRoot_D_L";        //左手武器根部挂点
        private static string WeaponRoot_D_R = "WeaponRoot_D_R";        //右手武器根部挂点
        private static string WeaponHurt_D_R = "WeaponHurt_D_R";        //右手或者双手武器特效挂点
        private static string WeaponHurt_D_L = "WeaponHurt_D_L";        //左手武器特效挂点
        private static string Root_D = "Root_D";                        //Root下（脚下）加特效挂点
        private static string Top_D = "Top_D";                          //头顶眩晕挂点
        private static string Foot_D_R = "Foot_D_R";                    //右脚脚底特效挂点
        private static string Foot_D_L = "Foot_D_L";                    //左脚脚底特效挂点
        private static string UnitInfo_D = "UnitInfo_D";                //头顶信息（气泡，公会，名字等）挂点
        private static string Bip001 = "Bip001";                        //尾巴骨位置挂点（用于主角模型的摄像机偏移）
        private static string Mouth_D = "Mouth_D";                      //嘴巴挂点
        private static string Mouth_D02 = "Mouth_D02";                      //嘴巴2挂点
        private static string Mouth_D03 = "Mouth_D03";                      //嘴巴3挂点


        private static List<string> CheckBindPointStringNames = new() {
    Hurt_D,BackWeapon_D,Wing_D,HandWeapon_D_L,HandWeapon_D_R,WeaponRoot_D_L,
    WeaponRoot_D_R,WeaponHurt_D_R,WeaponHurt_D_L,Root_D,Top_D,Foot_D_R,
    Foot_D_L,UnitInfo_D,Bip001,Mouth_D,Mouth_D02,Mouth_D03
    };

        private static void BindDummy(GameObject ArtModel)
        {
            if (ArtModel.GetComponent<ModelOffLineData>() == null)
            {
                ArtModel.gameObject.AddComponent<ModelOffLineData>();
            }


            Dictionary<string, Transform> BindDummyPos = ArtModel.GetComponent<ModelOffLineData>().BindDummyPos;
            BindDummyPos.Clear();


            /*Transform Root = ModelOffset.GetChild(0).Find("Root");*/
            Transform Root = ArtModel.transform.Find("Root");
            if (Root != null)
            {
                BindDummyPos.Add("Root", Root);
                string findName = "";

                for (int i = 0; i < CheckBindPointStringNames.Count; i++)
                {
                    findName = CheckBindPointStringNames[i];

                    for (int j = 0; j < Root.childCount; j++)
                    {
                        //目前单状态，所以在结果的itemlist里面全部遍历，就一个就好【0】
                        Transform child = Root.GetChild(j);
                        if (child.name == findName && !BindDummyPos.ContainsKey(findName))
                        {
                            BindDummyPos.Add(findName, child);
                        }
                        else
                        {
                            List<Transform> bindPart = Root.GetChild(j).DeepFirstTransList(findName);
                            if (bindPart != null && bindPart.Count != 0 && bindPart[0] != null && !BindDummyPos.ContainsKey(findName))
                            {
                                //ModelOffLineData
                                //bindPart = tranRoot.GetChild(i).DeepFirstTransList("item");

                                BindDummyPos.Add(findName, bindPart[0]);
                            }
                        }
                    }
                }
                if (BindDummyPos.ContainsKey("Top_D") && BindDummyPos.ContainsKey("Root"))
                {
                    float height = BindDummyPos["Top_D"].transform.position.y - BindDummyPos["Root"].transform.position.y;
                    ArtModel.GetComponent<ModelOffLineData>()._ModelHeight = height;
                    //UnityEngine.Debug.Log($"模型高度height={height}");
                }
            }
            else
            {
                BindDummyPos.Add("Root", ArtModel.transform);
            }
            // 设置值为脏数据，，编辑器会自己检测一下
            UnityEditor.EditorUtility.SetDirty(ArtModel);
        }

        /// <summary>
        /// 占位用
        /// </summary>
        private void Test()
        {
            UnityEngine.Debug.Log(-120 % -360 + 360);
            #region 复数操作
            //int i = 0;
            //foreach (var item in SkillEditorData.Skills)
            //{
            //    if (item.Value.Select == true)
            //    {
            //        item.Value.skillConfig.SkillLabels.Add(SkillLabel.QuanSB);
            //        item.Value.ExportJson();
            //        i++;
            //    }
            //}
            //if (i != 0)
            //{
            //    ReInit();
            //    ClearScene();
            //    UnityEngine.Debug.Log($"给{i}个项目加了敌人选择");
            //}
            #endregion
            #region 导出重复文本
            //var path = UnityEditor.EditorUtility.SaveFilePanel("选择导出位置", "", "skillString.csv", "*.csv");
            ////拿出所有等级为1的字段
            //List<string> allDesc = new List<string>();

            //foreach (var desc in SkillEditorData.jobSkillDescData.StaticJobSkillDescDatas)
            //{
            //    if (desc.Value.Level == 1)
            //    {
            //        allDesc.Add(desc.Value.Decs.Replace(" ","").Replace(",",""));
            //    }
            //}
            //foreach (var desc in SkillEditorData.partnerSkillDesc.StaticPartnerSkillDescDatas)
            //{
            //    if (desc.Value.Level == 1)
            //    {
            //        allDesc.Add(desc.Value.Decs.Replace(" ", ""));
            //    }
            //}
            //foreach (var desc in SkillEditorData.buffDesc.StaticBuffDescDatas)
            //{
            //    if (desc.Value.Level == 1)
            //    {
            //        allDesc.Add(desc.Value.Decs.Replace(" ", ""));
            //    }
            //}
            //foreach (var desc in SkillEditorData.passiveDesc.StaticPassiveDescDatas)
            //{
            //    if (desc.Value.Level == 1)
            //    {
            //        allDesc.Add(desc.Value.Decs.Replace(" ", ""));
            //    }
            //}
            ////对每一个字段进行检查并打印
            //List<string> repeatedStr = FindRepeatedStrings(allDesc);

            //var data = new StringBuilder();

            //foreach (var str in repeatedStr)
            //{
            //    if (!Regex.IsMatch(str, @"\d+") && !str.Contains(",") && !str.Contains("，") && !str.Contains(".") && !str.Contains("。") && !str.Contains("“") && !str.Contains("”") && !str.Contains("【") && !str.Contains("】"))
            //    {
            //        data.AppendFormat("{0}\n",
            //                            str
            //                            );
            //    }
            //}

            //if (File.Exists(path))
            //{
            //    File.Delete(path);
            //}

            //StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
            //writer.WriteLine(data.ToString());
            //writer.Flush();
            //writer.Close();
            //writer.Dispose();
            //UnityEditor.EditorUtility.DisplayDialog("消息", "导出成功", "ok");
            //UnityEngine.Debug.Log(repeatedStr.ToString());
            #endregion
        }

        public List<string> FindRepeatedStrings(List<string> inputStrings)
        {
            List<string> allChars = new List<string>();

            foreach (var str in inputStrings)
            {
                foreach (var word in GetAllSubstringsOfLengthTwoOrMore(str))
                {
                    allChars.Add(word);
                }
            }

            return GetRepeatedStrings(allChars);
        }

        public List<string> GetAllSubstringsOfLengthTwoOrMore(string input)
        {
            List<string> substrings = new List<string>();

            // 确保字符串长度足够
            if (input.Length < 2) return substrings;

            for (int i = 0; i <= input.Length - 2; i++) // 从0开始，直到倒数第二个字符
            {
                for (int j = i + 2; j <= Math.Min(input.Length, i + 4); j++) // 从i+2开始，确保长度至少为2
                {
                    substrings.Add(input.Substring(i, j - i));
                }
            }

            return substrings;
        }

        static List<string> GetRepeatedStrings(List<string> inputStrings)
        {
            Dictionary<string, int> stringCount = new Dictionary<string, int>();
            List<string> repeatedStrings = new List<string>();

            foreach (string str in inputStrings)
            {
                if (stringCount.ContainsKey(str))
                {
                    stringCount[str]++;
                }
                else
                {
                    stringCount[str] = 1;
                }
            }

            foreach (var pair in stringCount)
            {
                if (pair.Value > 1)
                {
                    repeatedStrings.Add(pair.Key);
                }
            }

            return repeatedStrings;
        }

        /// <summary>
        /// 修改相机类型
        /// </summary>
        private void ChangeCameraType()
        {
            if (SkillEditorData.EditorCameraType == EditorCameraType.Overlook)
            {
                SkillEditorData.EditorCameraType = EditorCameraType.Right;
            }
            else
            {
                SkillEditorData.EditorCameraType = (EditorCameraType)((int)SkillEditorData.EditorCameraType + 1);
            }

            double curTime = SkillEditorGlobal.PlayableDirector.time;
            var controlClips = TimelineTools.GetCurTimelineClips<StarControlTrack>(GetTrackType.After, SkillEditorGlobal.PlayableDirector.time);
            var effectClips = TimelineTools.GetCurTimelineClips<EffectTrack>(GetTrackType.Before, SkillEditorGlobal.PlayableDirector.time);

            //处理特效轴相关内容
            Vector3 cameraOffset = Vector3.zero;
            foreach (var clip in controlClips.curTracks)
            {
                ClipStarControlAsset clipStarControlAsset = (ClipStarControlAsset)clip.asset;
                if (clipStarControlAsset.prefabGameObject != null)
                {
                    CinemachineImpulseSource ccis = clipStarControlAsset.prefabGameObject.GetComponent<CinemachineImpulseSource>();
                    if (ccis != null)
                    {
                        double ratio = (curTime - clip.start) / ccis.m_ImpulseDefinition.m_ImpulseDuration;
                        cameraOffset += ccis.m_ImpulseDefinition.ImpulseCurve.Evaluate((float)ratio) * ccis.m_DefaultVelocity;
                    }
                    else
                    {
                        clipStarControlAsset.template.SetTrans(clipStarControlAsset);
                    }

                }
            }
            SkillEditorGlobal.Instance.SetCameraCurOffset(cameraOffset);
        }

        
        /// <summary>
        /// 刷新编辑器
        /// </summary>
        private void ReInit()
        {
            SkillEditorData.Refresh();

            InitBuff(tree);
            InitBullet(tree);
            InitSkill(tree);
            InitPassive(tree);
            ForceMenuTreeRebuild();

            UnityEditor.AssetDatabase.Refresh();

        }

        /// <summary>
        /// 拷贝所有资源到服务器和客户端
        /// </summary>
        public void CopyFile()
        {
            string cmdFullPath = System.IO.Path.GetFullPath("Assets\\DevTools\\SkillEditor\\Export\\Json\\Copy.bat");
            string jsonFullPtah = System.IO.Path.GetFullPath("Assets\\DevTools\\SkillEditor\\Export\\Json");
            ProcessStartInfo proc = new(cmdFullPath);
            proc.WorkingDirectory = jsonFullPtah;
            System.Diagnostics.Process.Start(proc);
        }

        /// <summary>
        /// 打开服务器
        /// </summary>
        private void ServerOpen()
        {
            string editorPath = "Assets/DevTools/SkillEditor";
            string serverFullPath = System.IO.Path.GetFullPath(editorPath).Replace("\\StarsProject_Client\\trunk\\Stars\\Assets\\DevTools\\SkillEditor", "\\StarsProject_Server\\trunk\\gameserver");
            string cmdFullPath = System.IO.Path.GetFullPath(editorPath).Replace("\\StarsProject_Client\\trunk\\Stars\\Assets\\DevTools\\SkillEditor", "\\StarsProject_Server\\trunk\\gameserver\\start.bat");
            ProcessStartInfo proc = new(cmdFullPath);
            proc.WorkingDirectory = serverFullPath;
            System.Diagnostics.Process.Start(proc);
        }

        /// <summary>
        /// 更新服务器
        /// </summary>
        private void ServerUpdate()
        {
            string editorPath = "Assets/DevTools/SkillEditor";
            string serverFullPath = System.IO.Path.GetFullPath(editorPath).Replace("\\StarsProject_Client\\trunk\\Stars\\Assets\\DevTools\\SkillEditor", "\\StarsProject_Server\\trunk\\gameserver");
            MapEditor.MapEditorUtils.RunBat(MapEditor.EditorConfigUtils.TortoiseProc, $"/command:update /path:\"{serverFullPath}\"");
            string cmdFullPath = System.IO.Path.GetFullPath("Assets/DevTools/SkillEditor/Export/Json/Copy2S.bat");
            string jsonFullPtah = System.IO.Path.GetFullPath("Assets\\DevTools\\SkillEditor\\Export\\Json");
            ProcessStartInfo proc = new(cmdFullPath);
            proc.WorkingDirectory = jsonFullPtah;
            System.Diagnostics.Process.Start(proc);
        }

        /// <summary>
        /// 更新编辑器
        /// </summary>
        public void EditorUpdate()
        {
            string editorPath = "Assets/DevTools/SkillEditor";
            string editorFullPath = System.IO.Path.GetFullPath(editorPath);
            MapEditor.MapEditorUtils.RunBat(MapEditor.EditorConfigUtils.TortoiseProc, $"/command:update /path:\"{editorFullPath}\"");
            ReInit();
        }

        /// <summary>
        /// 更新客户端
        /// </summary>
        public void ClientUpdate()
        {
            string editorPath = "Assets/DevTools/SkillEditor";
            string clientFullPath = System.IO.Path.GetFullPath(editorPath).Replace(editorPath.Replace("/", "\\"), "");
            MapEditor.MapEditorUtils.RunBat(MapEditor.EditorConfigUtils.TortoiseProc, $"/command:update /path:\"{clientFullPath}\"");
            ReInit();
            string cmdFullPath = System.IO.Path.GetFullPath("Assets/DevTools/SkillEditor/Export/Json/Copy2C.bat");
            string jsonFullPtah = System.IO.Path.GetFullPath("Assets\\DevTools\\SkillEditor\\Export\\Json");
            ProcessStartInfo proc = new(cmdFullPath);
            proc.WorkingDirectory = jsonFullPtah;
            System.Diagnostics.Process.Start(proc);
        }

        public void SelectAllSkill()
        {
            foreach (var item in SkillEditorData.Skills)
            {
                item.Value.Select = !selectSkill;
            }
            selectSkill = !selectSkill;
        }

        public void SelectAllBUFF()
        {
            foreach (var item in SkillEditorData.Buffs)
            {
                item.Value.Select = !selectBUFF;
            }
            selectBUFF = !selectBUFF;
        }

        public void SelectAllBullet()
        {
            foreach (var item in SkillEditorData.Bullets)
            {
                item.Value.Select = !selectBullet;
            }
            selectBullet = !selectBullet;
        }

        public void SelectAllPassive()
        {
            foreach (var item in SkillEditorData.Passives)
            {
                item.Value.Select = !selectPassive;
            }
            selectPassive = !selectPassive;
        }

        public OdinMenuTree tree;
        protected override void OnEnable()
        {
            base.OnEnable();
            LanguageManager.Instance.SkillEditorOpened = true;
            if (SkillEditorData.Skills != null) { SkillEditorData.Skills.Clear(); }
            if (SkillEditorData.Buffs != null) { SkillEditorData.Buffs.Clear(); }
            if (SkillEditorData.Bullets != null) { SkillEditorData.Bullets.Clear(); }
            if (SkillEditorData.Passives != null) { SkillEditorData.Passives.Clear(); }
            if (MenuTree != null && MenuTree.MenuItems != null)
            {
                foreach (var item in MenuTree.MenuItems)
                {
                    item.ChildMenuItems?.Clear();
                    item.Remove();
                }
                MenuTree.MenuItems.Clear();
            }

            ForceMenuTreeRebuild();

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LanguageManager.Instance.SkillEditorOpened = false;
            if (SkillEditorData.Skills != null) { SkillEditorData.Skills.Clear(); }
            if (SkillEditorData.Buffs != null) { SkillEditorData.Buffs.Clear(); }
            if (SkillEditorData.Bullets != null) { SkillEditorData.Bullets.Clear(); }
            if (SkillEditorData.Passives != null) { SkillEditorData.Passives.Clear(); }
            BaseGenera.freshDelegate = null;
        }

        public void WriteConfig(string content, string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            FileStream fs = new(path, FileMode.CreateNew);
            byte[] bytes = Encoding.UTF8.GetBytes(SkillEditorUtils.ConvertJsonString(content));
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();
            UnityEditor.AssetDatabase.Refresh();
        }
        private void InitSkill(OdinMenuTree tree)
        {
            SkillEditorData.Skills.Clear();
            var files = Directory.GetFiles("Assets/DevTools/SkillEditor/Export/Json/Skill", "*.Json", SearchOption.AllDirectories);
            files.Sort(delegate (string x, string y)
            {
                if (int.Parse(System.Text.RegularExpressions.Regex.Replace(x, @"[^0-9]+", "")) > int.Parse(System.Text.RegularExpressions.Regex.Replace(y, @"[^0-9]+", "")))
                    return 1;
                else
                    return -1;
            });
            foreach (var file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);
                int ID = int.Parse(System.Text.RegularExpressions.Regex.Replace(fileName, @"[^0-9]+", ""));
                ExplorerItem explorerItem = new(ID, BattleRuntimeTypeEnum.Skill);
                explorerItem.OnSkillEditorReInit += new System.Action(ReInit);
                explorerItem.CreateSkill += new System.Action<BattleRuntimeTypeEnum, int, ExplorerItem>(CreateSkill);
                SkillEditorData.Skills.Add(ID, explorerItem);
                var customMenuItem = new SkillMenuItem(tree, explorerItem, "Skill_");
                tree.AddMenuItemAtPath("技能", customMenuItem);
            }
        }

        private void InitBuff(OdinMenuTree tree)
        {
            SkillEditorData.Buffs.Clear();
            var files = Directory.GetFiles("Assets/DevTools/SkillEditor/Export/Json/Buff", "*.Json", SearchOption.AllDirectories);
            files.Sort(delegate (string x, string y)
            {
                if (int.Parse(System.Text.RegularExpressions.Regex.Replace(x, @"[^0-9]+", "")) > int.Parse(System.Text.RegularExpressions.Regex.Replace(y, @"[^0-9]+", "")))
                    return 1;
                else
                    return -1;
            });
            foreach (var file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);
                int ID = int.Parse(System.Text.RegularExpressions.Regex.Replace(fileName, @"[^0-9]+", ""));
                ExplorerItem explorerItem = new(ID, BattleRuntimeTypeEnum.Buff);
                explorerItem.OnSkillEditorReInit += new System.Action(ReInit);
                explorerItem.CreateSkill += new System.Action<BattleRuntimeTypeEnum, int, ExplorerItem>(CreateSkill);
                SkillEditorData.Buffs.Add(ID, explorerItem);
                var customMenuItem = new SkillMenuItem(tree, explorerItem, "Buff_");
                tree.AddMenuItemAtPath("BUFF", customMenuItem);
            }
        }

        private void InitBullet(OdinMenuTree tree)
        {
            SkillEditorData.Bullets.Clear();
            var files = Directory.GetFiles("Assets/DevTools/SkillEditor/Export/Json/Bullet", "*.Json", SearchOption.AllDirectories);
            files.Sort(delegate (string x, string y)
            {
                if (int.Parse(System.Text.RegularExpressions.Regex.Replace(x, @"[^0-9]+", "")) > int.Parse(System.Text.RegularExpressions.Regex.Replace(y, @"[^0-9]+", "")))
                    return 1;
                else
                    return -1;
            });
            foreach (var file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);
                int ID = int.Parse(System.Text.RegularExpressions.Regex.Replace(fileName, @"[^0-9]+", ""));
                ExplorerItem explorerItem = new(ID, BattleRuntimeTypeEnum.Bullet);
                explorerItem.OnSkillEditorReInit += new System.Action(ReInit);
                explorerItem.CreateSkill += new System.Action<BattleRuntimeTypeEnum, int, ExplorerItem>(CreateSkill);
                SkillEditorData.Bullets.Add(ID, explorerItem);
                var customMenuItem = new SkillMenuItem(tree, explorerItem, "Bullet_");
                tree.AddMenuItemAtPath("子弹", customMenuItem);
            }
        }

        private void InitPassive(OdinMenuTree tree)
        {
            SkillEditorData.Passives.Clear();
            var files = Directory.GetFiles("Assets/DevTools/SkillEditor/Export/Json/Passive", "*.Json", SearchOption.AllDirectories);
            files.Sort(delegate (string x, string y)
            {
                if (int.Parse(System.Text.RegularExpressions.Regex.Replace(x, @"[^0-9]+", "")) > int.Parse(System.Text.RegularExpressions.Regex.Replace(y, @"[^0-9]+", "")))
                    return 1;
                else
                    return -1;
            });
            foreach (var file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);
                int ID = int.Parse(System.Text.RegularExpressions.Regex.Replace(fileName, @"[^0-9]+", ""));
                ExplorerItem explorerItem = new(ID, BattleRuntimeTypeEnum.Passive);
                explorerItem.OnSkillEditorReInit += new System.Action(ReInit);
                explorerItem.CreateSkill += new System.Action<BattleRuntimeTypeEnum, int, ExplorerItem>(CreateSkill);
                SkillEditorData.Passives.Add(ID, explorerItem);
                var customMenuItem = new SkillMenuItem(tree, explorerItem, "Passive_");
                tree.AddMenuItemAtPath("被动", customMenuItem);
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            tree = new OdinMenuTree(true);

            tree.Config.SearchFunction = (menu) =>
            {
                if (menu.SearchString.Contains(tree.Config.SearchTerm))
                {
                    return true;
                }
                return false;
            };

            //tree.Config.ScrollPos = new Vector2(position.width, position.height - 84f);

            var customMenuStyle = new OdinMenuStyle
            {
                BorderPadding = 0f,
                AlignTriangleLeft = false,
                TriangleSize = 16f,
                TrianglePadding = 0f,
                Offset = 20f,
                Height = 23,
                IconPadding = 0f,
                BorderAlpha = 0.323f,
                Borders=true,
               
            };
            tree.DefaultMenuStyle = customMenuStyle;
            tree.Config.DrawSearchToolbar = true;

            InitSkill(tree);
            InitBuff(tree);
            InitBullet(tree);
            InitPassive(tree);

            return tree;
        }
#endif
    }

    public class SkillMenuItem : OdinMenuItem
    {
        private readonly string Tag;
        private readonly ExplorerItem instance;
        private bool isClick;
        public SkillMenuItem(OdinMenuTree tree, ExplorerItem instance, string tag) : base(tree, instance.ID.ToString(), instance)
        {
            this.instance = instance;
            this.Tag = tag;
            SearchString = SmartName;
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            labelRect.x -= 16;
            this.instance.Select = GUI.Toggle(labelRect.AlignMiddle(18).AlignLeft(16), this.instance.Select, GUIContent.none);
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                var selection = this.MenuTree.Selection
                    .Select(x => x.Value)
                    .OfType<ExplorerItem>();

                if (selection.Any())
                {
                    var enabled = !selection.FirstOrDefault().Select;
                    selection.ForEach(x => x.Select = enabled);
                    Event.current.Use();
                }
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                if (!isClick && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    isClick = true;
                    if (isClick)
                    {
                        instance.LoadPrefab(false);
                    }
                }
            }


            if (Event.current.type == EventType.MouseUp)
            {
                isClick = false;
            }
        }

        public override string SmartName { get { return this.Tag + this.instance.GetGroupName(); } }
    }

    public class ParamsInfo
    {
        //伤害ID
        public int ParamsID;
        //运行时ID
        public int RuntimeID;
        //运行时介绍
        public string RuntimeDesc;
        //运行时类型
        public BattleRuntimeTypeEnum RuntimeType;
        //出现次数
        public int Count;
        //活跃时间
        public int ActiveTime;
        //伤害段数
        public int Timing;
    }
}