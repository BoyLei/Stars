using System.Collections.Generic;

namespace CokingNodeEditor
{
    /// <summary>
    /// 一个节点树Json包含三个配置
    /// ①配置数据
    /// ②节点数据
    /// ③连接数据
    /// </summary>
    [System.Serializable]
    public class NodeTreeJson
    {
        //①配置数据
        public NodeTreeConfigJson Config;

        //②节点数据
        public Dictionary<int,NodeJson> NodeDic = new Dictionary<int,NodeJson>();
        
        //③连接数据
        public List<ConnectJson> ConnectJsons= new List<ConnectJson>();
    }
}
