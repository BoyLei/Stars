using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;
using SkillEditor;
using MessagePack;
using UnityEngine;
using PolymorphicMessagePack;

public class LuaTableOptimizer
{
    static string cfgPath = "Assets/Res/LuaScripts/Common/Config/";
    static string outputDir = "Assets/StreamingAssets/Database/";
    static string jsonPath = "Assets/DevTools/SkillEditor/Export/Json/";
    static string msgpackOutput = "Assets/Res/Config/Skill/";

    [MenuItem("Tools/Start Merge Skill JsonTable")]
    public static void StartMergeJsonTable()
    {
        PolymorphicRegister.Register();
        PolymorphicResolver.Instance.Init();

        MergeJsonTable<BuffJson>("Buff");
        MergeJsonTable<BulletJson>("Bullet");
        MergeJsonTable<PassiveJson>("Passive");
        MergeJsonTable<SkillJson>("Skill");

        Debug.Log("StartMergeJsonTable OK!!");
    }

    static void MergeJsonTable<T>(string dirName)
    {
        var dic = new Dictionary<int, T>();
        var files = Directory.GetFiles(jsonPath + dirName + "/", "*.json");
        foreach (var file in files)
        {
            var strs = File.ReadAllText(file);

            if (typeof(T) == typeof(BuffJson))
            {
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<BuffJson>(strs);   //需要json.config.ID
                if (json != null)
                {
                    dic.Add(json.config.ID, Newtonsoft.Json.JsonConvert.DeserializeObject<T>(strs));
                }
            }
            else if (typeof(T) == typeof(BulletJson))
            {
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<BulletJson>(strs);
                if (json != null)
                {
                    dic.Add(json.config.ID, Newtonsoft.Json.JsonConvert.DeserializeObject<T>(strs));
                }
            }
            else if (typeof(T) == typeof(PassiveJson))
            {
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<PassiveJson>(strs);
                if (json != null)
                {
                    dic.Add(json.config.ID, Newtonsoft.Json.JsonConvert.DeserializeObject<T>(strs));
                }
            }
            else if (typeof(T) == typeof(SkillJson))
            {
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<SkillJson>(strs);
                if (json != null)
                {
                    dic.Add(json.config.ID, Newtonsoft.Json.JsonConvert.DeserializeObject<T>(strs));
                }
            }
        }
        byte[] byteArrary = MessagePackSerializer.Serialize(dic);
        File.WriteAllBytes(msgpackOutput + dirName + ".bytes", byteArrary);
        AssetDatabase.Refresh();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    [MenuItem("Tools/LuaOpt")]
    public static void LuaOpt()
    {
        string[] filesPaths = Directory.GetFiles(Application.dataPath + "/Res/LuaScripts/Common/Config", "*.txt", SearchOption.AllDirectories);
        for (int i = 0; i < filesPaths.Length; i++)
        {
            string path = filesPaths[i];

            string text = File.ReadAllText(path);
            Debug.Log(path);
            string optText = GetLuaOptiomize(text);
            File.WriteAllText(path, optText);
        }
        AssetDatabase.Refresh();
    }

    static string Template = @"
local KeyMap = {
[KEY_MAP]
}

local R = {
[REF_CHUNK]
}

local [TABLE_NAME] = 
{
[TABLE_CONTENT]
}

do
    MT(KeyMap, [TABLE_NAME])
end

return [TABLE_NAME]
";

    /*创建元表函数。放到一个能全局访问到的地方
    function MT(KeyMap, table)
         local base = {
            __index = function(table,key)
                local keyIndex = KeyMap[key]
                if not keyIndex then
                    print("Key not found: ",key)
                    return nil
                end
                return table[keyIndex]
            end,
    --[[
            __newindex = function(table, key, val)
                local keyIndex = KeyMap[key]
                if not keyIndex then
                    print("Key not found: ",key)
                end
                table[keyIndex] = val
                error(""Attempt to modify read-only table"")
            end
    ]]--
        }
        for k, v in pairs(table) do
            setmetatable(v, base)
        end
        base.__metatable = false
        return base
    end 
     */

    static List<string> ExcludeFiles = new List<string>()
    {
        "ConfigDataAccessor.lua.txt",
        "备注.lua.txt"
    };

    static bool IsExcludeFile(string file)
    {
        foreach (var item in ExcludeFiles)
        {
            if (file.EndsWith(item))
                return true;
        }
        return false;
    }

    [MenuItem("Tools/Start IndexTableOptimizer")]
    public static void StartOptiomizer()
    {
        if (Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, true);
        }
        Directory.CreateDirectory(outputDir);

        var files = Directory.GetFiles(cfgPath, "*.lua.txt");
        foreach (var file in files)
        {
            if (/*IsExcludeFile(file)*/!file.EndsWith("\\Item.lua.txt"))
                continue;

            var lines = File.ReadAllText(file);

            var newText = GetLuaOptiomize(lines);

            var destFile = outputDir + Path.GetFileName(file);
            
            File.WriteAllText(destFile, newText, new UTF8Encoding(false));
        }
        AssetDatabase.Refresh();
    }

    class LuaTableChunk
    {
        public string index;
        public List<string> table;
    }

