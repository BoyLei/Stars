using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;
namespace SGF.Network
{

    /// <summary>
    /// 通用次数管理器
    /// 中间层次。所有服务器数据的缓存（包括首次全量）
    /// 服务器数表-（服务器中间层）-类-服务器之间同步--数据层次==同步客户端==本类就是我们数据层管理器
    /// /// 《首次反射，数据存储，解pb》：全是业务模块的解析
    /// </summary>
    public class GuildDonateMDMgr : MDMgrInterface
    {

        public GuildDonateMDMgr()
        {
            map = new Dictionary<ulong, GuildDonateMD>();
            mapli = new Dictionary<string, GuildDonateMD>();
        }

        Dictionary<ulong, GuildDonateMD> map;
        Dictionary<string, GuildDonateMD> mapli;
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public GuildDonateMD GetData(ulong key)
        {
            return map[key];
        }

        //大退出重登入

        public void Init()
        {

        }

        public void Notify(FixMessageManager.FixMessageNotifyData datalist)
        {
            //通知


        }


        public IMessage Add(string keyname, DBDataModel data)
        {

            var msgid = (int)MsgIDEnum.GuildDonateMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            GuildDonateMD GuildDonateMD = (GuildDonateMD)msg;
            map[GuildDonateMD.PID] = GuildDonateMD;
            mapli[keyname] = GuildDonateMD;
            return GuildDonateMD;
        }

        public void Update(string keyname)
        {

        }

        public IMessage Del(string keyname)
        {

            GuildDonateMD md = mapli[keyname];
            map.Remove(md.PID);
            mapli.Remove(keyname);
            return md;
        }

        public IMessage GetMsg(string keyname)
        {
            return mapli[keyname];
        }

        public void Release()
        {

        }

        public GuildDonateMD GetGuildDonateMD()
        {
            foreach (var item in map)
            {
                return item.Value;
            }

            return null;
        }
    }

}
