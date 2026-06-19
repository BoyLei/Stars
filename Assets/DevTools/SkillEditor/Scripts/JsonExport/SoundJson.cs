using MessagePack;

///--------------------------------------------------------------------
/// 文件名   :   SoundJson.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 14:14:57
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace SkillEditor
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public class SoundJson : CommonClipJson
    {
        public string EventName;
        public uint SoundEventID;
    }
}