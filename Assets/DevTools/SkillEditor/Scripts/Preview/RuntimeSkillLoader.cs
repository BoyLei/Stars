#if UNITY_EDITOR
//加载运行时技能
using Newtonsoft.Json;
using SGF.UI.Framework;
using SkillEditor;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.UI.StarWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RuntimeSkillLoader : MonoBehaviour
{
    private string SkillConfigPath = "Assets/DevTools/SkillEditor/Config/SkillConfig.json";

    IEnumerator Start()
    {
        yield return new WaitUntil(() => GameManager.Instance.M_MainPlayerCtrlBase != null);

        LoadSkills();
        StarWorldPage starWorldPage = (StarWorldPage)UIManager.Instance.M_Current_UIPage;
        starWorldPage.transform.localScale = Vector3.zero;
    }

    public ExplorerManager GetConfig(string path)
    {
        var skillConfig = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        if (skillConfig != null)
        {
            return JsonConvert.DeserializeObject<ExplorerManager>(skillConfig.text);
        }

        return null;
    }

    public void LoadSkills()
    {
        var subObjects = SkillEditorGlobal.Instance.transform.GetChildren();
        foreach (var item in subObjects)
        {
            GameObject.Destroy(item.gameObject);
        }

        //获取主角
        List<int> skillIds = new List<int>();
        (GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup)?.GetSkillIds(ref skillIds);

        //获取技能信息
        var skillConfig = GetConfig(SkillConfigPath);
        if (skillConfig != null && skillConfig.Items != null)
        {
            foreach (var item in skillConfig.Items)
            {
                if (skillIds.Contains(item.ID))
                {
                    if (!string.IsNullOrEmpty(item.PrefabPath))
                    {
                        var baseGenera = AssetDatabase.LoadAssetAtPath<GameObject>(item.PrefabPath);
                        if (baseGenera != null)
                        {
                            var go = PrefabUtility.InstantiatePrefab(baseGenera) as GameObject;
                            if (go != null)
                            {
                                go.transform.SetParent(SkillEditorGlobal.Instance.transform);
                                go.transform.localPosition = Vector3.zero;
                                go.transform.localRotation = Quaternion.identity;
                                go.transform.localScale = Vector3.one;
                                go.name = baseGenera.name;
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif