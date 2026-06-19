using System;
using System.Collections.Generic;
using XLua;

namespace StarProjectDef
{
    /// <summary>
    /// 服务器会有两个服务器同步
    /// 1，人物AOI
    /// 2，场景
    /// </summary>
    public static class GameLoginInfo
    {
        //客户端发送结构
        //0
        public static UserLoginReq M_UserLoginReq = new();

        public static GodChooseHeroReq M_GodChooseHeroReq = new();
        //1

        //2
        public static PickGroupReq M_PickGroupReq = new();
        //3
        public static UserCreateNPlayerReq M_UserCreateNPlayerReq;
        //4
        public static GetSrvSingleReq M_GetSrvSingleReq;
        //5
        public static ChooseHeroReq M_ChooseHeroReq;
        //6
        public static DeleteHeroReq M_DeleteHeroReq;

        public static string GameServerIpAddress;
        //返回参数
        //0
        public static ProtoMsg.UserLoginRet M_UserLoginNAck;
        //1
        public static GetGroupListAck M_GetGroupListAck;
        //2
        public static PickSrvAck M_PickSrvAck;
        //3
        public static UserCreateNPlayerAck M_UserCreateNPlayerAck;
        //4
        public static GetOwnerGroupListAck M_GetOwnerGroupListAck;
        //5
        public static ChossHeroAck M_ChossHeroAck;
        //6
        public static DeleteHeroAck M_DeleteHeroAck;

        private static UInt64 createTime = 0;
        public static UInt64 GetCreateTime()
        {
            if (GameLoginInfo.M_UserCreateNPlayerAck != null && GameLoginInfo.M_UserCreateNPlayerAck.PlayerData != null && GameLoginInfo.M_UserCreateNPlayerAck.PlayerData.Count > 0 && GameLoginInfo.M_UserCreateNPlayerAck.PlayerData[0].CreateTime > 0)
            {
                createTime= GameLoginInfo.M_UserCreateNPlayerAck.PlayerData[0].CreateTime;
                return createTime;
            }

            if (GameLoginInfo.M_PickSrvAck != null && GameLoginInfo.M_PickSrvAck.PlayerData.Count > 0 && M_ChooseHeroReq != null && M_ChooseHeroReq.PID > 0)
            {
                List<PlayerLoginData> playerDatas = GameLoginInfo.M_PickSrvAck.PlayerData;
                for (int i = 0; i < playerDatas.Count; i++)
                {
                    if (playerDatas[i].PID == M_ChooseHeroReq.PID && playerDatas[i].CreateTime > 0)
                    {
                        createTime= playerDatas[i].CreateTime;
                        return createTime;
                    }
                }
            }

            return createTime;
        }
    }

    #region 【0】新的登录协议，获取【时间戳、token等重要参数】

    public class UserLoginReq
    {
        public string Openid;       // (玩家的openid)：【玩家id随便换一个就是新号】
        public string Channel;      // (玩家的渠道)
    }

    public class UserLoginNAck
    {
        public long UID;
        public string TS;
        public int Result;
        public string ResultMsg;
        public string AccessToken;
        public bool IsAdult;
    }

    #endregion

    #region 模拟登录

    public class GodChooseHeroReq
    {
        public ulong uid;
        public ulong pid;
        public UInt32 groupid;
        public int userage;
        public bool isadult;
    }

    #endregion

    #region 【1】区服列表
    /// <summary>
    /// 无需请求，直接返回区服列表
    /// </summary>
    public class GetGroupListAck
    {
        public int Result;                                //> 0 和 1为成功，其他失败                  
        public List<AreaInfo> areaList = new();       //所有大区信息 
    }
    public class AreaInfo
    {
        public UInt64 AreaID;       //大区ID
        public string AreaName;     //大区名
        public List<GroupInfo> GroupList = new();
    }
    public class GroupInfo
    {
        public string groupName;    //小服名
        public UInt32 groupID;      //小服id
        public int groupLoad;       //小服负载      火爆/拥挤/流畅/维护中（传的是负数）
        public UInt64 areaID;       //所属大区id
        public UInt64 Pid;          //这个是服务器在数据库里面的主键(暂时无效）
        public bool isRecommend;    // 是否推荐服
        public bool isNew;          // 是否新服
        public Int64 CreateTime;    // 小区创建的时间戳
    }

    #endregion

    #region 【2】选择服务器

