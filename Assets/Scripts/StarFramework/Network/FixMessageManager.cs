using Google.Protobuf;
using ProtoMsg;
using SGF.Module.Framework;
using StarProject.Service.Business;
using System.Collections.Generic;

namespace SGF.Network
{
    [XLua.LuaCallCSharp]
    ///封装模块，对应数据库表服务器生成
    ///首次pb同步，无序信息，数据块分类存储
    ///服务器管理器名字：表名
    public static class FixUpdateDef
    {
        public const string ComCount = "comcount"; //通用次数
        public const string Items = "items"; //道具
        public const string EqRecasts = "eqrecasts"; //装备熔铸相关
        public const string Gacha = "gacha"; //抽卡相关
        public const string Hero = "hero"; //主角信息
        public const string Partner = "partner"; //伙伴数据
        public const string BankPlayer = "bankplayer"; //交易行玩家数据
        public const string Tweeter = "tweeter"; //鸣器数据
        public const string UserSundry = "usersundry"; //用户设置
        public const string B10register = "b10register"; //pvp
        public const string PersonSecret = "personsecret"; //个人秘境
        public const string PersonTower = "persontower"; //个人爬塔
        public const string GuildDonate = "guilddonate"; //公会捐献/资源回收
        public const string UserLifeSkills = "userLifeSkills"; //生活技能用户数据
        public const string UserSceneLogicData = "userSceneLogicData"; //生活技能用户数据
        public const string CheckIn = "checkin";        //签到
        public const string DailyActivity = "dailyactivity";    //每日活跃
        public const string PersonDaily = "persondaily";    //单人本
        public const string SevenDayGoal = "sevendaygoal";  //七日目标
        public const string FightMD = "fight";  //战力数据表
        public const string LobbyGamePlayMD = "lobbyGamePlay";  //新手目标
    }

    /// <summary>
    /// 协议数据差量更新
    /// </summary>
    /// <param name="DeleteList">删除的数据</param>
    /// <param name="ModifyList">修改的数据、新增的数据</param>
    [XLua.LuaCallCSharp]
    public delegate void FixMessageDelegate(FixMessageManager.FixMessageNotifyData NotifyData);

    [XLua.LuaCallCSharp]
    public class FixMessageManager : ServiceModule<FixMessageManager>
    {
        private Dictionary<string, FixMessageDelegate> key2MessageHandleDic = new();

        public void Init()
        {
            allFixMessageMgr.Clear();
            
            //管理器注册模块
            allFixMessageMgr.Add(FixUpdateDef.ComCount, new ComCountMDMgr()); //通用次数
            allFixMessageMgr.Add(FixUpdateDef.Items, new ItemMDMgr()); //道具
            allFixMessageMgr.Add(FixUpdateDef.EqRecasts, new EqRecastMDMgr()); //装备熔铸
            allFixMessageMgr.Add(FixUpdateDef.Gacha, new GachaMDMgr()); //抽卡
            allFixMessageMgr.Add(FixUpdateDef.Hero, new HeroMDMgr()); //主角數據
            allFixMessageMgr.Add(FixUpdateDef.Partner, new PartnerMDMgr()); //伙伴数据
            allFixMessageMgr.Add(FixUpdateDef.BankPlayer, new BankPlayerMDMgr()); //交易行玩家关注的道具类型和关注商品
            allFixMessageMgr.Add(FixUpdateDef.Tweeter, new TweeterMDMgr()); //鸣器数据
            allFixMessageMgr.Add(FixUpdateDef.UserSundry, new UserSundryMDMgr());
            allFixMessageMgr.Add(FixUpdateDef.B10register, new B10registerMDMgr());
            allFixMessageMgr.Add(FixUpdateDef.PersonSecret, new PersonSecretMDMgr());
            allFixMessageMgr.Add(FixUpdateDef.PersonTower, new PersonTowerMDMgr());
            allFixMessageMgr.Add(FixUpdateDef.GuildDonate, new GuildDonateMDMgr());
            allFixMessageMgr.Add(FixUpdateDef.UserLifeSkills, new LifeSkillUserDataMDMgr());         //生活技能数据
            allFixMessageMgr.Add(FixUpdateDef.UserSceneLogicData, new UserSceneLogicDataMDMgr());    //玩家场景数据

            allFixMessageMgr.Add(FixUpdateDef.CheckIn, new CheckInMDMgr());
            allFixMessageMgr.Add(FixUpdateDef.DailyActivity, new DailyActivityMDMgr());

            allFixMessageMgr.Add(FixUpdateDef.PersonDaily, new PDinfoMDMgr());

            allFixMessageMgr.Add(FixUpdateDef.SevenDayGoal, new SevenDayGoalMDMgr());
            allFixMessageMgr.Add(FixUpdateDef.FightMD, new FightMDMgr());
            allFixMessageMgr.Add(FixUpdateDef.LobbyGamePlayMD, new LobbyGamePlayMDMgr());
        }

        public void Close()
        {
        }

        public override void Release()
        {
            base.Release();
            this.Log("Release() NetWork Manager");
        }

