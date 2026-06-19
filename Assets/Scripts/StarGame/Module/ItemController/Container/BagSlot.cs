using Google.Protobuf;
using ProtoMsg;
using StarProject.Game.Data;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;


namespace StarProject.Module
{

    [XLua.LuaCallCSharp]

    //背包格子类
    public class BagSlot:BaseSlot
    {
        public bool IsSelected;//是否被选中

        public bool IsNew;//是否是新道具

        /// <summary>
        /// 初始化格子
        /// </summary>
        public override void Init(int id,int size, int state = 1, int maxStack = 0, int posX = 0, int posY = 0)
        {
            base.Init(id,size, state ,  maxStack , posX , posY );
            IsSelected = false;
            IsNew = false;
            TipsType = TipsType.Item;
        }

        public void MakeSelected(long baseID, ulong entityID, int num,bool isNew)
        {
            base.PutIn(baseID,entityID,num);
            IsNew = isNew;
        }

    }
}
