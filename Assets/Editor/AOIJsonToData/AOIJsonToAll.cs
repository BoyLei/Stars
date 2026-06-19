using UnityEditor;
/// <summary>
/// 所有AOI.json中props属性导入到AOIAttrDefine中
/// 所有AOI的json转换成cs类
/// </summary>
public static class AOIJsonToAll
{

    [MenuItem("自动化工具/AOI JSON/AOIJsonToAll", false, 1)]

    private static void GenerateAOIJsonToAll()
    {
        AOIJsonToDefine.GenerateAOIJsonToDefine();
        AOIJsonToCs.GenerateAOIJsonToCs();

        //Tset("Assets/Res/Animation/Roles/Monster/SmallOrcMonster/XiaoLR/Attack_01_1");
        //Tset("Assets/Res/Animation/Roles/Monster/SmallOrcMonster/CM/Attack_01_1");
        //Tset2("Assets/Res/Animation/Roles/Monster/SmallOrcMonster/CM/Attack_01_1");
        //Tset2("Assets/Res/Animation/CM/Attack_01_1");
    }


    private static void Tset(string path)
    {
        //int gangLastIndex = path.LastIndexOf('/') + 1;  // 最后一个斜杠
        //// pathNewstring animName = path.Substring(gangLastIndex, path.Length - gangLastIndex);   // 找到动作名
        //// string qianmiandestr = path.Substring(0, gangLastIndex - 1);    // 最后一个斜杠前面的字符
        //int gangLastIndex2 = path.LastIndexOf('/', gangLastIndex - 2) + 1;   // 倒数第二个斜杠
        //string replaceStr = path.Substring(gangLastIndex2, gangLastIndex - gangLastIndex2 - 1);   // 找到动作名前面一个斜杠里的字符
        //string pathNew = path.Replace(replaceStr, "CM");

        string[] words = path.Split('/');
        // 删除【CM/AnimName】前面一个斜杠内的字符
        // 如果前面一个斜杠的内容是【Animation】就不删除了
        if (words.Length - 2 > 0)
        {
            words[words.Length - 2] = "CM";
            // 删除后，重新合并数组
            string pathNew = words.KJoin("/");
            //SGF.Debuger.LogError($"----------------------path={path},,pathNew={pathNew}");
            SGF.Debuger.LogError($"----------------------path={path},,pathNew={pathNew}");
        }

    }

    private static void Tset2(string path)
    {
        //int gangLastIndex = path.LastIndexOf($"/CM/") - 3;  // 最后一个斜杠
        //string qianmiandestr = path.Substring(0, path.Length - 1 - gangLastIndex);
        //int gangLastIndex2 = qianmiandestr.LastIndexOf("/") - 1;  // 最后一个斜杠

        //string replaceStr = path.Substring(gangLastIndex2, gangLastIndex - gangLastIndex2 - 1);   // 找到动作名前面一个斜杠里的字符
        //string pathNew = path.Replace(replaceStr, "CM");

        // "Assets/Res/Animation/Roles/Monster/SmallOrcMonster/CM/Attack_01_1"

        string[] words = path.Split('/');
        // 删除【CM/AnimName】前面一个斜杠内的字符
        // 如果前面一个斜杠的内容是【Animation】就不删除了
        if (words.Length - 3 > 0)
        {
            if (words[words.Length - 3] != "Animation")
            {
                words[words.Length - 3] = "";
                // 删除后，重新合并数组
                string pathNew = words.KJoin("/");
                pathNew = pathNew.Replace($"//", "/");
                SGF.Debuger.LogError($"----------------------path={path},,pathNew={pathNew}");
                return;
            }
        }
        SGF.Debuger.LogError($"----------------------path={path}");
    }
}
