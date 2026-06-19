using Google.Protobuf;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Network;
using StarProject.Service.Battle;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.Resource;
using System;
using System.Text.RegularExpressions;

[XLua.LuaCallCSharp]
public static class ExtraData
{
    private static readonly Regex keywordReg = new(@"\[(.+?)\]", RegexOptions.Singleline);
    public static int GetBigEmoji(RepeatedField<Google.Protobuf.ByteString> bytes)
    {
        if (bytes == null || bytes.Count == 0)
        {
            return -1;
        }

        ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd((int)MsgIDEnum.ChatExtraInfosID);
        if (!protoInfo.HasValue)
        {
            return -1;
        }

        MessageParser parser = protoInfo.Value.parse;
        IMessage message = parser.ParseFrom(bytes[0]);
        var data = message as ChatExtraInfos;

        if (data.ChatExtraInfo.Count == 1 && data.ChatExtraInfo[0].EType == ProtoMsg.ChatExtraInfo.Types.EMsg.BigEmoji)
        {
            return data.ChatExtraInfo[0].BigEmojiID;
        }
        return -1;
    }

    public static string ClientToSrv(string str)
    {
        string srvStr = "";
        int curIndex = 0;
        int index = 0;
        foreach (Match match in keywordReg.Matches(str))
        {
            srvStr += str.Substring(curIndex, match.Index - curIndex);
            srvStr += $"[{index++}]";
            curIndex = match.Index + match.Length;
        }
        srvStr += str.Substring(curIndex, str.Length - curIndex);
        return srvStr;
    }

    public static string GetQualityColorRGB(int quality)
    {
        if (quality == 1)
        {
            return "#aaaaaa";
        }
        else if (quality == 2)
        {
            return "#78A096";
        }
        else if (quality == 3)
        {
            return "#7D9BCD";
        }
        else if (quality == 4)
        {
            return "#CDA4DE";
        }
        else if (quality == 5)
        {
            return "#e1af8c";
        }
        return "#aaaaaa";
    }

    public static string SrvToClient(string str, RepeatedField<Google.Protobuf.ByteString> bytes)
    {
        if (bytes == null || bytes.Count == 0)
        {
            return str;
        }

        ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd((int)MsgIDEnum.ChatExtraInfosID);
        if (!protoInfo.HasValue)
        {
            return "";
        }

        MessageParser parser = protoInfo.Value.parse;
        IMessage message = parser.ParseFrom(bytes[0]);
        var data = message as ChatExtraInfos;

        string cliStr = GetClientStr(str, data);
        return cliStr;
    }

