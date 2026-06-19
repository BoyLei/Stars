namespace CokingNodeEditor
{
    [System.Serializable]
    public class ConnectJson
    {
        //是第几个点输出的
        public ConnectionType Type = 0;
        //输出节点ID
        public int OutNode = 0;
        //输入节点ID
        public int InNode = 0;
    }
}

