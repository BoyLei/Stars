using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FancyScrollView.Example08;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using StarProject.Service.LocalData;
using UnityEngine;

delegate string ParseRichTextDelegate(int id,bool needColor);

/// <summary>
/// 富文本解析
/// </summary>
public static class RichTextUtils
{
    static Regex regex = new Regex(@"\[[^]]*\]");

    private static Dictionary<string, ParseRichTextDelegate> CacheString =
        new Dictionary<string, ParseRichTextDelegate>()
        {
            { "Item_Name_", GetItemName },
            { "Monster_Name_", GetMonsterName },
            { "RankCommon_RankName_", GetRankName },
        };


    private static string GetItemName(int id, bool needColor)
    {
        var ItemData = LocalDataManager.Instance.GetItemDataCell(id);
        if (ItemData != null)
        {
            if(needColor)
            {
                return string.Format("<color={0}>{1}</color>", ExtraData.GetQualityColorRGB(ItemData.GetQuality()), ItemData.Name);
            }
            else
            {
                return ItemData.Name;
            }
        }

        return string.Empty;
    }

    private static string GetMonsterName(int id, bool needColor)
    {
        var Data = LocalDataManager.Instance.GetMonsterDataCell(id);
        if (Data != null)
        {
            return Data.Name;
        }

        return string.Empty;
    }

    private static string GetRankName(int id, bool needColor)
    {
        var Data = LocalDataManager.Instance.GetRankCommonDataCell(id);
        if (Data != null)
        {
            return Data.RankName;
        }

        return string.Empty;
    }

    private static string GetCacheString(string key, int id, bool needColor)
    {
        if (CacheString.ContainsKey(key))
        {
            return CacheString[key]?.Invoke(id, needColor);
        }

        return string.Empty;
    }

    public static string ParseRunHorseText(string richText, string args, bool needColor = false)
    {
        string[] temps = args.Split(",");
        List<string> list = new List<string>();
        foreach (var item in temps)
        {
            if (!string.IsNullOrEmpty(item))
                list.Add(item);
        }

        return ParseRunHorseRichText(richText, list, needColor);
    }

    public static string ParseRunHorseRichText(string richText, List<string> args, bool needColor = false)
    {
        string[] list = new string[args.Count];
        for (int i = 0; i < args.Count; i++)
        {
            list[i] = args[i];
        }

        MatchCollection matches = regex.Matches(richText);
        foreach (Match match in matches)
        {
            string real = match.Value.Replace("[", string.Empty).Replace("]", string.Empty);
            string[] temps = real.Split("_");
            if (temps.Length >0)
            {
                string idStr = temps[temps.Length-1].Replace("{", string.Empty).Replace("}", string.Empty);
                int index = System.Int32.Parse(idStr);

                if (index > -1 && index < args.Count)
                {

                   
                    string key = "";
                    for(int i=0;i< temps.Length-1;i++)
                    {
                        key = key + temps[i] + "_";
                    }
                    string result = GetRunHorseString(key, args[index], needColor);
                    richText = richText.Replace(key, string.Empty);
                    list[index] = result;

                }
            }
        }

        richText = richText.Replace("[", string.Empty).Replace("]", string.Empty);
        return string.Format(richText, list);
    }

    public static string GetRunHorseString(string key,string arg,bool needColor)
    {
        switch(key)
        {
            case "Item_Name_":
                if (System.Int32.TryParse(arg, out int id))
                {
                    return GetItemName(id, needColor);
                }
                break;
            case "Monster_Name_":
                if (System.Int32.TryParse(arg, out int id2))
                {
                    return GetMonsterName(id2, needColor);
                }
                break;
            case "RankCommon_RankName_":
                if (System.Int32.TryParse(arg, out int id3))
                {
                    return GetRankName(id3, needColor);
                }
                break;
            case "Player_Name_":
            case "Guild_Name_":
            case "Layer_":
                return arg;
            default:
                return arg;
                break;
        }

        return "";
    }


    public static string ParseMailText(string richText, string args, bool needColor = false)
    {
        string[] temps = args.Split(",");
        List<string> list = new List<string>();
        foreach (var item in temps)
        {
            if(!string.IsNullOrEmpty(item))
                list.Add(item);
        }

        return ParseRichText(richText, list, needColor);
    }

    public static string ParseRichText(string richText, List<string> args, bool needColor = false)
    {
        string[] list = new string[args.Count];
        for (int i = 0; i < args.Count; i++)
        {
            list[i] = args[i];
        }

        MatchCollection matches = regex.Matches(richText);
        foreach (Match match in matches)
        {
            string real = match.Value.Replace("[", string.Empty).Replace("]", string.Empty);
            string[] temps = real.Split("_");
            if (temps.Length == 3)
            {
                string idStr = temps[2].Replace("{", string.Empty).Replace("}", string.Empty);
                int index = System.Int32.Parse(idStr);

                if (index > -1 && index < args.Count)
                {              
                    if (System.Int32.TryParse(args[index], out int id))
                    {
                        string result = ParseSingleRichText(real, id, needColor);
                        string key = string.Concat(temps[0], "_", temps[1], "_");
                        richText = richText.Replace(key, string.Empty);
                        list[index] = result;
                    }
                }
            }
        }

        richText = richText.Replace("[", string.Empty).Replace("]", string.Empty);
        return string.Format(richText, list);
    }


    public static string ParseSingleRichText(string inputText, int id, bool needColor)
    {
        string[] args = inputText.Split("_");
        if (args.Length == 3)
        {
            string key = string.Concat(args[0], "_", args[1], "_");
            string result = GetCacheString(key, id, needColor);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            result = ReadFromJson(args[0], args[1], id);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
        }

        return inputText;
    }


    private static string ReadFromJson(string jsonName, string tableName, int id)
    {
        /*
        var jsonAsset =
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadAssetSync<TextAsset>($"Config/Excel/{jsonName}");
        if (jsonAsset != null)
        {
            JObject jObject=  JObject.Parse(jsonAsset.text);
            var root = (JObject)jObject[$"Static{jsonName}Datas"] ;
            if (root.ContainsKey(id.ToString()))
            {
                var p = root.Property(id.ToString());
                return p.Value[tableName].ToString();
            }
        }
        */

        return LocalDataManager.Instance.ReadFromJson(jsonName, tableName, id);
    }
}