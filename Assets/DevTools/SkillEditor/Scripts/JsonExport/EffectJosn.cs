///--------------------------------------------------------------------
/// 文件名   :   EffectJosn
/// 内  容   :   
/// 说  明   :  Timeline 特效轨道Clip导出
/// 创建日期 :   2022/09/01 13:48:45
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public class EffectJosn
    {
        public List<EffectData> data;
        [JsonIgnore]
        public int Start;
        [JsonIgnore]
        public int Duration;

        public void ModifyTime()
        {
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                   //客户端执行时间=轴进入时间 （第一个 是轴进入时间，后续为0）+客户端延迟时间 
                    data[i].ClinetExecuteTime = i==0? Start:0 + data[i].ClientDelayTime;
                    //服务器执行时间=  客户端执行时间-服务器提前时间
                    data[i].ServerExecuteTime =Mathf.Max(0, data[i].ClinetExecuteTime - data[i].ServerPreTime);
                    //效果结束时间=下一效果延迟时间+客户端执行时间
                    data[i].EffectEndTime = data[i].ServerNextDelayTime + data[i].ClinetExecuteTime;
                }
            }
        }
    }

}