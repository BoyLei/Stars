using System.Collections.Generic;

namespace CokingNodeEditor
{
    [System.Serializable]
    public class NodeTreeConfigJson
    {
        //节点树的名字
        public string NodeTreeName = "";
        //节点树的备注
        public string NodeTreeDesc = "";
        //节点树的输入Key
        public List<InputKey> InputKeys = new();
        //节点树的输出Key
        public List<OutputKey> OutputKeys = new();
        //节点树大小
        public float totalScale = 1.0f;
    }
}

