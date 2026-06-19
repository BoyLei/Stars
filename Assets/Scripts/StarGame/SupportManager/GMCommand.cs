///--------------------------------------------------------------------
/// 文件名   :   GMCommad
/// 内  容   :  GM 命令 
/// 说  明   :  
/// 创建日期 :   2022/07/18 20:25:05
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev;
using SickDev.DevConsole;
using System;
using SGF.Network;
using ProtoMsg;
using SGF.Module.Framework;
using SickDev.CommandSystem;
using SGF.UI.Framework;
using StarProject;
using StarProject.Module;
using StarProjectDef;

public class GMCommand : SGF.Unity.MonoSingletonEx<GMCommand>
{
    private string msg;
    // Start is called before the first frame update

    void Awake()
    {

        //DontDestroyOnLoad(this);
    }

    void Start()
    {
        //发送全服消息 ntfAllUser msgCode 消息号ID
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int>(ntfAllUser));

        //设置角色等级 setlevel lv 等级
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int>(setlevel));

        //设置属性 setprop index value 属性index,值
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int, int>(setprop));

        //设置无敌    setgod bool 1是无敌，0是取消无敌
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<bool>(setgod));

        //地图刷怪    msm1 x y z name num x,y,z 是地图绝对坐标，只允许整数
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<string, string, int>(msm1));

        //以角色为原点刷怪    rsm1 x y z name num x,y,z 是角色坐标偏移量
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<string, string, int>(rsm1));

        //地图刷怪 msm2 x y z id num x, y, z 是地图绝对坐标，只允许整数
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<string, int, int>(msm2));

        //以角色为原点刷怪    rsm2 x y z id num   x,y,z 是角色坐标偏移量
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<string, int, int>(rsm2));

        //全屏清怪 killallmon  场景内怪物全清除，不走击杀和掉落
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand(killallmon));

        //地图跳转    flymap x y z mapname 如果只是本地图跳转，不需要填写mapname
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<string, string>(flymap));

        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<string>(flymap));
        //进入某个副本
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int>(enterins));
        //本服切场景
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int>(enterscene));
        //设置有无CD  nocd bool   1是无CD，0是有CD
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int>(nocd));

        //设置全图怪物的生命值为最大的百分比   setmonhpper value
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int>(setmonhpper));

        //设置全服AI开启、关闭 switchAI bool   1是开启AI，0是关闭AI
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int>(switchAI));

        //DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand(openInterActionPanel));

        //使用战斗技能 useskill int, int 是 战斗技能id
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int>(useskill));

        //添加道具
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int, int>(addItem));

        //自杀
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand(suicide));

        //生成木桩怪
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand(PlayAlong));

        //生成大片陪练
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int, int>(PlayAlongs));

        //关闭全图AI
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand(CloseAI));

        //增加伙伴
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int>(AddPartner));
        //设置伙伴状态
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<int, int, int>(SetPartner));

        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<string>(gm));

        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<uint>(ClientTaskState));

        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand(closefog));
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand(closefogtest));

        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand(ShowGM));

        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<bool>(SkipGuide));

        DevConsole.singleton.AddCommand(new ActionCommand<int>(DoGuide));
        DevConsole.singleton.AddCommand(new ActionCommand<int>(EndGuide));
        DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand<bool>(UseOldPartner));

        NetworkManager.Instance.OnMessageEnum(MsgIDEnum.GmCmdAckID, OnGmCommand, this);//增减数据



    }

    public void EndGuide(int GuideID)
    {
        var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TutorialModule);
        if (module != null)
        {
            var tutorral = module as TutorialModule;
            if (tutorral != null)
            {
                // tutorral.EndGuide(GuideID);
            }
        }
    }

    public void DoGuide(int GuideID)
    {
        var module = ModuleManager.Instance.GetModule(ModuleDef.Name.TutorialModule);
        if (module != null)
        {
            var tutorral = module as TutorialModule;
            if (tutorral != null)
            {
                tutorral.TestTutorial(GuideID);
            }
        }
    }

    private void SkipGuide(bool show)
    {
        SaveManager.Instance.Save<bool>(TutorialModule.GUIDESKIP, show, ModuleDef.Name.TutorialModule.ToString());
    }

    private void UseOldPartner(bool show)
    {
        TaskHelper.UseOldPartner = show;
    }

    private void ShowGM()
    {
        if (AppConfig.IsGM())
        {
            //UIRoot.GmRoot.SetActive(true);
            //UIManager.Instance.OpenWidget(StarProjectDef.UIDef.GmWidget, true, null, UIRoot.UIROOT.transform, StarProjectDef.MainPageCommond.HideNone, true, true);
            UIManager.Instance.OpenWidgetAsync(UIDef.GmWidget, null, true, null, UIRoot.UIROOT.transform, MainPageCommond.HideNone, true, true);
        }
    }

    private void ClientTaskState(uint taskID)
    {
        if (TaskHelper.IsTaskFinsh(taskID))
        {
            StarDebug.LogTag(StarDebug.LogTagEnum.UI, $"{taskID}任务已完成");
            return;
        }

        if (TaskHelper.ContainTask(taskID))
        {
            StarDebug.LogTag(StarDebug.LogTagEnum.UI, $"{taskID}任务进行中");
            return;
        }

        StarDebug.LogTag(StarDebug.LogTagEnum.UI, $"{taskID}任务不存在");
    }

    private void gm(string gm)
    {
        SendGM(gm);
    }

    private void openInterActionPanel()
    {
        //var p = UIManager.Instance.OpenWidget("InterAction/InterActionObject", false, null, null, StarProjectDef.MainPageCommond.HideNone, true);
    }

    private void OnGmCommand(MessageHandleData data)
    {
        GmCmdAck gm = (GmCmdAck)data.data;
        SGF.Debuger.Log(gm.Result + "[" + gm.Msg + "]");
    }

    private void SendGM(string command)
    {
        SocketBase battleSocket = NetworkManager.Instance.gameSocket;
        GmCmdReq gmCmdReq = new GmCmdReq();
        gmCmdReq.Cmd = command;
        battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);
    }

    private void ntfAllUser(int msgID)
    {
        msg = $"ntfAllUser {msgID}";
        SendGM(msg);
    }
    private void closefogtest() { }
    private void closefog()
    {
        StarProject.Game.GameManager.Instance.SetFog(false);
    }
    private void setlevel(int level)
    {
        msg = $"setlevel {level}";
        SendGM(msg);
    }

    private void addItem(int id, int count)
    {
        msg = $"addItem {id} {count}";
        SendGM(msg);
    }

    private void setprop(int index, int value)
    {
        msg = $"setprop {index} {value}";
        SendGM(msg);
    }
    private void setgod(bool god)
    {
        msg = $"setgod {god}";
        SendGM(msg);
    }


    private void msm1(string position, string name, int num)
    {
        Vector3Int pos1 = StrToVectorInt(position);
        msg = $"msm1 {pos1.x} {pos1.y} {pos1.z} {name} {num}";
        SendGM(msg);
    }
    private void rsm1(string position, string name, int num)
    {
        Vector3Int pos1 = StrToVectorInt(position);
        msg = $"rsm1 {pos1.x} {pos1.y} {pos1.z} {name} {num}";
        SendGM(msg);
    }
    private void msm2(string position, int id, int num)
    {
        Vector3Int pos1 = StrToVectorInt(position);
        msg = $"msm2 {pos1.x} {pos1.y} {pos1.z} {id} {num}";
        SendGM(msg);

    }
    private void rsm2(string position, int id, int num)
    {
        Vector3Int pos1 = StrToVectorInt(position);
        msg = $"rsm2 {pos1.x} {pos1.y} {pos1.z} {id} {num}";
        SendGM(msg);
    }
    private void killallmon()
    {
        msg = "killallmon";
        SendGM(msg);
    }

    private void flymap(string position, string mapname)
    {
        Vector3Int pos1 = StrToVectorInt(position);
        msg = $"flymap {pos1.x} {pos1.y} {pos1.z} {mapname}";
        SendGM(msg);
    }
    private void flymap(string position)
    {
        Vector3Int pos1 = StrToVectorInt(position);
        msg = $"flymap {pos1.x} {pos1.y} {pos1.z}";
        SendGM(msg);
    }
    private void nocd(int nocd)
    {
        msg = $"nocd {nocd}";
        SendGM(msg);
    }

    private void setmonhpper(int hp)
    {
        msg = $"setmonhpper {hp}";
        SendGM(msg);
    }

    private void enterins(int mapid)
    {
        msg = $"enterins {mapid}";
        SendGM(msg);
    }
    private void enterscene(int mapid)
    {
        msg = $"enterscene {mapid}";
        SendGM(msg);
    }
    private void switchAI(int open)
    {
        msg = $"switchAI {open}";
        SendGM(msg);
    }

    private void useskill(int skillid)
    {
        msg = $"useskill {skillid}";
        SendGM(msg);
    }

    private void suicide()
    {
        SendGM("sui");
    }

    private void PlayAlong()
    {
        SendGM("PlayAlong");
    }

    private void PlayAlongs(int i, int j)
    {
        SendGM($"PlayAlongs {i} {j}");
    }

    private void CloseAI()
    {
        SendGM("AI");
    }

    private void AddPartner(int id)
    {
        SendGM($"ap {id}");
    }

    private void SetPartner(int i, int j, int k)
    {
        SendGM($"setpartner {i} {j} {k}");
    }

    private Vector3Int StrToVectorInt(string str)
    {
        Vector3Int vector = Vector3Int.zero;
        if (!string.IsNullOrEmpty(str))
        {
            string[] positions = str.Split(',');
            if (positions != null && positions.Length > 0)
            {
                int x = 0;
                int y = 0;
                int z = 0;
                if (positions.Length == 1)
                {
                    System.Int32.TryParse(positions[0], out x);
                }
                if (positions.Length == 2)
                {
                    System.Int32.TryParse(positions[0], out x);
                    System.Int32.TryParse(positions[1], out y);

                }
                if (positions.Length == 3)
                {
                    System.Int32.TryParse(positions[0], out x);
                    System.Int32.TryParse(positions[1], out y);
                    System.Int32.TryParse(positions[2], out z);
                }
                vector.x = x;
                vector.y = y;
                vector.z = z;
            }
        }
        return vector;
    }
}