    private static string GetClientStr(string str, ChatExtraInfos data)
    {
        string cliStr = "";
        int curIndex = 0;
        int matchIndex = 0;
        string namestr = str;
        if (data.ChatExtraInfo.Count > 0)
        {
            if (data.ChatExtraInfo[0].EType == ChatExtraInfo.Types.EMsg.TeamRecruit
            || data.ChatExtraInfo[0].EType == ChatExtraInfo.Types.EMsg.TeamGather
            || data.ChatExtraInfo[0].EType == ChatExtraInfo.Types.EMsg.TeamUserOut
            || data.ChatExtraInfo[0].EType == ChatExtraInfo.Types.EMsg.TeamCaptainChange
            || data.ChatExtraInfo[0].EType == ChatExtraInfo.Types.EMsg.ShareWantedTask
            || data.ChatExtraInfo[0].EType == ChatExtraInfo.Types.EMsg.GuildCreate
            )
            {
                str = "[1]";
            }
        }
        foreach (Match match in keywordReg.Matches(str))
        {
            if (matchIndex < data.ChatExtraInfo.Count)
            {
                cliStr += str.Substring(curIndex, match.Index - curIndex);
                switch (data.ChatExtraInfo[matchIndex].EType)
                {
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.Emoji:
                        {
                            string iconName = "Emoticon_nom_";
                            if (data.ChatExtraInfo[matchIndex].EmojiID < 10)
                            {
                                iconName = "Emoticon_nom_0";
                            }
                            iconName += data.ChatExtraInfo[matchIndex].EmojiID;
                            int index = BattleManager.Instance.GetCurrencySpriteSpriteIndexFromName(iconName);
                            cliStr += $"<sprite={index}>";
                        }
                        break;
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.Equip:
                        {
                            long id = data.ChatExtraInfo[matchIndex].EquipData[0].BaseID;
                            var cfgdata = LocalDataManager.Instance.GetItemDataCell(id);
                            if (cfgdata != null)
                            {
                                string image = string.Empty;
                                var currencyCfg = LocalDataManager.Instance.GetCurrencyCfgDataCell((int)id);
                                if (currencyCfg != null)
                                {
                                    string itemIconName = "";
                                    var iconPath = cfgdata.Icon.Split("/");
                                    if (iconPath.Length > 0)
                                    {
                                        itemIconName = iconPath[iconPath.Length - 1];
                                        int index = BattleManager.Instance.GetCurrencySpriteSpriteIndexFromName(itemIconName);
                                        image = $"<sprite={index}>";
                                    }
                                }
                                cliStr += $"<link>{image}<color={GetQualityColorRGB(cfgdata.GetQuality())}>[{cfgdata.Name}]</color></link>";
                            }
                        }
                        break;
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.Location:
                        {
                            var locationInfo = data.ChatExtraInfo[matchIndex].LocationInfo;
                            if (locationInfo != null)
                            {
                                var mapCfgData = LocalDataManager.Instance.GetMapCfgData(locationInfo.MapId);
                                if (mapCfgData != null)
                                {
                                    string mapName = mapCfgData.MapName;
                                    ProtoMsg.Vector3 pos = locationInfo.Pos;
                                    //cliStr += $"<link><color=#67E4FF>[{mapName}：{pos.X:#0.0},{pos.Z:#0.0}] {GameConfig.LocalStr["ChatClickGoto"]}</color></link>";
                                    cliStr += $"<link><color=#67E4FF>[{mapName}：{pos.X:#0.0},{pos.Z:#0.0}] {LanguageManager.Instance.GetLanguageByKey("ChatClickGoto")}</color></link>";
                                }
                            }
                        }
                        break;
                    case ChatExtraInfo.Types.EMsg.TeamRecruit:
                        {
                            var strName = string.Format(LanguageManager.Instance.GetLanguageByKey("Local_Str_Team_DefaultName"), data.ChatExtraInfo[matchIndex].TRecruit.TeamName);
                            //cliStr = $"{strName}{GameConfig.LocalStr["ChatTeamCreatFinish"]}<link><color=#67E4FF>[{GameConfig.LocalStr["ChatClickJoin"]}]</color></link>";
                            cliStr = $"{strName}{LanguageManager.Instance.GetLanguageByKey("ChatTeamCreatFinish")}<link><color=#67E4FF>[{LanguageManager.Instance.GetLanguageByKey("ChatClickJoin")}]</color></link>";
                        }
                        break;
                    case ChatExtraInfo.Types.EMsg.TeamGather:
                        {
                            //cliStr = $"{namestr}{GameConfig.LocalStr["ChatTeamTips"]}";
                            cliStr = $"{namestr}{LanguageManager.Instance.GetLanguageByKey("ChatTeamTips")}";
                        }
                        break;
                    case ChatExtraInfo.Types.EMsg.TeamUserOut:
                        {
                            //cliStr = $"{GameConfig.LocalStr["ChatTeamTips1"]}";
                            cliStr = $"{namestr}{LanguageManager.Instance.GetLanguageByKey("ChatTeamTips1")}";
                        }
                        break;
                    case ChatExtraInfo.Types.EMsg.TeamCaptainChange:
                        {
                            //cliStr = String.Concat(data.ChatExtraInfo[matchIndex].Captainer, $"{GameConfig.LocalStr["ChatTeamTips2"]}");
                            cliStr = String.Concat(data.ChatExtraInfo[matchIndex].Captainer, $"{LanguageManager.Instance.GetLanguageByKey("ChatTeamTips2")}");
                        }
                        break;
                    case ChatExtraInfo.Types.EMsg.GuildEvent:
                        {
                            // 替换公会事件的替换符
                            var eventInfo = data.ChatExtraInfo[matchIndex].GEInfo2;
                            if (eventInfo != null)
                            {
                                var cfg = LocalDataManager.Instance.GetGuildEventDataCell(eventInfo.EventID);
                                string eventStr = cfg.EventDec;
                                // 先把时间的替换，给清空了
                                {
                                    string oldStr = "{0}";
                                    string newStr = "";
                                    eventStr = eventStr.Replace(oldStr, newStr);
                                }
                                var temp = eventInfo.EventStr.Split(",");
                                for (int i = 0; i < temp.Length; i++)
                                {
                                    int index = i + 1;
                                    string oldStr = "{" + index + "}";
                                    string newStr = temp[i];
                                    if ((eventInfo.EventID == 6 && i == 2) || (eventInfo.EventID == 5 && i == 1))
                                    {
                                        int id = int.Parse(newStr);
                                        var titleCfg = LocalDataManager.Instance.GetGuildTitleDataCell(id);
                                        if (titleCfg != null)
                                        {
                                            newStr = titleCfg.Comments;
                                        }
                                    }
                                    eventStr = eventStr.Replace(oldStr, newStr);
                                }
                                cliStr = eventStr;
                            }
                        }
                        break;
                    case ChatExtraInfo.Types.EMsg.GuildCreate:
                        {
                            // 公会创建
                            var strName = string.Format(LanguageManager.Instance.GetLanguageByKey("Local_Str_Auto_Tips_264"), data.ChatExtraInfo[matchIndex].Captainer);
                            cliStr = $"<link>{strName}</link>";
                        }
                        break;
                    case ChatExtraInfo.Types.EMsg.ShareWantedTask:
                        {
                            // 共享通缉任务[参数队长名]
                            //string desc = GameConfig.LocalStr["ChatWantedTips"];
                            string desc = LanguageManager.Instance.GetLanguageByKey("ChatWantedTips");
                            var windowCfg = LocalDataManager.Instance.GetWindowDataCell(42);
                            if (windowCfg != null)
                            {
                                desc = windowCfg.Desc;
                            }
                            cliStr = string.Format(desc, data.ChatExtraInfo[matchIndex].Captainer);
                        }
                        break;
                }
                curIndex = match.Index + match.Length;
                matchIndex++;
            }
        }
        cliStr += str.Substring(curIndex, str.Length - curIndex);
        return cliStr;
    }

