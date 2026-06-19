///--------------------------------------------------------------------
/// 文件名   :   BaseState.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/04 16:12:56
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClientNpc
{
    public abstract class BaseState : IState
    {
        public abstract StateEnum GetStateEnum();

        protected ClientNpc mClientNpc;

        public BaseState(ClientNpc npc)
        {
            mClientNpc = npc;
        }
        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnUpdate()
        {
        }

    }
}