///--------------------------------------------------------------------
/// 文件名   :   IState.cs
/// 内  容   :   
/// 说  明   :  状态机
/// 创建日期 :   2023/05/04 13:33:52
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClientNpc
{
    public interface IState
    {

        StateEnum GetStateEnum();
        void OnEnter();

        void OnUpdate();

        void OnExit();
    }

    public enum StateEnum
    {
        Idle = 0,         //待机
        Patrol = 1,       //巡逻
        Inter = 2,        //交互
    }
}