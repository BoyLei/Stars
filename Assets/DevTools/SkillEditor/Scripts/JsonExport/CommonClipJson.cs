using MessagePack;

///--------------------------------------------------------------------
/// 文件名   :   CommonClipJson.cs
/// 内  容   :   Timeline Clip通用的数据导出格式
/// 说  明   :  
/// 创建日期 :   2022/09/01 14:16:16
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace SkillEditor
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public class CommonClipJson
    {
        public int Start;
        public int End;
        public int Duration;
        public int ClipIn;
        public int clipCaps;
        public int Index;
        public double SpeedMultiplier=1;
    }
}
