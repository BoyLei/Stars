using MessagePack;
using System.Collections.Generic;
/// 注意,此脚本为 UIQueueToDefine.cs 动态生成

namespace StarProjectDef
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]

    public class UIQueueToDefine
    {
        [Key(0)]
        public Dictionary<string, UIQueueCfgData> UIQueueCfgDatas = new();
    }

    [System.Serializable]
    [MessagePackObject]
    public class UIQueueCfgData
    {
        [Key(0)]
        public string UIPath = string.Empty;
        [Key(1)]
        public UISeatType UISeatType = UISeatType.None;
        [Key(2)]
        public int QueuePriority = 0;
        [Key(3)]
        public bool IsLongTime = false;

        public UIQueueCfgData(string _uipath, UISeatType _uiSeatType, int _queuePriority, bool _isLongTime)
        {
            UIPath = _uipath;
            UISeatType = _uiSeatType;
            QueuePriority = _queuePriority;
            IsLongTime = _isLongTime;
        }

    }
}
