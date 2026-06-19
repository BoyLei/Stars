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
    public class ComCountMDMgr : MDMgrInterface
    {

        public ComCountMDMgr()
        {
            map = new Dictionary<int, ComCountMD>();
            mapli = new Dictionary<string, ComCountMD>();
        }

        Dictionary<int, ComCountMD> map;
        Dictionary<string, ComCountMD> mapli;
        public void ClearAllData()
        {
            map.Clear();
            mapli.Clear();
        }
        public ComCountMD GetData(int key)
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

            var msgid = (int)MsgIDEnum.ComCountMDID;
            IMessage msg = SGF.Network.ProtoUtils.Deserialize(msgid, data.MsgContent.ToByteArray());
            ComCountMD comCountMD = (ComCountMD)msg;
            map[comCountMD.BaseId] = comCountMD;
            mapli[keyname] = comCountMD;
            return comCountMD;
        }

        public void Update(string keyname)
        {

        }

        public IMessage Del(string keyname)
        {

            ComCountMD md = mapli[keyname];
            map.Remove(md.BaseId);
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


        /// <summary>
        /// 获取通用剩余次数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetRemainCount(int key)
        {
            if (map.ContainsKey(key))
            {
                return GetData(key).RemainCount;
            }
            else
            {
                return 0;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetAddCount(int key)
        {
            if (map.ContainsKey(key))
            {
                return GetData(key).AddCount;
            }
            else
            {
                return 0;
            }

        }

    }

}