        public void OnMessage(string key, FixMessageDelegate callback)
        {
            if (key2MessageHandleDic.ContainsKey(key))
            {
                key2MessageHandleDic[key] += callback;
            }
            else
            {
                key2MessageHandleDic.Add(key, callback);
            }
        }

        public void OffMessage(string key, FixMessageDelegate callback)
        {
            if (key2MessageHandleDic.ContainsKey(key))
            {
                key2MessageHandleDic[key] -= callback;
                if (key2MessageHandleDic[key] == null)
                {
                    key2MessageHandleDic.Remove(key);
                }
            }
        }

        //**************************协议数据差异更新*********************

        /// <summary>
        /// 记录所有的差量消息模块的管理器
        /// </summary>
        private Dictionary<string, MDMgrInterface> allFixMessageMgr = new();


        public void ClearAllDatas()
        {
            foreach (var item in allFixMessageMgr)
            {
                item.Value.ClearAllData();
            }
            BusinessManager.Instance.IsInitAllItem = false;
        }

        /// <summary>
        /// 差量更新的回调数据包
        /// </summary>
        public class FixMessageNotifyData
        {
            public FixMessageNotifyData()
            {
                AllDatas = new List<IMessage>();
                Delets = new List<IMessage>();
                Changes = new List<IMessage>();
            }

            public string TableName;
            public List<IMessage> AllDatas;
            public List<IMessage> Delets;
            public List<IMessage> Changes;
        }

        /// <summary>
        /// 处理差量更新消息
        ///
        /// </summary>
        /// <param name="datasRet"></param>
        public void HandleFixMessageCS(DBUpUserDatasReq datasRet)
        {
            //UnityEngine.Debug.LogError("11111111111HandleFixMessageCS");
            if (datasRet == null)
            {
                return;
            }

            //差量更新的回调字典
            Dictionary<string, FixMessageNotifyData> fixMessageNotifyDic = new();

            var msgid = (int)MsgIDEnum.MapModelID;

            bool IsUpdateItem = false;
            // 增改数据
            for (int i = 0; i < datasRet.DBList.Count; i++)
            {
                DBDataModel dbDataModel = datasRet.DBList[i];
                string kn = dbDataModel.KeyName.Split(':')[0];

                if (!allFixMessageMgr.TryGetValue(kn, out var mgr))
                {
                    //Debug.LogError("不包含："+ kn+"差量模块。");
                    continue;
                }

                if (kn.Equals(FixUpdateDef.Items))
                {
                    IsUpdateItem = true;
                }

                IMessage md;
                if (dbDataModel.IsPartial)
                {
                    //差量
                    ProtoMsg.MapModel msgData = (ProtoMsg.MapModel)SGF.Network.ProtoUtils.Deserialize(msgid, dbDataModel.MsgContent.ToByteArray());
                    md = mgr.GetMsg(dbDataModel.KeyName);
                    Frame.Util.UpPackData(md, msgData);
                    mgr.Update(dbDataModel.KeyName);
                }
                else
                {
                    //全量
                    md = mgr.Add(dbDataModel.KeyName, dbDataModel);
                }

                if (!fixMessageNotifyDic.TryGetValue(kn, out var val))
                {
                    val = new FixMessageNotifyData();
                    val.TableName = kn;
                    fixMessageNotifyDic.Add(kn, val);
                }

                val.Changes.Add(md);
                if (!dbDataModel.IsPartial)
                {
                    // 全量
                    val.AllDatas.Add(md);
                }
                //if (dbDataModel.IsPartial)
                //{
                //    // 差量
                //    val.Changes.Add(md);
                //}
                //else
                //{
                //    // 全量
                //    val.AllDatas.Add(md);
                //}
            }

            //删除数据，同时也要删除上层数据
            for (int i = 0; i < datasRet.DelList.Count; i++)
            {
                DBDataModel dbDataModel = datasRet.DelList[i];
                string kn = dbDataModel.KeyName.Split(':')[0];

                if (kn.Equals(FixUpdateDef.Items))
                {
                    IsUpdateItem = true;
                }


                var mgr = allFixMessageMgr[kn];
                IMessage md = mgr.Del(dbDataModel.KeyName);

                if (!fixMessageNotifyDic.TryGetValue(kn, out var val))
                {
                    val = new FixMessageNotifyData();
                    val.TableName = kn;
                    fixMessageNotifyDic.Add(kn, val);
                }

                if (md != null)
                {
                    val.Delets.Add(md);
                }
            }

            //通知
            foreach (var item in fixMessageNotifyDic)
            {
                //var mgr = allFixMessageMgr[item.Key];
                //mgr.Notify(item.Value);

                if (key2MessageHandleDic.ContainsKey(item.Key))
                {
                    key2MessageHandleDic[item.Key](item.Value);
                }
            }

            //背包刷新回调
            if(IsUpdateItem)
            {
                StarProject.GlobalEvent.OnItemChangeToRefeshBag.Invoke(null);
            }
        }

        public MDMgrInterface GetMDMgr(string keyname)
        {
            return allFixMessageMgr[keyname];
        }


        //*************************************************************

        // public Action 
    }
}