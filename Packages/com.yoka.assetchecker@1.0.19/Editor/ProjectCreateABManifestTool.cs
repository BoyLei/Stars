using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ProjectCreateABManifestTool
{
    static string filePath = Application.dataPath + "/EditorABManifest.manifest";
    
    [MenuItem("Assets/Tools/其他/创建 AB Manifest", false, 99)]
    static void CreateABManifest()
    {
        var manifestGroup = GetABManifest();

        StreamWriter sw = new StreamWriter(filePath);
        sw.Write("ManifestFileVersion: 0\nCRC: 41205597\nAssetBundleManifest:\n  AssetBundleInfos:\n");
        List<KeyValuePair<string, List<string>>> manifestGroupList = manifestGroup.ToList();
        for (int i = 0; i < manifestGroup.Count; i++)
        {
            sw.Write("\tInfo_" + i + ":\n");
            sw.Write("\t  Name: "+manifestGroupList[i].Key+"\n");
            if (manifestGroupList[i].Value.Count == 0)
            {
                sw.Write("\t  Dependencies: {}\n");
            }
            else
            {
                sw.Write("\t  Dependencies:\n");
            }
            
            for (int j = 0; j < manifestGroupList[i].Value.Count; j++)
            {
                sw.Write("\t\tDependency_"+j+": "+manifestGroupList[i].Value[j]+"\n");
            }
        }
        sw.Close();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static Dictionary<string, List<string>> GetABManifest()
    {
        Dictionary<string, List<string>> manifestGroup = new Dictionary<string, List<string>>();
        var ABGroup = GetProjectABInfo();
        int index = 0;
        foreach (var data in ABGroup)
        {
            List<string> abGroup = new List<string>();
            EditorUtility.DisplayProgressBar("生成项目 AB Manifest......", "当前:" + index + "/" + ABGroup.Count, index * 1.0f / ABGroup.Count);
            string srcAB = data.Key;
            foreach (var path in data.Value)
            {
                string[] results = AssetDatabase.GetDependencies(path);
                foreach (var result in results)
                {
                    string fullAB = GetFullABInfo(result);
                    if (fullAB == "" || srcAB == fullAB) continue;

                    if (!abGroup.Contains(fullAB)) abGroup.Add(fullAB);
                }
            }
            manifestGroup.Add(srcAB, abGroup);
            index++;
        }
        EditorUtility.ClearProgressBar();
        return manifestGroup;
    }

    static string GetFullABInfo(string path)
    {
        string fullAB = "";
        string resultAB = AssetDatabase.GetImplicitAssetBundleName(path);
        if(resultAB == "") return "";
                    
        string resultABVariant = AssetDatabase.GetImplicitAssetBundleVariantName(path);
        if(resultABVariant != "")
        {
            fullAB = resultAB + "." + resultABVariant;
        }
        else
        {
            fullAB = resultAB;
        }
        return fullAB;
    }
      

    static Dictionary<string, List<string>> GetProjectABInfo()
    {
        string[] guids = AssetDatabase.FindAssets("", new string[] { "Assets" });
        Dictionary<string, List<string>> ABGroup = new Dictionary<string, List<string>>();
        int abResourceNum = 0;
        foreach (string guid in guids)
        {
            string originalPath = AssetDatabase.GUIDToAssetPath(guid);

            string ab = GetFullABInfo(originalPath);
            //排除非ab的资源
            if (ab == "") continue;

            abResourceNum++;
            if (!ABGroup.ContainsKey(ab))
            {
                List<string> pathGroup = new List<string>();
                pathGroup.Add(originalPath);
                ABGroup.Add(ab,pathGroup);
            }
            else
            {
                List<string> pathGroup = ABGroup[ab];
                pathGroup.Add(originalPath);
                ABGroup.Remove(ab);
                ABGroup.Add(ab, pathGroup);
            }
        }
        return ABGroup;
    }
}
