using StarProjectDef;
using System;
using System.Collections.Generic;

public static class ResourcesUtli
{

    // "Assets/Res/Animation/Roles/Monster/SmallOrcMonster/XiaoLR/Attack_01_1"
    // "Assets/Res/Animation/Roles/Monster/SmallOrcMonster/CM/Attack_01_1"
    // "Assets/Res/Animation/Roles/Monster/CM/Attack_01_1"
    // "Assets/Res/Animation/Roles/CM/Attack_01_1"
    // "Assets/Res/Animation/CM/Attack_01_1"

    private static Dictionary<E_AssetType, Dictionary<int, string>> PathMap =
        new();

    public static bool GetCachePath(string path, E_AssetType type, out string result)
    {
        result = path;
        if (PathMap.ContainsKey(type))
        {
            int key = path.GetHashCode();
            if (PathMap[type].TryGetValue(key, out result) && !string.IsNullOrEmpty(result))
            {
                return true;
            }
        }
        return false;
    }

    public static string GetReadPath(string path, E_AssetType type, Func<string, E_AssetType, bool> funcContains)
    {
        string res = path;
        if (!GetCachePath(path, type, out res))
        {
            res = path;
            bool isContains = false;
            int i = 0;
            do
            {
                if (i > 10)
                {
                    break;
                }
                i++;
                isContains = funcContains.Invoke(res, type);
                if (!isContains)
                {
                    res = GetNewPath(res, type);
                }
                if (res == "")
                {
                    break;
                }
            } while (!isContains);

            int key = path.GetHashCode();
            if (PathMap.ContainsKey(type))
            {
                if (PathMap[type].ContainsKey(key))
                {
                    PathMap[type][key] = res;
                }
                else
                {
                    PathMap[type].Add(key, res);
                }
            }
            else
            {
                var dic = new Dictionary<int, string>();
                dic.Add(key, res);
                PathMap.Add(type, dic);
            }
        }
        return res;
    }

    // Animation/Roles/Monster/PersonMonster/YiJ_XZ_ML/YiJ_XZ_YJ/cm/Idle_01
    // Animation/Roles/Monster/PersonMonster/YiJ_XZ_ML/cm/cm/Idle_01
    // Animation/Roles/Monster/PersonMonster/cm/cm/Idle_01
    // Animation/Roles/Monster/cm/cm/Idle_01
    // Animation/Roles/cm/cm/Idle_01

    // Animation\Roles\Monster\PersonMonster\YiJ_XZ_ML\cm

    // Animation/Roles/Monster/PersonMonster/YiJ_XZ_ML/YiJ_XZ_YJ/cm
    private static string GetNewPath(string path, E_AssetType type)
    {
        string pathNew = "";
        string[] words = path.Split('/');
        if (words.Length - 2 > 0)
        {
            if (words[words.Length - 2] != "cm")
            {
                words[words.Length - 2] = "cm";
                // 删除后，重新合并数组
                pathNew = words.KJoin("/");
                //SGF.Debuger.LogError($"-------------1111---------path={path},,pathNew={pathNew}");
                return pathNew;
            }
            else
            {
                if (words.Length - 3 > 0)
                {
                    string defineStr = type.ToString();
                    if (words[words.Length - 3] != defineStr)
                    {
                        words[words.Length - 3] = "";
                        pathNew = words.KJoin("/");
                        pathNew = pathNew.Replace($"//", "/");
                        //SGF.Debuger.LogError($"----------2222------------path={path},,pathNew={pathNew}");
                        return pathNew;
                    }
                }
            }
        }
        return pathNew;
    }

    private static void CachePath()
    {

    }

}
