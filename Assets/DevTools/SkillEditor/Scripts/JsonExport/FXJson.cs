///--------------------------------------------------------------------
/// 文件名   :   FXJson.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/02 13:42:16
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace SkillEditor
{
    [System.Serializable]
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class FXJson : CommonClipJson
    {
        public string EffectName;
        public string EffectPath;

        public int FxDuration;

        /// <summary>
        /// 是否摄像机震动
        /// </summary>
        public bool IsCameraImpulse;

        /// <summary>
        /// 是否摄像机偏移效果
        /// </summary>
        public bool IsCamaeraOffset;

        public bool ControlActivation;
        public int postPlayback;

        public bool ControlPlayableDirectors;
        public bool ControlParticleSystems;
        public bool ControlTimeControl;
        public bool ControlChildren;//暂时未导出来
        public uint RandomSpeed;
        public SpecEffect config = new SpecEffect();
    }

}