    public static ChatMsgNotice GetChatTypeMsg(ChatMsgNoticeS chatMsgNotices, ProtoMsg.ChatExtraInfo.Types.EMsg msgType)
    {
        ChatMsgNotice ret = null;
        foreach (var chatMsgNotice in chatMsgNotices.CMNotices)
        {
            RepeatedField<Google.Protobuf.ByteString> bytes = chatMsgNotice.SendData;
            if (bytes == null || bytes.Count == 0)
            {
                continue;
            }

            ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd((int)MsgIDEnum.ChatExtraInfosID);
            if (!protoInfo.HasValue)
            {
                continue;
            }

            MessageParser parser = protoInfo.Value.parse;
            IMessage message = parser.ParseFrom(bytes[0]);
            ChatExtraInfos chatExtraInfos = message as ChatExtraInfos;

            if (chatExtraInfos.ChatExtraInfo != null && chatExtraInfos.ChatExtraInfo.Count > 0)
            {
                if (chatExtraInfos.ChatExtraInfo[0].EType == msgType)
                {
                    ret = chatMsgNotice;
                }
            }
        }
        return ret;
    }

    public static bool IsMsgType(ChatMsgNotice chatMsgNotice, ProtoMsg.ChatExtraInfo.Types.EMsg msgType)
    {
        RepeatedField<Google.Protobuf.ByteString> bytes = chatMsgNotice.SendData;
        if (bytes == null || bytes.Count == 0)
        {
            return false;
        }

        ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd((int)MsgIDEnum.ChatExtraInfosID);
        if (!protoInfo.HasValue)
        {
            return false;
        }

        MessageParser parser = protoInfo.Value.parse;
        IMessage message = parser.ParseFrom(bytes[0]);
        ChatExtraInfos chatExtraInfos = message as ChatExtraInfos;

        if (chatExtraInfos.ChatExtraInfo != null && chatExtraInfos.ChatExtraInfo.Count > 0)
        {
            if (chatExtraInfos.ChatExtraInfo[0].EType == msgType)
            {
                return true;
            }
        }
        return false;
    }