    public class PickGroupReq
    {
        public string Openid;       // (玩家的openid)：【玩家id随便换一个就是新号】
        public string Channel;      // (玩家的渠道)
        public UInt32 GroupID;      // (选择的区服id)

    }

    public class PickSrvAck
    {
        public UInt64 UID;                                                      //> 玩家账号id   
        public string LobbyAddr;                                                //> lobby服地址
        public int Result;                                                      //>  Result 0 和 1 为成功 2 没有玩家 3 服务器忙 3 参数错误 4 重名
        public string ResultMsg;                                                 //> result解释
        public bool HB;
        public List<PlayerLoginData> PlayerData = new();  //> 玩家数据
    }
    public class PlayerLoginData
    {
        public UInt64 PID;                               //> 玩家角色id       
        public string NickName;                          //> 名字
        public int ModelID;                              //> 模型id
        public int JobID;                                // 职业id
        public int Level;
        public string Equip1MainColor;                   //用;分开 r;g;b
        public string Equip1SubColor;                    //用;分开 r;g;b
        public string Token;                             //> 验证的token
        public UInt64 LoginTime;           // 登录时间戳
        public UInt64 CreateTime;                       // 创角角色时间
    }


    #endregion

    #region 【3】创号(根据不同情况返回参数可能不同)
    public class UserCreateNPlayerReq
    {
        public UInt64 UID;
        public int GroupID;
        public string NickName;
        public int ModelID;
        public int JobID;                                // 职业id
        public string Equip1MainColor;                   //用;分开 r;g;b
        public string Equip1SubColor;                    //用;分开 r;g;b
        public string VerifyTok;
    }


    public class UserCreateNPlayerAck
    {
        public UInt64 UID;
        public string Token;
        public int Result;                                //> 0 和 1为成功，其他失败                  
        public string ResultMsg;                         //> 会具体说明返回结果的原因
        public List<PlayerLoginData> PlayerData = new();            //> 玩家数据
        public string LobbyAddr;


    }


    #endregion

    #region 【4】得到玩家有号的那些区服
    public class GetSrvSingleReq
    {
        public string Openid;
        public string Channel;
    }

    public class GetOwnerGroupListAck
    {
        public int Result;                                //> 0 和 1为成功，其他失败                  
        public List<OwnerGroupInfo> ownergroupList = new();
    }

    /// <summary>
    /// 这里的小写，是服务器json的Key小写了
    /// </summary>
    public class OwnerGroupInfo
    {
        public string groupName;                //  服务器名字
        public int groupID;                     //  服务器id
        public int groupLoad;                   //  服务器负载
        public UInt64 areaID;                   //  大区id
        public UInt64 heroID;                   //  当前角色id ，为了显示不同模型
        public int level;                       //  等级
        public string nickName;                 //  名字
        public UInt64 PID;                      //这里是角色ID//账号ID，【！角色ID！】，实体ID；
        public UInt64 LoginTime;           // 登录时间戳
    }


    #endregion

    #region【5】选择某个角色进入游戏

    public class ChooseHeroReq
    {
        public UInt64 PID; //这里是角色ID//账号ID，【！角色ID！】，实体ID；
        public UInt32 GroupID;
        public UInt64 UID;
    }

    public class ChossHeroAck
    {
        public string LobbyAddr;
        public int Result;          // 0 和 1 均表示成功
        public string ResultMsg;
        public string Token;
    }

    #endregion

    #region【6】删除某个角色

    //删除角色请求
    public class DeleteHeroReq
    {
        public UInt64 PID; //这里是角色ID//账号ID，【！角色ID！】，实体ID；
        public UInt32 GroupID;
        public UInt64 UID;
    }
    //删除角色回复
    public class DeleteHeroAck
    {
        public int Result;          // 0 和 1 均表示 成功
        public string ResultMsg;
    }

    #endregion

    #region 登录时 请求的 公告http 结构
    /// <summary>
    /// 单个公告的数据结构
    /// </summary>
    [Hotfix]
    public class AnnouncementData
    {
        public int id;
        public string tab_title;
        public string page_title;
        public string type;
        public string content;
        public int weight;
        public long time_release;
    }

    /// <summary>
    /// 公告列表数据
    /// </summary>
    [Hotfix]
    public class Announcements
    {
        public int code;
        public List<AnnouncementData> data;
    }

    #endregion
}
