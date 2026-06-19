///--------------------------------------------------------------------
/// 文件名   :   TaskConfigs.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/15 21:14:32
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using System.Collections.Generic;
namespace Task
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class TaskConfigs
    {
        [Key(0)]
        public Dictionary<uint, TaskConfigInfo> list = new Dictionary<uint, TaskConfigInfo>();
        [Key(1)]
        public Dictionary<uint, TaskTypeInfo> typelist = new Dictionary<uint, TaskTypeInfo>();
    }
}