    public static string GetNewStr(RepeatedField<string> strs)
    {
        if (strs != null && strs.Count > 0)
        {
            string strKey = strs[0];

            object[] args = new object[strs.Count - 1];
            for (int i = 1; i < strs.Count; i++)
            {
                args[i - 1] = strs[i];
            }
            return string.Format(LanguageManager.Instance.GetLanguageByKey(strKey), args);
        }

        return null;
    }

    public static string SrvToClientHud(ChatMsgNotice chatMsgNotice)
    {
        string cliStr = "";

        var newStr1 = GetNewStr(chatMsgNotice.Data2);
        if (newStr1 != null)
        {
            return newStr1;
        }

        string str = chatMsgNotice.Data;
        RepeatedField<Google.Protobuf.ByteString> bytes = chatMsgNotice.SendData;
        //string channelStr = GetchannelStr(chatMsgNotice.Channel);
        //cliStr += channelStr;
        //cliStr = String.Concat(cliStr, channelStr);
        if (bytes == null || bytes.Count == 0)
        {
            string entityName = GetEntityName(chatMsgNotice.ChatName, true);
            cliStr = String.Concat(cliStr, entityName, str);
            //cliStr += entityName;
            //cliStr += str;
            return cliStr;
        }

        ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd((int)MsgIDEnum.ChatExtraInfosID);
        if (!protoInfo.HasValue)
        {
            string entityName = GetEntityName(chatMsgNotice.ChatName, true);
            cliStr = String.Concat(cliStr, entityName, str);
            //cliStr += entityName;
            //cliStr += str;
            return cliStr;
        }

        MessageParser parser = protoInfo.Value.parse;
        IMessage message = parser.ParseFrom(bytes[0]);
        ChatExtraInfos chatExtraInfos = message as ChatExtraInfos;

        // 判断系统的类型
        // 【系统消息都是服务器下发的，只会在数组的第一个里面有值】
        if (chatExtraInfos.ChatExtraInfo != null && chatExtraInfos.ChatExtraInfo.Count > 0)
        {
            switch (chatExtraInfos.ChatExtraInfo[0].EType)
            {
                case ChatExtraInfo.Types.EMsg.Emoji:
                case ChatExtraInfo.Types.EMsg.Equip:
                case ChatExtraInfo.Types.EMsg.Location:
                case ChatExtraInfo.Types.EMsg.BigEmoji:
                case ChatExtraInfo.Types.EMsg.TeamRecruit:
                    {
                        string entityName = GetEntityName(chatMsgNotice.ChatName, true);
                        //string dataStr = GetClientStr(str, chatExtraInfos);
                        if (chatExtraInfos.ChatExtraInfo[0].EType == ChatExtraInfo.Types.EMsg.TeamRecruit)
                        {
                            str = "[1]";
                        }
                        string dataStr = GetDataStr(str, chatExtraInfos);
                        cliStr = String.Concat(cliStr, entityName, dataStr);
                        //cliStr += dataStr;
                    }
                    break;
                case ChatExtraInfo.Types.EMsg.SupLevel:
                    {
                        //string dataStr = $"恭喜{GetEntityName(chatMsgNotice.ChatName, false)}等级提升到<color=#B98A41>{chatExtraInfos.ChatExtraInfo[0].Level}</color>";
                        //string dataStr = string.Format(GameConfig.LocalStr["ChatSystemLevelUpTips"], GetEntityName(chatMsgNotice.ChatName, false), $"<color=#B98A41>{chatExtraInfos.ChatExtraInfo[0].Level}</color>");
                        string dataStr = string.Format(LanguageManager.Instance.GetLanguageByKey("ChatSystemLevelUpTips"), GetEntityName(chatMsgNotice.ChatName, false), $"<color=#B98A41>{chatExtraInfos.ChatExtraInfo[0].Level}</color>");
                        cliStr = String.Concat(cliStr, dataStr);
                        //cliStr += dataStr;
                    }
                    break;
                case ChatExtraInfo.Types.EMsg.TeamGather:
                    {
                        //cliStr = GameConfig.LocalStr["ChatTeamTips3"];
                        cliStr = LanguageManager.Instance.GetLanguageByKey("ChatTeamTips3");
                    }
                    break;
                case ChatExtraInfo.Types.EMsg.TeamUserOut:
                    {
                        //cliStr = GameConfig.LocalStr["ChatTeamTips1"];
                        cliStr = LanguageManager.Instance.GetLanguageByKey("ChatTeamTips1");
                    }
                    break;
                case ChatExtraInfo.Types.EMsg.TeamCaptainChange:
                    {
                        //cliStr = String.Concat(chatExtraInfos.ChatExtraInfo[0].Captainer, GameConfig.LocalStr["ChatTeamTips2"]);
                        cliStr = String.Concat(chatExtraInfos.ChatExtraInfo[0].Captainer, LanguageManager.Instance.GetLanguageByKey("ChatTeamTips2"));
                    }
                    break;
                case ChatExtraInfo.Types.EMsg.GuildEvent:
                    {
                        // 替换公会事件的替换符
                        var eventInfo = chatExtraInfos.ChatExtraInfo[0].GEInfo2;
                        if (eventInfo != null)
                        {
                            var cfg = LocalDataManager.Instance.GetGuildEventDataCell(eventInfo.EventID);
                            string eventStr = cfg.EventDec;
                            // 先把时间的替换，给清空了
                            {
                                string oldStr = "{0}";
                                string newStr = "";
                                eventStr = eventStr.Replace(oldStr, newStr);
                            }
                            var temp = eventInfo.EventStr.Split(",");
                            for (int i = 0; i < temp.Length; i++)
                            {
                                int index = i + 1;
                                string oldStr = "{" + index + "}";
                                string newStr = temp[i];
                                if ((eventInfo.EventID == 6 && i == 2) || (eventInfo.EventID == 5 && i == 1))
                                {
                                    int id = int.Parse(newStr);
                                    var titleCfg = LocalDataManager.Instance.GetGuildTitleDataCell(id);
                                    if (titleCfg != null)
                                    {
                                        newStr = titleCfg.Comments;
                                    }
                                }
                                eventStr = eventStr.Replace(oldStr, newStr);
                            }
                            cliStr = eventStr;
                        }
                    }
                    break;
                case ChatExtraInfo.Types.EMsg.GuildCreate:
                    {
                        // 公会创建
                        var strName = string.Format(LanguageManager.Instance.GetLanguageByKey("Local_Str_Auto_Tips_264"), chatExtraInfos.ChatExtraInfo[0].Captainer);
                        cliStr = $"<link>{strName}</link>";
                    }
                    break;
                case ChatExtraInfo.Types.EMsg.ShareWantedTask:
                    {
                        // 共享通缉任务[参数队长名]
                        //string desc = "队长{0}想要同步通缉任务，是否确认接取？";
                        //string desc = GameConfig.LocalStr["ChatWantedTips"];
                        string desc = LanguageManager.Instance.GetLanguageByKey("ChatWantedTips");
                        string captainerName = chatExtraInfos.ChatExtraInfo[0].Captainer;
                        var windowCfg = LocalDataManager.Instance.GetWindowDataCell(42);
                        if (windowCfg != null)
                        {
                            desc = windowCfg.Desc;
                        }
                        cliStr = string.Format(desc, captainerName);
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            string entityName = GetEntityName(chatMsgNotice.ChatName, true);
            cliStr = String.Concat(cliStr, entityName, str);
        }

        return cliStr;
    }

    private static string GetchannelStr(ChatChannel chatChannel)
    {
        string channelStr = "";
        switch (chatChannel)
        {
            case ChatChannel.ChannelNull:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel1")} </color>";
                }
                break;
            case ChatChannel.ChannelSystem:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel2")}] </color>";
                }
                break;
            case ChatChannel.ChannelSpace:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel3")}] </color>";
                }
                break;
            case ChatChannel.ChannelWorld:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel4")}] </color>";
                }
                break;
            case ChatChannel.ChannelUnion:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel5")}] </color>";
                }
                break;
            case ChatChannel.ChannelJob:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel6")}] </color>";
                }
                break;
            case ChatChannel.ChannelTeam:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel7")}] </color>";
                }
                break;
            case ChatChannel.ChannelCall:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel8")}] </color>";
                }
                break;
            case ChatChannel.ChannelPrivate:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel9")}] </color>";
                }
                break;
            case ChatChannel.ChannelSetting:
                {
                    channelStr = $"<color=#A6C889>[{LanguageManager.Instance.GetLanguageByKey("ChatChannelLabel10")}] </color>";
                }
                break;
            default:
                break;
        }
        return channelStr;
    }

    private static string GetEntityName(string name, bool isSymbol)
    {
        string str = "";
        //if (name.Length > 7)
        //{
        //    name = name.Substring(0, 7);
        //    name += "...";
        //}
        if (isSymbol)
        {
            str = $"<color=#8CC8DC><size=24>{name}: </size></color>";
        }
        else
        {
            str = $"<color=#8CC8DC><size=24>{name} </size></color>";
        }
        return str;
    }

    private static string GetDataStr(string dataStr, ChatExtraInfos chatExtraInfos)
    {
        string cliStr = "";
        int curIndex = 0;
        int matchIndex = 0;
        foreach (Match match in keywordReg.Matches(dataStr))
        {
            cliStr += dataStr.Substring(curIndex, match.Index - curIndex);
            if (matchIndex < chatExtraInfos.ChatExtraInfo.Count)
            {
                switch (chatExtraInfos.ChatExtraInfo[matchIndex].EType)
                {
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.Emoji:
                        {
                            string iconName = "Emoticon_nom_";
                            if (chatExtraInfos.ChatExtraInfo[matchIndex].EmojiID < 10)
                            {
                                iconName = "Emoticon_nom_0";
                            }
                            iconName += chatExtraInfos.ChatExtraInfo[matchIndex].EmojiID;
                            int index = BattleManager.Instance.GetCurrencySpriteSpriteIndexFromName(iconName);
                            cliStr += $"<sprite={index}>";
                        }
                        break;
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.Equip:
                        {
                            long id = chatExtraInfos.ChatExtraInfo[matchIndex].EquipData[0].BaseID;
                            var cfgdata = LocalDataManager.Instance.GetItemDataCell(id);
                            if (cfgdata != null)
                            {
                                string image = string.Empty;
                                var currencyCfg = LocalDataManager.Instance.GetCurrencyCfgDataCell((int)id);
                                if (currencyCfg != null)
                                {
                                    string itemIconName = "";
                                    var iconPath = cfgdata.Icon.Split("/");
                                    if (iconPath.Length > 0)
                                    {
                                        itemIconName = iconPath[iconPath.Length - 1];
                                        int index = BattleManager.Instance.GetCurrencySpriteSpriteIndexFromName(itemIconName);
                                        image = $"<sprite={index}>";
                                    }
                                }
                                cliStr += $"<link>{image}<color={GetQualityColorRGB(cfgdata.GetQuality())}>[{cfgdata.Name}]</color></link>";
                            }
                        }
                        break;
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.Location:
                        {
                            var locationInfo = chatExtraInfos.ChatExtraInfo[matchIndex].LocationInfo;
                            if (locationInfo != null)
                            {
                                var mapCfgData = LocalDataManager.Instance.GetMapCfgData(locationInfo.MapId);
                                if (mapCfgData != null)
                                {
                                    string mapName = mapCfgData.MapName;
                                    ProtoMsg.Vector3 pos = locationInfo.Pos;
                                    //cliStr += $"<link><color=#67E4FF>[{mapName}：{pos.X:#0.0},{pos.Z:#0.0}] {GameConfig.LocalStr["ChatClickJoin"]}</color></link>";
                                    cliStr += $"<link><color=#67E4FF>[{mapName}：{pos.X:#0.0},{pos.Z:#0.0}] {LanguageManager.Instance.GetLanguageByKey("ChatClickJoin")}</color></link>";
                                }
                            }
                        }
                        break;
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.BigEmoji:
                        //cliStr += $"[{GameConfig.LocalStr["ChatEmoji"]}]";
                        cliStr += $"[{LanguageManager.Instance.GetLanguageByKey("ChatEmoji")}]";
                        break;
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.SupLevel:
                        break;
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.TeamRecruit:
                        {
                            var strName = string.Format(LanguageManager.Instance.GetLanguageByKey("Local_Str_Team_DefaultName"), chatExtraInfos.ChatExtraInfo[matchIndex].TRecruit.TeamName);
                            //cliStr = $"{strName}{GameConfig.LocalStr["ChatTeamCreatFinish"]}<link><color=#67E4FF>[{GameConfig.LocalStr["ChatClickJoin"]}]</color></link>";
                            cliStr = $"{strName}{LanguageManager.Instance.GetLanguageByKey("ChatTeamCreatFinish")}<link><color=#67E4FF>[{LanguageManager.Instance.GetLanguageByKey("ChatClickJoin")}]</color></link>";
                        }
                        break;
                }
                curIndex = match.Index + match.Length;
                matchIndex++;
            }
        }

        cliStr += dataStr.Substring(curIndex, dataStr.Length - curIndex);

        return cliStr;
    }

    public static string GetEquipStrByID(long id)
    {
        string image = string.Empty;
        var cfgdata = LocalDataManager.Instance.GetItemDataCell(id);
        if (cfgdata != null)
        {
            var currencyCfg = LocalDataManager.Instance.GetCurrencyCfgDataCell((int)id);
            if (currencyCfg != null)
            {
                string itemIconName = "";
                var iconPath = cfgdata.Icon.Split("/");
                if (iconPath.Length > 0)
                {
                    itemIconName = iconPath[iconPath.Length - 1];
                    int index = BattleManager.Instance.GetCurrencySpriteSpriteIndexFromName(itemIconName);
                    if (index == -1)
                    {
                        index = BattleManager.Instance.GetCurrencySpriteSpriteIndexFromName(itemIconName.ToLower());
                    }
                    image = $"<sprite={index}>";
                }
            }
        }
        return image;
    }
    // 聊天，历史记录
    //public static string HistoryClientToClient(string str, ChatExtraInfos data)
    //{
    //    if (data == null)
    //    {
    //        return str;
    //    }
    //    if (data.ChatExtraInfo.Count <= 0)
    //    {
    //        return str;
    //    }
    //    string cliStr = "";
    //    int curIndex = 0;
    //    int matchIndex = 0;
    //    foreach (Match match in keywordReg.Matches(str))
    //    {
    //        cliStr += str.Substring(curIndex, match.Index - curIndex);
    //        var child = data.ChatExtraInfo[matchIndex];
    //        if (child != null)
    //        {
    //            switch (child.EType)
    //            {
    //                case ProtoMsg.ChatExtraInfo.Types.EMsg.Emoji:
    //                    {
    //                        cliStr += $"<sprite={data.ChatExtraInfo[matchIndex].EmojiID}>";
    //                    }
    //                    break;
    //                case ProtoMsg.ChatExtraInfo.Types.EMsg.Equip:
    //                    {
    //                        int id = data.ChatExtraInfo[matchIndex].EquipID;
    //                        var cfgdata = LocalDataManager.Instance.GetItemDataCell(id);
    //                        cliStr += $"<link><color={GetQualityColorRGB(cfgdata.GetQuality())}>[{cfgdata.Name}]</color></link>";
    //                    }
    //                    break;
    //                case ProtoMsg.ChatExtraInfo.Types.EMsg.Location:
    //                    {
    //                        var locationInfo = data.ChatExtraInfo[matchIndex].LocationInfo;
    //                        if (locationInfo != null)
    //                        {
    //                            string mapName = LocalDataManager.Instance.GetUniMapSceneName(locationInfo.MapId);
    //                            ProtoMsg.Vector3 pos = locationInfo.Pos;
    //                            cliStr += $"<link><color=#67E4FF>[{mapName}：{pos.X:#0.0},{pos.Z:#0.0}]</color></link>";
    //                        }
    //                    }
    //                    break;
    //            }
    //        }

    //        curIndex = match.Index + match.Length;
    //        matchIndex++;
    //    }
    //    cliStr += str.Substring(curIndex, str.Length - curIndex);
    //    return cliStr;
    //}

    public static string HistoryClientToClient(string str, string chatExtraInfosStr)
    {
        if (string.IsNullOrEmpty(chatExtraInfosStr))
        {
            return str;
        }
        var protoInfo = ProtoDic.Instance.GetProtoInfoByCmd((int)MsgIDEnum.ChatExtraInfosID);
        if (!protoInfo.HasValue)
        {
            SGF.Debuger.LogError($"[ExtraData] HistoryClientToClient 获取不到 {MsgIDEnum.ChatExtraInfosID} 对应的 protoInfo, 炸了！！！");
            return str;
        }

        IMessage bbMessage = protoInfo.Value.parse.ParseJson(chatExtraInfosStr);
        ChatExtraInfos data = (ChatExtraInfos)bbMessage;
        if (data == null)
        {
            return str;
        }
        if (data.ChatExtraInfo.Count <= 0)
        {
            return str;
        }
        string cliStr = "";
        int curIndex = 0;
        int matchIndex = 0;
        foreach (Match match in keywordReg.Matches(str))
        {
            cliStr += str.Substring(curIndex, match.Index - curIndex);
            var child = data.ChatExtraInfo[matchIndex];
            if (child != null)
            {
                switch (child.EType)
                {
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.Emoji:
                        {
                            string iconName = "Emoticon_nom_";
                            if (data.ChatExtraInfo[matchIndex].EmojiID < 10)
                            {
                                iconName = "Emoticon_nom_0";
                            }
                            iconName += data.ChatExtraInfo[matchIndex].EmojiID;
                            int index = BattleManager.Instance.GetCurrencySpriteSpriteIndexFromName(iconName);
                            cliStr += $"<sprite={index}>";
                        }
                        break;
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.Equip:
                        {
                            long id = data.ChatExtraInfo[matchIndex].EquipData[0].BaseID;
                            var cfgdata = LocalDataManager.Instance.GetItemDataCell(id);
                            cliStr += $"<link><color={GetQualityColorRGB(cfgdata.GetQuality())}>[{cfgdata.Name}]</color></link>";
                        }
                        break;
                    case ProtoMsg.ChatExtraInfo.Types.EMsg.Location:
                        {
                            var locationInfo = data.ChatExtraInfo[matchIndex].LocationInfo;
                            if (locationInfo != null)
                            {
                                var mapCfgData = LocalDataManager.Instance.GetMapCfgData(locationInfo.MapId);
                                if (mapCfgData != null)
                                {
                                    string mapName = mapCfgData.MapName;
                                    ProtoMsg.Vector3 pos = locationInfo.Pos;
                                    cliStr += $"<link><color=#67E4FF>[{mapName}：{pos.X:#0.0},{pos.Z:#0.0}]</color></link>";
                                }
                            }
                        }
                        break;
                }
            }

            curIndex = match.Index + match.Length;
            matchIndex++;
        }
        cliStr += str.Substring(curIndex, str.Length - curIndex);
        return cliStr;
    }

    public static ByteString ToByteString(IMessage extrainfos)
    {
        return extrainfos.ToByteString();
    }


}