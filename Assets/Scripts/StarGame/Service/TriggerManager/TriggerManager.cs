/////--------------------------------------------------------------------
///// 文件名   :   TriggerManager.cs
///// 内  容   :   
///// 说  明   :  
///// 创建日期 :   2023/05/04 18:32:43
///// 创建人   :   赵尔东
///// 版权所有 :   游卡网络科技技术有限公司 
/////--------------------------------------------------------------------
//using SGF.Module.Framework;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Trigger
//{
//    public class TriggerManager   : ServiceModule<TriggerManager>
//    {
//        public Dictionary<ulong, ILogicTrigger> Triggers =null;
//        public void Init()
//        {
//            Triggers = new Dictionary<ulong, ILogicTrigger>();
//        }


//        public override void Release()
//        {
//            base.Release();
//        }
//    }

//    public enum TriggerExecuteType
//    {
//        Npc=0,                  //NPC
//        MainPlayer=1,           //玩家    
//    }

//    public interface ILogicTrigger
//    {
//        TriggerExecuteType M_ExecuteType { get; }

//        void EnterFrame(int frameIndex);
//    }
//}