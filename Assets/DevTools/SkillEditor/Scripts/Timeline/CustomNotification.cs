///--------------------------------------------------------------------
/// 文件名   :   CustomNotification.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 18:35:22
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public class CustomNotification : INotification
{
    public PropertyName id { get; }
}
