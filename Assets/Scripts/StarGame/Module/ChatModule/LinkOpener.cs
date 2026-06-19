using Google.Protobuf;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using StarProject;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProject.Service.SystemOpen;
using StarProjectDef;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
[XLua.LuaCallCSharp]
public class LinkOpener : MonoBehaviour, IPointerClickHandler
{
    private Dictionary<int, ChatLocationInfo> posLinks = new();
    private Dictionary<int, RepeatedField<ItemMD>> itemLinks = new();

    private Dictionary<int, RepeatedField<ItemMD>> itemMdLinks = new();
    private Dictionary<int, ulong> teamLinks = new();
    private Dictionary<int, ulong> guildLinks = new();
    private int index = 0;

    private int source = 0; // 设置超链接的来源  1为hud聊天item

    public void SetSource(int _source)
    {
        source = _source;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TMP_Text pTextMeshPro = GetComponent<TMP_Text>();
        //没有就不传递，默认要么给ui相机，要么就给overlayCanvas的（相机）模式就不给网上查的，原理一样
        /*   CameraBase cameraBase = CameraManager.Instance.GetCamera(*//*E_CameraType.StarWorldCam*//*E_CameraType.UICam);//UICam*/
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, eventData.position, /*cameraBase.Camera*/null);
        if (linkIndex != -1)
        {
            if (posLinks.ContainsKey(linkIndex))
            {
                var data = posLinks[linkIndex];
                if (data != null)
                {
                    var curMapType = GameManager.Instance.GetCurMapType();

                    // 是否可超链接传送
                    var mapCfgData = LocalDataManager.Instance.GetMapCfgData(data.MapId);
                    if (mapCfgData != null && !mapCfgData.IsCameraTeleport)
                    {
                        if (curMapType != data.SpType || GameManager.Instance.GetSpaceID() != data.SpaceID)
                        {
                            Frame.Util.ShowMessageByCode(2107);
                            return;
                        }
                    }
                    // 策划需求：
                    // 1.自己当前的地图不是大场景 如果点击的地图是大场景【那就过滤然后提示】
                    if (curMapType != SpaceType.SpaceScene)
                    {
                        if (data.SpType != curMapType)
                        {
                            Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_Chat_04);
                            return;
                        }
                    }
                    // 1.如果点击的地图不是大场景
                    // 2.跟自己不同地图类型 || 跟自己地图ID不一样 || 跟自己SpaceID不一样【那就过滤然后提示】 || SpaceID不一样
                    if (data.SpType != SpaceType.SpaceScene)
                    {
                        if (data.SpType != curMapType || data.MapId != GameManager.Instance.GetCurMapId() || GameManager.Instance.GetSpaceID() == data.SpaceID)
                        {
                            Frame.Util.ShowMessageByCode(CRetMsgEnum.Tips_Chat_03);
                            return;
                        }
                    }

                    UnityEngine.Vector3 pos = UnityEngine.Vector3.zero;
                    pos.x = data.Pos.X;
                    pos.y = data.Pos.Y;
                    pos.z = data.Pos.Z;
                    TaskHelper.FindPostion(data.MapId, pos, 0.1f, null);
                    //BusinessManager.Instance.FindPath(posLinks[linkIndex]);
                    //Debug.Log($"自动寻路到：{posLinks[linkIndex]}");
                }
                if (GameManager.Instance.IsOpenItemTips)
                {
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.ItemTipsModule, "OnCloseTips", new object[] { });
                }
                if (GameManager.Instance.IsOpenFuncTips)
                {
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.FuncTipsModule, "OnCloseTips", new object[] { });
                }
            }
            if (itemLinks.ContainsKey(linkIndex))
            {
                //Debug.Log($"查看道具：{itemLinks[linkIndex][0]}");
                //ItemMD ebd = itemLinks[linkIndex][0];
                
                //Dictionary<ulong, ItemMD> gemTable = new();
                //for (int i=0;i<ebd.GemSlots.List.Count;i++)
                //{
                //    gemTable.Add(ebd.GemSlots.List[i].GemID,BusinessManager.Instance.GetItemByItemEntityId(ebd.GemSlots.List[i].GemID));
                //}

                List<ItemMD> ebds = new();
                for (int i = 0; i < itemLinks[linkIndex].Count; i++)
                {
                    ebds.Add(itemLinks[linkIndex][i]);
                }
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ItemTipsModule, "OnOpenTipsWithItemMDList", new object[] { ebds, transform });
                if (source == 1)
                {
                    GlobalEvent.OnChatHudShowTipsEvent?.Invoke(true);
                }
            }

            if (itemMdLinks.ContainsKey(linkIndex))
            {
                //Debug.Log($"查看装备：{itemMdLinks[linkIndex].BaseID}");
                List<ItemMD> ebds = new();
                for (int i = 0; i < itemMdLinks[linkIndex].Count; i++)
                {
                    ebds.Add(itemMdLinks[linkIndex][i]);
                }
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ItemTipsModule, "OnOpenTipsWithItemMDList", new object[] { ebds, transform });
                if (source == 1)
                {
                    GlobalEvent.OnChatHudShowTipsEvent?.Invoke(true);
                }
            }
            if (teamLinks.ContainsKey(linkIndex))
            {
                Debug.Log($"队伍id：{teamLinks[linkIndex]}");
                // 
                if (!SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.TeamFind))
                {
                    Frame.Util.ShowMessage(SystemOpenManager.Instance.GetSystemNoOpenTips(SystemOpenType.TeamFind));
                    return;
                }
                ulong teamId = teamLinks[linkIndex];
                var joinTeamReq = new ProtoMsg.JoinTeamReq();
                joinTeamReq.TeamID = teamId;
                SGF.Network.NetworkManager.Instance.gameSocket.SendRPCMsg(SGF.Network.ServerType.ServerTypeSpace, joinTeamReq, false);
            }
            if (guildLinks.ContainsKey(linkIndex))
            {
                Debug.Log($"公会id：{guildLinks[linkIndex]}");
                if (!SystemOpenManager.Instance.SystemIsOpen(SystemOpenType.Guild))
                {
                    Frame.Util.ShowMessage(SystemOpenManager.Instance.GetSystemNoOpenTips(SystemOpenType.Guild));
                    return;
                }
                var myGuildID = BusinessManager.Instance.GetGuildID();
                ulong guildId = guildLinks[linkIndex];
                if (myGuildID != guildId)
                {
                    var guMgrApplyReq = new ProtoMsg.GuMgrApplyReq();
                    guMgrApplyReq.GuildID.Add(guildId);
                    var gameSocket = NetworkManager.Instance.gameSocket;
                    gameSocket.SendClient2CenterMsg(guMgrApplyReq);
                }
            }
        }
        else
        {
            if (source == 1)
            {
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "OnOpenChatWindowShow", new object[] { true });
            }
        }
    }

    public bool GetIsHaveItemLinks()
    {
        return itemLinks.Count > 0 || itemMdLinks.Count > 0;
    }

    private void AddPosLink(ChatLocationInfo chatLocationInfo)
    {
        posLinks[index++] = chatLocationInfo;
    }

    private void AddItemLink(RepeatedField<global::ProtoMsg.ItemMD> itemDatas)
    {
        itemLinks.Add(index++, itemDatas);
    }
    
    public void ClearItemMDLink()
    {
        itemMdLinks.Clear();
    }

    private void AddItemMDLink(RepeatedField<global::ProtoMsg.ItemMD> itemDatas)
    {
        itemLinks.Add(index++, itemDatas);
    }

    public void AddItemMDLink(global::ProtoMsg.ItemMD itemData)
    {
        RepeatedField<global::ProtoMsg.ItemMD> itemDatas = new RepeatedField<ItemMD>();
        itemDatas.Add(itemData);
        itemLinks.Add(index++, itemDatas);
    }

    public void ClearGuildInfoLink()
    {
        guildLinks.Clear();
    }

    public void AddGuildInfoLink(ulong guildId)
    {
        guildLinks[index++] = guildId;
    }

    private void AddTeamLink(ulong teamId)
    {
        teamLinks[index++] = teamId;
    }

    public void SetLinks(RepeatedField<Google.Protobuf.ByteString> bytes)
    {
        if (bytes == null || bytes.Count == 0)
        {
            return;
        }

        ProtoInfo? protoInfo = ProtoDic.Instance.GetProtoInfoByCmd((int)MsgIDEnum.ChatExtraInfosID);
        if (!protoInfo.HasValue)
        {
            return;
        }

        MessageParser parser = protoInfo.Value.parse;
        IMessage message = parser.ParseFrom(bytes[0]);
        var data = message as ChatExtraInfos;
        int index = 0;
        foreach (var item in data.ChatExtraInfo)
        {
            switch (item.EType)
            {
                case ProtoMsg.ChatExtraInfo.Types.EMsg.Equip:
                    AddItemLink(item.EquipData);
                    break;
                case ProtoMsg.ChatExtraInfo.Types.EMsg.Location:
                    var locationInfo = item.LocationInfo;
                    if (locationInfo != null)
                    {
                        AddPosLink(locationInfo);
                    }
                    break;
                case ProtoMsg.ChatExtraInfo.Types.EMsg.TeamRecruit:
                    {
                        var trecruit = item.TRecruit;
                        if (trecruit != null)
                        {
                            AddTeamLink(trecruit.TeamID);
                        }
                    }
                    break;
                case ProtoMsg.ChatExtraInfo.Types.EMsg.GuildCreate:
                    {
                        AddGuildInfoLink(item.GuildID);
                    }
                    break;
            }
            index++;
        }
    }
}