    public static string GetLuaOptiomize(string luaCode)
    {
        var lines = luaCode.Split('\n');

        var (name, dic, keyMaps, refs) = ParseAnTable(lines);

        var refChunk = string.Join("\n", refs);
        var newText = Template.Replace("[REF_CHUNK]", refChunk);

        newText = newText.Replace("[TABLE_NAME]", name);

        ///KEY_MAP
        var keyMapVar = string.Empty;
        for (int i = 0; i < keyMaps.Count; i++)
        {
            var key = keyMaps[i];
            keyMapVar += string.Format("\t{0} = {1},\n", key, i + 1);
        }
        newText = newText.Replace("[KEY_MAP]", keyMapVar);

        ///TABLE_CONTENT
        var rowBuilder = new StringBuilder();
        foreach (var entry in dic)
        {
            rowBuilder.Append(string.Format("\t[{0}]=\n", entry.index));
            rowBuilder.Append("\t{\n");
            foreach (var value in entry.table)
            {
                rowBuilder.Append(string.Format("\t\t{0},\n", value));
            }
            rowBuilder.Append("\t},\n");
        }
        newText = newText.Replace("[TABLE_CONTENT]", rowBuilder.ToString());
        return newText;
    }

    static (string, List<LuaTableChunk>, List<string>, List<string>) ParseAnTable(string[] lines)
    {
        var chunkDic = new List<LuaTableChunk>();
        var keyMaps = new List<string>();
        var refValues = new List<string>(); 
        var repeatedValues = new Dictionary<string, int>();

        var tableName = string.Empty;
        List<string> dicTable = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Replace("\t", string.Empty);
            if (string.IsNullOrEmpty(line) || line.StartsWith("-"))
            {
                continue;
            }
            if (line.StartsWith("return "))
            {
                var strs = line.Split(' ');
                tableName = strs.Length > 1 ? strs[1] : string.Empty;
                continue;
            }
            else if (line.Contains("[") && line.Contains("]="))
            {
                var index = line.Replace("[", string.Empty).Replace("]=", string.Empty).Trim();

                i++;
                dicTable = new();
                chunkDic.Add(new LuaTableChunk { index = index, table = dicTable });
                continue;
            }
            else if (dicTable != null && line.Trim().StartsWith("}"))
            {
                dicTable = null;
                continue;
            }
            if (dicTable != null)
            {
                var index = line.IndexOf("=");
                var k = line.Substring(0, index).Trim();

                if (!keyMaps.Contains(k))
                    keyMaps.Add(k);

                var v = line.Substring(index + 1).Trim();

                if (v.EndsWith(","))
                {
                    v = v.Remove(v.Length - 1, 1);
                }
                dicTable.Add(v);

                if (!string.IsNullOrEmpty(v) && v != "")
                {
                    if (v.StartsWith("L(\""))   //多语言函数
                    {
                        var (key, _) = GetLangKey(v);

                        if (repeatedValues.ContainsKey(key))
                            repeatedValues[key]++;
                        else
                            repeatedValues[key] = 1;
                    }
                    else
                    {
                        if (v.StartsWith("\"") || v.StartsWith("'") || IsValidTable(v))
                        {
                            if (repeatedValues.ContainsKey(v))
                                repeatedValues[v]++;
                            else
                                repeatedValues[v] = 1;
                        }
                    }
                }
            }
        }
        int refIndex = 0;
        foreach (var kv in repeatedValues)
        {
            if (kv.Value > 1)
            {
                var refVar = "_" + (++refIndex);
                var codeKey = string.Format("\t{0} = {1},", refVar, kv.Key);

                refValues.Add(codeKey);

                foreach (var entry in chunkDic)
                {
                    var list = entry.table;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] == kv.Key)
                        {
                            list[i] = "R." + refVar;
                        }
                        //else if (list[i].StartsWith("L(\""))
                        //{
                        //    var (key, value) = GetLangKey(list[i]);
                        //    if (key == kv.Key)
                        //    {
                        //        list[i] = "L(R." + refVar + "," + value + ")";
                        //    }
                        //}
                    }
                }
            }
        }
        return (tableName, chunkDic, keyMaps, refValues);
    }

    static bool IsValidTable(string v)
    {
        return v.StartsWith("{") && v.EndsWith("}") && v != "{}";
    }

    static (string, string) GetLangKey(string line)
    {
        var left = line.IndexOf('"');
        var right = line.IndexOf("\"", left + 1);
        var count = right - left - 1;
        if (count <= 0)
        {
            throw new System.Exception(line);
        }
        var substr = line.Substring(left + 1, count);

        var index = -1;
        for (int i = 0; i < substr.Length; i++)
        {
            if (!char.IsDigit(substr[i]))
            {
                index = i;
            }
            else
            {
                break;
            }
        }
        var keyName = string.Empty;
        var value = string.Empty;
        if (index == substr.Length - 1)   //没数字
        {
            keyName = substr;
        }
        else
        {
            keyName = substr.Substring(0, index + 1);
            value = substr.Substring(index + 1);
        }
        return ("'" + keyName + "'", value);
    }
}
