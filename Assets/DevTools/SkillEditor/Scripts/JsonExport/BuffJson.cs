///--------------------------------------------------------------------
/// 文件名   :   BuffJson.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/14 17:24:17
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using MessagePack;
using System.Collections.Generic;
namespace SkillEditor
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public class BuffJson
    {
        /// <summary>
        /// Buff配置数据
        /// </summary>
        public BuffConfig config;

        /// <summary>
        /// 阶段
        /// </summary>
        public List<StageJson> Normals = new List<StageJson>();

        /// <summary>
        /// 其他阶段
        /// </summary>
        public List<StageJson> Others = new List<StageJson>();

        /// <summary>
        /// 子弹阶段
        /// </summary>
        public List<StageJson> Bullets = new List<StageJson>();
    }
}
