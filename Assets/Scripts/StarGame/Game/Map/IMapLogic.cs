///--------------------------------------------------------------------
/// 文件名   :   IMapLogic.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/07 09:56:52
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMapLogic
{
    LogicType GetLogicType();
    void OnCreate();
    void OnLoad();
    void OnClear();
    void OnUnLoad();
}


public enum LogicType
{
    Area = 1,               //区域
    Obstacle = 2,           //动态阻挡
    SceneCamera = 3,        //场景虚拟相机
}