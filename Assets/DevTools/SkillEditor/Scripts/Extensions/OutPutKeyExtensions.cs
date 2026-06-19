using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections.Generic;
///--------------------------------------------------------------------
/// 文件名   :   OutPutKeyExtensions.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/16 14:04:46
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
namespace SkillEditor
{
    /// <summary>
    /// 输出KEY
    /// </summary>
    public partial class OutputKey
    {
        public string GetOutputKey()
        {
            return Result;
        }

        public void OnResultChange()
        {

        }
    }

}
